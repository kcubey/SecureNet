using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecureNet.Pages.Register
{
    class Users
    {
        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        public Int32 userId { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string userKey { get; set; }
        public string userIV { get; set; }

        // Key IV generator
        public static string KeyIvGenerator(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return res.ToString();
        }


        // AES256 Encryption for Users Info
        private static byte[] EncryptAES256(byte[] content, string key, string iv)
        {
            // Convert PT to byte
            byte[] plainbyte = content;

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encryptedstring = crypto.TransformFinalBlock(plainbyte, 0, plainbyte.Length);


            return encryptedstring;
        }

        // AES256 Decryption for Users Info
        public static byte[] DecryptAES256(byte[] content, string key, string iv)
        {
            byte[] encryptedtext = content;

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] plaintext = crypto.TransformFinalBlock(encryptedtext, 0, encryptedtext.Length);
            return plaintext;
        }
        public static int GetUserIdByEmailAndPassword(string userEmail, string masterPass)
        {
            // this is the value we will return
            int userId = 0;


            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            using (
                SqlCommand cmd =
                    new SqlCommand("SELECT userID, userMaster, userPhone, userguid FROM [Users] WHERE userEmail=@userEmail", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@userEmail", userEmail);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
             
                    // read data from database if we found any user
                    int dbUserId = Convert.ToInt32(dr["userID"]);
                    string dbPassword = Convert.ToString(dr["userMaster"]);
                    string dbUserGuid = Convert.ToString(dr["userguid"]);

                    // Now we hash the UserGuid from the database with the password we want to check
                    // In the same way as saving it to the database in the first place.
                    string hashedPassword = SecurityHash.HashSHA1(masterPass + dbUserGuid);
                    using (
                        SqlCommand cmd2 = new SqlCommand("SELECT * FROM [UserValue] WHERE userEmail=@userEmail", GetConnection()))
                    {
                        cmd2.Parameters.AddWithValue("@userEmail", userEmail);
                        
                        SqlDataReader dr2 = cmd2.ExecuteReader();
                        while (dr2.Read())
                        {
                            string userKey = Convert.ToString(dr2["userKey"]);
                            string userIV = Convert.ToString(dr2["userIV"]);
                            byte[] decryptPhone = DecryptAES256((byte[])dr["userPhone"], userKey, userIV);
                            string decryptedPhone = System.Text.Encoding.UTF8.GetString(decryptPhone);
                        }
                                              
                    }       

                    //  SqlCommand cmd1 =new SqlCommand ("SELECT verified FROM [Users] WHERE Username=@Username", con);

                    // if its correct password the result of the hash is the same as in the database
                    if (dbPassword == hashedPassword)
                    {
                        // The password is correct
                        userId = dbUserId;
                    }
                    else
                    {

                    }
                }
              
            }

            // Return the user id which is 0 if we did not found a user.

            return userId;
        }
        //Add new user
        public static bool addUser(string email, string phone, string password)
        {
            string keyGen = KeyIvGenerator(32);
            string ivGen = KeyIvGenerator(16);

            // Encryption for Email
           // byte[] EmailByte = System.Text.Encoding.UTF8.GetBytes(email);
           // byte[] encryptedEmail = EncryptAES256(EmailByte, keyGen, ivGen);

            byte[] phoneByte = System.Text.Encoding.UTF8.GetBytes(phone);
            byte[] encryptedPhone = EncryptAES256(phoneByte, keyGen, ivGen);

            //byte[] passwordByte = System.Text.Encoding.UTF8.GetBytes(password);
            //byte[] encryptedpassword = EncryptAES256(passwordByte, keyGen, ivGen);

           Guid userGuid = System.Guid.NewGuid();
           string hashedPassword = SecurityHash.HashSHA1(password + userGuid.ToString());

            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            using (SqlCommand cmd = new SqlCommand
                ("INSERT INTO [Users](userEmail, userMaster, userPhone, userGuid) values (@userEmail, @userMaster, @userPhone, @userGuid)", Users.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@userEmail", email);
                //cmd.Parameters.AddWithValue("@userMaster", encryptedpassword);
                cmd.Parameters.AddWithValue("@userMaster", hashedPassword); //store hashed value
                cmd.Parameters.AddWithValue("@userPhone", encryptedPhone);            
                cmd.Parameters.AddWithValue("@userGuid", userGuid);

                con.Open();

                SqlCommand cmd2 = new SqlCommand("INSERT into UserValue(userEmail,userKey,userIV)"
                   + "VALUES(@userEmail,@userKey,@userIV)", Users.GetConnection());
                cmd2.Parameters.Add(new SqlParameter("@userEmail", SqlDbType.NVarChar, 32));
                cmd2.Parameters.Add(new SqlParameter("@userKey", SqlDbType.NVarChar, 32));
                cmd2.Parameters.Add(new SqlParameter("@userIV", SqlDbType.NVarChar, 16));
                cmd2.Prepare();

                cmd2.Parameters["@userEmail"].Value = email;
                cmd2.Parameters["@userKey"].Value = keyGen;
                cmd2.Parameters["@userIV"].Value = ivGen;
                cmd2.ExecuteNonQuery();

                cmd.ExecuteNonQuery();
                con.Close();
            }

            return true;
        }
    }
}