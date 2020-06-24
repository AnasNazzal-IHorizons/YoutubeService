using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace YoutubeService
{
    static class DataAccess
    {
        public static string Connectionstring = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
        /// <summary>
        /// Get Access token Data 
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAcessTokenData()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connectionstring))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand sqlCommand = new SqlCommand("[dbo].[spGetAcessTokenData]", sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    dataAdapter.SelectCommand = sqlCommand;
                    dataAdapter.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;

            }

            return ds;


        }
        /// <summary>
        /// Update the access token data  
        /// </summary>
        /// <param name="AccessTokenValue"></param>
        /// <param name="Requested_date"></param>
        /// <param name="Expire_date"></param>
        public static void UpdateAccessTokenData(string AccessTokenValue,DateTime Requested_date, DateTime Expire_date)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connectionstring))
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Open();
                    }
                    SqlCommand sqlCommand = new SqlCommand("[dbo].[spUpdateAccessTokenData]", sqlConnection);
                    sqlCommand.Parameters.Add(new SqlParameter("@Access_token_value", AccessTokenValue));
                    sqlCommand.Parameters.Add(new SqlParameter("@Requested_date", Requested_date));
                    sqlCommand.Parameters.Add(new SqlParameter("@Expire_date", Expire_date));

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        /// <summary>
        /// get asset data 
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAssetsData()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connectionstring))
                {
                    int NumberOfReternRecords = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfReternRecords"].ToString());
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand sqlCommand = new SqlCommand("[dbo].[SPGetAssetsProcessing]", sqlConnection);
                    sqlCommand.Parameters.Add(new SqlParameter("@NumberOfReternRecords", NumberOfReternRecords));
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    dataAdapter.SelectCommand = sqlCommand;
                    dataAdapter.Fill(ds);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;

            }

            return ds;
        }
        /// <summary>
        /// update Process status from 0 to 1
        /// </summary>
        /// <param name="AssetID"></param>
        /// <param name="lable"></param>
        public static void UpdateProcessAsset(string AssetID,string lable)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connectionstring))
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Open();
                    }
                    SqlCommand sqlCommand = new SqlCommand("[dbo].[spUpdateProcessAsset]", sqlConnection);
                    sqlCommand.Parameters.Add(new SqlParameter("@AssetID", AssetID));
                    sqlCommand.Parameters.Add(new SqlParameter("@Labels", lable));

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
