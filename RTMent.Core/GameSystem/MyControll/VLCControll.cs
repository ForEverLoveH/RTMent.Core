using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.MyControll;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
 
namespace RTMent.Core.GameSystem.MyControll
{
    //public delegate void ConnectionOnlineCamera(string ipAddress, string account, string pass,VLCControll vLC);
    public partial class VLCControll : UserControl
    {
        public VLCControll()
        {
            InitializeComponent();
            InitVLCCamera();

        }
        IntPtr libvlc_instance_Box= IntPtr.Zero;
        IntPtr libvlc_media_player_Box= IntPtr.Zero;
        /// <summary>
        /// 
        /// </summary>
        [Description("ip地址"), Category("自定义属性")]
        public string panel_IPAddress 
        {
            get { return textBox1.Text.Trim(); }
            set { textBox1.Text = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("账号"), Category("自定义属性")]
        public string panel_User
        {
            get
            {
                return textBox2.Text.Trim();
            }set { textBox2.Text = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("密码"), Category("自定义属性")]
        public string panel_Password
        {
            get => textBox3.Text.Trim();
            set => textBox3.Text = value; 
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.Trim() == "隐藏")
            {
                textBox3.PasswordChar='*';
                button1.Text = "显示";

            }
            else if(button1.Text.Trim() =="显示")
            {
                textBox3.PasswordChar = '\0';
                button1.Text = "隐藏";
            }
           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected  void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()) && !string.IsNullOrEmpty(textBox2.Text.Trim()) && !string.IsNullOrEmpty(textBox3.Text.Trim()))
                ConnectionOnLineCamera();
        }
        /// <summary>
        /// 连接网络相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionOnLineCamera()
        {
            
            string ipaddress= textBox1.Text.Trim();
            string acc = textBox2.Text.Trim();
            string pass = textBox3.Text.Trim();
            if (!string.IsNullOrEmpty(ipaddress) && !string.IsNullOrEmpty(acc) && !string.IsNullOrEmpty(pass))
            {
                BindVideoCamera(ipaddress, acc, pass);
                SaveOnLineCameraData(ipaddress, acc, pass);
            }
        }
        string PATH= @"data/CameraData.txt";
        private void SaveOnLineCameraData(string ipaddress, string acc, string pass)
        { 
            string SL = string.Format("IPAddress:{0};Account:{1};Password:{2}",ipaddress,acc,pass);
            var po=File.ReadAllText(PATH);
            List<string> list = new List<string>();
            foreach(var pl in po)
            {
                list.Add(pl.ToString());
            }
            if (!list.Contains(SL))
            {
                System.IO.File.WriteAllText(PATH, SL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="acc"></param>
        /// <param name="pass"></param>
        private void BindVideoCamera(string ipaddress, string acc, string pass)
        {
            string sts = string.Format("rtsp://{0}:{1}@{2}:554/stream1", acc, pass, ipaddress);
            Console.WriteLine(sts);
            bool res = MediaHelper.Instance.NetWork_Media_Play(libvlc_instance_Box, libvlc_media_player_Box, sts);
            if (res == false)
            {
                MediaHelper.Instance.MediaPlayer_Stop(libvlc_media_player_Box);
            }
            panel_ToolStrip = "摄像头已连接";
            panel_ToolStripColor = Color.MediumSeaGreen;

        }

        /// <summary>
        /// 
        /// </summary>
        [Description("连接相机"), Category("自定义属性")]
        public string panel_ToolStrip
        {
            get => toolStripStatusLabel1.Text.Trim();
            set => toolStripStatusLabel1.Text = value;
        }
        [Description("连接相机颜色"), Category("自定义属性")]
        public Color panel_ToolStripColor
        {
            get => toolStripStatusLabel1.ForeColor;
            set => toolStripStatusLabel1.ForeColor = value;
        }



        /// <summary>
        /// 
        /// </summary>
        private void InitVLCCamera()
        {
            libvlc_instance_Box = MediaHelper.Instance.Create_Media_Instance();
            libvlc_media_player_Box = MediaHelper.Instance.Create_MediaPlayer(libvlc_instance_Box, panel6.Handle);
            SafeNativeMethods.libvlc_audio_set_volume(libvlc_media_player_Box, 0);

        
        }
    }
}
