 
using RTMent.Core.GameSystem.GameHelper;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper
{
    public delegate void ReciveDataCallback(byte[] btAryReceiveData);

    public delegate void SendDataCallback(byte[] btArySendData);

    public delegate void AnalyDataCallback(SerialMessageTran msgTran);

    public class SerialReader
    {
        public SerialPort iSerialPort;
        private int m_nType = -1;
        public ReciveDataCallback ReceiveCallback;
        public SendDataCallback SendCallback;
        public AnalyDataCallback AnalyCallback;

        /// <summary>
        /// FE...FF (1+2+5*machineNum+1)
        ///
        //主机发送成绩：FF  00 07 94 05 33   00 10     01         00 02      00 01   00 07 93 64 44    00 11      01         00 02      00 02   FF
        //          头码     ID卡号5位    分数两位  01固定模式  时间两位   手柄号       ID卡号5位     分数两位   01固定模式    时间两位    手柄号  结尾码
        /// FF...FF (1+machineNum*12+1)
        /// </summary>
        ///匹配设备数量
        public int machineNum
        {
            get { return mn; }
            set
            {
                mn = value;
                if (mn <= 0) mn = 1;
                FEmn = 4 + 4 * mn;
                FFmn = 2 + 12 * mn;
            }
        }

        private int FEmn = 0;
        private int FFmn = 0;
        private int mn = 0;
        private System.Timers.Timer waitTimer;
        private System.Timers.Timer HandleBytesTimer;
        public string qType = "";

        //结束获取成绩
        //接收握手请求
        public byte[] qRec = new byte[] { 0x02, 0x31, 0x30, 0x02, 0x00, 0x00, 0x03, 0x02 };

        //回复握手请求
        public byte[] qSend = new byte[] { 0x02, 0x31, 0x30, 0x02, 0x30, 0x30, 0x03, 0x02 };

        //主机接收校验回复结束码
        public byte[] endCodeBuffer = new byte[] { 0x02, 0x31, 0x31, 0x01, 0x00, 0x03, 0x00 };

        //二次缓存数据
        public List<byte> _buffer = new List<byte>();

        //70 61 69 72 70 6C 61 79 63 6F 75 6E 74 3A 30
        //pairplaycount:0

        private SerialMessageTran FFmsgTran = null;

        public SerialReader()
        {
            iSerialPort = new SerialPort();
            iSerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceivedComData);
        }

        public int OpenCom(string strPort, int nBaudrate, out string strException)
        {
            strException = string.Empty;
            if (iSerialPort == null)
            {
                iSerialPort = new SerialPort();
                iSerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceivedComData);
            }
            if (iSerialPort.IsOpen)
            {
                iSerialPort.Close();
            }
            try
            {
                iSerialPort.PortName = strPort;
                iSerialPort.BaudRate = nBaudrate;
                iSerialPort.StopBits = StopBits.One;
                iSerialPort.Parity = Parity.None;
                iSerialPort.ReadTimeout = 10;
                iSerialPort.WriteTimeout = 1000;
                iSerialPort.ReadBufferSize = 4096 * 10;
                //iSerialPort.ReceivedBytesThreshold = 8;
                iSerialPort.Open();

                OpenwaitTimer();
                OpenHandleBytesTimer();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                strException = ex.Message;
                return -1;
            }
            m_nType = 0;
            return 0;
        }
        /// <summary>
        /// 
        /// </summary>
        private void OpenwaitTimer()
        {
            try
            {
                if (waitTimer != null)
                {
                    if (waitTimer.Enabled)
                    {
                        waitTimer.Stop();
                    }
                    waitTimer.Dispose();
                    waitTimer = null;
                }
                //建立定时器处理数据
                waitTimer = new System.Timers.Timer(1);//实例化Timer类，设置间隔时间为10000毫秒；
                waitTimer.Elapsed += new System.Timers.ElapsedEventHandler(AnalyReceivedData);//到达时间的时候执行事件；
                waitTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                waitTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
                waitTimer.Start(); //启动定时器
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OpenHandleBytesTimer()
        {
            try
            {
                if (HandleBytesTimer != null)
                {
                    if (HandleBytesTimer.Enabled)
                    {
                        HandleBytesTimer.Stop();
                    }
                    HandleBytesTimer.Dispose();
                    HandleBytesTimer = null;
                }
                HandleBytesTimer = new System.Timers.Timer(10);
                HandleBytesTimer.Elapsed += new System.Timers.ElapsedEventHandler(HandleBytesData);
                HandleBytesTimer.AutoReset = true;
                HandleBytesTimer.Enabled = true;
                HandleBytesTimer.Start();
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void CloseCom()
        {
            if (iSerialPort.IsOpen)
            {
                iSerialPort.Close();
            }
            if (waitTimer != null)
            {
                if (waitTimer.Enabled)
                {
                    waitTimer.Stop();
                }

                waitTimer.Dispose();
                waitTimer = null;
            }
            if (HandleBytesTimer != null)
            {
                if (HandleBytesTimer.Enabled)
                {
                    HandleBytesTimer.Stop();
                }
                HandleBytesTimer.Dispose();
                HandleBytesTimer = null;
            }
            m_nType = -1;
        }

        /// <summary>
        /// 串口是否开启
        /// </summary>
        /// <returns></returns>
        public bool IsComOpen()
        {
            try
            {
                return iSerialPort.IsOpen;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //缓存
        private byte[] s232Buffer = new byte[2048];
        private int s232Buffersp = 0;
        private byte[] s232EndBuffer = new byte[2048];
        private int s232EndBuffersp = 0;
        private byte btCheckAryCRC = 0x00;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceivedComData(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int nCount = iSerialPort.BytesToRead;

                if (nCount == 0)
                {
                    return;
                }
                byte[] btAryBuffer = new byte[nCount];
                iSerialPort.Read(btAryBuffer, 0, nCount);
                for (int i = 0; i < nCount; i++)
                {
                    s232Buffer[s232Buffersp] = btAryBuffer[i];
                    if (s232Buffersp < (s232Buffer.Length - 2))
                        s232Buffersp++;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 定时器处理数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void AnalyReceivedData(object source, System.Timers.ElapsedEventArgs e)
        {
            if (waitTimer != null)
                waitTimer.Stop();
            try
            {
                try
                {
                    if (qType == "qSendFinish")
                    {
                        ///接收成绩超时判断 500毫秒没收到成绩发一次重取成绩
                        HandleBytesTimerStep++;
                        if (HandleBytesTimerStep > 300)
                        {
                            Task.Run(() =>
                            {
                                HandleBytesTimerStep = 0;
                                string code = "Post All Score";
                                byte[] WriteBufferALL = System.Text.Encoding.Default.GetBytes(code);
                                qType = "qStart";
                                SendMessage(WriteBufferALL);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }

                if (s232Buffersp != 0)
                {
                    if (s232Buffersp >= s232Buffer.Length)
                        s232Buffersp = s232Buffer.Length;
                    byte[] btAryBuffer = new byte[s232Buffersp];
                    if (s232Buffersp >= s232Buffer.Length) s232Buffersp = s232Buffer.Length;
                    Array.Copy(s232Buffer, 0, btAryBuffer, 0, s232Buffersp);
                    Array.Clear(s232Buffer, 0, s232Buffersp);
                    s232Buffersp = 0;
                    _buffer.AddRange(btAryBuffer);
                    ReceiveCallback?.Invoke(btAryBuffer);

                    //RunReceiveDataCallback(btAryBuffer);
                    //string code = CCommondMethod.ByteArrayToString(btAryBuffer, 0, btAryBuffer.Length);
                    //Console.WriteLine($"------------------------receiveCount:{btAryBuffer.Length}   recv:{code}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                if (waitTimer != null)
                {
                    waitTimer.Start();
                }
                else
                {
                    OpenwaitTimer();
                }
            }
        }

        private bool IsHandleData = false;
        private int HandleBytesTimerStep = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void HandleBytesData(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                IsHandleData = true;
                if (HandleBytesTimer != null)
                    HandleBytesTimer.Stop();

                if (_buffer.Count == 0) return;
                int nCount = _buffer.Count;
                if (qType == "paircount")
                {
                    //70 61 69 72 70 6C 61 79 63 6F 75 6E 74 3A 30
                    int step = 0;
                    while (_buffer.Count > 4)
                    {
                        if (_buffer[0] == 0x70 && _buffer[1] == 0x61
                            && _buffer[2] == 0x69 && _buffer[3] == 0x72)
                        {
                            if (_buffer.Count < 17) continue;
                            byte[] btAryAnaly = new byte[17];
                            _buffer.CopyTo(0, btAryAnaly, 0, 17);
                            SerialMessageTran msgTran = new SerialMessageTran(btAryAnaly, -1);
                            AnalyCallback?.Invoke(msgTran);
                            _buffer.RemoveRange(0, 17);
                            step = 1;
                        }
                        else
                        {
                            _buffer.RemoveAt(0);
                        }
                    }
                    if (step == 1)
                        qType = "";
                }
                else if (qType == "qStart")
                {
                    try
                    {
                        while (_buffer.Count > 0)
                        {
                            if (_buffer[0] == 0xFE)
                            {
                                if (_buffer.Count < FEmn) continue;
                                if (_buffer[FEmn - 1] == 0xFF)
                                {
                                    byte[] btAryAnaly = new byte[FEmn];
                                    _buffer.CopyTo(0, btAryAnaly, 0, FEmn);
                                    //_buffer.RemoveRange(0, FEmn);
                                    _buffer.Clear();
                                    SerialMessageTran msgTran = new SerialMessageTran(btAryAnaly, 0);
                                    AnalyCallback?.Invoke(msgTran);
                                }
                            }
                            else if (_buffer[0] == 0xFF)
                            {
                                //接收结束数据
                                if (_buffer.Count < FFmn) continue;
                                if (_buffer[FFmn - 1] == 0xFF)
                                {
                                    byte[] btAryAnaly = new byte[FFmn];
                                    _buffer.CopyTo(0, btAryAnaly, 0, FFmn);
                                    //_buffer.RemoveRange(0, FFmn);
                                    _buffer.Clear();
                                    for (int i = 0; i < btAryAnaly.Length; i++)
                                    {
                                        btCheckAryCRC ^= btAryAnaly[i];
                                    }
                                    byte[] btCheckAryData = new byte[7];
                                    Array.Clear(btCheckAryData, 0, btCheckAryData.Length);
                                    //收到最终数据发送回答
                                    btCheckAryData[0] = 0x02;
                                    btCheckAryData[1] = 0x31;
                                    btCheckAryData[2] = 0x30;
                                    btCheckAryData[3] = 0x01;
                                    btCheckAryData[4] = btCheckAryCRC;//成绩校验和
                                    btCheckAryData[5] = 0x03;
                                    btCheckAryData[6] = CheckSum(btCheckAryData, 0, 6);
                                    _buffer.Clear();
                                    SendMessage(btCheckAryData);
                                    qType = "qSendFinish";
                                    HandleBytesTimerStep = 0;
                                    FFmsgTran = new SerialMessageTran(btAryAnaly, 1);
                                }
                            }
                            else if (_buffer[0] == 0x02)
                            {
                                bool flag = true;
                                for (int i = 0; i < nCount; i++)
                                {
                                    if (qRec[i] != _buffer[i])
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    //主机结束发送握手数据
                                    btCheckAryCRC = 0x00;
                                    _buffer.Clear();
                                    FFmsgTran = null;
                                    SendMessage(qSend);
                                    //qType = "qEnd";
                                }
                            }
                            else
                            {
                                _buffer.Clear();
                            }
                            _buffer.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                    }
                    finally
                    {
                        //_buffer.RemoveAt(0);
                        //_buffer.Clear();
                    }
                }
                else if (qType == "qSendFinish")
                {
                    //主机接收校验回复结束码
                    while (_buffer.Count >= endCodeBuffer.Length)
                    {
                        int nCount0 = endCodeBuffer.Length;
                        bool flag = true;
                        for (int i = 0; i < nCount0; i++)
                        {
                            if (endCodeBuffer[i] != _buffer[i])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            if (FFmsgTran != null)
                            {
                                AnalyCallback?.Invoke(FFmsgTran);
                                FFmsgTran = null;
                                qType = "";
                                _buffer.Clear();
                            }
                        }
                        else
                        {
                            _buffer.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    _buffer.Clear();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                if (HandleBytesTimer != null)
                {
                    HandleBytesTimer.Start();
                }
                else
                {
                    OpenHandleBytesTimer();
                }
                _buffer.Clear();
                //if (_buffer.Count > 999) _buffer.Clear();
                IsHandleData = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private int findFirstIndex(byte[] array, byte value, int startIndex = 0)
        {
            int index = -1;
            try
            {
                for (int i = startIndex; i < array.Length; i++)
                {
                    if (array[i] == value)
                    {
                        index = i;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                index = -1;
            }

            return index;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private int findLastIndex(byte[] array, byte value)
        {
            int index = -1;
            try
            {
                for (int i = array.Length - 1; i >= 0; i--)
                {
                    if (array[i] == value)
                    {
                        index = i;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                index = -1;
            }

            return index;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="btAryBuffer"></param>
        private void RunReceiveDataCallback(byte[] btAryBuffer)
        {
            try
            {
                if (ReceiveCallback != null)
                {
                    ReceiveCallback(btAryBuffer);
                }
                int nCount = btAryBuffer.Length;
                if (qType == "paircount")
                {
                    byte[] btAryAnaly = new byte[nCount];
                    Array.Copy(btAryBuffer, 0, btAryAnaly, 0, nCount);
                    SerialMessageTran msgTran = new SerialMessageTran(btAryAnaly, -1);
                    if (AnalyCallback != null)
                    {
                        AnalyCallback(msgTran);
                    }
                }
                else if (qType == "qStart")
                {
                    if (nCount == 8 && btAryBuffer[0] != 0xFE)
                    {
                        bool flag = true;
                        for (int i = 0; i < nCount; i++)
                        {
                            if (qRec[i] != btAryBuffer[i])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            btCheckAryCRC = 0x00;
                            Array.Clear(s232Buffer, 0, s232Buffersp);
                            s232Buffersp = 0;
                            SendMessage(qSend);
                            Array.Clear(s232EndBuffer, 0, s232EndBuffer.Length);
                            s232EndBuffersp = 0;
                            qType = "qEnd";
                            return;
                        }
                    }
                    if (btAryBuffer[0] != 0xFE || btAryBuffer[btAryBuffer.Length - 1] != 0xFF)
                        return;
                    int FEindex = findFirstIndex(btAryBuffer, 0xFE);
                    int FFindex = findLastIndex(btAryBuffer, 0xFF);
                    int nLen = FFindex - FEindex + 1;
                    if (nLen > 0 && FEindex > -1 && FFindex > -1)
                    {
                        byte[] btAryAnaly = new byte[nLen];
                        Array.Copy(btAryBuffer, FEindex, btAryAnaly, 0, nLen);
                        SerialMessageTran msgTran = new SerialMessageTran(btAryAnaly, 0);
                        if (AnalyCallback != null)
                        {
                            AnalyCallback(msgTran);
                        }
                    }
                }
                else if (qType == "qEnd")
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        if (s232EndBuffersp > 0 && btAryBuffer[i] == 0xFF)
                        {
                            byte[] btCheckAryData = new byte[7];
                            Array.Clear(btCheckAryData, 0, btCheckAryData.Length);
                            //收到最终数据发送回答
                            btCheckAryData[0] = 0x02;
                            btCheckAryData[1] = 0x31;
                            btCheckAryData[2] = 0x30;
                            btCheckAryData[3] = 0x01;
                            btCheckAryData[4] = btCheckAryCRC;//成绩校验和
                            btCheckAryData[5] = 0x03;
                            btCheckAryData[6] = CheckSum(btCheckAryData, 0, 6);
                            SendMessage(btCheckAryData);
                            qType = "qSendFinish";
                        }
                        if (btAryBuffer[i] != 0xFF)
                        {
                            btCheckAryCRC ^= btAryBuffer[i];
                        }
                        s232EndBuffer[s232EndBuffersp] = btAryBuffer[i];
                        if (s232EndBuffersp < (s232EndBuffer.Length - 2))
                            s232EndBuffersp++;
                    }
                    if (qType == "qSendFinish")
                    {
                        byte[] s232EndBuffer0 = new byte[s232EndBuffersp];
                        Array.Copy(s232EndBuffer, 0, s232EndBuffer0, 0, s232EndBuffersp);
                        SerialMessageTran msgTran = new SerialMessageTran(s232EndBuffer0, 1);
                        if (AnalyCallback != null)
                        {
                            AnalyCallback(msgTran);
                        }
                        qType = "";
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggerHelper.Debug(ex);
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="btArySenderData"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public int SendMessage(byte[] btArySenderData, int len = 0)
        {
            //串口连接方式
            if (m_nType == 0)
            {
                if (!iSerialPort.IsOpen)
                {
                    return -1;
                }
                if (len == 0)
                {
                    len = btArySenderData.Length;
                }
                if (!IsHandleData)
                {
                    _buffer.Clear();
                }
                iSerialPort.Write(btArySenderData, 0, len);

                if (SendCallback != null)
                {
                    SendCallback(btArySenderData);
                }

                return 0;
            }
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="btAryData"></param>
        /// <returns></returns>
        public byte CheckValue(byte[] btAryData)
        {
            return CheckSum(btAryData, 0, btAryData.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="btAryBuffer"></param>
        /// <param name="nStartPos"></param>
        /// <param name="nLen"></param>
        /// <returns></returns>
        public static byte CheckSum(byte[] btAryBuffer, int nStartPos, int nLen)
        {
            byte btSum = 0x00;

            for (int nloop = nStartPos; nloop < nStartPos + nLen; nloop++)
            {
                btSum ^= btAryBuffer[nloop];
            }
            return btSum;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isExaming()
        {
            if (qType == "qStart" || qType == "qSendFinish" || qType == "qEnd")
            {
                return true;
            }
            return false;
        }
    }
}