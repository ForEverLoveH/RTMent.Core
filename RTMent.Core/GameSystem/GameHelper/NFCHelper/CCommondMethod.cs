using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class CCommondMethod
    {
        /// <summary>
        /// 字符串转16进制数组，字符串以空格分割
        /// </summary>
        /// <param name="strHexValue"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string strHexValue)
        {
            string[] strAryHex = strHexValue.Split(' ');
            byte[] btAryHex = new byte[strAryHex.Length];

            try
            {
                int nIndex = 0;
                foreach (string strTemp in strAryHex)
                {
                    btAryHex[nIndex] = Convert.ToByte(strTemp, 16);
                    nIndex++;
                }
            }
            catch (System.Exception ex)
            {

            }

            return btAryHex;
        }

        /// <summary>
        /// 字符数组转为16进制数组
        /// </summary>
        /// <param name="strAryHex"></param>
        /// <param name="nLen"></param>
        /// <returns></returns>
        public static byte[] StringArrayToByteArray(string[] strAryHex, int nLen)
        {
            if (strAryHex.Length < nLen)
            {
                nLen = strAryHex.Length;
            }

            byte[] btAryHex = new byte[nLen];

            try
            {
                int nIndex = 0;
                foreach (string strTemp in strAryHex)
                {
                    btAryHex[nIndex] = Convert.ToByte(strTemp, 16);
                    nIndex++;
                }
            }
            catch (System.Exception ex)
            {

            }

            return btAryHex;
        }

        /// <summary>
        /// 16进制字符数组转成字符串
        /// </summary>
        /// <param name="btAryHex"></param>
        /// <param name="nIndex"></param>
        /// <param name="nLen"></param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] btAryHex, int nIndex, int nLen)
        {
            if (nIndex + nLen > btAryHex.Length)
            {
                nLen = btAryHex.Length - nIndex;
            }

            string strResult = string.Empty;

            for (int nloop = nIndex; nloop < nIndex + nLen; nloop++)
            {
                string strTemp = string.Format("{0:X2}", btAryHex[nloop]);

                strResult += strTemp;
            }

            return strResult;
        }

        /// <summary>
        /// 10进制字符数组转成字符串
        /// </summary>
        /// <param name="btAryHex"></param>
        /// <param name="nIndex"></param>
        /// <param name="nLen"></param>
        /// <returns></returns>
        public static string ByteArrayToStringDEC(byte[] btAryHex, int nIndex, int nLen)
        {
            if (nIndex + nLen > btAryHex.Length)
            {
                nLen = btAryHex.Length - nIndex;
            }

            string strResult = string.Empty;

            for (int nloop = nIndex; nloop < nIndex + nLen; nloop++)
            {
                string strTemp = string.Format("{0}", btAryHex[nloop]);

                strResult += strTemp;
            }

            return strResult;
        }

        /// <summary>
        /// 将字符串按照指定长度截取并转存为字符数组，空格忽略
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="nLength"></param>
        /// <returns></returns>
        public static string[] StringToStringArray(string strValue, int nLength)
        {
            string[] strAryResult = null;

            if (!string.IsNullOrEmpty(strValue))
            {
                System.Collections.ArrayList strListResult = new System.Collections.ArrayList();
                string strTemp = string.Empty;
                int nTemp = 0;

                for (int nloop = 0; nloop < strValue.Length; nloop++)
                {
                    if (strValue[nloop] == ' ')
                    {
                        continue;
                    }
                    else
                    {
                        nTemp++;

                        //校验截取的字符是否在A~F、0~9区间，不在则直接退出
                        System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^(([A-F])*(\d)*)$");
                        if (!reg.IsMatch(strValue.Substring(nloop, 1)))
                        {
                            return strAryResult;
                        }

                        strTemp += strValue.Substring(nloop, 1);

                        //判断是否到达截取长度
                        if ((nTemp == nLength) || (nloop == strValue.Length - 1 && !string.IsNullOrEmpty(strTemp)))
                        {
                            strListResult.Add(strTemp);
                            nTemp = 0;
                            strTemp = string.Empty;
                        }
                    }
                }

                if (strListResult.Count > 0)
                {
                    strAryResult = new string[strListResult.Count];
                    strListResult.CopyTo(strAryResult);
                }
            }

            return strAryResult;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="btErrorCode"></param>
        /// <returns></returns>
        public static string FormatErrorCode(byte btErrorCode)
        {
            string strErrorCode = "";
            switch (btErrorCode)
            {
                case 0x10:
                    strErrorCode = "命令已执行";
                    break;
                case 0x11:
                    strErrorCode = "命令执行失败";
                    break;
                case 0x20:
                    strErrorCode = "CPU 复位错误";
                    break;
                case 0x21:
                    strErrorCode = "打开CW 错误";
                    break;
                case 0x22:
                    strErrorCode = "天线未连接";
                    break;
                case 0x23:
                    strErrorCode = "写Flash 错误";
                    break;
                case 0x24:
                    strErrorCode = "读Flash 错误";
                    break;
                case 0x25:
                    strErrorCode = "设置发射功率错误";
                    break;
                case 0x31:
                    strErrorCode = "盘存标签错误";
                    break;
                case 0x32:
                    strErrorCode = "读标签错误";
                    break;
                case 0x33:
                    strErrorCode = "写标签错误";
                    break;
                case 0x34:
                    strErrorCode = "锁定标签错误";
                    break;
                case 0x35:
                    strErrorCode = "灭活标签错误";
                    break;
                case 0x36:
                    strErrorCode = "无可操作标签错误";
                    break;
                case 0x37:
                    strErrorCode = "成功盘存但访问失败";
                    break;
                case 0x38:
                    strErrorCode = "缓存为空";
                    break;
                case 0x40:
                    strErrorCode = "访问标签错误或访问密码错误";
                    break;
                case 0x41:
                    strErrorCode = "无效的参数";
                    break;
                case 0x42:
                    strErrorCode = "wordCnt 参数超过规定长度";
                    break;
                case 0x43:
                    strErrorCode = "MemBank 参数超出范围";
                    break;
                case 0x44:
                    strErrorCode = "Lock 数据区参数超出范围";
                    break;
                case 0x45:
                    strErrorCode = "LockType 参数超出范围";
                    break;
                case 0x46:
                    strErrorCode = "读卡器地址无效";
                    break;
                case 0x47:
                    strErrorCode = "Antenna_id 超出范围";
                    break;
                case 0x48:
                    strErrorCode = "输出功率参数超出范围";
                    break;
                case 0x49:
                    strErrorCode = "射频规范区域参数超出范围";
                    break;
                case 0x4A:
                    strErrorCode = "波特率参数超过范围";
                    break;
                case 0x4B:
                    strErrorCode = "蜂鸣器设置参数超出范围";
                    break;
                case 0x4C:
                    strErrorCode = "EPC 匹配长度越界";
                    break;
                case 0x4D:
                    strErrorCode = "EPC 匹配长度错误";
                    break;
                case 0x4E:
                    strErrorCode = "EPC 匹配参数超出范围";
                    break;
                case 0x4F:
                    strErrorCode = "频率范围设置参数错误";
                    break;
                case 0x50:
                    strErrorCode = "无法接收标签的RN16";
                    break;
                case 0x51:
                    strErrorCode = "DRM 设置参数错误";
                    break;
                case 0x52:
                    strErrorCode = "PLL 不能锁定";
                    break;
                case 0x53:
                    strErrorCode = "射频芯片无响应";
                    break;
                case 0x54:
                    strErrorCode = "输出达不到指定的输出功率";
                    break;
                case 0x55:
                    strErrorCode = "版权认证未通过";
                    break;
                case 0x56:
                    strErrorCode = "频谱规范设置错误";
                    break;
                case 0x57:
                    strErrorCode = "输出功率过低";
                    break;
                case 0xFF:
                    strErrorCode = "未知错误";
                    break;

                default:
                    break;
            }

            return strErrorCode;
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
                btSum += btAryBuffer[nloop];
            }

            return Convert.ToByte(((~btSum) + 1) & 0xFF);
        }
        /// <summary>
        /// 
        /// </summary>
        public enum CMD
        {
            cmd_read_gpio_value = 0x60,
            cmd_write_gpio_value = 0x61,
            cmd_set_ant_connection_detector = 0x62,
            cmd_get_ant_connection_detector = 0x63,
            cmd_set_temporary_output_power = 0x66,
            cmd_set_reader_identifier = 0x67,
            cmd_get_reader_identifier = 0x68,
            cmd_set_rf_link_profile = 0x69,
            cmd_get_rf_link_profile = 0x6A,
            cmd_set_antenna_group = 0x6C,
            cmd_get_antenna_group = 0x6D,

            cmd_reset = 0x70,
            cmd_set_uart_baudrate = 0x71,
            cmd_get_firmware_version = 0x72,
            cmd_set_reader_address = 0x73,
            cmd_set_work_antenna = 0x74,
            cmd_get_work_antenna = 0x75,
            cmd_set_output_power = 0x76,
            [Obsolete("this command is Deprecated")]
            cmd_get_output_power = 0x77,
            cmd_set_frequency_region = 0x78,
            cmd_get_frequency_region = 0x79,
            cmd_set_beeper_mode = 0x7A,
            cmd_get_reader_temperature = 0x7B,
            cmd_set_drm_mode = 0x7C,
            cmd_get_drm_mode = 0x7D,
            cmd_get_rf_port_return_loss = 0x7E,

            cmd_inventory = 0x80,
            cmd_read = 0x81,
            cmd_write = 0x82,
            cmd_lock = 0x83,
            cmd_kill = 0x84,
            cmd_set_access_epc_match = 0x85,
            cmd_get_access_epc_match = 0x86,
            cmd_real_time_inventory = 0x89,
            cmd_fast_switch_ant_inventory = 0x8A,
            cmd_customized_session_target_inventory = 0x8B,
            cmd_set_impinj_fast_tid = 0x8C,
            cmd_set_and_save_impinj_fast_tid = 0x8D,
            cmd_get_impinj_fast_tid = 0x8E,

            cmd_get_inventory_buffer = 0x90,
            cmd_get_and_reset_inventory_buffer = 0x91,
            cmd_get_inventory_buffer_tag_count = 0x92,
            cmd_reset_inventory_buffer = 0x93,
            cmd_block_write = 0x94,
            //SetBufferDataFrameInterval = 0x94,
            //GetBufferDataFrameInterval = 0x95,
            cmd_get_output_power_eight = 0x97,
            cmd_tag_select = 0x98,

            // HardwareCalibrate
            //cmdSetCustomFunctionID = 0xA0 id
            cmdGetCustomFunctionID = 0xA1,
            cmd_open_all_ldo_voltage = 0xA2,
            cmdHardwareCalibrate = 0xA3,
            // cmdTestCenterFreqOutputPower = A3 00
            // cmdTestPllLock = A3 01
            // cmdTestPD_1 = A3 02
            // cmdTestPD_2 = A3 03
            // cmdTestAllFreqOutputPower = A3 04
            // cmdTestAutoCalibrateAntennaDetect = A3 10
            // cmdResetCalibrateValue = A3 11
            cmd_get_calibrate_value = 0xA4,
            cmd_get_internal_version = 0xAA,

            // ISO18000 6B
            cmd_iso18000_6b_inventory = 0xB0,
            cmd_iso18000_6b_read = 0xB1,
            cmd_iso18000_6b_write = 0xB2,
            cmd_iso18000_6b_lock = 0xB3,
            cmd_iso18000_6b_query_lock = 0xB4,

            // NXP
            cmd_nxp_untraceable = 0xE1,
            cmd_change_eas = 0xE4,

            // Bluetooth
            cmdGetBluetoothVersion = 0xF0,
            cmdGetBluetoothMac = 0xF1,
            cmdSetBluetoothBroadcastAddr = 0xF2,
            cmdGetBluetoothBoardSn = 0xF3,
            cmdSetBluetoothBoardSn = 0xF4,
            cmdBluetoothShutDown = 0xF5,
            cmdGetBluetoothBoardVersion = 0xF6,
            cmdGetBluetoothVoltage = 0xF7,
            cmdSetBluetoothBuzzer = 0xF8,
            cmdSetBluetoothEnableMode = 0xF9,
            cmdRecvBluetoothReserved = 0xFA,      // single direction Recv
            cmdRecvBluetoothBoardSleep = 0xFB,    // single direction Recv
            cmdRecvBluetoothTriggerKey = 0xFC,    // single direction Recv

            // FuDan
            cmd_fundan = 0xFD,
        }
    }



}
