using HZH_Controls.Controls;
using HZH_Controls.Forms;
using RTMent.Core.GameSystem.GameHelper;
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
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class PersonImportWindow : Form
    {
        public PersonImportWindow()
        {
            InitializeComponent();
        }

        string projectName = "";
        string groupName = "";
        string projectID = "";
        List<ProjectDataModel> projects = new List<ProjectDataModel>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PersonImportWindow_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            LoadingProjectView();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadingProjectView()
        {
            projects.Clear();
            uiTreeView1.Nodes.Clear();
            projects = PersonImportWindowSys.Instance.LoadingProjectView();
            if (projects.Count > 0)
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
                            this.uiTreeView1.Nodes[i].Nodes[j].BackColor = Color.MediumSpringGreen;
                        else
                            this.uiTreeView1.Nodes[i].Nodes[j].BackColor = Color.White;

                    }
                }
            }
            else
            {
                UIMessageBox.ShowWarning("数据库错误！！请重新操作！！");
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
            projectName = "";
            groupName = "";
            if (uiTreeView1.SelectedNode != null)
            {
                string path = uiTreeView1.SelectedNode.FullPath;
                string[] fullPath = path.Split('\\');
                if (e.Node.Level == 0)
                {
                    projectName = fullPath[0];
                    PersonImportWindowSys.Instance.LoadingProjectAttribute(projectName, ref projectID, txt_Type,
                        txt_RoundCount, txt_TestMethod, txt_projectName, txt_BestScoreMode, txt_FloatType);
                }
                else if (e.Node.Level == 1)
                {
                    projectName = fullPath[0];
                    groupName = fullPath[1];
                    txt_GroupName.Text = groupName;
                    PersonImportWindowSys.Instance.LoadingProjectStudentData(projectID, groupName, ucDataGridView1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTreeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode nodes = uiTreeView1.GetNodeAt(e.X, e.Y);
                if (nodes != null)
                {
                    uiTreeView1.SelectedNode = nodes;
                }

            }
            else if (e.Button == MouseButtons.Right)
            {
                uiContextMenuStrip1.Show(e.Location);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 新建项目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProjectWindow createNewProjectWindow = new CreateNewProjectWindow();
            if (createNewProjectWindow.ShowDialog() == DialogResult.OK)
            {
                string projects = createNewProjectWindow.projectName;
                if (PersonImportWindowSys.Instance.CreateNewProject(projects))
                {
                    UIMessageBox.ShowSuccess("新建成功！！");
                    LoadingProjectView();
                }
                else
                {
                    UIMessageBox.ShowError("新建失败！！");
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除项目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                UIMessageBox.ShowWarning("请先选择你需要删除的项目数据！！");
                return;
            }
            else
            {
                if (MessageBox.Show($"是否删除当前 {projectName}项目？", "Warning", MessageBoxButtons.OKCancel) ==
                    DialogResult.OK)
                {
                    if (PersonImportWindowSys.Instance.DeleteCurrentProject(projectName))
                    {
                        UIMessageBox.ShowSuccess("删除成功！！");
                        LoadingProjectView();
                    }
                    else
                    {
                        UIMessageBox.ShowError("删除失败！！");
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 删除选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count > 0)
            {
                string projectName = txt_projectName.Text.Trim();
                if (!string.IsNullOrEmpty(projectName))
                {
                    if (PersonImportWindowSys.Instance.DeleteCurrentChooseStudentData(ucDataGridView1, projectName))
                    {
                        UIMessageBox.ShowSuccess("删除成功！！");
                        PersonImportWindowSys.Instance.LoadingProjectStudentData(projectID, groupName, ucDataGridView1);
                    }
                    else
                    {
                        UIMessageBox.ShowError("删除失败！！");
                        return;
                    }
                }
                else
                {
                    UIMessageBox.ShowWarning("请先选择项目信息");
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            string groupName = txt_GroupName.Text.Trim();
            if (!string.IsNullOrEmpty(groupName))
            {
                string projectName = txt_projectName.Text.Trim();
                if (!string.IsNullOrEmpty(projectName))
                {
                    if (PersonImportWindowSys.Instance.DeleteCurrentGroupStudent(groupName, projectName))
                    {
                        UIMessageBox.ShowSuccess("删除成功！！");
                        LoadingProjectView();
                        PersonImportWindowSys.Instance.LoadingProjectStudentData(projectID, groupName, ucDataGridView1);
                    }
                    else
                    {
                        UIMessageBox.ShowError("删除失败！！");
                        return;
                    }
                }
                else
                {
                    UIMessageBox.ShowWarning("请先选择项目信息");
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先选择项目信息");
                return;
            }
        }

        /// <summary>
        /// 名单导入按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton3_Click(object sender, EventArgs e)
        {
            string projectName = txt_projectName.Text;
            if (!String.IsNullOrEmpty(projectName))
            {
                if (PersonImportWindowSys.Instance.ShowImportStudentDataWindow(projectName))
                {
                    LoadingProjectView();
                }
                else
                {
                    UIMessageBox.ShowWarning("导入失败！！");
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先确定学生信息");
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton4_Click(object sender, EventArgs e)
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
                UIMessageBox.ShowWarning("导出成功");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton5_Click(object sender, EventArgs e)
        {
            try
            {
                string name0 = uiTreeView1.SelectedNode.Text;
                string Name = txt_projectName.Text;
                int Type = txt_Type.SelectedIndex;
                int RoundCount = txt_RoundCount.SelectedIndex;
                int BestScoreMode = txt_BestScoreMode.SelectedIndex;
                int TestMethod = txt_TestMethod.SelectedIndex;
                int FloatType = txt_FloatType.SelectedIndex;
                if (PersonImportWindowSys.Instance.SaveCurrentProjectSetting(name0, Name, Type, RoundCount,
                        BestScoreMode, TestMethod, FloatType))
                {
                    UIMessageBox.ShowSuccess("保存成功！！");
                    LoadingProjectView();
                }
                else
                {
                    UIMessageBox.ShowError("保存失败！！");
                    return;
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
    }
}
