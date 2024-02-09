using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WithdrawerMain;
using Timer = System.Timers.Timer;

namespace WithdrawerDaemon
{
    public partial class Service1 : ServiceBase
    {
        public static readonly string Identifier = "WITHDRAWERB";
        public static readonly byte CryptKey = 0x6A;
        public static readonly int Version = 1;

        public static FileStream logFile = new FileStream(
            $"{Consts.LogFolderPath}\\DaemonLog {DateTime.Now.ToString("HH-mm-ss")}.txt",
            FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        public static System.Timers.Timer WorkTimer = new Timer();

        public static System.Threading.Timer ListenTimer =
            new System.Threading.Timer(new TimerCallback(Listen), null, 0, 1000);

        public static Config AppliedConfig = Configuration.CreateDefault();

        public static void Log(string text, string level = "I")
        {
            string msg = $"[{DateTime.Now.ToShortTimeString()}] <{level}> {text}\r\n";
            logFile.Write(Encoding.UTF8.GetBytes(msg), 0, Encoding.UTF8.GetBytes(msg).Length);
            logFile.Flush();
        }

        public Service1()
        {
            InitializeComponent();

            Config cfg = Configuration.Read(Consts.ProgRootPath+Consts.ConfigFilePath);
            if (cfg != null)
            {
                AppliedConfig = cfg;
                Log(AppliedConfig.Shout());
            }
            else
            {
                Log("LOAD FAILED", "W");
                AppliedConfig = Configuration.CreateDefault();
            }

            Log(AppliedConfig.Shout());
            WorkTimer.Interval = 1000;
            WorkTimer.AutoReset = true;
            WorkTimer.Elapsed += Run;
            WorkTimer.Enabled = true;
        }

        public void Test()
        {
            OnStart(new string[0]);
        }

        protected override void OnStart(string[] args)
        {
            Log("Service Start");
            WorkTimer.Start();
        }

        protected override void OnStop()
        {
            WorkTimer.Stop();
            Log("Stopping...");
            logFile.Close();
            base.OnStop();
        }

        private static void Run(object s, ElapsedEventArgs e)
        {
            WorkTimer.Stop();
            bool Changed = false;
            try
            {
                foreach (var t in AppliedConfig.Executables)
                {
                    string exp = "";
                    string fp = "";
                    string pn = "";
                    if (t.State == DaemonState.Enabled)
                    {
                        Log("Trying" + t.ExecutableFolder);
                        if (Directory.Exists(t.ExecutableFolder))
                        {
                            Log($"exist {t.ExecutableFolder}");
                            if (File.Exists(t.ExecutableFolder + "\\" + ".daemon"))
                            {
                                Log("exsit .daemon");
                                using (FileStream fs = new FileStream(t.ExecutableFolder + "\\" + ".daemon",
                                           FileMode.Open,
                                           FileAccess.Read))
                                {
                                    try
                                    {
                                        byte[] bc = new byte[fs.Length];
                                        fs.Read(bc, 0, (int)fs.Length);
                                        string c = Encoding.UTF8.GetString(bc);
                                        DaemonInfo d = Configuration.Deserializer.Deserialize<DaemonInfo>(c);
                                        if (File.Exists(t.ExecutableFolder + "\\" + d.Executable))
                                        {
                                            exp = t.ExecutableFolder + "\\" + d.Executable;
                                            pn = d.Executable;
                                            fp = t.ExecutableFolder + "\\" + d.Flag + d.FlagExt;
                                            if (!File.Exists(fp))
                                            {
                                                if (Process.GetProcessesByName(pn.Replace(".exe", "")).Length == 0)
                                                {
                                                    Process.Start(exp);
                                                    Log($"Restart {exp}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Log($"Invalid exe {t.ExecutableFolder + "\\" + d.Executable}");
                                            t.State = DaemonState.Invalid;
                                            Changed = true;
                                            continue;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log(ex.Message + "\n" + ex.StackTrace);
                                        t.State = DaemonState.Invalid;
                                        Changed = true;
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Log("Daemon Unsupported");
                                t.State = DaemonState.Unsupported;
                                Changed = true;
                                continue;
                            }
                        }
                        else
                        {
                            Log($"Invalid folder {t.ExecutableFolder}");
                            t.State = DaemonState.Invalid;
                            Changed = true;
                            continue;
                        }
                    }
                    else
                    {
                        // Log($"Target {t.ExecutableFolder} {t.State}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message,"E");
                return;
            }

            if (Changed)
            {
                if (File.Exists(Consts.ProgRootPath + Consts.ConfigFilePath))
                {
                    using (FileStream fs = new FileStream(Consts.ProgRootPath + Consts.ConfigFilePath, FileMode.Create,
                               FileAccess.Write))
                    {
                        string c = Configuration.Serializer.Serialize(AppliedConfig);
                        byte[] bc = Encoding.UTF8.GetBytes(c);
                        fs.Write(bc, 0, bc.Length);
                        fs.Close();
                        Log("Targets updated");
                    }
                }
            }

            WorkTimer.Interval = AppliedConfig.Interval;
            WorkTimer.Start();
        }

        private static void Listen(object o)
        {
            try
            {
                // Log("Listening");
                string useFlag = "";
                FileStream ListenStream = null;
                DirectoryInfo dir = new DirectoryInfo(Consts.ControlFolderPath);
                foreach (var file in dir.GetFiles())
                {
                    if (file.Extension == Consts.FlagExt && Consts.Flags.Contains(file.Name.Split('.')[0]))
                    {
                        useFlag = file.Name.Split('.')[0];
                        Log($"Found Flag {useFlag}, Verifying");
                        ListenStream = new FileStream(Consts.ControlFolderPath + "\\" + useFlag + Consts.FlagExt,
                            FileMode.Open, FileAccess.ReadWrite);
                        goto VerifyProcedure;
                    }
                    else
                    {
                        goto FinishProcedue;
                    }

                    VerifyProcedure:
                    if (ListenStream == null)
                    {
                        return;
                    }

                    byte[] bc = new byte[ListenStream.Length];
                    ListenStream.Read(bc, 0, bc.Length);
                    string c = Encoding.UTF8.GetString(bc);
                    ListenStream.Close();
                    Log(FileUtils.ExCode(c, CryptKey, Encoding.UTF8) + "  " + Identifier);
                    if (FileUtils.ExCode(c, CryptKey, Encoding.UTF8) == Identifier)
                    {
                        Log($"{useFlag} Verified, Implementing");
                        goto HandleProcedure;
                    }
                    else
                    {
                        Log("Invalid Control");
                        File.Copy(Consts.ControlFolderPath + "\\" + useFlag + Consts.FlagExt,
                            Consts.ControlFolderPath + "\\" + useFlag + Consts.FlagExt + "x", true);
                        File.Delete(Consts.ControlFolderPath + "\\" + useFlag + Consts.FlagExt);
                        goto FinishProcedue;
                    }

                    HandleProcedure:
                    switch (useFlag)
                    {
                        case "STOP":
                            //ListenTimer.Dispose();
                            Log("Stopping...");
                            if (logFile != null)
                            {
                                logFile.Close();
                            }

                            File.Delete(Consts.ControlFolderPath + "\\" + useFlag + Consts.FlagExt);
                            Environment.Exit(0);
                            break;
                        case "RELOAD":
                            Config cfg = Configuration.Read(Consts.ProgRootPath+Consts.ConfigFilePath);
                            if (cfg != null)
                            {
                                AppliedConfig = cfg;
                                Log(AppliedConfig.Shout());
                            }
                            else
                            {
                                Log("RELOAD FAILED", "W");
                            }

                            break;
                    }

                    File.Delete(Consts.ControlFolderPath + "\\" + useFlag + Consts.FlagExt);
                    goto FinishProcedue;
                    FinishProcedue:
                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
    }
}