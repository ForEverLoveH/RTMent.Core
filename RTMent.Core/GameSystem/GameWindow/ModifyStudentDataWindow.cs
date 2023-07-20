using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameWindowSys;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class ModifyStudentDataWindow : Form
    {
        
        public string projectName { set; get; }
        public string groupName { set; get; }
        public string IdNumber { set; get; }
        public string stuName { set; get; }
        public string projectId { set; get; }
        public int CurrentRound { get; internal set; }

        public string personId;
        /// <summary>
        /// 轮次
        /// </summary>
        public int roundId { set; get; }

        public string score;

        public string State;
        public int iState;


        public ModifyStudentDataWindow()
        {
            InitializeComponent();
        }  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyStudentDataWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            ModifyStudentDataWindowSys.Instance.LoadingInitData(projectId, IdNumber, CurrentRound, ref personId, ref score, ref State);
            uiTextBox1.Text = projectName;
            uiTextBox2.Text=groupName;
            uiTextBox3.Text=stuName; 
            uiTextBox4.Text=IdNumber    ;
            uiTextBox5.Text=$"第{CurrentRound}轮";
            if (string.IsNullOrEmpty(score)) score = "0.0";
            uiTextBox6.Text = score;
            int.TryParse(State, out int iState0);
            iState = iState0;
            comboBox2.SelectedIndex = iState;

        }

        private void uiTextBox6_TextChanged(object sender, EventArgs e)
        {
            score = uiTextBox6.Text;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            iState = comboBox2.SelectedIndex;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(score))
            {
                ModifyStudentDataWindowSys.Instance.SetModifyData(score, personId, iState);
                DialogResult = DialogResult.OK;
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
