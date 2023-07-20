using AForge.Video;
using AForge.Video.DirectShow;
using HZH_Controls;
using HZH_Controls.Controls;
using HZH_Controls.Forms;
using OpenCvSharp;
using OpenCvSharp.Flann;
using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindowSys;
using RTMent.Core.GameSystem.MyControll;
using Serilog;
using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
 

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class RunningTestingWindow : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public string ProjectName;
        public string Type;
        public int RoundCount;
        public int BestScoreMode;
        public int TestMethod;
        public int FloatType;
        public string GroupName;
        public string ProjectID;
        public string FormTitle;
        /// <summary>
        /// 自动匹配
        /// </summary>
        private bool autoMatchFlag = false;
        private bool autoWriteScore = true;
        /// <summary>
        /// 
        /// </summary>
        private NFCHelper USBWatcher = new NFCHelper();
        private ScanerHook  hookListener = new ScanerHook();
        private  SerialReader sReader = null;
        private List<UserControl1> userControls = new List<UserControl1>();
        private List<TestingStudentData> studentDatas = new List<TestingStudentData>();
        private List<SelectStudentDataModel> studentDataModels = new List<SelectStudentDataModel>();
        //是否播放开始语音
        private bool voiceFlag = true;
        //开始计时后恢复
        private bool titleFlag = false;
        //是否收到最终成绩
        private bool getFinalFlag = false;
        private  string connectPortName = string.Empty;
        private int currentRound = 1;
        private Thread hookThread = null;
        private int equipMentNum = 0;
        private  bool IsTesting= false;
        private  string imgaeDir = Application.StartupPath + "\\Image\\";
        private string CurrentDir = "";
        bool isWriteScoreSucess = false;


        //private Thread WriteScoreThread;

        public RunningTestingWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunningTestingWindow_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            string code = "程序集版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string code1 = "文件版本：" + Application.ProductVersion.ToString();
            toolStripStatusLabel1.Text = code1;
            USBWatcher.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 1));
            LoadingInitData();
            ThreadStart threadStart = new ThreadStart(() =>
            {
                CreateHookListener();
            });
            hookThread = new Thread(threadStart);
            hookThread.IsBackground = true;
            hookThread.Start();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunningTestingWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseCurrentCamera();
            if (sReader != null && sReader.IsComOpen())
            {
                sReader.CloseCom();
            }
        }
        private void uiTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiTabControl1.SelectedIndex == 1)
            {
                  LoadingLineCamera();
            }
        }

        

        /// <summary>
        /// 创建钩子监听
        /// </summary>
        private void CreateHookListener()
        {
             hookListener.Start();
             uiTextBox1.Text = string.Empty;
            hookListener.ScanerEvent += RecieveDataFromHook;
        }
        private void RecieveDataFromHook(ScanerCodes code)
        {
            string codes = code.Result;
            if (!string.IsNullOrEmpty(codes))
            {
                uiTextBox1.Text=codes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void USBEventHandler(object sender, EventArrivedEventArgs e)
        {
            var watcher = sender as ManagementEventWatcher;
            watcher.Stop();
            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {
                Console.WriteLine("设备连接");
            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
            {
                if (sReader == null || !sReader.IsComOpen())
                {
                    Reconnect();
                }
            }

            watcher.Start();
        }
        private Thread reconnectThread = null;
        private bool isReconnect = false;
        private bool reconnecting = false;
        private bool isSendMachineInfo = false;  
        #region 串口

        /// <summary>
        /// 
        /// </summary>
        private void Reconnect()
        {
            if (isReconnect) return;
            isReconnect = true;
            reconnecting = true;
            //检测断开,断开提示
            MessageBox.Show("设备断开请检查");
            reconnectThread = new Thread(new ThreadStart(TryReconnect));
            reconnectThread.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        private void TryReconnect()
        {
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                timer1.Stop();
                serialConnectStripStatusLabel1.Text = "重连中";
                serialConnectStripStatusLabel1.ForeColor = Color.Red;
            });
            try
            {
                while (isReconnect)
                {
                    ///重连
                    ReadInit();
                    if (sReader != null && sReader.IsComOpen())
                    {
                        isReconnect = false;
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                isReconnect = false;
                reconnecting = false;
                ControlHelper.ThreadInvokerControl(this, () =>
                {
                    timer1.Start();
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ReadInit()
        {
            if (sReader != null) sReader.CloseCom();
            //初始化访问读写器实例
            sReader = new SerialReader();
            //回调函数
            sReader.AnalyCallback = AnalyData;
            sReader.ReceiveCallback = ReceiveData;
            sReader.SendCallback = SendData;
            ComPortsInit();
        }
        /// <summary>
        /// 
        /// </summary>
        private void ComPortsInit()
        {
            RefreshComPorts();
            OpenConnectionPort();
            uiComboBox1.SelectedIndex = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        private void GetMachineNums()
        {
            try
            {
                if(sReader != null)
                {
                    if (!sReader.IsComOpen())
                    {
                       UIMessageBox.ShowWarning( "未打开串口");
                        return;
                    }
                    if (sReader.isExaming())
                    {
                        UIMessageBox.ShowWarning("考试中请勿操作");
                        return;
                    }
                    string code = "paircount";
                    //发送获取设备数量数据
                    sReader.qType = code;
                    //code = code.ToCharArray().Aggregate("", (result, c) => result += ((!string.IsNullOrEmpty(result) && (result.Length + 1) % 2 == 0) ? " " : "") + c.ToString());
                    byte[] paircount_b = Encoding.UTF8.GetBytes(code);
                    sReader.SendMessage(paircount_b);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OpenConnectionPort()
        {
            try
            {
                string ports = SerialPortCbx.Text.Trim();
                 
                if (sReader!=null&&sReader.IsComOpen())
                {
                    sReader.CloseCom();
                }
                else
                {
                    if (string.IsNullOrEmpty(ports))
                    {
                        serialConnectStripStatusLabel1.Text = $"串口连接失败";
                        serialConnectStripStatusLabel1.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        OpenComPort(ports);
                    }
                }
                if (sReader.IsComOpen())
                {
                    connectPortName = ports;
                    serialConnectStripStatusLabel1.Text = $"串口:{ports}已连接";
                    serialConnectStripStatusLabel1.ForeColor = Color.Green;
                    ControlHelper.ThreadInvokerControl(this, () =>
                    {
                       // groupBox3.Enabled = true;
                        uiButton12.Text = "关闭端口";
                        timer1.Start();
                    });
                    GetMachineNums();
                }
                else
                {
                    connectPortName = string.Empty;
                    serialConnectStripStatusLabel1.Text = $"串口连接失败";
                    serialConnectStripStatusLabel1.ForeColor = Color.Red;
                    uiButton12.Text = "打开端口";
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        private bool OpenComPort(string ports)
        {
            try
            {
                if (sReader.IsComOpen())
                {
                    sReader.CloseCom();
                }
                string strException = string.Empty;
                int nRet = sReader.OpenCom(ports, 115200, out strException);
                if (nRet == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsSerialOpen()
        {
            if(sReader.IsComOpen())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 刷新串口
        /// </summary>
        /// <param name="portn"></param>
        public void RefreshComPorts(string portn = "USB Serial Port")
        {
            try
            {
                SerialPortCbx.Items.Clear();
                SerialPortCbx.Text = "";
                string[] portNames = RunningTestingWindowSys.Instance.GetPortDeviceName(portn);
                if (portNames.Length == 0)
                {
                    portn = "USB-SERIAL";
                    portNames = RunningTestingWindowSys.Instance.GetPortDeviceName(portn);
                }
                if (portNames.Length == 0)
                {
                    portn = "USB-to-Serial";
                    portNames = RunningTestingWindowSys.Instance.GetPortDeviceName(portn);
                }

                if (portNames != null && portNames.Length > 0)
                {
                    foreach (string portName in portNames)
                    {
                        SerialPortCbx.Items.Add(RunningTestingWindowSys.Instance.PortName2Port(portName));
                    }
                }

                if (SerialPortCbx.Items.Count > 0)
                {
                    SerialPortCbx.SelectedIndex = SerialPortCbx.Items.Count - 1;
                }
                else
                {
                    SerialPortCbx.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>        
        private void SendData(byte[] data)
        {
            string code = CCommondMethod.ByteArrayToString(data, 0, data.Length);
            Console.WriteLine($"sendCount:{data.Length}   send:{code}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveData(byte[] data)
        {
            string code = CCommondMethod.ByteArrayToString(data, 0, data.Length);
            Console.WriteLine($"------receiveCount:{data.Length}   recv:{code}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgTran"></param>
        private void AnalyData(SerialMessageTran data)
        {
            if(sReader.qType == "paircount")
            {
                try
                {
                    byte[] byteData = data.btAryTranData;
                    string sl=Encoding.UTF8.GetString(byteData, 0, byteData.Length);
                    int num = 0;
                    if (sl.Contains("pairplaycount:") && byteData.Length == 17)
                    {
                         num = byteData[14];
                    }

                    //int.TryParse(v, out int vNum);
                    if (num > 0 && num < 11)
                    {
                        sReader.machineNum = num;
                        num--;
                       SerialPortCbx.SelectedIndex =num;
                    }
                    Console.WriteLine();
                
                
                }
                catch(Exception ex)
                {
                    LoggerHelper.Debug(ex);
                    return;
                }

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                byte[] bs = data.btAryTranData;
                string code = CCommondMethod.ByteArrayToString(bs, 0, bs.Length);
                sb.AppendLine("recv:" + code);
                LoggerHelper.Info(sb.ToString());
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
            switch(data.strCmd)
            {
                case 0xFE:
                    SetAnalyData0xFE(data);
                    break;
                case 0xFF:
                    SetAnalyData0xFF(data);
                    break;            
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SetAnalyData0xFF(SerialMessageTran data)
        {
            try
            {
                autoMatchFlag = false;
                StringBuilder sb = new StringBuilder();
                int k = 0;
                var list = data.ints1;int len = list.Count;
                if(len == 0)
                {
                    if (sReader != null && sReader.IsComOpen())
                    {
                        String code = "Post All Score";
                        byte[] bs= System.Text.Encoding.Default.GetBytes(code);
                        sReader.qType = "qStart";
                        sReader.SendMessage(bs);
                    }
                    return;
                }
                else
                {
                    string nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    for (int i = 0; i < len; i++)
                    {
                        int score = list[i];
                        string str = "";
                        string idNumber = userControls[i].panel_idNumber;
                        string name = userControls[i].panel_name;
                        int state = userControls[i].panel_status;
                        string stuState = ResultStateHelper.Instance.ResultState2Str(state);
                        if (score < 0)
                        {
                            str = "00.00";
                        }
                        else
                        {
                            TimeSpan span = new TimeSpan(0, 0, 0, score, 0);
                            str = $"{span.Minutes.ToString("00")}.{span.Seconds.ToString("00")}";
                        }
                        string states=ResultStateHelper.Instance.ResultState2Str(state);
                        if (state > 1)
                        {
                            userControls[k].panel_Score = states;
                        }
                        else
                        {
                            if(state==0)
                            {
                                userControls[k].panel_status = 1;
                            }
                            userControls[k].panel_Score = str;
                            sb.AppendLine($"考号:{idNumber},姓名:{name},成绩:{str},状态:{stuState},测试时间:{nowStr}");
                            k++;
                          // Console.WriteLine()  
                        }
                    }
                    System.IO.File.AppendAllText("成绩日志.txt", sb.ToString());
                    ControlHelper.ThreadInvokerControl(this, () =>
                    {
                        ucledNums1.Value = "01:00";
                        uiButton1.Enabled = true;
                        uiButton2.Enabled = true;
                        uiButton4.Enabled = true;
                        uiButton3.Enabled=true;
                        uiButton5.Enabled=true;

                    });
                    getFinalFlag = true;
                    Thread thread = new Thread(new ThreadStart(() => { WriteCurrentStudentScore(); }));
                    thread.IsBackground = true;
                    thread.Start();
                    IsTesting = false;
                    
                    SaveImageDataToLocalFile();
                    uiLabel6.Text = "未开始";
                    uiLabel6.ForeColor = Color.Red;
                    
                }

            }
            catch(Exception ex )
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SaveImageDataToLocalFile()
        {
            
            if (userControls.Count > 0)
            {
                string path = Path.Combine(CurrentDir, string.Format("组{0}的{1}第{2}次分配成绩", GroupName, ProjectName,autoCount) + ".mp4");
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                OpenVideoOutPut(path);
                Thread thread = new Thread(() =>
                {
                    for (int i = 0; i < imgMsses.Count; i++)
                    {
                        Bitmap bmp = ImageHelper.Instance.BitmapDeepCopy((Bitmap)ImageHelper.Instance.MemoryStream2Bitmap(imgMsses[i].img));
                        VideoWriteFrame(bmp);
                    }
                    CloseCurrentCamera();
                    isWriteScoreSucess = false;
                });
                thread.IsBackground = true;
                thread.Start();
                //ReleaseVideoOutPut();
                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        private void VideoWriteFrame(Bitmap bitmap)
        {
            OpenCvSharp.Mat mat = ImageHelper.Instance.Bitmap2Mat(bitmap);
            videoWriter.Write(mat);
        }

        OpenCvSharp.VideoWriter videoWriter= null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private  void  OpenVideoOutPut(string path)
        {
            try
            {
                ReleaseVideoOutPut();
                if (imgMsses.Count == 0)
                {
                    return;
                }
                else
                {
                    videoWriter = new VideoWriter(path, OpenCvSharp.FourCC.MP42, 60, new OpenCvSharp.Size(width, height));    
                }
            }
            catch(Exception ex )
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ReleaseVideoOutPut()
        {
            if(videoWriter != null  &&videoWriter.IsOpened())
            {
                videoWriter.Release();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private List<bool> showTitleBools = new List<bool>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SetAnalyData0xFE(SerialMessageTran data)
        {
            try
            {
                int k = 0;
                int len = data.ints0.Count;
                len=len>userControls.Count ? userControls.Count : len;
                foreach(var str in data.ints0)
                {
                    int state = userControls[k].panel_status;
                    string state1 = ResultStateHelper.Instance.ResultState2Str(state);
                    if (state > 1)
                        userControls[k].panel_Score = state1;
                    else
                        userControls[k].panel_Score = str;
                    if (showTitleBools.Count > k && showTitleBools[k])
                    {
                        if (data.ints[k]==1)
                        {
                            userControls[k].Panel_Ready = false;
                            showTitleBools[k] = false;
                        }
                    }
                    k++;
                    if (k >= len) break;
                }
                TimeSpan ts = data.timeSpan;
                if(ts.TotalMilliseconds > 0)
                {
                    ControlHelper.ThreadInvokerControl(this, () =>
                    {
                        if(voiceFlag)
                        {
                            SpeekHelper.Instance.AddDataToQueue("开始考试");
                            showTitleBools.Clear();
                            for(int i = 0;i<userControls.Count;i++)
                            {
                                userControls[i].Panel_Ready = true;
                                showTitleBools.Add(true);
                            }
                            voiceFlag = false;
                            titleFlag = true;
                        }
                        ucledNums1.Value= $"{String.Format("{0:00}", ts.Minutes)}:{String.Format("{0:00}", ts.Seconds)}";
                    });
                }
            }
            catch(Exception ex )
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        private void LoadingInitData()
        {
            RunningTestingWindowSys.Instance.UpDateGroupCbx(GroupCbx,ProjectID, ref GroupName);
            UpdateRoundCountCbx();
            ReadInit();
            LoadingUserControll();
            RunningTestingWindowSys.Instance. UpDateStudentListView(ProjectID,GroupName,1,uiDataGridView1,uiLabel1);       
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadingUserControll()
        {
            string cous = uiComboBox1.Text.Trim();
            if (!String.IsNullOrEmpty(cous))
            {
                flowLayoutPanel1.Controls.Clear();
                if (userControls.Count != 0)
                {
                    userControls.Clear();
                }
                int count = int.Parse(cous);
                for (int i = 0; i < count; i++)
                {
                    UserControl1 userControl = new UserControl1();
                    userControl.panel_name = "未分配";
                    userControl.panel_idNumber = "未分配";
                    userControl.panel_title = $"{i+1}号设备";
                    userControl.panel_Score = "未分配";
                    userControl.panel_status = 0;
                    userControl.Panel_title_Color = Color.Blue;
                    userControl.Panel_Ready= true;
                    userControl.StateSwitchCallback = StateSwitchCallBack;
                    userControls.Add(userControl);
                }
                if(userControls.Count != 0)
                {
                    equipMentNum = userControls.Count;
                    foreach (UserControl1 userControl in userControls)
                    {
                        flowLayoutPanel1.Controls.Add(userControl);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void StateSwitchCallBack()
        {
            try
            {
                if (studentDatas.Count == 0) return;
                if(!autoMatchFlag) { return; }
                MatchDataSendInfoToSerial(studentDatas);   
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 发送名单
        /// </summary>
        private void MatchDataSendInfoToSerial(List<TestingStudentData> studentDatas)
        {
            try
            {
                if (sReader == null || !sReader.IsComOpen())
                {
                    UIMessageBox.ShowError("设备断开！！");
                    return;
                }
                if(studentDatas.Count == 0) return;
                byte[] ff= new byte[] { 0xff };
                byte[] headCode = System.Text.Encoding.Default.GetBytes("name:");
                byte[] endByte = System.Text.Encoding.Default.GetBytes("end");
                Byte[] endCode = new Byte[endByte.Length + ff.Length];
                endByte.CopyTo(endCode, 0);
                ff.CopyTo(endCode, endByte.Length);
                Byte[] WriteBufferALL = Enumerable.Repeat((Byte)0x00, 404).ToArray(); ;
                int li = 0;
                Byte[] AbsenceList = System.Text.Encoding.Default.GetBytes("lack:");
                Byte[] WriteAbsenceBufferALL = new byte[AbsenceList.Length + 20];
                int abStep = AbsenceList.Length;
                Array.Copy(AbsenceList, 0, WriteAbsenceBufferALL, 0, abStep);

                //添加头码
                for (int l = li; l < headCode.Length; l++)
                {
                    WriteBufferALL[l] = headCode[l];
                    li++;
                }
                Byte[] contentByte = new byte[20];
                int contentByteStep = 0;
                for (int i = 0; i <studentDatas.Count; i++)
                {
                    string GName = studentDatas[i].name;
                    int state0 = userControls[i].panel_status;
                    if (state0 == 3)
                    {
                        contentByte[contentByteStep] = 0xee;
                        contentByteStep++;
                    }
                    else
                    {
                        contentByte[contentByteStep] = 0x00;
                        contentByteStep++;
                    }
                    if (GName.Trim().Length > 4)
                    {
                        GName = GName.Trim().Substring(0, 4);
                    }
                    Byte[] name1hex = System.Text.Encoding.Default.GetBytes(GName);
                    Byte[] name2hex = Enumerable.Repeat((Byte)0x00, 8).ToArray();
                    name1hex.CopyTo(name2hex, 0);
                    for (int ii = 0; ii < name2hex.Length; ii++)
                    {
                        WriteBufferALL[li] = name2hex[ii];
                        li++;
                    }
                }
                //添加结束码
                for (int l = 0; l < endCode.Length; l++)
                {
                    WriteBufferALL[li] = endCode[l];
                    li++;
                }
                Array.Copy(contentByte, 0, WriteAbsenceBufferALL, abStep, contentByte.Length);
                Task.Run(() =>
                {
                    //发送正常名单
                    sReader.SendMessage(WriteBufferALL, li);
                    Thread.Sleep(1000);
                    //发送缺考
                    sReader.SendMessage(WriteAbsenceBufferALL, 0);
                });
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateRoundCountCbx()
        {
            try
            {
                RoundCbx.Items.Clear();
                for(int i = 0; i < RoundCount; i++)
                {
                    RoundCbx.Items.Add((i+1).ToString());
                }
                RoundCbx.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        #region 页面事件
        /// <summary>
        ///自动匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (!IsTesting)
            {
                ClearCurrentMatchUserControll();
                List<int> choose = new List<int>();
                RunningTestingWindowSys.Instance.LoadingTestingStudentData(ProjectID, GroupName, currentRound, equipMentNum, uiDataGridView1, ref studentDatas, ref choose);
                int len = uiDataGridView1.Rows.Count;
                for (int i = 0; i < len; i++)
                {
                    if (choose.Contains(i))
                        uiDataGridView1.Rows[i].Selected = true;
                    else
                        uiDataGridView1.Rows[i].Selected = false;
                }
                AutoMatchUserControll();
            }
            else
            {
                UIMessageBox.ShowWarning("考试中，请勿操作！！");
                return;
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            if (!IsTesting) 
            {
                ClearCurrentMatchUserControll();
                int index = 0;
                if (uiDataGridView1.SelectedRows == null)
                {
                    UIMessageBox.ShowWarning("请选择学生数据！！");
                    return;
                }
                else
                {
                    for (int i = 0; i < uiDataGridView1.SelectedRows.Count; i++)
                    {
                        index = uiDataGridView1.SelectedRows.Count - 1 - i;
                        string Name = uiDataGridView1.SelectedRows[index].Cells[1].Value.ToString();
                        string idNum = uiDataGridView1.SelectedRows[index].Cells[2].Value.ToString();
                        string score = uiDataGridView1.SelectedRows[index].Cells[4].Value.ToString();
                        string stuID = uiDataGridView1.SelectedRows[index].Cells[6].Value.ToString();
                        if (score != "无成绩")
                        {
                            UIMessageBox.ShowWarning("选择的考生中，包含已经测试的学生,请重新选择！！");
                            studentDatas.Clear();
                            ClearCurrentMatchUserControll();
                            return;
                        }
                        else
                        {
                            studentDatas.Add(new TestingStudentData()
                            {
                                RaceStudentDataId = i + 1,
                                id = stuID,
                                name = Name,
                                idNumber = idNum,
                                score = 0,
                                state = 0,
                                RoundId = currentRound,
                            });
                            AutoMatchUserControll();

                        }
                    }
                }
            }
            else
            {
                UIMessageBox.ShowWarning("考试中请勿操作！！");
                return;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton3_Click(object sender, EventArgs e)
        {
            if (!IsTesting)
            {
                ClearCurrentMatchUserControll();
            }
            else
            {
                UIMessageBox.ShowWarning("考试中请勿操作！！");
                return;
            }
        }
        /// <summary>
        /// 预备按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton4_Click(object sender, EventArgs e)
        {
            if (!IsTesting)
            {
                if (studentDatas.Count == 0)
                {
                    UIMessageBox.ShowWarning("请先选择考生数据,匹配考生！！");
                    return;
                }
                else
                {
                    SpeekHelper.Instance.AddDataToQueue("请各位考生做好准备");
                    UIMessageBox.ShowWarning("请按下开始按钮！！");
                    uiButton5.BackColor = Color.Red;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("考试中请勿操作！！");
                return;
            }
        }
        /// <summary>
        /// 发令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton5_Click(object sender, EventArgs e)
        {
            if (!IsTesting)
            {
                if (rgbVideoSource.IsRunning)
               {
                    try
                    {
                        isSendMachineInfo = false;
                        getFinalFlag = false;
                        IsTesting = true;
                        if (IsSerialOpen())
                        {
                            if (sReader != null)
                                sReader.machineNum = uiComboBox1.SelectedIndex + 1;
                            if (studentDatas.Count == 0)
                            {
                                UIMessageBox.ShowWarning("没有分配考生！！");
                                IsTesting = false;
                                return;
                            }
                            string speak = "各就各位,预备！！";
                            SpeekHelper.Instance.AddDataToQueue(speak);
                            voiceFlag = true;
                            sReader.qType = "qStart";
                            string startCommand = "start";
                            byte[] WriteBufferALL = System.Text.Encoding.Default.GetBytes(startCommand);
                            sReader.SendMessage(WriteBufferALL);
                            uiLabel6.Text = "测试中";
                            // timer2.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                        return;
                    }
               }
                else
                {
                    UIMessageBox.ShowWarning("请打开相机");
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("考试中请勿操作！！");
                return;
            }
        }
       
        /// <summary>
        /// 写入当前成绩
        /// </summary>
        private void WriteCurrentStudentScore()
        {
            if (autoWriteScore)
            {
                if (!getFinalFlag)
                {
                    if (sReader != null && sReader.IsComOpen())
                    {
                        string code = "Post All Score";
                        byte[] data = Encoding.Default.GetBytes(code);
                        sReader.qType = "qStart";
                        sReader.SendMessage(data);
                    }
                    else
                        MessageBox.Show("设备断开！！");
                    return;
                }
                else
                {
                    UpDataCurrentStudentDataListScore();
                    if (studentDatas.Count == 0)
                    {
                        UIMessageBox.ShowWarning("数据错误！！");
                        return;
                    }
                    RunningTestingWindowSys.Instance.UpDataCurrentStudentDataScore(studentDatas, currentRound);
                    isWriteScoreSucess = true;
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                    //AutoMatchUserControll();
                    isWriteScoreSucess = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton6_Click(object sender, EventArgs e)
        {
            if (!IsTesting)
            {
                uiButton6.Enabled = false;
                uiButton6.Text = "上传中";
                ThreadStart threadStart = new ThreadStart(() =>
                {
                    UpLoadingCurrentStudentGroupScore();
                });
                Thread thread = new Thread(threadStart);
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                UIMessageBox.ShowWarning("考试中请勿操作！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton7_Click(object sender, EventArgs e)
        {
            if (!IsTesting)
            {
                try
                {
                    uiButton7.Enabled = false;
                    DialogResult result = MessageBox.Show("是否重取成绩!!!", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        if (sReader != null && sReader.IsComOpen())
                        {
                            string code = "Post All Score";
                            byte[] da = Encoding.Default.GetBytes(code);
                            sReader.qType = "qStart";
                            sReader.SendMessage(da);
                        }
                        else
                        {
                            UIMessageBox.ShowWarning("设备断开！！");
                            return;
                        }
                    }
                    else
                        return;
                    
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                    return;
                }
                finally { uiButton7.Enabled = true; }
            }
            else
            {
                UIMessageBox.ShowWarning("考试中，请勿操作！！");
                return;
            }
        }
        /// <summary>
        /// 全部暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton8_Click(object sender, EventArgs e)
        {
            if (IsTesting)
            {
                try
                {
                    if (sReader == null || !sReader.IsComOpen())
                    {
                        UIMessageBox.ShowWarning("设备断开！！");
                        return;
                    }
                    if (IsSerialOpen())
                    {
                        sReader.qType = "stop";
                        string cmd = "stop";
                        byte[] data = Encoding.Default.GetBytes(cmd);
                        sReader.SendMessage(data);

                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先开始考试！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton9_Click(object sender, EventArgs e)
        {
            if (IsTesting)
            {
                if (sReader == null || !sReader.IsComOpen())
                {
                    MessageBox.Show("设备断开");
                    return;
                }

                if (IsSerialOpen())
                {
                    string EndgameCommand = "Endgame";
                    byte[] WriteBufferALL = System.Text.Encoding.Default.GetBytes(EndgameCommand);
                    sReader.SendMessage(WriteBufferALL);
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先开始考试！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton10_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(GroupName) && !string.IsNullOrEmpty(currentRound.ToString()))
            {
                RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
            }
        }
        /// <summary>
        /// 刷新端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton11_Click(object sender, EventArgs e)
        {
           RefreshComPorts();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton12_Click(object sender, EventArgs e)
        {
            OpenConnectionPort();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton13_Click(object sender, EventArgs e)
        {
            GetMachineNums();
        }
        
        /// <summary>
        /// 配置相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton14_Click(object sender, EventArgs e)
        {
            width = 1280;
            height = 720;
            OpenCameraSettinng();
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton15_Click(object sender, EventArgs e)
        {
            if (IsTesting)
            {
                UIMessageBox.ShowWarning("考试中，请勿操作！！");
                return;
            }
            else
            {
                if (uiButton15.Text == "关闭相机")
                {
                    CloseCurrentCamera();
                }
                else
                {
                    OpenCurrentChooseCamera(cameraIndex);
                     
                    // StarCameraRes();
                    //StarCameraRes();

                }
                 
            }
        }

        
        List<ImgMss> imgMsses = new List<ImgMss>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="image"></param>
        private void rgbVideoSource_NewFrame(object sender, ref Bitmap image)
        {
           
            if (image != null)
            {
                Bitmap bmp = ImageHelper.Instance.BitmapDeepCopy(image);
                if (bmp != null) 
                {

                    if (!isWriteScoreSucess)
                    {
                        ImgMss mss = new ImgMss()
                        {
                            DateTime = DateTime.Now,
                            Name = "img" + imgMsses.Count,
                        };
                         Bitmap bitmap = ImageHelper.Instance.BitmapDeepCopy(bmp);
                         mss.img = ImageHelper.Instance.Bitmap2MemoryStream(bitmap);
                        imgMsses.Add(mss);
                        pictureBox1.Image = bmp;
                    }
                                        
                }
            }
        }
        


        bool IsMakeGroupSucess=false;
        /// <summary>
        /// 查询按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton16_Click(object sender, EventArgs e)
        {
            SelectStudentData();
        }
        private void uiTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectStudentData();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        private void SelectStudentData()
        {
            string IDNum = uiTextBox1.Text.Trim();
            if (!string.IsNullOrEmpty(IDNum))
            {
                if (!IsMakeGroupSucess)
                {
                    SelectStudentDataModel studentDataModel = RunningTestingWindowSys.Instance.SelectStudentDataByIDNumber(ProjectID, IDNum);
                    if (studentDataModel != null)
                    {
                        if (!studentDataModels.Contains(studentDataModel))
                            studentDataModels.Add(studentDataModel);
                        RunningTestingWindowSys.Instance.SetStudentDataInDataView(studentDataModels, uiDataGridView2);
                        Type ty = studentDataModel.score.GetType();
                        Type ty2 = studentDataModel.score1.GetType();
                        if (ty.Name == "Double" && ty2.Name == "Double")
                        {
                            UIMessageBox.ShowWarning("当前考生已经全部测试完成！！");
                            return;
                        }
                        else
                        {
                            if (ty.Name == "String" && ty2.Name == "Double")
                            {
                                int state = ResultStateHelper.Instance.ResultState2Int(studentDataModel.score.ToString());
                                if (state == 0)
                                {
                                    string speeking = $"请{studentDataModel.Name}考生做好第一轮测试准备！！";
                                    SpeekHelper.Instance.AddDataToQueue(speeking);
                                }
                                else
                                {
                                    string speeking = $"当前考生{studentDataModel.Name}已全部测试完毕！！";
                                    SpeekHelper.Instance.AddDataToQueue(speeking);
                                }
                            }
                            else if (ty2.Name == "String" && ty.Name == "Double")
                            {
                                int state = ResultStateHelper.Instance.ResultState2Int(studentDataModel.score1.ToString());
                                if (state == 0)
                                {
                                    string speeking = $"请{studentDataModel.Name}考生做好第二轮测试准备！！";
                                    SpeekHelper.Instance.AddDataToQueue(speeking);
                                }
                                else
                                {
                                    string speeking = $"当前考生{studentDataModel.Name}已全部测试完毕！！";
                                    SpeekHelper.Instance.AddDataToQueue(speeking);
                                }
                            }
                            else if (ty.Name == "String" && ty2.Name == "String")
                            {
                                int state = ResultStateHelper.Instance.ResultState2Int(studentDataModel.score.ToString());
                                int states = ResultStateHelper.Instance.ResultState2Int(studentDataModel.score1.ToString());
                                if (state == 0)
                                {
                                    string speeking = $"请{studentDataModel.Name}考生做好第一轮测试准备！！";
                                    SpeekHelper.Instance.AddDataToQueue(speeking);
                                }
                                else
                                {

                                    if (states == 0)
                                    {
                                        string speeking = $"请{studentDataModel.Name}考生做好第二轮测试准备！！";
                                        SpeekHelper.Instance.AddDataToQueue(speeking);
                                    }
                                    else
                                    {
                                        string speeking = $"当前考生{studentDataModel.Name}已全部测试完毕！！";
                                        SpeekHelper.Instance.AddDataToQueue(speeking);
                                    }
                                }

                            }

                        }
                        uiTextBox1.Text = "";
                    }
                    else
                    {
                        UIMessageBox.ShowWarning("未查找到对应的学生数据！！");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先输入或者扫码输入学生考号！！");
                return;
            }
        }

        /// <summary>
        /// 刷新按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton17_Click(object sender, EventArgs e)
        {
            string groupName = "DYL__" + DateTime.Now.ToString("yyyy_MMdd");
            uiComboBox2.Items.Add(groupName);
            int index = 0;
            for(int i = 0; i < uiComboBox2.Items.Count; i++)
            {
                if (uiComboBox2.Items[i].ToString() == groupName)
                {
                    index = i;
                    break;
                }
            }
            uiComboBox2.SelectedIndex=index;
            uiButton17.Enabled = false;
        }
        /// <summary>
        /// 完成编组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton18_Click(object sender, EventArgs e)
        {
            if(studentDataModels.Count==0)
            {
                UIMessageBox.ShowWarning("当前没有考生，无法完成编组,请重试");
                return;
            }
            else
            {
                string name=uiComboBox2.Text.Trim();
                if (!string.IsNullOrEmpty(name))
                {           
                    IsMakeGroupSucess = true;
                    if (studentDatas.Count > 0)
                        studentDatas.Clear();
                    RunningTestingWindowSys.Instance.SaveDataToChipInfo(studentDataModels,name);
                    foreach (var item in studentDataModels)
                    {
                        TestingStudentData testing = new TestingStudentData()
                        {
                            name = item.Name,
                            id = item.Id,
                            idNumber = item.idNumber,
                            RaceStudentDataId = int.Parse(item.Id),

                        };
                        Type ty = item.score.GetType();
                        Type ty2 = item.score1.GetType();
                        if (ty.Name == "Double" && ty2.Name == "Double")
                        {
                            UIMessageBox.ShowWarning("当前考生已经全部测试完成！！");
                            return;
                        }
                        else
                        {
                            if (ty.Name == "String" && ty2.Name == "Double")
                            {
                                int state = ResultStateHelper.Instance.ResultState2Int(item.score.ToString());
                                if (state == 0)
                                {
                                    testing.state = 0;
                                    testing.score = 0;
                                    testing.RoundId = 1;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (ty2.Name == "String" && ty.Name == "Double")
                            {
                                int state = ResultStateHelper.Instance.ResultState2Int(item.score1.ToString());
                                if (state == 0)
                                {
                                    testing.state = 0;
                                    testing.score = 0;
                                    testing.RoundId = 2;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (ty.Name == "String" && ty2.Name == "String")
                            {
                                int state = ResultStateHelper.Instance.ResultState2Int(item.score.ToString());
                                int states = ResultStateHelper.Instance.ResultState2Int(item.score1.ToString());
                                if (state == 0)
                                {
                                    testing.state = 0;
                                    testing.score = 0;
                                    testing.RoundId = 1;
                                }
                                else
                                {

                                    if (states == 0)
                                    {
                                        testing.state = 0;
                                        testing.score = 0;
                                        testing.RoundId = 2;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                            }

                        }
                        studentDatas.Add(testing);

                    }
                }
                else
                {
                    UIMessageBox.ShowError("请输入编组号！");
                    return;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiDataGridView2_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                uiContextMenuStrip1.Show(e.Location);
            }
        }
        /// <summary>
        /// 考生上道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton19_Click(object sender, EventArgs e)
        {
            if(studentDataModels.Count==int.Parse(uiComboBox1.Text))
                 AutoMatchUserControll();
            else
            {
                if(MessageBox.Show("当前没有那么多设备，是否重新调整？？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK) 
                { 
                    LoadingUserControll();
                    AutoMatchUserControll();
                }
            }
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton20_Click(object sender, EventArgs e)
        {
            VLCControll vLCControll = new VLCControll();
            string ipaddress = "";
            string acc = "";
            string pass = "";
            if (saveCameraData.Count > 0)
            { 
                Random random = new Random();
                var index = random.Next(0, saveCameraData.Count);
                {
                    var ds= saveCameraData[index];
                    string[] pl = ds.Split(new char[] { ';' });
                    if (pl.Length > 0)
                    {
                        for (int i = 0; i < pl.Length; i++)
                        {
                            
                            string[] po = pl[i].Split(new char[] { ':' });
                            if (po.Length > 0)
                            {
                                if (i == 0)
                                    ipaddress = po[1];
                                if (i == 1)
                                    acc = po[1];
                                if (i == 2) pass = po[1];
                            }
                        }
                        if (!string.IsNullOrEmpty(ipaddress) && !string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(acc))
                        {

                            vLCControll.panel_IPAddress = ipaddress;
                            vLCControll.panel_User = acc;
                            vLCControll.panel_Password = pass;
                            vLCControll.panel_ToolStrip = "网络摄像头未连接";
                           
                            
                        };
                    }
                }
            }
            else
            {
                vLCControll.panel_IPAddress = ipaddress;
                vLCControll.panel_User = acc;
                vLCControll.panel_Password = pass;
                vLCControll.panel_ToolStrip = "网络摄像头未连接";
               // vLCControll.Panel_Picture = null;
               
            }    
            VLCControls.Add(vLCControll);
            flowLayoutPanel2.Controls.Add(vLCControll);
        }

      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton21_Click(object sender, EventArgs e)
        {
            if (VLCControls != null && VLCControls.Count > 0)
            {
                 VLCControls.RemoveAt(VLCControls.Count-1);
            }
            flowLayoutPanel2.Controls.Clear();
            if (VLCControls.Count > 0)
            {
                foreach(var item in VLCControls)
                {
                    flowLayoutPanel2.Controls.Add(item);
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton22_Click(object sender, EventArgs e)
        {
            if (VLCControls!=null&&VLCControls.Count > 0)
                 VLCControls.Clear();
            flowLayoutPanel2.Controls.Clear();
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPortCbx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupCbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            string groups = GroupCbx.Text.Trim();
            if (!string.IsNullOrEmpty(groups))
            {
                GroupName = groups;
            }
            CurrentDir = RunningTestingWindowSys.Instance.SetCurrentGrroupImageDir(ProjectName,groups, imgaeDir);
            RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundCbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            string round= RoundCbx.Text.Trim();
            if (!string.IsNullOrEmpty(round))
            {
                currentRound = int.Parse(round);
            }
            RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadingUserControll();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiDataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(e.Location);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiTabControl2.SelectedIndex == 1)
            {
                RunningTestingWindowSys.Instance.LoadingChipInfos(uiComboBox2);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sl=uiComboBox2.Text.Trim();
            if (!string.IsNullOrEmpty(sl))
            {
                studentDataModels = RunningTestingWindowSys.Instance.LoadingStudentChipData(sl);
                if (studentDataModels != null)
                {
                    RunningTestingWindowSys.Instance.SetStudentDataInDataView(studentDataModels, uiDataGridView2);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除分组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string groupNames = uiComboBox2.Text.Trim();
                if (!string.IsNullOrEmpty(groupNames))
                {
                    RunningTestingWindowSys.Instance.DeleteSelectChipsInfo(groupNames);
                    UIMessageBox.ShowSuccess("删除成功！！"); 
                    RunningTestingWindowSys.Instance.LoadingChipInfos(uiComboBox2);
                }
                else
                {
                    UIMessageBox.ShowError("删除失败！！");
                    return;
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView2.SelectedRows .Count== 0)
            {
                UIMessageBox.ShowWarning("请先选择学生信息！！");
                return;
            }
            else
            {
                for (int i = 0; i < uiDataGridView2.SelectedRows.Count; i++) {
                   // string name = uiDataGridView2.SelectedRows[i].Cells[1].Value.ToString();
                    string idnumber = uiDataGridView2.SelectedRows[i].Cells[2].Value.ToString();
                    string personid = uiDataGridView2.SelectedRows[i].Cells[8].Value.ToString();
                    RunningTestingWindowSys.Instance.DeleteSelectChipsInfoByChoose(idnumber, personid); 
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 缺考ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView1.SelectedRows.Count == 0)
            {
                UIMessageBox.ShowWarning("请先选择学生数据！！");
                return;
            }
            else
            {
                string idNumber = uiDataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                string name = uiDataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                string id = uiDataGridView1.SelectedRows[0].Cells[6].Value.ToString();
                int state = 3;
                if(RunningTestingWindowSys.Instance.SetErrorState(idNumber,name, id, state,currentRound))
                {
                    UIMessageBox.ShowSuccess("修改成功！！");
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                }
                else
                {
                    UIMessageBox.ShowError("操作失败！！");
                    return;
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 中退ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView1.SelectedRows.Count == 0)
            {
                UIMessageBox.ShowWarning("请先选择学生数据！！");
                return;
            }
            else
            {
                string idNumber = uiDataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                string name = uiDataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                string id = uiDataGridView1.SelectedRows[0].Cells[6].Value.ToString();
                int state = 2;
                if (RunningTestingWindowSys.Instance.SetErrorState(idNumber, name, id, state, currentRound))
                {
                    UIMessageBox.ShowSuccess("修改成功！！");
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                }
                else
                {
                    UIMessageBox.ShowError("操作失败！！");
                    return;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 犯规ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView1.SelectedRows.Count == 0)
            {
                UIMessageBox.ShowWarning("请先选择学生数据！！");
                return;
            }
            else
            {
                string idNumber = uiDataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                string name = uiDataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                string id = uiDataGridView1.SelectedRows[0].Cells[6].Value.ToString();
                int state = 4;
                if (RunningTestingWindowSys.Instance.SetErrorState(idNumber, name, id, state, currentRound))
                {
                    UIMessageBox.ShowSuccess("操作成功！！");
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                }
                else
                {
                    UIMessageBox.ShowError("操作失败！！");
                    return;
                }

            }
        }
        private bool FrmModifyScoreOneRoundShow = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 修正成绩ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView1.SelectedRows.Count == 0)
            {
                UIMessageBox.ShowWarning("请先选择考生！！");
                return;
            }
            string idNumber = uiDataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            string name = uiDataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            string id = uiDataGridView1.SelectedRows[0].Cells[6].Value.ToString();
            if (FrmModifyScoreOneRoundShow)
            {
               UIMessageBox.ShowWarning("未处理上一个数据");
                return;
            }
            FrmModifyScoreOneRoundShow = true;
            if (RunningTestingWindowSys.Instance.ShowModifyStudentDataWindow(ProjectName, ProjectID, GroupName, idNumber, name, currentRound))
            {
                string score = ModifyStudentDataWindowSys.scores;
                string personID = ModifyStudentDataWindowSys.personID;
                int state = ModifyStudentDataWindowSys.state;
                if(RunningTestingWindowSys.Instance.FixCurrentChooseStudentGrade(score, personID, state, currentRound, idNumber, name, id)) 
                    UIMessageBox.ShowSuccess("修改成绩成功！！");
                else
                {
                    UIMessageBox.ShowError("修改成绩失败！！");
                    return;
                }
                RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 弃权ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView1.SelectedRows.Count == 0)
            {
                UIMessageBox.ShowWarning("请先选择学生数据！！");
                return;
            }
            else
            {
                string idNumber = uiDataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                string name = uiDataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                string id = uiDataGridView1.SelectedRows[0].Cells[6].Value.ToString();
                int state = 5;
                if (RunningTestingWindowSys.Instance.SetErrorState(idNumber, name, id, state, currentRound))
                {
                    UIMessageBox.ShowSuccess("操作成功！！");
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                }
                else
                {
                    UIMessageBox.ShowError("操作失败！！");
                    return;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 本次重测ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiDataGridView1.SelectedRows.Count == 0)
            {
                UIMessageBox.ShowWarning("请先选择学生数据！！");
                return;
            }
            else
            {
                string idNumber = uiDataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                string name = uiDataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                string id = uiDataGridView1.SelectedRows[0].Cells[6].Value.ToString();
                int state = 6;
                if (RunningTestingWindowSys.Instance.SetErrorState(idNumber, name, id, state, currentRound))
                {
                    UIMessageBox.ShowSuccess("修改成功！！");
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                }
                else
                {
                    UIMessageBox.ShowError("操作失败！！");
                    return;
                }

            }
        }
        /// <summary>
        /// 自动写入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            autoWriteScore = uiCheckBox1.Checked==true? true:false;
        }
        /// <summary>
        /// 
        /// </summary>
        private int m_reader_connect_mode = 0;
        private int timerStep = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (timerStep++ > 10) GC.Collect();
                if (sReader != null && sReader.IsComOpen())
                {
                    if (m_reader_connect_mode != 1)
                    {
                        string port = connectPortName;
                        if (string.IsNullOrEmpty(port))
                        {
                            port = sReader.iSerialPort.PortName;
                        }

                        serialConnectStripStatusLabel1.Text = $"串口:{port}已连接";
                        serialConnectStripStatusLabel1.ForeColor = Color.Green;
                      //  groupBox3.Enabled = true;
                        m_reader_connect_mode = 1;
                    }
                }
                else
                {
                    connectPortName = string.Empty;
                    if (m_reader_connect_mode != 0)
                    {
                        serialConnectStripStatusLabel1.Text = $"串口未连接";
                        serialConnectStripStatusLabel1.ForeColor = Color.Red;
                        m_reader_connect_mode = 0;
                        //MessageBox.Show("串口断开");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
       
        #endregion
        /// <summary>
        /// 清空当前匹配
        /// </summary>
        ///// <exception cref="NotImplementedException"></exception>
        private void ClearCurrentMatchUserControll()
        {
            try
            {   
                if(studentDatas.Count>0   )
                {
                    studentDatas.Clear();
                    if(userControls.Count>0 )
                    {
                        for(int i = 0; i < userControls.Count; i++)
                        {
                            userControls[i].panel_name= "";
                            userControls[i].panel_idNumber = "";
                            userControls[i].panel_Score = "";
                            userControls[i].panel_status = 0;
                           // userControls[i].Panel_title_Color = Color.Green;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        int autoCount = 0;
        /// <summary>
        /// 自动分配
        /// </summary>   
        private void AutoMatchUserControll()
        {
            try
            {
                autoMatchFlag = true;
                isSendMachineInfo = true;
                int len = 10;
                if(len>studentDatas.Count)
                    len = studentDatas.Count;
                for(int i = 0; i<len;i++)
                {
                    var sl = studentDatas[i];
                    userControls[i].panel_name = sl.name;
                    userControls[i].panel_idNumber= sl.idNumber;
                    userControls[i].panel_status = sl.state;
                    userControls[i].panel_Score = "0.00";
                    userControls[i].Panel_Ready = true;
                    userControls[i].Panel_title_Color = Color.Blue;
                }
                StateSwitchCallBack();
                autoCount++;

            }
            catch (Exception ex) 
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 更新当前学生列表中的学生分数
        /// </summary>
        private void UpDataCurrentStudentDataListScore()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                for(int i = 0;i<studentDatas.Count;i++)
                {
                    string score = userControls[i].panel_Score;
                    double.TryParse(score, out double sc);
                    int state = userControls[i].panel_status;
                    studentDatas[i].state= state;
                    studentDatas[i].score = sc;

                }
            }
            catch(Exception ex) 
            { 
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 成绩上传
        /// </summary>
        private void UpLoadingCurrentStudentGroupScore()
        {
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                uiButton6.Enabled = false;
                uiButton6.Text = "上传中";
                uiButton6.BackColor = Color.Red;
            });
            string[] fsp = new string[2];
            fsp[0] = ProjectName;
            fsp[1] = GroupName;
            string message = RunningTestingWindowSys.Instance.UpLoadStudentDataScore(fsp,currentRound);
            if (string.IsNullOrEmpty(message))
            {
                UIMessageBox.ShowWarning("上传结束");
                return;
            }
            else
            {

                MessageBox.Show(message);
                ControlHelper.ThreadInvokerControl(this, () =>
                {
                    uiButton6.Enabled = true;
                    uiButton6.Text = "成绩上传";
                    RunningTestingWindowSys.Instance.UpDateStudentListView(ProjectID, GroupName, currentRound, uiDataGridView1, uiLabel1);
                });
                return;
            }

        }
        #region 相机相关
        int width = 0;
        int height = 0;
        private string cameraName = String.Empty;
        public int cameraIndex = -1;  
        private int maxFps = 0; 
        private int Fps = 0;
        private int cameraSkip = 0;
       // AForge.Video.FFMPEG.VideoFileWriter videoFileWriter;
        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        private FilterInfoCollection filterInfoCollection;
        /// <summary>
        /// RGB摄像头设备
        /// </summary>
        private VideoCaptureDevice rgbDeviceVideo;
        public List<string> ExistMonikerStrings = new List<string>();

        private System.Timers.Timer timer_count;
        /// <summary>
        /// 是否开始录像
        /// </summary>
        private bool IsStartVideo = false;
        /// <summary>
        /// 写入次数
        /// </summary>
        private int tick_num = 0;
        /// <summary>
        /// 录制多少小时,只是为了定时间计算录制时间使用
        /// </summary>
        private int Hour = 0;
        /// <summary>
        /// 
        /// </summary>
        public void OpenCameraSettinng()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            CameraSettingWindow cameraWindow = new CameraSettingWindow();
            cameraWindow.filterInfoCollection = filterInfoCollection;
            cameraWindow.ExistMonikerStrings = ExistMonikerStrings;
            if(cameraWindow.ShowDialog() == DialogResult.OK)
            {
                cameraName=cameraWindow.cameraName;
                cameraIndex=cameraWindow.cameraIndex;
                maxFps = cameraWindow.maxFps;
                Fps= cameraWindow.Fps;
                if (Fps == 0)
                    cameraSkip = maxFps;
                else
                    cameraSkip = maxFps / Fps;
                if (!string.IsNullOrEmpty(cameraName))
                {
                    OpenCurrentChooseCamera(cameraIndex);
                }
            }
            else
            {
                UIMessageBox.ShowWarning("相机打开失败 ！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraIndex"></param>
        private void OpenCurrentChooseCamera(int cameraIndex)
        {
            LoadingCamera(cameraIndex);
        }
        /// <summary>
        /// 加载相机
        /// </summary>
        /// <param name="index"></param>
        private void LoadingCamera(int index)
        {
            bool flag=false;
            try
            {
                if (rgbVideoSource.IsRunning)
                {
                    rgbVideoSource.SignalToStop();
                    //rgbVideoSource.Hide();
                }
                Boolean findIt = false;
                string MonikerString = string.Empty;
                if (filterInfoCollection != null)
                {
                    if (index < filterInfoCollection.Count)
                    {
                        FilterInfo device = filterInfoCollection[index];
                        rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);

                        for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                        {
                            if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == width
                                && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == height)
                            {
                                rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                                findIt = true;
                                break;
                            }
                        }
                        if (!findIt)
                        {
                            rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                        }
                    }
                    if (findIt)
                    {
                        rgbVideoSource.VideoSource = rgbDeviceVideo;
                        rgbVideoSource.Start();
                        //rgbVideoSource.Show();
                        rgbVideoSource.SendToBack();
                        flag = true;
                        ExistMonikerStrings.Add(rgbVideoSource.VideoSource.Source);
                    }
                    if (rgbVideoSource.IsRunning)
                    { flag = true; }
                    else
                    { flag = false; }
                    if (!flag)
                    {
                        uiButton15.Text = "打开相机";
                        uiButton15.BackColor = Color.White;
                        toolStripStatusLabel2.Text = "摄像头未开启";
                        toolStripStatusLabel1.ForeColor = Color.Red;
                        UIMessageBox.ShowWarning("打开摄像头失败!");
                    }
                    else
                    {
                        uiButton15.Text = "摄像头开启中";
                        toolStripStatusLabel1.ForeColor = Color.Green;
                        uiButton15.Text = "关闭相机";
                        uiButton15.BackColor = Color.Red;
                    }
                }

            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return;
            }
        }
        /// <summary>
        ///关闭相机
        /// </summary>
        private void CloseCurrentCamera()
        {
            try
            {
                if (!rgbVideoSource.IsRunning) return;
                if (rgbVideoSource != null && rgbVideoSource.IsRunning)
                {
                    rgbVideoSource.SignalToStop();
                    //rgbVideoSource.Hide();
                }
                ExistMonikerStrings.RemoveAll(a => a == rgbVideoSource.VideoSource.Source);
                uiButton15.Text = "打开相机";
                toolStripStatusLabel2.Text = "摄像头未开启";
                toolStripStatusLabel2.ForeColor = Color.Red;
                uiButton15.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }

        #endregion
        #region 网络相机相关
        List<string> saveCameraData= new List<string>();
        List<VLCControll> VLCControls =new List<VLCControll>();
        string CameraData = @"data/CameraData.txt";
        /// <summary>
        /// 加载网路摄像机
        /// </summary>
        private void LoadingLineCamera()
        {
            InitVLCControll();
            
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitVLCControll()
        {
            flowLayoutPanel2.Controls.Clear();
            if(VLCControls!=null&&VLCControls.Count > 0 ) 
                 VLCControls.Clear(); 
            if(!File.Exists(CameraData))
            {
                File.Create(CameraData);
            }
            string[] strArray = File.ReadAllLines(CameraData);
            foreach (string str in strArray)
            {
                saveCameraData.Add(str);
            }

        }



        #endregion

         
    }
}
