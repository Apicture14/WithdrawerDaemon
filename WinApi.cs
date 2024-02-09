using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Automation;
using YamlDotNet.Serialization;

namespace WithdrawerMain
{
    public static class WinApi
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowA", SetLastError = true)]
        public static extern IntPtr FindWindowA(string wclass,string name);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parent,IntPtr child,string wclass,string name);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern int SendMessage(IntPtr handle, int msg, int wParm, int lParm);
        [DllImport("user32.dll", EntryPoint = "GetWindowTextA", SetLastError = true)]
        public static extern int GetWindowText(IntPtr handle, StringBuilder text, int length);
        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr handle);
        [DllImport("user32.dll", EntryPoint = "WindowFromPoint", SetLastError = true)]
        public static extern IntPtr WindowFromPoint(Point point);
        [DllImport("user32.dll", EntryPoint = "GetWindow", SetLastError = true)]
        public static extern IntPtr GetNextWindow(IntPtr thisHandle, uint wCmd);

        [DllImport("user32.dll", EntryPoint = "GetClassName", SetLastError = true)]
        public static extern int GetClassName(IntPtr handle, StringBuilder sb, int lParm);

        

        public static int w = Screen.PrimaryScreen.Bounds.Width;
        public static int h = Screen.PrimaryScreen.Bounds.Height;

        public delegate bool EnumWindowsCallBack(int handle, int lparm);

        [DllImport("user32.dll", EntryPoint = "EnumWindows")]
        public static extern bool EnumWindows(EnumWindowsCallBack callBack, int lParm);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr handle, int swCmd);

        [DllImport("user32.dll", EntryPoint = "GetMinimized", SetLastError = true)]
        public static extern bool GetMinimized(IntPtr handle);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr handle);
        
        public static int MSG_CLOSE = 0x0010;
        
        public static int SW_HIDE = 0;
        public static int SW_NORMAL = 1;
        public static int SW_MINIMIZED = 2;
        public static int SW_MAXIUMIZED = 3;
    }
}