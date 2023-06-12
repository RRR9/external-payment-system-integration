using System.Data;
using System.Data.SqlClient;

namespace ZudamalZetMobileServices
{
    class SqlServer
    {
        static private readonly string _сonnect = @"Data Source=(local);database=;Integrated Security=True";

        public static DataTable GetData(string spName, SqlParameter[] sqlParam)
        {
            using (SqlConnection ConnectToDB = new SqlConnection(_сonnect))
            {
                ConnectToDB.Open();
                SqlCommand sqlCommand = new SqlCommand(spName, ConnectToDB);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                if (sqlParam != null)
                {
                    for (int i = 0; i < sqlParam.Length; i++)
                    {
                        sqlCommand.Parameters.Add(sqlParam[i]);
                    }
                }
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    DataSet dataSet = new DataSet();
                    sqlDataAdapter.Fill(dataSet);
                    return dataSet.Tables[0];
                }
            }
        }

        public static int ExecSP(string spName, SqlParameter[] sqlParam)
        {
            using (SqlConnection ConnectToDB = new SqlConnection(_сonnect))
            {
                ConnectToDB.Open();
                SqlCommand sqlCommand = new SqlCommand(spName, ConnectToDB);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                if (sqlParam != null)
                {
                    for (int i = 0; i < sqlParam.Length; i++)
                    {
                        sqlCommand.Parameters.Add(sqlParam[i]);
                    }
                }
                sqlCommand.Parameters.Add("@retVal", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                sqlCommand.ExecuteNonQuery();
                return (int)sqlCommand.Parameters["@retVal"].Value;
            }
        }

        public static void UpdateBalance(string balance, int provId)
        {
            int res;
            string sql = "UPDATE [DB].[dbo].[Provider] SET [OnlineBalance] = " + balance + " WHERE [ID] =" + provId;
            if (!string.IsNullOrEmpty(balance))
            {
                using (SqlConnection conn = new SqlConnection(_сonnect))
                {
                    SqlCommand command = new SqlCommand(sql, conn);
                    command.Connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            else
            {
                throw new ZetMobileException("Balance is null");
            }
        }
    }
}
