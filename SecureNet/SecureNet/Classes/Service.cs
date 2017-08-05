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
using System.Text.RegularExpressions;
using System.Windows.Input;
using SecureNet.Classes;

namespace SecureNet.Classes
{

    class Service
    {

        public string name { get; set; }
        public string url { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string notes { get; set; }
        public int serviceId { get; set; }
  
        //Retrieve Records
        public static List<Service> retrieveRecords(int userId)
        {

            List<Service> columnData = new List<Service>();
            SqlCommand cmd = new SqlCommand();
            byte[] usernameBytes;
            byte[] passwordBytes;
            byte[] notesBytes;


            cmd.CommandText = "Procedure";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Connection = Service.GetConnection();
         
            using (SqlDataReader saReader = cmd.ExecuteReader())
            {

                while (saReader.Read())
                {
                    if (saReader.HasRows)
                    {
                        Service serviceData = new Service();
                        serviceData.serviceId = Convert.ToInt32(saReader["serviceId"].ToString());
                        serviceData.name = saReader["serviceName"].ToString();
                        serviceData.url = saReader["serviceUrl"].ToString();
                        usernameBytes = (byte[])saReader["serviceUsername"];
                        passwordBytes = (byte[])saReader["servicePassword"];


                       
                        byte[] aesKey = Convert.FromBase64String(saReader["aesKey"].ToString());
                        aesKey = deAesKey(aesKey);
                        serviceData.password = DecryptStringFromBytes_Aes(passwordBytes, aesKey);
                        string password = serviceData.password;
                        serviceData.username = DecryptStringFromBytes_Aes(usernameBytes, aesKey);

                        if (saReader["serviceNotes"] == System.DBNull.Value)
                        {
                            serviceData.notes = null;
                        }
                        else
                        {
                            notesBytes = (byte[])saReader["serviceNotes"];
                            serviceData.notes = DecryptStringFromBytes_Aes(notesBytes, aesKey);
                        }

                        columnData.Add(serviceData);
                    }
                    else
                    {
                        columnData = null;
                    }
                }
            }
            cmd.Connection.Close();
            return columnData;


        }

        //Generate Key and send for encryption
        public static void genKeyIv(Service service, int userId, int svcId)
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

                if (svcId == -1)
                {
                    int serviceId = insertService(service.name, service.url, usernameBytes, passwdBytes, notesBytes, userId);
                    insertKey(serviceId, myAes.Key);
                }
                else
                {
                    updateService(service.name, service.url, usernameBytes, passwdBytes, notesBytes, svcId);
                    updateKey(svcId, myAes.Key);
                }
            }



        }

        //Decryption
        private static string DecryptStringFromBytes_Aes(byte[] cipherTextCombined, byte[] Key)
        {
            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
             

                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];
                byte[] cipherText = new byte[cipherTextCombined.Length - IV.Length];

