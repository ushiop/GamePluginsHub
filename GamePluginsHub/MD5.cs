using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GamePluginsHub
{
    class File
    {
        public static string GetFileMd5(string filepath)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] m = md5.ComputeHash(fs);
            fs.Close();
            StringBuilder re = new StringBuilder();
            for (int i = 0; i < m.Length; i++)
            {
                re.Append(m[i].ToString("x2"));
            }
            return re.ToString().ToUpper();
        }
    }
}
