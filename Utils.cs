using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WithdrawerDaemon;
using YamlDotNet.Serialization;

namespace WithdrawerMain
{
    public static class KillUtils
    {
        
    }

    public static class FileUtils
    {
        public static string ExCode(string ori, byte key,Encoding encoding)
        {
            byte[] bori = encoding.GetBytes(ori);
            for (int i = 0; i < bori.Length; i++)
            {
                bori[i] = (byte)(bori[i] ^ key);
            }

            return encoding.GetString(bori);
        }

        public static bool Check()
        {
            try
            {
                if (!Directory.Exists(Consts.ControlFolderPath))
                {
                    Directory.CreateDirectory(Consts.ControlFolderPath);
                }

                if (!Directory.Exists(Consts.LogFolderPath))
                {
                    Directory.CreateDirectory(Consts.LogFolderPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public static class Configuration
    {
        public static Serializer Serializer = new Serializer();
        public static Deserializer Deserializer = new Deserializer();

        public static Config CreateDefault()
        {
            return new Config()
            {
                Executables = new List<DaemonTarget>()
                {
                    new DaemonTarget()
                    {
                        State = DaemonState.Disabled,
                        ExecutableFolder = "eee"
                    }
                },
                Interval = 1000,
                Version = 1,
                Identifier = FileUtils.ExCode(Service1.Identifier,Service1.CryptKey,Encoding.UTF8)
            };
        }

        public static Config Read(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream ReadStream = new FileStream(path,FileMode.Open,FileAccess.Read))
                {
                    try
                    {
                        byte[] bcontent = new byte[ReadStream.Length];
                        ReadStream.Read(bcontent, 0, bcontent.Length);
                        ReadStream.Close();
                        string content = Encoding.UTF8.GetString(bcontent);
                        return Deserializer.Deserialize<Config>(content);
                    }
                    catch (Exception e)
                    {
                        Service1.Log(e.Message+"\r\n"+e.StackTrace,"E");
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        
        public static bool Write(Config cfg,string path)
        {
            if (Directory.Exists(path))
            {
                string nPath = path + Consts.ConfigFilePath;
                try
                {
                    using (FileStream WriteStream = new FileStream(nPath,FileMode.Create,FileAccess.Write))
                    {
                        byte[] bc = Encoding.UTF8.GetBytes(Serializer.Serialize(cfg));
                        WriteStream.Write(bc,0,bc.Length);
                        WriteStream.Close();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }       
            }
            else
            {
                return false;
            }
        }
    }

    public class Config
    {
        public string Identifier { get; set; }
        public int Version { get; set; }
        public int Interval { get; set; }
        public List<DaemonTarget> Executables { get; set; }
        public string Shout()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Config at Version {Version}\r\n");
            sb.Append($"Interval {Interval}\r\n");
            sb.Append("Tagets:\r\n");
            foreach (var target in Executables)
            {
                sb.Append($"{target.ExecutableFolder} {target.State}\r\n");
            }
            return sb.ToString();
        }
}

    
}