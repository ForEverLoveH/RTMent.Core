using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class TreeViewHelper : Singleton<TreeViewHelper>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="listView1"></param>
        /// <param name="projectId"></param>
        public void LoadingCurrentStudentData(SQLiteHelper helper, string projectName, string groupName, ListView listView1, ref string projectId)
        {
            try
            {
                listView1.Items.Clear();
                string sql = $"SELECT b.Id,b.RoundCount,b.FloatType,b.Type " +$"FROM DbGroupInfos AS a,SportProjectInfos AS b WHERE a.ProjectId=b.Id AND a.Name='{groupName}' AND b.Name='{projectName}'";
                var sl = helper.ExecuteReader(sql);
                int roundCount = 0;
                int floatType = 0;
                int type0=0;  
                while(sl.Read())
                {
                    projectId=sl.GetValue(0).ToString();
                    roundCount = sl.GetInt16(1);
                    floatType= sl.GetInt16(2);
                    type0= sl.GetInt16(3);
                }
                sql = $"SELECT dpi.GroupName,dpi.Name,dpi.IdNumber,dpi.State,dpi.FinalScore,dpi.Id" + $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{groupName}' AND dpi.ProjectId='{projectId}'";
                sl = helper.ExecuteReader(sql);
                int index = 1;
                listView1.BeginUpdate();
                InitListViewHeader(roundCount, listView1);
                listView1.Items.Clear ();
                while (sl.Read())
                {
                    string num = sl.GetString(2);
                    int state = sl.GetInt16(3);
                    string personID = sl.GetValue(5).ToString();
                    ListViewItem listViewItem = new ListViewItem()
                    {
                        UseItemStyleForSubItems = false,
                        Text = index.ToString(),
                    };
                    listViewItem.SubItems.Add(projectName);
                    listViewItem.SubItems.Add(sl.GetString(0));
                    listViewItem.SubItems.Add(sl.GetString(1));
                    listViewItem.SubItems.Add(num);
                    if (state == 1)
                    {
                        listViewItem.SubItems.Add("已测试");
                        listViewItem.SubItems[listViewItem.SubItems.Count - 1].BackColor = Color.MediumSpringGreen;
                    }
                    else
                        listViewItem.SubItems.Add("未测试");
                    double maxScore = 1000;
                    sql = $"SELECT SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{personID}'";
                    var res = helper.ExecuteReaderList(sql);
                    if (res.Count > 0)
                    {
                        listViewItem.SubItems.Add("已测试");
                        listViewItem.SubItems[listViewItem.SubItems.Count - 1].BackColor = Color.MediumSpringGreen;
                    }
                    else
                    {
                        listViewItem.SubItems.Add("未测试");
                        listViewItem.SubItems[listViewItem.SubItems.Count - 1].BackColor = Color.Red;
                    }
                    int k = 0;
                    List<double> list = new List<double>();
                    foreach (var dic in res)
                    {
                        int.TryParse(dic["RoundId"], out int RoundId);
                        double.TryParse(dic["Result"], out double Result);
                        string resultStr = ResultStateHelper.Instance.ResultState2Str(dic["State"]);
                        if (resultStr == "已测试")
                        {
                            if (maxScore > Result)
                            {
                                maxScore = Result;
                            }
                            resultStr = decimal.Round(decimal.Parse(Result.ToString("0.0000")), floatType).ToString();
                            listViewItem.SubItems.Add(resultStr);
                        }
                        else
                        {
                            listViewItem.SubItems.Add(resultStr);
                            listViewItem.SubItems[listViewItem.SubItems.Count - 1].ForeColor = Color.Red;
                        }
                        listViewItem.SubItems[listViewItem.SubItems.Count - 1].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);

                        if (dic["uploadState"] == "0")
                        {
                            listViewItem.SubItems.Add("未上传");
                            listViewItem.SubItems[listViewItem.SubItems.Count - 1].ForeColor = Color.Red;
                        }
                        else
                        {
                            listViewItem.SubItems.Add("已上传");
                            listViewItem.SubItems[listViewItem.SubItems.Count - 1].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
                            listViewItem.SubItems[listViewItem.SubItems.Count - 1].ForeColor = Color.Green;
                        }
                        k++;
                    }
                    for (int j = k; j < roundCount; j++)
                    {
                        listViewItem.SubItems.Add("无成绩");
                        listViewItem.SubItems.Add("未上传");
                    }
                    if (maxScore != 1000)
                    {
                        if (list.Count > 0)
                        {
                            for (int j = 0; j < list.Count - 1; j++)
                            {
                                var s = (int)list[j];
                                var p = (int)list[j + 1];
                                if (s > p)
                                {
                                    maxScore = list[j];
                                }
                                else
                                {
                                    maxScore = list[j + 1];
                                }
                            }
                        }
                        listViewItem.SubItems.Add(decimal.Round(decimal.Parse(maxScore.ToString("0.0000")), floatType).ToString());
                        listViewItem.SubItems[listViewItem.SubItems.Count - 1].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
                    }
                    else
                    {
                        listViewItem.SubItems.Add("无成绩");
                    }
                    listView1.Items.Insert(listView1.Items.Count, listViewItem);
                    index++;
                }
                //自动列宽
                AutoResizeColumnWidth(listView1);
                listView1.EndUpdate();
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lv"></param>
        public void AutoResizeColumnWidth(ListView lv)
        {
            int allWidth = lv.Width;
            int count = lv.Columns.Count;
            int MaxWidth = 0;
            Graphics graphics = lv.CreateGraphics();
            int width;
            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            if (count == 0) return;
            for (int i = 0; i < count; i++)
            {
                string str = lv.Columns[i].Text;
                //MaxWidth = lv.Columns[i].Width;
                MaxWidth = 0;

                foreach (ListViewItem item in lv.Items)
                {
                    try
                    {
                        str = item.SubItems[i].Text;
                        width = (int)graphics.MeasureString(str, lv.Font).Width;
                        if (width > MaxWidth)
                        {
                            MaxWidth = width;
                        }
                    }
                    catch (Exception)
                    {

                        break;
                    }

                }

                lv.Columns[i].Width = MaxWidth;
                allWidth -= MaxWidth;
            }

            if (allWidth > count && count != 0)
            {
                allWidth /= count;
                for (int i = 0; i < count; i++)
                {
                    lv.Columns[i].Width += allWidth;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roundCount"></param>
        /// <param name="listView1"></param>
        private void InitListViewHeader(int roundCount, ListView listView1)
        {
            listView1.View = View.Details;
            ColumnHeader[] Header = new ColumnHeader[100];
            int sp = 0;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "序号";
            Header[sp].Width = 40;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "项目名称";
            Header[sp].Width = 80;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "组别名称";
            Header[sp].Width = 40;

            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "姓名";
            Header[sp].Width = 100;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "准考证号";
            Header[sp].Width = 100;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "考试状态";
            Header[sp].Width = 40;
            sp++;
            for (int i = 1; i <= roundCount; i++)
            {
                Header[sp] = new ColumnHeader();
                Header[sp].Text = $"第{i}轮";
                Header[sp].Width = 40;
                sp++;

                Header[sp] = new ColumnHeader();
                Header[sp].Text = $"上传状态";
                Header[sp].Width = 80;
                sp++;
            }

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "最好成绩";
            Header[sp].Width = 60;
            sp++;

            ColumnHeader[] Header1 = new ColumnHeader[sp];
            listView1.Columns.Clear();
            for (int i = 0; i < Header1.Length; i++)
            {
                Header1[i] = Header[i];
            }
            listView1.Columns.AddRange(Header1);
        }
    }
    
}
