using System.Collections.Generic;
using System.Text;
using System;

namespace RTMent.Core.GameSystem.GameHelper
{
    public class SerialMessageTran
    {
        public byte[] btAryTranData;  //完整数据包
        public int btAryTranDataLen; //数据包长度
        public byte[] btCheckAryData = new byte[7];//收到结束成绩后的校验码
        public byte strCmd = 0x00;
        public List<string> ints0 = new List<string>();
        public List<int> ints1 = new List<int>();
        public int strTime = 0;
        public TimeSpan timeSpan;
        //public StringBuilder handleData = new StringBuilder();
        public List<int> ints = new List<int>();
        public bool RecEndFlag = true;

        public SerialMessageTran(byte[] btTranData, int status = 0)
        {

            this.btAryTranData = btTranData;
            this.btAryTranDataLen = btTranData.Length;
            strCmd = btTranData[0];
            if (status == 0 && btAryTranData[0] == 0xFE)
            {
                qStartMessageTran();
            }
            else if (status == 1 && btAryTranData[0] == 0xFF)
            {
                qEndMessageTran();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void qStartMessageTran()
        {
            /*
            FE 00 06 00 1F F1 00 00 20 F1 01 FF 
            // fe 头码
            第2，3位   00 06  时间
            第4，5位   00 1f  分数1
            第6，7位   F1 00  道号01
            第8，9位   00 20  分数2
            第10，11位 F1 01  道号02
            结束位 ：ff
            */

            /*FE 00 06 00 1F F1 00 00 20 F1 01 FF */
            /*FE 00 05   10 0E F1 00 00   10 3C F1 01 00   FF 
              FE 00 05 10 19 F1 00 01 10 78 F1 01 00 FF */
            try
            {
                ints0.Clear();
                ints.Clear();
                //handleData.Clear();
                if (btAryTranDataLen == 0) return;
                int bt0 = btAryTranData[1];
                int bt1 = btAryTranData[2];

                if (bt0 > 16)
                {
                    bt0 -= 16;
                    strTime += 16 << 8;
                    strTime += bt0 << 12;
                }
                else
                {
                    strTime += bt0 << 8;
                }
                strTime += bt1;
                timeSpan = new TimeSpan(0, 0, strTime);
                int loop = 3;
                while (btAryTranDataLen - loop > 4)
                {
                    byte[] temp = new byte[4];
                    Array.Copy(btAryTranData, loop, temp, 0, 4);
                    string score1 = temp[0].ToString("X2") + temp[1].ToString("X2");
                    int len1 = score1.Length;
                    double dscore = 0;
                    for (int j = 0; j < len1; j++)
                    {
                        string code = score1.Substring(j, 1);
                        // int.TryParse(height0.Substring(i, 1), out int height0_0);
                        byte[] bcode = CCommondMethod.StringToByteArray(code);
                        dscore += (bcode[0] * Math.Pow(16, len1 - j - 1));
                    }
                    int score = (int)(dscore / 100);
                    if (score > 0)
                    {
                        ints.Add(1);
                        TimeSpan ts = new TimeSpan(0, 0, 0, score, 0);
                        //ints1.Add(score + "");
                        string instr0 = $"{ts.Minutes.ToString("00")}.{ts.Seconds.ToString("00")}";
                        ints0.Add(instr0);
                    }
                    else
                    {
                        ints.Add(0);
                        ints0.Add("00.00");
                    }
                    loop += 4;
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
        public void qEndMessageTran()
        {
            try
            {
                RecEndFlag = true;
                //handleData.Clear();
                ints1.Clear();
                //主机发送成绩：FF  00 07 94 05 33   00 10     01         00 02      00 01   00 07 93 64 44    00 11      01         00 02      00 02   FF
                //////////////头码     ID卡号5位    分数两位  01固定模式  时间两位   手柄号       ID卡号5位     分数两位   01固定模式    时间两位    手柄号  结尾码
                for (int i = 1; i < btAryTranDataLen; i += 12)
                {

                    byte[] temp = new byte[12];
                    if (i + 12 > btAryTranDataLen) { break; }
                    Array.Copy(btAryTranData, i, temp, 0, 12);
                    string score1 = temp[5].ToString("X2");
                    int len1 = score1.Length;
                    StringBuilder sscore = new StringBuilder(); ;
                    //10进制 分 秒
                    for (int j = 0; j < len1; j++)
                    {
                        string code = score1.Substring(j, 1);
                        byte[] bcode = CCommondMethod.StringToByteArray(code);
                        sscore.Append(bcode[0].ToString());
                    }
                    int.TryParse(sscore.ToString(), out int score);

                    if (score > 0)
                    {
                        /*  TimeSpan ts = new TimeSpan(0, 0, 0, score, 0);
                          string instr0 = $"{ts.Minutes.ToString("00")}.{ts.Seconds.ToString("00")}";*/
                        ints1.Add(score);
                    }
                    else
                    {
                        ints1.Add(0);
                    }

                }

            }
            catch (Exception ex)
            {

                LoggerHelper.Debug(ex);
            }


        }


        public static byte CheckSum(byte[] btAryBuffer, int nStartPos, int nLen)
        {
            byte btSum = 0x00;

            for (int nloop = nStartPos; nloop < nStartPos + nLen; nloop++)
            {
                btSum ^= btAryBuffer[nloop];
            }
            return btSum;
        }
    }
}
