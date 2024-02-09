using System.IO;
using System.Text;
using WithdrawerMain;

namespace WithdrawerDaemon
{
    public class DaemonUtils
    {
        public static void AddToDaemon(string ExecutablePath,string FlagName)
        {
            if (File.Exists(ExecutablePath))
            {
                DaemonInfo d = new DaemonInfo()
                {
                    Executable = ExecutablePath,
                    Flag = FlagName
                };
                using (FileStream WriteStream = new FileStream("./.deamon",FileMode.OpenOrCreate,FileAccess.ReadWrite))
                {
                    byte[] bc = Encoding.UTF8.GetBytes(Configuration.Serializer.Serialize(d));
                    WriteStream.Write(bc,0,bc.Length);
                    WriteStream.Close();
                }
            }
        }

        public static void ReleaseDaemon()
        {
            if (File.Exists("./.daemon"))
            {
                File.Delete("./.daemon");
            }
            else
            {
                return;
            }
        }
    }

    public class DaemonInfo
    {
        public string Executable { get; set; }
        public string Flag { get; set; }
        public string FlagExt { get; set; } = ".flg";
    }
    public enum DaemonState
    {
        Enabled,
        Disabled,
        Invalid,
        Unsupported
    }
    public class DaemonTarget
    {
        public string ExecutableFolder { get; set; }
        public DaemonState State { get; set; } = DaemonState.Disabled;
    }
}