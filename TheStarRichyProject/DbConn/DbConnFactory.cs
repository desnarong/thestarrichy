using Dapper;
using System.Data.SqlClient;

namespace TheStarRichyProject.DbConn
{
    public class DbConnFactory
    {
        protected static DbConnFactory _instance;

        protected static IConfiguration _config;

        public static DbConnFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    SqlMapper.Settings.CommandTimeout = 300;
                    _instance = new DbConnFactory();
                }
                return _instance;
            }
        }


        protected DbConnFactory()
        {
            _config = CommonConfig.Instance.Config;
        }

        public SqlConnection CreateConnection()
        {
            SqlConnection newConn = new SqlConnection(_config["DbConnectionString"]);
            newConn.Open();

            return newConn;
        }
        public SqlConnection CreateCryptConnection()
        {
            SqlConnection newConn = new SqlConnection(_config["DbConnectionString"]);
            newConn.Open();
            newConn.Execute("exec [comm].[sp_sec_open_key]");

            return newConn;
        }
    }
}
