using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WithdrawerMain;

namespace WithdrawerDaemon
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            if (!FileUtils.Check())
            {
                return;
            }
            try
            {
                if (args.Length!=0&&args[0] == "Gen")
                {
                    Configuration.Write(Configuration.CreateDefault(), "D:\\");
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new Service1()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}
