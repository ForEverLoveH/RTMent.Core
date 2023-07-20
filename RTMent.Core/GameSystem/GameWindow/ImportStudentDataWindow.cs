
using HZH_Controls.Forms;
using MiniExcelLibs;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindowSys;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class ImportStudentDataWindow : Form
    {
        public ImportStudentDataWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        private List<Dictionary<string, string>> localInfos = new List<Dictionary<string, string>>();
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, string> localValues = new Dictionary<string, string>();
       

        public string ProjectName { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportStudentDataWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            localInfos = ImportStudentDataWindowSys.Instance.LoadingInitData();
            if(localInfos.Count>0&&localInfos!=null)
            {
                foreach (var item in localInfos)
                {
                    localValues.Add(item["key"], item["value"]);
                }
            }
            SoftWareProperty SoftWareProperty = new SoftWareProperty ();
            if (SoftWareProperty.singleMode != "0")
            {
               uiTitlePanel2.Enabled = false;
            }
        }
        /// <summary>
        /// 平台设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            ImportStudentDataWindowSys.Instance.ShowPlatformSettingWindow();
        }
        /// <summary>
        /// 拉取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            string num = uiTextBox1.Text.Trim();
            if (string.IsNullOrEmpty(num ))
            {
                UIMessageBox.ShowWarning("请先输入你需要拉取的学生数目！！");
                return;
            }
            else
            { 
                string path =ImportStudentDataWindowSys.Instance.LoadingPlatformStudentDataView(num,localValues);
                var  workSheet = MiniExcel.GetSheetNames(path);
                LoadingDataMessage(path, workSheet[0]);
                ThreadStart st = new ThreadStart(() => 
                {
                    ImportStudentDataWindowSys.Instance.ImportCurrentDataToDataBase(dataList, ProjectName, ref proVal, ref proMax);
                }); 
                Thread  thread= new Thread( st );
                thread.IsBackground= true;
                thread.Start();
                DialogResult = DialogResult.OK;

            }
        }
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton3_Click(object sender, EventArgs e)
        {
            if(ImportStudentDataWindowSys.Instance.InitDataBase())
            {
                UIMessageBox.ShowSuccess("数据库初始化成功！！");
               
            }
            else
            {
                UIMessageBox.ShowError("数据库初始化失败！！");
                return;
            }
        }
        /// <summary>
        /// 数据库备份
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton4_Click(object sender, EventArgs e)
        {
            if (ImportStudentDataWindowSys.Instance.BackDataBase())
            {
                UIMessageBox.ShowSuccess("数据库备份成功！！");

            }
            else
            {
                UIMessageBox.ShowError("数据库备份失败！！");
                return;
            }
        }
        /// <summary>
        /// 浏览本地
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton5_Click(object sender, EventArgs e)
        {
            string path =ImportStudentDataWindowSys.Instance.OpenLocalXlsxFile();
            if (!string.IsNullOrEmpty(path))
            {
                PathInput.Text = path;
                uiComboBox1.Items.Clear();
                var sl = MiniExcel.GetSheetNames(path);
                if (sl.Count > 0)
                {
                    foreach (var s in sl)
                    {
                        uiComboBox1.Items.Add(s);
                    }
                    uiComboBox1.SelectedIndex = 0;
                }
                else
                {
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先 选择本地Excel 文件");
                return;
            }
        }
        int proVal = 0;
        int proMax = 0;
        List<InputData> dataList = new List<InputData>();
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton6_Click(object sender, EventArgs e)
        {
            string path = PathInput.Text.Trim();
            string workSheetName=uiComboBox1.Text.Trim();
            
            LoadingDataMessage(path,workSheetName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="workSheetName"></param>
        private   void   LoadingDataMessage(string path, string workSheetName)
        {
            dataList.Clear();
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(workSheetName))
            {
                dataList = ImportStudentDataWindowSys.Instance.LoadingLocalImportStudentData(ProjectName, path, workSheetName);
                if (dataList.Count > 0)
                {
                    uiLabel4.Text = $"导入的excel文件中的学生数据有{dataList.Count}条";
                    ImportStudentDataWindowSys.Instance.ShowLocalStudentDataView(dataList, listView1, ProjectName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton7_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "xlsx file(*.xlsx)|*.xlsx|All files(*.*)|*.*";
            saveImageDialog.RestoreDirectory = true;
            saveImageDialog.FileName = $"导出模板{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            string path = Application.StartupPath + "\\excel\\output.xlsx";

            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                path = saveImageDialog.FileName;
                if (File.Exists(path)) File.Delete(path);
                File.Copy(@"./模板/导入名单模板1.xlsx", path);
               UIMessageBox.ShowWarning( "导出成功");
            }
        }
        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton8_Click(object sender, EventArgs e)
        {
            if (dataList.Count > 0)
            {
               
                if(ImportStudentDataWindowSys.Instance.ImportCurrentDataToDataBase(dataList,ProjectName,ref proVal,ref proMax))
                {
                    UIMessageBox.ShowSuccess("导入成功！！");
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    UIMessageBox.ShowSuccess("导入失败！！");
                    DialogResult = DialogResult.Cancel;
                }
               
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (proMax != 0)
            {
                progressBar1.Maximum = proMax;
                if (proVal > proMax)
                {
                    proVal = proMax;
                    timer1.Stop();
                }
                progressBar1.Value = proVal;
            }
        }

        
    }
}
