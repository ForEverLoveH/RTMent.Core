using OpenCvSharp.Features2D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using static System.Net.WebRequestMethods;

namespace RTMent.Core.GameSystem.GameHelper
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="success"></param>
    /// <param name="message"></param>
    /// <param name="recieveData"></param>
    /// <param name="receiveHexData"></param>
    /// <param name="resultData"></param>
    public delegate void MobusDataCallback(bool success, string message, List<int> recieveData, string receiveHexData, string resultData);

    /// <summary>
    /// 信捷plc mobus 指令
    /// </summary>
    public class MobusXJHelper
    {
        private SerialPort _serialPort = null;

        /// <summary>
        ///
        /// </summary>
        public MobusDataCallback MobusDataCallback = null;

        /// <summary>
        /// 接收到的数据
        /// </summary>
        private string _receiveHexData = null;

        /// <summary>
        /// 最后发送的数据
        /// </summary>
        private string _lastSendData = null;

        /// <summary>
        ///
        /// </summary>
        public bool AutoOpenOrCloseSerial = false;

        public MobusXJHelper(SerialPort serialPort)
        {
            this._serialPort = serialPort;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="port"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <param name="handshake"></param>
        /// <param name="dtrEnable"></param>
        /// <param name="rtsEnable"></param>
        public MobusXJHelper(int port, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.Even, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None, bool dtrEnable = true, bool rtsEnable = true)
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM" + port;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopBits;
            _serialPort.Handshake = handshake;
            _serialPort.DtrEnable = dtrEnable;
            _serialPort.RtsEnable = rtsEnable;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="port"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <param name="handshake"></param>
        /// <param name="dtrEnable"></param>
        /// <param name="rtsEnable"></param>
        public MobusXJHelper(int port, int baudRate, int dataBits, int parity, int stopBits, int handshake, bool dtrEnable = true, bool rtsEnable = true)
        {
            //实例化串口
            _serialPort = new SerialPort();

            _serialPort.PortName = "COM" + port;            //端口号
            _serialPort.BaudRate = baudRate;                //波特率
            _serialPort.DataBits = dataBits;                //数据位
            _serialPort.Parity = (Parity)parity;            //奇偶检验位
            _serialPort.StopBits = (StopBits)stopBits;      //停止位
            _serialPort.Handshake = (Handshake)handshake;   //握手协议
            _serialPort.DtrEnable = dtrEnable;
            _serialPort.RtsEnable = rtsEnable;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllCom()
        {
            var list = new List<string>();
            foreach (string port in SerialPort.GetPortNames())
            {
                list.Add(port);
            }
            return list;
        }

        /// <summary>
        /// 通过注册表获取串口
        /// </summary>
        /// <returns></returns>
        public static List<string> GetComByReg()
        {
            Microsoft.Win32.RegistryKey keyCom = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
            if (keyCom != null) return keyCom.GetValueNames().ToList();
            return null;
        }

        public void OpenSerial()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    _serialPort.DataReceived += SerialPort_DataReceived;
                }
            }
        }

        public void CloseSerial()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                }
            }
        }

        /// <summary>
        /// 读取线圈状态
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public void COL_ReadData(short device, short addr, short count, MobusDataCallback callback)
        {
            string cmd = device.ToString("X2") + " 01 " + addr.ToString("X4") + " " + count.ToString("X4");
            SendData(cmd, callback);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public void COL_WriteData(short device, short addr, short value, MobusDataCallback callback)
        {
            string cmd = device.ToString("X2") + " 05 " + addr.ToString("X4") + " " + value.ToString("X4");
            SendData(cmd, callback);
        }

        /// <summary>
        /// 写入多个线圈状态
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="addrLength"></param>
        /// <param name="values"></param>
        /// <param name="callback"></param>
        public void COL_WriteMoreData(short device, short addr, short addrLength, short[] values, MobusDataCallback callback)
        {
            int ys = addrLength % 8;
            int byteCount = ys == 0 ? addrLength / 8 : (addrLength / 8) + 1;
            string cmd = device.ToString("X2") + " 0F " + addr.ToString("X4") + " " + addrLength.ToString("X4") + " " + byteCount.ToString("X2");
            List<int> sendValues = new List<int>();
            //拼合 二进制 转为 十进制
            string byteStr = "";
            for (var i = 0; i < values.Length; i++)
            {
                byteStr += values[i];
                if (i > 0 && i % 7 == 0)
                {
                    byteStr = string.Concat(byteStr.Reverse());
                    sendValues.Add(Convert.ToInt32(byteStr, 2));
                    byteStr = "";
                }
            }
            if (!string.IsNullOrWhiteSpace(byteStr))
            {
                byteStr = string.Concat(byteStr.Reverse());
                sendValues.Add(Convert.ToInt32(byteStr, 2));
                byteStr = "";
            }
            //转为十六进制
            foreach (int value in sendValues)
            {
                cmd += value.ToString("X2") + " ";
            }
            SendData(cmd, callback);
        }

        /// <summary>
        /// 读输入线圈指令
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public void INR_ReadData(short device, short addr, short count, MobusDataCallback callback)
        {
            string cmd = device.ToString("X2") + " 02 " + addr.ToString("X4") + " " + count.ToString("X4");
            SendData(cmd, callback);
        }

        /// <summary>
        /// 读输入寄存器指令
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public void INR_ReadMoreData(short device, short addr, short count, MobusDataCallback callback)
        {
            string cmd = device.ToString("X2") + " 04 " + addr.ToString("X4") + " " + count.ToString("X4");
            SendData(cmd, callback);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public void REG_ReadData(short device, short addr, short count, MobusDataCallback callback)
        {
            string cmd = device.ToString("X2") + " 03 " + addr.ToString("X4") + " " + count.ToString("X4");
            SendData(cmd, callback);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public void REG_WriteData(short device, short addr, short value, MobusDataCallback callback)
        {
            string cmd = device.ToString("X2") + " 06 " + addr.ToString("X4") + " " + value.ToString("X4");
            SendData(cmd, callback);
        }

        /// <summary>
        /// 写入多个数据
        /// </summary>
        /// <param name="device"></param>
        /// <param name="addr"></param>
        /// <param name="values"></param>
        /// <param name="callback"></param>
        public void REG_WriteMoreData(short device, short addr, short addrLength, short[] values, MobusDataCallback callback)
        {
            //报文格式示例：
            //从机地址   功能码   寄存器起始地址高字节   寄存器起始地址低字节   寄存器数量高字节   寄存器数量低字节   字节数   数据1高字节   数据1低字节   数据2高字节  数据2低字节   CRC校验高字节   CRC校验低字节
            //11             10         00                                 01                                00                           02                          04         00                  0A                 01                  02                 C6                       F0

            string cmd = device.ToString("X2") + " 10 " + addr.ToString("X4") + " " + addrLength.ToString("X4") + " " + (addrLength * 2).ToString("X2");

            //对应写入 不够写入0
            for (int i = 0; i < addrLength; i++)
            {
                int value = (values.Length >= (i - 1)) ? values[i] : 0;
                cmd += value.ToString("x4");
            }

            SendData(cmd, callback);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="callback"></param>
        private void SendData(string cmd, MobusDataCallback callback)
        {
            if (_serialPort == null)
            {
                callback?.Invoke(false, "串口未定义", null, null, null);
                return;
            }
            if (string.IsNullOrEmpty(cmd))
            {
                callback?.Invoke(false, "发送的数据不可为空", null, null, null);
                return;
            }
            if (AutoOpenOrCloseSerial == false && _serialPort.IsOpen)
            {
                callback?.Invoke(false, "请先打开串口", null, null, null);
                return;
            }
            try
            {
                MobusDataCallback = callback;
                _lastSendData = cmd;
                _receiveHexData = "";
                SendHexdecimalData(cmd);
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return;
            }
        }

        /// <summary>
        /// 发送16 进制数据
        /// </summary>
        /// <param name="cmd"></param>
        private void SendHexdecimalData(string sendData)
        {
            sendData = sendData.Replace("", "");
            byte[] dats = new byte[sendData.Length / 2 + 2];
            int k = 0;
            for (int i = 0; i < sendData.Length - 1; i += 2)
            {
                string str = sendData.Substring(i, 2);
                dats[k] = Convert.ToByte(str, 16);
                k++;
            }
            string crc = CRCCoreCode(sendData);
            //高位
            string crcH = crc.Substring(2, 2);
            //低位
            string crcL = crc.Substring(0, 2);
            //将CRC转换成一个字节的10进制
            dats[sendData.Length / 2] = Convert.ToByte(crcH, 16);
            //将CRC转换成一个字节的10进制
            dats[sendData.Length / 2 + 1] = Convert.ToByte(crcL, 16);
            if (AutoOpenOrCloseSerial)
            {
                while (true)
                {
                    try
                    {
                        OpenSerial();
                        break;
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            _serialPort.Write(dats, 0, dats.Length);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            int len = _serialPort.BytesToRead;
            if (len <= 0) { return; }
            byte[] data = new byte[len];
            int ls = _serialPort.Read(data, 0, data.Length);
            if (ls <= 2 || data == null) return;
            string resultData = Encoding.ASCII.GetString(data, 0, data.Length);
            List<int> list = new List<int>();
            foreach (int i in data)
            {
                list.Add(Convert.ToInt32(i));
                _receiveHexData += i.ToString("X2") + " ";
            }
            _receiveHexData = _receiveHexData.Trim();
            string rhd = _receiveHexData.Replace("", "");
            string rhd_check = data[data.Length - 2].ToString("X2") + data.Last().ToString("X2");
            string rhd_Data = rhd.Replace(rhd_check, "");
            bool isRightCRC = false;
            string code = CRCCoreCode(rhd_Data);
            if (code == rhd_check)
            {
                isRightCRC = true;
            }
            MobusDataCallback?.Invoke(isRightCRC, "OK", list, _receiveHexData, resultData);
            if (AutoOpenOrCloseSerial)
            {
                CloseSerial();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rhd_Data"></param>
        /// <returns></returns>
        private string CRCCoreCode(string data)
        {
            Int32 crc = 65535;
            string res = "";
            for (int i = 0; i < data.Length - 1; i += 2)
            {
                string str = "";
                Int32 dataDec = 0;
                str = data.Substring(i, 2);
                dataDec = Convert.ToInt32(str, 16);
                crc = crc ^ dataDec;
                for (int j = 0; j < 8; j++)
                {
                    string binstr;
                    binstr = DECToBIN(crc);
                    if (binstr.Substring(15, 1) == "0")//截取二进制的最低位判断是不是0
                    {
                        crc = crc >> 1;  //如果是0直接右边移1位
                    }
                    else
                    {
                        crc = crc >> 1;  //右边移1位
                        crc = crc ^ 40961;  //与多项式异或
                    }
                }
            }
            res = Convert.ToString(crc, 16).PadLeft(4, '0').ToUpper();
            return res;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        private string DECToBIN(int dec)
        {
            string result = "";
            while (dec > 0)
            {
                result = (dec % 2).ToString() + result;
                dec = dec / 2;
            }

            //返回16位的二进制
            return result.PadLeft(16, '0');
        }
    }
}