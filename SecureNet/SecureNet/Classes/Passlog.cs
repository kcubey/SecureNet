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

        public int logId { get; set; }


    

        public static List<Passlog> retrieveLogs(int userId)
        {
            

            List<Passlog> logData = new List<Passlog>();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT logDateTime , logDetails, logId From Passlog Where userId = @userId ORDER BY logDateTime DESC;";
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

                        logEntry.logId = Convert.ToInt32(saReader["logId"]);
                        
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


        public static void lockDown (List<int> logEntries, int userId)
        {
            string selected = null;
            foreach(int logId in logEntries)
            {
                selected += logId.ToString() + ",";
            }

             SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Insert Into Lockdown(userId, lockDetails) Values(@userId, cast(@lockDetails as nvarchar(MAX)));";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@lockDetails", selected);
            cmd.Connection = Service.GetConnection();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();



        }
    }
}
