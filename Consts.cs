using System.Collections.Generic;

namespace WithdrawerDaemon
{
    public class Consts
    {
        public static readonly string ProgRootPath = @"C:\yService\Daemon";
        public static readonly string LogFolderPath = @"C:\yService\Daemon\Logs";
        public static readonly string ControlFolderPath = @"C:\yService\Daemon\Controls";
        public static readonly string ConfigFilePath = @"\config.yaml";
        
        public static List<string> Flags = new List<string>(){ "STOP", "RELOAD" };
        public static readonly string FlagExt = ".ctr";
    }
}