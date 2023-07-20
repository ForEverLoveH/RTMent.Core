using RTMent.Core.GameSystem.GameHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core
{
    public class GameRoot
    {
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern void SetForegroundWindow(IntPtr mainWindowHandle);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        /// <summary>
        /// 
        /// </summary>
        public void StartGame()
        {
            WriteLogHelper.Instance.WriteLog();
        }
    }
}
