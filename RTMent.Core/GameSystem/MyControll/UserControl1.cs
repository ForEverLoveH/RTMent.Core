using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.MyControll
{
    public delegate void StateSwitch();
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }
        public StateSwitch StateSwitchCallback;

        [Description("标题"), Category("自定义属性")]
        public string panel_title
        {
            get
            {
                return this.title.Text;
            }
            set
            {
                this.title.Text = value;
            }
        }

        [Description("姓名"), Category("自定义属性")]
        public string panel_name
        {
            get
            {
                return this.NameValue.Text;
            }
            set
            {
                this.NameValue.Text = value;
            }
        }

        [Description("考号"), Category("自定义属性")]
        public string panel_idNumber
        {
            get
            {
                return this.IdNumberValue.Text;
            }
            set
            {
                this.IdNumberValue.Text = value;
            }
        }

        [Description("成绩"), Category("自定义属性")]
        public string panel_Score
        {
            get
            {
                return this.scoreValue.Text;
            }
            set
            {
                this.scoreValue.Text = value;
            }
        }

        [Description("状态"), Category("自定义属性")]
        public int panel_status
        {
            get
            {
                foreach (RadioButton r in panel4.Controls)
                {
                    if (r.Checked)
                    {
                        return r.TabIndex;
                        break;
                    }
                }
                return -1;
            }
            set
            {
                foreach (RadioButton r in panel4.Controls)
                {
                    if (r.TabIndex == value)
                    {
                        r.Checked = true;
                        break;
                    }
                }
                //radioButton1.Checked = true;
            }
        }

        [Description("标题颜色"), Category("自定义属性")]
        public Color Panel_title_Color
        {
            set
            {
                this.title.ForeColor = value;
            }
        }

        [Description("预备图标"), Category("自定义属性")]
        public bool Panel_Ready
        {
            set
            {
                pictureBox1.Visible = value;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                StateSwitchCallback?.Invoke();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
                StateSwitchCallback?.Invoke();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                StateSwitchCallback?.Invoke();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                StateSwitchCallback?.Invoke();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                StateSwitchCallback?.Invoke();
        }
    }
}
