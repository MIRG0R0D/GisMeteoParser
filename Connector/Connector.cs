using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public sealed class Connector : IDisposable
    {
        private static MySqlConnection conn;
        private static readonly Connector instance = new Connector();

        public string server = "localhost";
        public string user = "root";
        public string password = "root";
        public string database = "gismeteo";

        private Connector()
        {
            //constructor todo
            try
            {
                OpenConnection();
                if (IsConnected())
                {
                    string sql = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{database}';";
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    string result = command.ExecuteScalar()?.ToString();
                    if (!(string.IsNullOrEmpty(result)) && result.Equals(database))
                    { 
                        OpenConnection(true);
                        sql = $"SELECT count(*) FROM information_schema.tables WHERE table_schema = '{database}' AND table_name = 'city';;";
                        command = new MySqlCommand(sql, conn);
                        if (int.Parse(command.ExecuteScalar().ToString())==0)
                        {
                            CreateTables();
                        }
                    }
                    else
                        createSchema();    //throw new Exception($"DB missing schema '{database}'");
                    return;
                }
                else throw new Exception("DB server connection failed");

            }
            catch (Exception)
            {
            }
        }

        private void createSchema()
        {
            string sql = $"CREATE DATABASE IF NOT EXISTS {database};";
            MySqlCommand command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
            OpenConnection(true);
            CreateTables();

        }

        private static void CreateTables()
        {
            string sql;
            
            sql = $"CREATE TABLE IF NOT EXISTS `city` (`id` int NOT NULL ,`name` varchar(45) DEFAULT NULL,PRIMARY KEY(`id`)); ";
            var command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = $"CREATE TABLE IF NOT EXISTS `forecast` (`idcity` int NOT NULL,`date` date NOT NULL,`daytemp` int NOT NULL,`nighttemp` int NOT NULL);";
            command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public static Connector GetInstance()
        {
            var state = conn.State;
            return instance;
        }

        public static void Main(string[] args) 
        {
            /*
            Connector.OpenConnection();
            Console.WriteLine("Connection opened");
            Connector.CloseConnection();
            Console.ReadKey();*/
        }

        private void OpenConnection(bool useDB=false)
        {
            string connStr = $"server={server};user={user};password={password};" + ((useDB) ? $"database={database};" : "");


            conn = new MySqlConnection(connStr);
            conn.Open();
        }

        private void CloseConnection()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }

        public bool IsConnected()
        {
            return conn.State == System.Data.ConnectionState.Open;
        }

        public bool IsCityExist(string cityId)
        {
            string sql = $"SELECT count(name) FROM city where id = '{cityId}'";
            MySqlCommand command = new MySqlCommand(sql, conn);
            return int.Parse(command.ExecuteScalar().ToString()) != 0;
        }

        public void AddCity(string cityId, string name)
        {
            string sql = $"INSERT INTO `city` (`id`, `name`) VALUES('{cityId}', '{name}');";
            MySqlCommand command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void AddForecast(DateTime date, string dayTemp, string nightTemp, string Name, string id)
        {
            if (!IsCityExist(id)) AddCity(id, Name);
            string sql;
            if (isForecastExist(id, date)) sql = $"UPDATE forecast SET `daytemp` = '{dayTemp}', `nighttemp` = '{nightTemp}' WHERE `idcity` = '{id}' AND date = '{date.ToString("yyyy-MM-dd")}';";
            else sql = $"INSERT INTO forecast (`idcity`, `date`, `daytemp`, `nighttemp`) VALUES ('{id}', '{date.ToString("yyyy-MM-dd")}', '{dayTemp}', '{nightTemp}');";
            MySqlCommand command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public CityTemp GetForecast(string idCity, DateTime date)
        {
            CityTemp result = null;
            string sql = $@"SELECT 
                            f.idcity, f.date, f.daytemp, f.nighttemp, c.name
                        FROM
                            forecast f LEFT JOIN
                            city c ON (f.idcity = c.id)
                        WHERE
                            (f.idcity = '{idCity}' AND 
                            date = '{date.ToString("yyyy-MM-dd")}');";
            MySqlCommand command = new MySqlCommand(sql, conn);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read() && reader.FieldCount == 5)
                {
                    result = new CityTemp()
                    {
                        id = reader[0].ToString(),
                        Date = DateTime.Parse(reader[1].ToString()),
                        DayTemp = reader[2].ToString(),
                        NightTemp = reader[3].ToString(),
                        Name = reader[4].ToString()
                    };
                }
                return result;
            }
        }

        private bool isForecastExist(string idCity, DateTime date)
        {
            string sql = $"SELECT count(idcity) FROM forecast where idcity = '{idCity}' AND date = '{date.ToString("yyyy-MM-dd")}';";
            MySqlCommand command = new MySqlCommand(sql, conn);
            int result = int.Parse(command.ExecuteScalar().ToString());
            return result > 0;
        }

        public void PrintCities()
        {
            string sql = "SELECT * FROM city";

            MySqlCommand command = new MySqlCommand(sql, conn);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader[0].ToString() + " " + reader[1].ToString());
                }
            }
        }

        public Dictionary<string, string> GetCityList()
        {
            string sql = "SELECT c.id, c.name FROM forecast AS f LEFT JOIN city AS c ON (f.idcity = c.id) WHERE f.date = CURDATE() + INTERVAL 1 DAY;";
            Dictionary<string, string> result = new Dictionary<string, string>();
            MySqlCommand command = new MySqlCommand(sql, conn);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    //Console.WriteLine(reader[0].ToString() + " " + reader[1].ToString());
                    result.Add(reader[0].ToString(), reader[1].ToString());
                }
            }
            return result;
        }

        public void Dispose()
        {  
            //todo check if disposing
            CloseConnection();
        }
    }
}
