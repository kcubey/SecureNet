using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;


namespace SecureNet.Classes
{
    public class PrivateKey
    {
        public static byte[] privKey
        {

            get;set;
        }

        public static byte[] privIV
        {
            get;set;
        }

     
        public void genPrivKey(string password, int userId)
        {

            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] saltBytes = getGuid(userId);

            using (Aes AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                privKey = AES.Key;
                privIV = AES.IV;


            }


        }

        private byte[] getGuid(int userId)
        {
            string guid = null;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "SELECT userguid FROM [Users] WHERE userID = @userID";
            cmd.Parameters.AddWithValue("@userID", userId);
            cmd.Connection = Service.GetConnection();


            using (SqlDataReader saReader = cmd.ExecuteReader())
            {

                while (saReader.Read())
                {
                    if (saReader.HasRows)
                    {
                        guid = Convert.ToString(saReader["userguid"]);
                    }
                }
            }

            return Encoding.ASCII.GetBytes(guid);


        }

    }
}
