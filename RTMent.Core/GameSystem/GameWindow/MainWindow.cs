using HZH_Controls;
using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindowSys;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private List<ProjectDataModel> projects = new List<ProjectDataModel>();
        private string projectId = "";
        private string projectName = "";
        private string groupName = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            string code = "程序集版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string code1 = "文件版本：" + Application.ProductVersion.ToString();
            toolStripStatusLabel1.Text = code;
            LoadingProjectData();
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadingProjectData()
        {
            projects.Clear();
            uiTreeView1.Nodes.Clear();
            projects = MainWindowSys.Instance.LoadingCurrentProjectData();
            if(projects.Count > 0 )
            {
                for (int i = 0; i < projects.Count; i++)
                {
                    System.Windows.Forms.TreeNode tn1 = new System.Windows.Forms.TreeNode(projects[i].projectName);
                    List<GroupDataModel> list = projects[i].Groups;
                    for (int j = 0; j < list.Count; j++)
                    {
                        tn1.Nodes.Add(list[j].GroupName);
                    }
                    this.uiTreeView1.Nodes.Add(tn1);
                    //全部测试完成显示绿色
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].IsAllTested != 0)
                        {
                            this.uiTreeView1.Nodes[i].Nodes[j].BackColor = Color.MediumSpringGreen;
                        }
                        else {
                            this.uiTreeView1.Nodes[i].Nodes[j].BackColor = Color.White;
                        }
                    }
                }
            }
            else
            {
                UIMessageBox.ShowWarning("数据错误，请重新操作！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTreeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                System.Windows.Forms.TreeNode node = uiTreeView1.GetNodeAt(e.X, e.Y);
                if (node != null)
                    uiTreeView1.SelectedNode = node;
                else
                    return;  
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(uiTreeView1.SelectedNode != null)
            {
                string path = uiTreeView1.SelectedNode.FullPath.Trim();
                string[] fullPath=path. Split('\\');
                if (e.Node.Level == 0)
                {
                    projectName = fullPath[0];
                }
                if (e.Node.Level == 1)
                {
                    projectName = fullPath[0];
                    groupName = fullPath[1];
                }
                MainWindowSys.Instance.LoadingCurrentStudentData(projectName, groupName,listView1,ref projectId);
            }
        }

        private void ucNavigationMenu1_ClickItemed(object sender, EventArgs e)
        {
            var txt = ucNavigationMenu1.SelectItem;
            if(txt != null )
            {
                string name = txt.Text.Trim();
                if(!string.IsNullOrEmpty(name) )
                {
                    switch (name)
                    {
                        case "人员导入":
                            if (MainWindowSys.Instance.ShowPersonImportWindow())
                                LoadingProjectData();
                            break;
                        case "系统参数设置":
                            break;
                        case "平台设备码":
                            MainWindowSys.Instance.ShowPlatformSettingWindow();
                            break;
                        case "修改成绩":
                            if (ModifyCurrentChooseStudentGrade())
                            {
                                UIMessageBox.ShowSuccess("修改成功！！");
                                MainWindowSys.Instance.LoadingCurrentStudentData(projectName, groupName,listView1,ref projectId);
                            }
                            break;
                        case "导出成绩":

                            break;
                        case "清除成绩":
                            if (DeleteCurrentChooseStudentGrade())
                            {
                                UIMessageBox.ShowSuccess("清除成功！！");
                                MainWindowSys.Instance.LoadingCurrentStudentData(projectName, groupName,listView1,ref projectId);
                            }
                            else
                            {
                                UIMessageBox.ShowSuccess("清除失败！！");
                                return;
                            }
                            break;
                        case "上传成绩":
                            ParameterizedThreadStart method = new ParameterizedThreadStart(UpdateLoadScore);
                            Thread threadRead = new Thread(method);
                            threadRead.IsBackground = true;
                            threadRead.Start();
                            break;
                        case "启动测试":
                            RunningTesting();
                            break;
                        case "数据库初始化":
                            if (MainWindowSys.Instance.InitDataBase())
                            {
                                UIMessageBox.ShowSuccess("数据库初始化成功！！");
                            }
                            else
                            {
                                UIMessageBox.ShowError("数据库初始化失败！！");
                                return;
                            }
                            break;
                        case "数据库备份":
                            if(MainWindowSys.Instance.BackDataBase()) 
                            {
                                UIMessageBox.ShowSuccess("数据库初始化成功！！");
                            }
                            else
                            {
                                UIMessageBox.ShowError("数据库初始化失败！！");
                                return;
                            }
                            break;



                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void RunningTesting()
        {
            if (uiTreeView1.SelectedNode != null)
            {
                string paths = uiTreeView1.SelectedNode.FullPath;
                string[] fsp = paths.Split('\\');
                if (fsp.Length > 0)
                {
                    try
                    {
                        this.Hide();
                        if (MainWindowSys.Instance.ShowRunningTestingWindow(fsp))
                        {
                            MainWindowSys.Instance.LoadingCurrentStudentData(projectName, groupName, listView1, ref projectId);
                        }
                    }
                    finally
                    {
                        this.Show();
                    }
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先选择项目数据！！");
                return;
            }
        }

        /// <summary>
        /// 上传成绩
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateLoadScore(object obj)
        {
            string fullPath = uiTreeView1.SelectedNode.FullPath;
            string[] fsp = fullPath.Split('\\');
            if (fsp.Length > 0)
                projectName = fsp[0];
            if (string.IsNullOrEmpty(projectName))
            {
                UIMessageBox.ShowWarning("请先选择项目信息！！");
                return;
            }

            string message = "";
            if (fsp.Length > 0)
            {
                message = MainWindowSys.Instance.UpLoadStudentGrade(fsp,ref proVal,ref proMax,progressBar1,timer1);
            }
        }
        private int proMax = 0;
        private int proVal = 0;
        /// <summary>
        /// 清除成绩
        /// </summary>
        /// <returns></returns>
        private bool DeleteCurrentChooseStudentGrade()
        {
            if (listView1.SelectedItems.Count==0)
            {
                UIMessageBox.ShowWarning("请先选择考生数据！！");
                return false;
            } 
            string Name = listView1.SelectedItems[0].SubItems[3].Text;
            string PersonIdNumber = listView1.SelectedItems[0].SubItems[4].Text;
            if (MessageBox.Show($"是否清除当前{Name}考生的成绩？？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                if ( MainWindowSys.Instance.DeleteCurrentChooseStudentGrade( PersonIdNumber))
                {  
                    return true;
                }
                else
                { 
                    return false;
                }     
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 修改成绩
        /// </summary>
        /// <returns></returns>
        private bool ModifyCurrentChooseStudentGrade()
        {
            if (listView1.SelectedItems.Count==0)
            {
                UIMessageBox.ShowWarning("请先选择考生数据！！");
                return false;
            }
            int index = listView1.SelectedItems[0].Index;
            string projectName = listView1.SelectedItems[0].SubItems[1].Text;
            string groupName = listView1.SelectedItems[0].SubItems[2].Text;
            string Name = listView1.SelectedItems[0].SubItems[3].Text;
            string IdNumber = listView1.SelectedItems[0].SubItems[4].Text;
            string status = listView1.SelectedItems[0].SubItems[5].Text;
            return MainWindowSys.Instance.ModifyCurrentChooseStudentGrade(projectName, this.groupName, Name, IdNumber,
                status);
        }
       
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (proMax == 0 || proMax == proVal || proVal == 0)
            {
                return;
            }
            int upV = (int)(((double)proVal / (double)proMax) * 100);
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                progressBar1.Value = upV;
            });
        }
    }
}
