using MySql.Data.MySqlClient;

namespace SalaryApp
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "91.191.231.76";
            int port = 3307;
            string database = "pr_is207_22";
            string username = "pr_is207_22";
            string password = "pr_is207_22";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}