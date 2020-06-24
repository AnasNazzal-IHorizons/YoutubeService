using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTubePartner.v1;
using Google.Apis.YouTubePartner.v1.Data;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;
using System.Xml.Linq;

namespace YoutubeService
{
    class Program
    {

        private static System.Timers.Timer aTimer;
     
        static void Main(string[] args)
        {
            SetTimer();
            Console.ReadLine();
        }
        private static void SetTimer()
        {
          
            aTimer = new System.Timers.Timer(2000);//wait 2 seconds when run the Console   Application 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            aTimer.Stop();
            try
            {
                Execute();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            aTimer.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["TimeInterval"]) * 60000;

            aTimer.Enabled = true;
            aTimer.Start();
        }


        /// <summary>
        /// Execute the main function 
        /// </summary>
        public static void Execute()
        {
            string str_Access_Token = GetAccessToken();

            DataSet dsAssetsData = DataAccess.GetAssetsData();

            if (dsAssetsData.Tables.Count > 0 && dsAssetsData.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsAssetsData.Tables[0].Rows.Count; i++)
                {
                    try
                    {
                        bool result = UpdateLabelForAssets(dsAssetsData.Tables[0].Rows[i]["AssetID"].ToString(), dsAssetsData.Tables[0].Rows[i]["ChannelID"].ToString(), str_Access_Token);
                        if (result == true)
                        {
                            string str_lable = GetlabelValueByChannelID(dsAssetsData.Tables[0].Rows[i]["ChannelID"].ToString());
                            DataAccess.UpdateProcessAsset(dsAssetsData.Tables[0].Rows[i]["AssetID"].ToString(), str_lable);
                            Console.WriteLine(dsAssetsData.Tables[0].Rows[i]["AssetID"].ToString() + "---Done");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                Console.WriteLine("--------------------");

            }
            else
            {
                Console.WriteLine("No Data Found...");
            }
        }

        /// <summary>
        /// update the lable based on Channel ID 
        /// </summary>
        /// <param name="AssetID"></param>
        /// <param name="ChannelID"></param>
        /// <param name="Access_token"></param>
        /// <returns></returns>
        public static bool UpdateLabelForAssets(string AssetID, string ChannelID, string Access_token)
        {
            bool status = false;
            try
            {
                string UpdateURL = string.Format("{0}{1}?key={2}", "https://www.googleapis.com/youtube/partner/v1/assets/", AssetID, ConfigurationManager.AppSettings["API_Key"].ToString());

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UpdateURL);
                request.Method = "PUT";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + Access_token);
                labels _labels = new labels();

                _labels.label = ReadlabelValueByChannelID(ChannelID);

                string postlableData = JsonConvert.SerializeObject(_labels);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postlableData);
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }

                status = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                status = false;
            }
            return status;
        }


        /// <summary>
        /// generate Access token from DB or Call API
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken()
        {

            string str_Access_Token = string.Empty;
            //get the access token data in order to check the expire date for access token 

            DataSet ds_AccesstokenData = DataAccess.GetAcessTokenData();
      

            if (DateTime.Now > Convert.ToDateTime(ds_AccesstokenData.Tables[0].Rows[0]["Expire_date"].ToString()))
            {
                string keyFilePath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["client_secrets_Path"].ToString());
                string jsonContent = System.IO.File.ReadAllText(keyFilePath);
                PersonalServiceAccountCred cr = JsonConvert.DeserializeObject<PersonalServiceAccountCred>(jsonContent);

                DateTime Requested_date = new DateTime();
                DateTime Expire_date = new DateTime();
                try
                {

                    ServiceAccountCredential xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(cr.client_email)
                    {
                        Scopes = new[]
                       {
                       YouTubePartnerService.Scope.Youtubepartner
                      }
                    }.FromPrivateKey(cr.private_key));

                    YouTubePartnerService ytX = new YouTubePartnerService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = xCred

                    });
                    Asset _asset = ytX.Assets.Get("A528497684258438").Execute();

                    str_Access_Token = xCred.Token.AccessToken;
                    Requested_date = xCred.Token.Issued;
                    Expire_date = xCred.Token.Issued.AddSeconds(Convert.ToDouble(xCred.Token.ExpiresInSeconds));

                    //update access token data in the DB 
                    DataAccess.UpdateAccessTokenData(str_Access_Token, Requested_date, Expire_date);

                }
                catch (Exception ex)
                {
                    str_Access_Token = string.Empty;

                }
            }
            else
            {
                str_Access_Token = ds_AccesstokenData.Tables[0].Rows[0]["Access_token_value"].ToString();

            }

            return str_Access_Token;

        }

        /// <summary>
        /// Read the Lable Value based on Channel ID
        /// </summary>
        /// <param name="ChannelID"></param>
        /// <returns></returns>
        public static string[] ReadlabelValueByChannelID(string ChannelID)
        {
            XDocument labelDoc;
            string configPath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["LabelSettingsPath"].ToString());

            labelDoc = XDocument.Load(configPath);
            string labelValue = (from Channels in labelDoc.Descendants("Channel")
                                    where Channels.Attribute("ChannelID").Value == ChannelID
                                    select Channels.Attribute("labelValue").Value).FirstOrDefault();

            string[] lable = { labelValue };

            return lable;

        }
        /// <summary>
        /// Get the Lable Value as string to save it on DB  based on Channel ID
        /// </summary>
        /// <param name="ChannelID"></param>
        /// <returns></returns>
        public static string GetlabelValueByChannelID(string ChannelID)
        {
            XDocument labelDoc;
            string configPath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["LabelSettingsPath"].ToString());

            labelDoc = XDocument.Load(configPath);
            string labelValue = (from Channels in labelDoc.Descendants("Channel")
                                 where Channels.Attribute("ChannelID").Value == ChannelID
                                 select Channels.Attribute("labelValue").Value).FirstOrDefault();


            return labelValue;


        }
    }
}
