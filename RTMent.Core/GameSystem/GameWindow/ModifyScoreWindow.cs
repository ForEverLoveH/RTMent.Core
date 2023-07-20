using System;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.GameWindow
{
    public partial class ModifyScoreWindow : Form
    {
        public ModifyScoreWindow()
        {
            InitializeComponent();
        }
        public string projectName { set; get; }
        public string groupName { set; get; }
        public string Name { set; get; }
        public string IdNumber { set; get; }
        public string status { set; get; }
        public int rountid { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyScoreWindow_Load(object sender, System.EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            uiTextBox1.Text=projectName; 
            uiTextBox2.Text=groupName;
            uiTextBox3.Text=Name; 
            uiTextBox5.Text=IdNumber;
            uiComboBox1.Items.Clear();
            for (int i = 0; i < rountid; i++)
            {
                uiComboBox1.Items.Add($"第{i + 1}轮");
            }
            if (uiComboBox2.Items.Contains(status))
            {
                uiComboBox2.SelectedIndex = uiComboBox2.Items.IndexOf(status);
            }
        }
        public  int updaterountId = 0;
        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiComboBox1.SelectedIndex != -1) 
            {
                updaterountId = uiComboBox1.SelectedIndex+1;
            }
        }
        public double updateScore =0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTextBox4_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(uiTextBox4.Text, out updateScore);
        }

        private void uiComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            status = uiComboBox2.Text.ToString();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}