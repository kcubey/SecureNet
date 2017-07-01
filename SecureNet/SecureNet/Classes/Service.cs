using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Data.SqlTypes;

namespace SecureNet.Classes
{
    class Service
    {

        public string name { get; set; }
        public string url { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string notes { get; set; }
       




        //Add Service
        private static int insertService(string nameFinal, string urlFinal, byte[] usernameBytes, byte[] passwdBytes, byte[] notesBytes, int userId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Insert Into Service(serviceName, serviceUrl, serviceUsername, servicePassword, serviceNotes, userId)" +
                " Values(cast(@serviceName as nvarchar(50))," +
                " cast(@serviceUrl as nvarchar(150))," +
                " @serviceUsername, @servicePassword, @serviceNotes, @userId);SELECT SCOPE_IDENTITY();";
            cmd.Parameters.AddWithValue("@serviceName", nameFinal);
            cmd.Parameters.AddWithValue("@serviceUrl", urlFinal);
            cmd.Parameters.AddWithValue("@serviceUsername", usernameBytes);
            cmd.Parameters.AddWithValue("@servicePassword", passwdBytes);
            cmd.Parameters.AddWithValue("@userId", userId);
            if (notesBytes != null) {
                cmd.Parameters.AddWithValue("@serviceNotes", notesBytes);
            }
            else
            {
                cmd.Parameters.AddWithValue("@serviceNotes", SqlBinary.Null);
            }
            cmd.Connection = GetConnection();

            
           int serviceId = Convert.ToInt32(cmd.ExecuteScalar());
           
            cmd.Connection.Close();

            return serviceId;
        }

        //Connection
        private static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        private static void insertKey(int serviceId, byte[] aesKey)
        {
            string aesKeyString = Convert.ToBase64String(aesKey);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Insert Into AesKey(aesKey,serviceId)" +
                " Values(cast(@aesKey as nvarchar(MAX))," +
                " @serviceId);";
            cmd.Parameters.AddWithValue("@aesKey", aesKeyString);
            cmd.Parameters.AddWithValue("@serviceId", serviceId);
            cmd.Connection = GetConnection();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            
        }
        public static void genKeyIv(Service service, int userId)
        {
            
            using (Aes myAes = Aes.Create())
            {
                myAes.KeySize = 256;

                byte[] usernameBytes = encryptInformation(service.username, myAes.Key);
                byte[] passwdBytes = encryptInformation(service.password, myAes.Key);
                byte[] notesBytes;

                if (!testEmpty(service.notes))
                {
                    notesBytes = encryptInformation(service.notes, myAes.Key);
                }
                else
                {
                    notesBytes = null;
                }

                int serviceId = insertService(service.name, service.url, usernameBytes, passwdBytes, notesBytes, userId);

                insertKey(serviceId, myAes.Key);
            }

            

        }

        private static byte[] encryptInformation(string input, byte[] aesKey)
        {
            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.BlockSize = 128;
                aesAlg.KeySize = 256;
                aesAlg.Key = aesKey;
                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(input);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

            }
            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream. 
            return combinedIvCt;
        }

        //Test Empty Fields
        private static bool testEmpty(string testText)
        {
            return String.IsNullOrEmpty(testText) || String.IsNullOrWhiteSpace(testText);
        }


    }
}
