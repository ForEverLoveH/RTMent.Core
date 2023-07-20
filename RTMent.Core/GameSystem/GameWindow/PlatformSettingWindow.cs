using HZH_Controls.Forms;
using RTMent.Core.GameSystem.GameWindowSys;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RTMent.Core.GameSystem.GameModel;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class PlatformSettingWindow : Form
    {
        public PlatformSettingWindow()
        {
            InitializeComponent();
        }
        public Dictionary<string, string> localValues = new Dictionary<string, string>();
        public List<Dictionary<string, string>> localInfos = new List<Dictionary<string, string>>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlatformSettingWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            localInfos = PlatformSettingWindowSys.Instance.LoadingInitData();
            if (localInfos.Count > 0)
            {
                string MachineCode = String.Empty;
                string ExamId = String.Empty;
                string Platform = String.Empty;
                string Platforms = String.Empty;
                foreach (var item in localInfos)
                {
                    localValues.Add(item["key"], item["value"]);
                    switch (item["key"])
                    {
                        case "MachineCode":
                            MachineCode = item["value"];
                            break;
                        case "ExamId":
                            ExamId = item["value"];
                            break;
                        case "Platform":
                            Platform = item["value"];
                            break;
                        case "Platforms":
                            Platforms = item["value"];
                            break;
                    }
                }

                if (string.IsNullOrEmpty(MachineCode))
                {
                    UIMessageBox.ShowWarning("设备码为空");
                }
                else
                {
                    comboBox1.Text = MachineCode;
                }


                if (string.IsNullOrEmpty(ExamId))
                {
                    UIMessageBox.ShowWarning("考试id为空");
                }
                else
                {
                    comboBox3.Text = ExamId;
                }
                if (string.IsNullOrEmpty(Platforms))
                {
                    UIMessageBox.ShowWarning("平台码为空");
                }
                else
                {
                    string[] Platformss = Platforms.Split(';');
                    comboBox2.Items.Clear();
                    foreach (var item in Platformss)
                    {
                        comboBox2.Items.Add(item);
                    }

                }
                if (string.IsNullOrEmpty(Platform))
                {
                    UIMessageBox.ShowWarning("平台码为空");
                }
                else
                {
                    comboBox2.Text = Platform;
                }
            }
        }
        /// <summary>
        /// 获取考试id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       
        private void uiButton1_Click(object sender, EventArgs e)
        {
           comboBox3.Items.Clear();
           string url = comboBox2.Text.Trim();
           if (string.IsNullOrWhiteSpace(url))
           {
               UIMessageBox.ShowWarning("网址为空！！");
               return;
           }
           var sl= PlatformSettingWindowSys.Instance.GetExamID(url,localValues);
           if (sl != null)
           {
               if (sl.Results.Count == 0)
               {
                   UIMessageBox.ShowWarning(string.Format("提交错误，错误码为：{0}",sl.Error));
                   return;
               }
               foreach (var res in sl.Results)
               {
                   string str = $"{res.title}_{res.exam_id}";
                   comboBox3.Items.Add(str);
               }
               UIMessageBox.ShowSuccess("获取成功！！");
               comboBox3.SelectedIndex = 0;
           }
        }
        /// <summary>
        /// 获取机器码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
             comboBox1.Items.Clear();
             string examID = comboBox3.Text.Trim();
             if (string.IsNullOrWhiteSpace(examID))
             {
                 UIMessageBox.ShowWarning("考试id为空！！");
                 return;
             }
             if (examID.IndexOf('_') != -1)
                 examID=examID.Substring(examID.IndexOf('_') + 1);
             string url = comboBox2.Text.Trim();
             if (string.IsNullOrWhiteSpace(url))
             {
                 UIMessageBox.ShowWarning("网址为空！！");
                 return;
             }
             var sl = PlatformSettingWindowSys.Instance.GetEquipMentID(examID, url, localValues);
             if (sl!=null)
             {
                 if (sl.Results.Count==0)
                 {
                     UIMessageBox.ShowWarning($"提交错误,错误码:[{sl.Error}]");
                     return;
                 }
                 foreach (var res in sl.Results)
                 {
                     string str = $"{res.title}_{res.MachineCode}";
                     comboBox1.Items.Add(str);
                 }
                 UIMessageBox.ShowSuccess("获取成功！！");
                 comboBox1.SelectedIndex = 0;
             }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton3_Click(object sender, EventArgs e)
        {
            string platform = comboBox2.Text.Trim();
            string examID = comboBox3.Text.Trim();
            string machineCode = comboBox1.Text.Trim();
            if (PlatformSettingWindowSys.Instance.SaveDataSetting(platform, examID, machineCode))
            {
                UIMessageBox.ShowSuccess("保存成功！！");
            }
            else
            {
                UIMessageBox.ShowError("保存失败！！");
                return;
            }
        }
    }
}
