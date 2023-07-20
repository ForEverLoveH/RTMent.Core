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

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class CreateNewProjectWindow : Form
    {
        public CreateNewProjectWindow()
        {
            InitializeComponent();
        }

        private void CreateNewProjectWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

        }

         public string projectName = "";
        private void uiButton1_Click(object sender, EventArgs e)
        {
            projectName= uiTextBox1.Text.Trim();
            if (!string.IsNullOrEmpty(projectName))
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                UIMessageBox.ShowWarning("请先输入你需要新建项目的项目名！！");
                return;
            }
        }
    }
}
