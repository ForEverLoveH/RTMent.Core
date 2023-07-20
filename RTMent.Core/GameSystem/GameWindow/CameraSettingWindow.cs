using AForge.Video.DirectShow;
using HZH_Controls;
using RTMent.Core.GameSystem.GameHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class CameraSettingWindow : Form
    {
        public CameraSettingWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        public FilterInfoCollection filterInfoCollection;

        public string cameraName = string.Empty;

        public int maxFps = 0;

        public int Fps = 0;

        public int _width = 1280;
        public int _height = 720;
        public int cameraIndex = 0;

        public List<string> ExistMonikerStrings = new List<string>();
        private void CameraSettingWindow_Load(object sender, EventArgs e)
        {
            this.Text = "摄像头参数设置";
            uiComboBox2.SelectedIndex = 0;
            LoadingLocalCameraData();
        }

        private void LoadingLocalCameraData()
        {
            
            uiComboBox1.Items.Clear();
            try
            {
                filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if(filterInfoCollection.Count==0)
                {
                    throw new Exception();
                }foreach(FilterInfo device in filterInfoCollection)
                {
                    uiComboBox1.Items.Add(device.Name);

                }
                if (uiComboBox1.Items.Count > 0)
                {
                   uiComboBox1.SelectedIndex = uiComboBox1.Items.Count - 1;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
            }catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }

        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cameraName = uiComboBox1.Text.Trim();
            if(!string.IsNullOrEmpty(cameraName) )
            {
                cameraIndex = uiComboBox1.SelectedIndex;
                ChooseCamera(cameraName);
            }
        }
        public List<string> FpsList = new List<string>();
        bool _isCameraUse = false;
        private void ChooseCamera(string name)
        {
            FpsList.Clear();
            _isCameraUse = false;
            foreach (FilterInfo device in filterInfoCollection)
            {
                if (device.Name == name)
                {
                    if (ExistMonikerStrings.Contains(device.MonikerString)) _isCameraUse = true;
                    VideoCaptureDevice rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                    for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                    {
                        if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == _width
                            && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == _height)
                        {
                            //rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                            string fps = rgbDeviceVideo.VideoCapabilities[i].AverageFrameRate + "";
                            if (!FpsList.Contains(fps))
                                FpsList.Add(fps);
                            break;
                        }
                    }
                    break;
                }
            }
            if (FpsList.Count == 0)
            {
                foreach (FilterInfo device in filterInfoCollection)
                {
                    if (device.Name == name)
                    {
                        VideoCaptureDevice rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                        for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                        {
                            if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == 1920
                                && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == 1080)
                            {
                                //rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                                string fps = rgbDeviceVideo.VideoCapabilities[i].AverageFrameRate + "";
                                if (!FpsList.Contains(fps))
                                    FpsList.Add(fps);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            uiComboBox3.Items.Clear();
            foreach (var item in FpsList)
            {
                int.TryParse(item, out int fps);
                maxFps = fps;
                fps /= 2;
                while (fps >= 30)
                {
                    if (fps >= 30)
                        uiComboBox3.Items.Add(fps + "fps");
                    fps /= 2;
                }
                uiComboBox3.Items.Add(maxFps + "fps");
                break;
            }
            if (uiComboBox3.Items.Count > 0)
            {
                uiComboBox3.SelectedIndex = 0;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(uiComboBox3.Text))
            {
                string cb1 = uiComboBox3.Text;
                string cb2 = cb1.Substring(0, cb1.IndexOf("fps"));
                int.TryParse(cb2, out Fps);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cameraName))
                DialogResult = DialogResult.OK;
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            
             DialogResult = DialogResult.Cancel;
            
        }
    }
}