                Array.Copy(cipherTextCombined, IV, IV.Length);
                Array.Copy(cipherTextCombined, IV.Length, cipherText, 0, cipherText.Length);

                
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        //Encryption
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
                
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

            }

            //Migration: No longer need to combine IV with Cipher Text
            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

           
            return combinedIvCt;
        }

        //Add Service
        private static int insertService(string nameFinal, string urlFinal, byte[] usernameBytes, byte[] passwdBytes, byte[] notesBytes, int userId)
        {
            SqlCommand cmd = new SqlCommand();
           
            cmd.CommandText = "Insert Into Service(serviceName, serviceUrl, serviceUsername, " +
                "servicePassword, serviceNotes, userId)" +
                "Values(cast(@serviceName as nvarchar(50)), " +
                "cast(@serviceUrl as nvarchar(150)), @serviceUsername," +
                "@servicePassword, @serviceNotes, @userId);" +
                " SELECT SCOPE_IDENTITY(); ";
       
            cmd.Parameters.AddWithValue("@serviceName", nameFinal);
            cmd.Parameters.AddWithValue("@serviceUrl", urlFinal);
            cmd.Parameters.AddWithValue("@serviceUsername", usernameBytes);
            cmd.Parameters.AddWithValue("@servicePassword", passwdBytes);
            cmd.Parameters.AddWithValue("@userId", userId);
            if (notesBytes != null)
            {
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

        //Update Service
        private static void updateService(string nameFinal, string urlFinal, byte[] usernameBytes,
           byte[] passwdBytes, byte[] notesBytes, int svcId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Update Service Set serviceName = @serviceName, " +
                "serviceUrl = @serviceUrl, serviceUsername = @serviceUsername," +
                "servicePassword = @servicePassword, serviceNotes = @serviceNotes " +
                "Where serviceId = @serviceId;";
            cmd.Parameters.AddWithValue("@serviceName", nameFinal);
            cmd.Parameters.AddWithValue("@serviceUrl", urlFinal);
            cmd.Parameters.AddWithValue("@serviceUsername", usernameBytes);
            cmd.Parameters.AddWithValue("@servicePassword", passwdBytes);
            cmd.Parameters.AddWithValue("@serviceId", svcId);
            if (notesBytes != null)
            {
                cmd.Parameters.AddWithValue("@serviceNotes", notesBytes);
            }
            else
            {
                cmd.Parameters.AddWithValue("@serviceNotes", SqlBinary.Null);
            }
            cmd.Connection = GetConnection();

            cmd.ExecuteNonQuery();

            cmd.Connection.Close();

        }

        //Delete Service
        public static void deleteService(int serviceId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Delete From Service Where serviceId = @serviceId;" +
                "Delete From AesKey Where serviceId = @serviceId;";
            cmd.Parameters.AddWithValue("@serviceId", serviceId);
            cmd.Connection = GetConnection();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        //Add AesKey
        private static void insertKey(int serviceId, byte[] aesKey)
        {
            aesKey = enAesKey(aesKey);
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

        //Update AesKey
        private static void updateKey(int serviceId, byte[] aesKey)
        {
            aesKey = enAesKey(aesKey);
            string aesKeyString = Convert.ToBase64String(aesKey);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Update AesKey Set aesKey = @aesKey Where serviceId = @serviceId;";
            cmd.Parameters.AddWithValue("@aesKey", aesKeyString);
            cmd.Parameters.AddWithValue("@serviceId", serviceId);
            cmd.Connection = GetConnection();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

        }

        //Log action
        private static void insertLog(int userId, string details)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Insert Into Passlog(logDetails, userId)" +
                " Values(cast(@logDetails as nvarchar(MAX)), @userId);" ;
            cmd.Parameters.AddWithValue("@logDetails", details);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Connection = GetConnection();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void logCommand(string serviceName, int commandType, string selectedvalue, int userId)
        {
            string details = null;
            if (commandType == 1)
            {
                details = "Copied " + selectedvalue + " from "+ serviceName + " login details";
            }
            else if(commandType == 2)
            {
                details = "Viewed the login details for " + serviceName;
            }
            else if (commandType == 3)
            {
                details = "Inserted login details for " + serviceName;
            }
            else if (commandType == 4)
            {
                details = "Updated login details for " + serviceName;
            }
            else if(commandType == 5)
            {
                details = "Deleted login details for " + serviceName;
            }
            else
            {
                details = "Viewed plain password for " + serviceName;
            }

            insertLog(userId, details);
        }

       
        private static byte[] enAesKey( byte[] aesKey)
        {
            byte[] enAesKey = null;
            using (Aes AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
             
                AES.Key = PrivateKey.privKey;
                AES.IV = PrivateKey.privIV;

                AES.Mode = CipherMode.CBC;

              

                using (var msEncrypt = new MemoryStream())
                {
                    using (var cs = new CryptoStream(msEncrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(aesKey, 0, aesKey.Length);
                        cs.Close();
                    }
                    enAesKey = msEncrypt.ToArray();
                }

                return enAesKey;

            }
        }

        private static byte[] deAesKey(byte[] aesKey)
        {
            byte[] deAesKey = null;
            using (Aes AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
               
                AES.Key = PrivateKey.privKey;
                AES.IV = PrivateKey.privIV;

                AES.Mode = CipherMode.CBC;



                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(aesKey, 0, aesKey.Length);
                        cs.Close();
                    }
                    deAesKey = ms.ToArray();
                }

                return deAesKey;

            }
        }



        //Connection
        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }


        //Test Empty Fields
        public static bool testEmpty(string testText)
        {
            return String.IsNullOrEmpty(testText) || String.IsNullOrWhiteSpace(testText);
        }

        //Text Max Chars
        private static bool testMax(string text, int max)
        {
            return text.Length > max;

        }

        //TestName
        public static bool testName(string name)
        {

            Regex rgxName = new Regex("^[a-zA-Z0-9\x20]*$");

            bool result = rgxName.IsMatch(name);
            if (testMax(name, 50))
            {

                return false;

            }
            else if (!rgxName.IsMatch(name))
            {


                return false;
            }
            else
            {

                return true;
            }
        }

        //Test Url
        public static bool testUrl(string url)
        {

            bool test = validUrl(url);


            if (testMax(url, 150))
            {

                return false;

            }
            else if (!validUrl(url))
            {


                return false;

            }
            else
            {

                return true;
            }
        }
        private static bool validUrl(string uriName)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
           && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

       

      


    }
}
