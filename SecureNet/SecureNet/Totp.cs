using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using SecureNet.Classes;
using System.IO;
using System.Data.SqlClient;

[assembly: InternalsVisibleTo("TotpTests")]

namespace TOTP
{
    public class Totp
    {
        private readonly int digits = 6;
        private readonly HMACSHA1 hmac;
        private readonly Int32 t1 = 30;
        internal byte[] key;

        public Totp(string base32string)
        {
            key = FromBase32String(base32string);
            hmac = new HMACSHA1(key);
        }

        public Totp(string base32string, Int32 t1, int digits) : this(base32string)
        {
            this.t1 = t1;
            this.digits = digits;
        }

        private byte[] FromBase32String(string base32string)
        {
            return Base32.FromBase32String(base32string);
        }

        public static string ToBase32String(byte[] data)
        {
            return Base32.ToBase32String(data);
        }

        public int getCode()
        {
            var unixTimestamp = (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return getCode(unixTimestamp);
        }

        public int getCode(UInt64 timeStamp)
        {
            var message = timeStamp / (UInt64)t1;
            var msg_byte = BitConverter.GetBytes(message);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(msg_byte, 0, msg_byte.Length);
            var hash = hmac.ComputeHash(msg_byte);
            var offset = (hash[hash.Length - 1] & 0xf);
            var i = ((hash[offset] & 0x7f) << 24) | ((hash[offset + 1] & 0xff) << 16) | ((hash[offset + 2] & 0xff) << 8) |
                    (hash[offset + 3] & 0xff);
            var code = i % (int)Math.Pow(10, digits);
            return code;
        }

        public String getCodeString()
        {
            var unixTimestamp = (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return getCodeString(unixTimestamp);
        }

        public String getCodeString(UInt64 timeStamp)
        {
            var ret = getCode(timeStamp) + "";
            return ret.PadLeft(digits, '0');
        }

        public static bool CheckBase32(string base32string)
        {
            try
            {
                var tmp = Base32.FromBase32String(base32string);
                return tmp != null;
            }
            catch
            {
                return false;
            }
        }
        public static string newOTP(int userId)
        {
            string seed = decryptSeed(userId);
            Totp totp = new Totp(seed, 60, 6);
            string totpCode = totp.getCodeString();
            return totpCode;
        }

        private static string decryptSeed(int userId)
        {
            byte[] seed = retrieveSeed(userId);
            string seedBase32 = null;
            using (Aes AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                AES.Key = PrivateKey.privKey;
                AES.IV = PrivateKey.privIV;

                AES.Mode = CipherMode.CBC;



                using (var msEncrypt = new MemoryStream())
                {
                    using (var cs = new CryptoStream(msEncrypt, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(seed, 0, seed.Length);
                        cs.Close();
                    }
                    seed = msEncrypt.ToArray();
                }

                seedBase32 = Base32.ToBase32String(seed);

            }

            return seedBase32;

        }

        private static byte[] retrieveSeed(int userId)
        {
            byte[] encryptedSeed = null;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT seedText From UserSeed Where userId = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Connection = Service.GetConnection();
            using (SqlDataReader saReader = cmd.ExecuteReader())
            {

                while (saReader.Read())
                {
                    if (saReader.HasRows)
                    {
                        encryptedSeed = (byte[])saReader["seedText"];
                    }
                }
            }

            cmd.Connection.Close();
            return encryptedSeed;
        }

        
       /* public static void encryptSeed(int userId)
        {
            string seed = "CMFC4FUMEN7QNNHK4OZD2UVUIF6NJKPB";
            byte[] seedBytes = Base32.FromBase32String(seed);
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
                        cs.Write(seedBytes, 0, seedBytes.Length);
                        cs.Close();
                    }
                    seedBytes = msEncrypt.ToArray();
                }

               
            }

            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "Insert Into UserSeed(seedText, userId) " +
                "Values(@seedText, @userId);";
            cmd.Parameters.AddWithValue("@seedText", seedBytes);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Connection = Service.GetConnection();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }*/
        
    }
}