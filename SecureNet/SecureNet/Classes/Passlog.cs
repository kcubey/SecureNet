using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace SecureNet.Classes
{
    class Passlog
    {
        public string logDetails { get; set; }
        public DateTime logDateTime { get; set; }

        public static List<Passlog> retrieveLogs(int userId)
        {
            

            List<Passlog> logData = new List<Passlog>();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT logDateTime , logDetails From Passlog Where userId = @userId ORDER BY logDateTime DESC;";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Connection = Service.GetConnection();

            using (SqlDataReader saReader = cmd.ExecuteReader())
            {

                while (saReader.Read())
                {
                    if (saReader.HasRows)
                    {
                        Passlog logEntry = new Passlog();
                        logEntry.logDateTime = Convert.ToDateTime(saReader["logDateTime"]);
                       
                        logEntry.logDetails = saReader["logDetails"].ToString();
                        
                        logData.Add(logEntry);
                    }
                    else
                    {
                        logData = null;
                    }
                }
            }
            cmd.Connection.Close();
            return logData;

        }
    }
}
