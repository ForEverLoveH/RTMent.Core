using HZH_Controls;
using HZH_Controls.Controls;
using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindow;
using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Web.UI;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Xml.Linq;
using System.Xml.Schema;
using static System.Windows.Forms.AxHost;

namespace RTMent.Core.GameSystem.GameWindowSys
{
    public class RunningTestingWindowSys : Singleton<RunningTestingWindowSys>
    {
        SQLiteHelper helper;
        RunningTestingWindow RunningTestingWindow = null;
        public bool ShowRunningTestingWindow(SQLiteHelper helper, string[] fsp, Dictionary<string, string> dic)
        {
            this.helper = helper;
            RunningTestingWindow = new RunningTestingWindow();
            RunningTestingWindow.ProjectName = fsp[0];
            if(fsp.Length > 1 )
                RunningTestingWindow.GroupName= fsp[1];
            RunningTestingWindow.ProjectID = dic["Id"];
            RunningTestingWindow.Type = dic["Type"];
            RunningTestingWindow.RoundCount = Convert.ToInt32(dic["RoundCount"]);
            RunningTestingWindow.BestScoreMode = Convert.ToInt32(dic["BestScoreMode"]);
            RunningTestingWindow.TestMethod = Convert.ToInt32(dic["TestMethod"]);
            RunningTestingWindow.FloatType = Convert.ToInt32(dic["FloatType"]);
            RunningTestingWindow.FormTitle = string.Format("考试项目:{0}", fsp[0]);
            if( RunningTestingWindow.ShowDialog()==System.Windows.Forms.DialogResult.OK )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public  string[] GetPortDeviceName(string name)
        {
            List<string> strs = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        if (deviceName.Contains(name) || deviceName.Contains("Prolific"))
                        {
                            strs.Add(deviceName);
                        }
                    }
                }
            }
            return strs.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public string PortName2Port(string deviceName)
        {
            string str = "";
            try
            {
                int a = deviceName.IndexOf("(COM") + 1;//a会等于1
                str = deviceName.Substring(a, deviceName.Length - a);
                a = str.IndexOf(")");//a会等于1
                str = str.Substring(0, a);
            }
            catch (Exception ex)
            {
                str = "";
                LoggerHelper.Debug(ex);
            }

            return str;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupCbx"></param>  
        public void UpDateGroupCbx(UIComboBox groupCbx,string _ProjectId, ref string GroupName,string groupname ="")
        {
            groupCbx.Items.Clear();
            groupCbx.Text = string.Empty;
            try
            {
                string sql = $"SELECT Name FROM DbGroupInfos WHERE Name LIKE'%{groupname}%' AND ProjectId='{_ProjectId}'";
                var ds = helper.ExecuteReader(sql);
                while (ds.Read())
                {
                    string groupName = ds.GetString(0);
                    groupCbx.Items.Add(groupName);
                }
                if(string.IsNullOrEmpty(GroupName)&&groupCbx.Items.Count>0)
                {
                    GroupName = groupCbx.Items[0].ToString();
                }
                else
                {
                    int index = groupCbx.Items.IndexOf(GroupName);
                    if (index>=0)
                    {
                        groupCbx.SelectedIndex=index;
                    }
                }
               
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="groupName"></param>
        /// <param name="round"></param>
        /// <param name="uiDataGridView1"></param>
        /// <param name="uiLabel1"></param>
        public void UpDateStudentListView(string projectID, string groupName, int round ,UIDataGridView uiDataGridView1, UILabel uiLabel1)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                try
                {
                    string sql = $"SELECT Id,Name,IdNumber FROM DbPersonInfos WHERE ProjectId='{projectID}' and GroupName='{groupName}'";
                    var ds = helper.ExecuteReaderList(sql);
                    List<DataGridViewRow> rows = new List<DataGridViewRow>();
                    int index = 1;
                    int finish = 0;
                    int max = 0;
                    if (ds.Count > 0)
                    {
                        max = ds.Count;
                        foreach (var dic in ds)
                        {
                            string id = dic["Id"]; 
                            string name = dic["Name"];
                            string idNum = dic["IdNumber"];
                            DataGridViewRow data = new DataGridViewRow();
                            data.Cells.Add(GetNewDataGridViewCell(index, Color.Black, Color.White));
                            data.Cells.Add(GetNewDataGridViewCell(name, Color.Black, Color.White));
                            data.Cells.Add(GetNewDataGridViewCell(idNum, Color.Black, Color.White));
                            data.Cells.Add(GetNewDataGridViewCell($"第{round}轮", Color.Black, Color.White));
                            sql = $"SELECT PersonName,Result,State,uploadState  FROM ResultInfos WHERE PersonId='{id}' AND RoundId={round}";
                            var sl = helper.ExecuteReaderList(sql);
                            if ( sl.Count > 0)
                            {
                                foreach (var po in sl)
                                {
                                    string person = po["PersonName"];
                                    double res = double.Parse(po["Result"]);
                                    int state = int.Parse(po["State"]);
                                    int uploadState = int.Parse(po["uploadState"]);
                                    if (state > 1)
                                    {
                                        string st = ResultStateHelper.Instance.ResultState2Str(state);
                                        data.Cells.Add(GetNewDataGridViewCell(st, Color.Red, Color.White));
                                    }
                                    else
                                    {
                                        data.Cells.Add(GetNewDataGridViewCell(res, Color.Black, Color.White));
                                    }
                                    if (uploadState == 1)
                                        data.Cells.Add(GetNewDataGridViewCell("已上传", Color.Green, Color.White));
                                    else
                                    {
                                        data.Cells.Add(GetNewDataGridViewCell("未上传", Color.Red, Color.White));
                                    }
                                    finish++;
                                }
                            }
                            else
                            {
                                data.Cells.Add(GetNewDataGridViewCell("无成绩", Color.Black, Color.White));
                                //dgr.Cells.Add(getNewDataGridViewCell(idNumber, Color.Black, Color.White));
                                data.Cells.Add(GetNewDataGridViewCell("未上传", Color.Red, Color.White));
                                //finish++;
                            }
                            data.Cells.Add(GetNewDataGridViewCell(id, Color.Black, Color.White));
                            rows.Add(data);
                            index++;
                        }

                    }
                    if (rows.Count > 0)
                    {
                        DataGridViewRow[] dats = new DataGridViewRow[rows.Count ];
                        for (int i = 0; i < rows.Count; i++)
                        {
                            dats[i ] = rows[i];
                        }
                        if (dats.Length > 0)
                        {
                            uiDataGridView1.Rows.Clear();
                            uiDataGridView1.Rows.AddRange(dats);
                            int noNum = max - finish;
                            noNum = noNum < 0 ? 0 : noNum;
                            uiLabel1.Text = string.Format("当前组{0}还有{1}人未完成", groupName, noNum);
                            if (noNum > 0)
                            {
                                uiLabel1.ForeColor = Color.Red;
                            }
                            else { uiLabel1.ForeColor = Color.Black; }

                        }
                    }

                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("选择组信息为空！！");
                return;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="foreColor"></param>
        /// <param name="backColor"></param>
        /// <returns></returns>
        public DataGridViewTextBoxCell GetNewDataGridViewCell(object value, Color foreColor, Color backColor)
        {
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            cell.Value = value.ToString();
            cell.Style.ForeColor = foreColor;
            cell.Style.BackColor = backColor;
            return cell;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="groupName"></param>
        /// <param name="currentRound"></param>
        /// <param name="equipMentNum"></param>
        /// <param name="uiDataGridView1"></param>
        /// <param name="studentDatas"></param>
        public void LoadingTestingStudentData(string projectID, string groupName, int currentRound, int equipMentNum, UIDataGridView uiDataGridView1, ref List<TestingStudentData> studentDatas,ref List<int>choose)
        {
            try
            {
                string sql = $"SELECT Id,Name,IdNumber FROM DbPersonInfos WHERE ProjectId='{projectID}' and GroupName='{groupName}'";
                var ds = helper.ExecuteReaderList(sql);
                List<DataGridViewRow> rows = new List<DataGridViewRow>();
                int index = 1;
               // int finish = 0;
                int max = 0;
                if (ds.Count > 0)
                {
                    max = ds.Count;
                    foreach (var dic in ds)
                    {
                        string id = dic["Id"];
                        string name = dic["Name"];
                        string idNum = dic["IdNumber"];
                        DataGridViewRow data = new DataGridViewRow();
                        data.Cells.Add(GetNewDataGridViewCell(index, Color.Black, Color.White));
                        data.Cells.Add(GetNewDataGridViewCell(name, Color.Black, Color.White));
                        data.Cells.Add(GetNewDataGridViewCell(idNum, Color.Black, Color.White));
                        data.Cells.Add(GetNewDataGridViewCell($"第{currentRound}轮", Color.Black, Color.White));
                        sql = $"SELECT PersonName,Result,State,uploadState  FROM ResultInfos WHERE PersonId='{id}' AND RoundId={currentRound}";
                        var sl = helper.ExecuteReaderList(sql);
                        if (sl.Count > 0)
                        {
                            foreach (var po in sl)
                            {
                                string person = po["PersonName"];
                                double res = double.Parse(po["Result"]);
                                int state = int.Parse(po["State"]);
                                int uploadState = int.Parse(po["uploadState"]);
                                if (state > 1)
                                {
                                    string st = ResultStateHelper.Instance.ResultState2Str(state);
                                    data.Cells.Add(GetNewDataGridViewCell(st, Color.Red, Color.White));
                                }
                                else
                                {
                                    data.Cells.Add(GetNewDataGridViewCell(res, Color.Black, Color.White));
                                }
                                if (uploadState == 1)
                                    data.Cells.Add(GetNewDataGridViewCell("已上传", Color.Green, Color.White));
                                else
                                {
                                    data.Cells.Add(GetNewDataGridViewCell("未上传", Color.Red, Color.White));
                                }
                                //finish++;
                            }
                        }
                        else
                        {
                            data.Cells.Add(GetNewDataGridViewCell("无成绩", Color.Black, Color.White));
                            //dgr.Cells.Add(getNewDataGridViewCell(idNumber, Color.Black, Color.White));
                            data.Cells.Add(GetNewDataGridViewCell("未上传", Color.Red, Color.White));
                            if(studentDatas.Count < equipMentNum)
                            {
                                studentDatas.Add(new TestingStudentData
                                {
                                    RaceStudentDataId = index,
                                    id = id,
                                    name = name,
                                    idNumber = idNum,
                                    score = 0,
                                    state = 0,
                                    RoundId = currentRound,
                                });
                                choose.Add(index-1);
                            }
                        }
                        data.Cells.Add(GetNewDataGridViewCell(id, Color.Black, Color.White));
                        rows.Add(data);
                        index++;
                    }

                }
                if (rows.Count > 0)
                {
                    DataGridViewRow[] dats = new DataGridViewRow[rows.Count];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        dats[i] = rows[i];
                    }
                    uiDataGridView1.Rows.Clear();
                    uiDataGridView1.Rows.AddRange(dats);

                }

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
        /// <param name="idNumber"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="currentCound"></param>
        /// <returns></returns>
        public bool SetErrorState(string idNumber, string name, string id, int state,int currentCound)
        {
            try
            {
                if (state != 6)
                {
                    string sql = $"UPDATE DbPersonInfos SET State=1,FinalScore=1 WHERE Id='{id}'";
                    int result = helper.ExecuteNonQuery(sql);
                    sql = $"UPDATE ResultInfos SET State={state} WHERE PersonId='{id}' AND RoundId={currentCound} AND IsRemoved=0";
                    result = helper.ExecuteNonQuery(sql);

                    if (result == 0)
                    {
                        var sortid = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                        string sortid0 = "1";
                        if (sortid != null && sortid.ToString() != "") sortid0 = sortid.ToString();

                        sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                                 $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {sortid0}, 0, '{id}',0,'{name}','{idNumber}',{currentCound},{0},{state})";
                        //处理写入数据库
                        helper.ExecuteNonQuery(sql);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    string sql = $"DELETE FROM ResultInfos WHERE PersonIdNumber='{idNumber}' AND PersonId='{id}' And RoundId='{currentCound}'";
                    if (helper.ExecuteNonQuery(sql) == 1)
                        return true;
                    else
                        return false;
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug  (ex); return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentDatas"></param>
        /// <param name="currentRound"></param>
        public void UpDataCurrentStudentDataScore(List<TestingStudentData> studentDatas, int currentRound)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                System.Data.SQLite.SQLiteTransaction sQLiteTransaction = helper.BeginTransaction();
                foreach(TestingStudentData studentData in studentDatas)
                {
                    string sql = $"select Result from ResultInfos where PersonId='{studentData.id}' and RoundId='{currentRound}' and IsRemoved=0";
                    var list = helper.ExecuteReaderList(sql);
                    int state=studentData.state==0?1:studentData.state;
                    if (list.Count == 0)
                    {
                        var sort = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                        string po = "";
                        if(sort!=null && sort.ToString() !="")po= sort.ToString();
                        sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                          $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {po}, 0, '{studentData.id}',0,'{studentData.name}','{studentData.idNumber}',{currentRound},{studentData.score},{state})";
                        helper.ExecuteNonQuery (sql);
                        helper.ExecuteNonQuery($"UPDATE DbPersonInfos SET State = 1, FinalScore = 1 WHERE Id = '{studentData.id}'");

                    }
                    else
                        sb.Append($"{studentData.idNumber},{studentData.name} 本轮已测试\n");
                }
                helper.CommitTransaction(sQLiteTransaction);
                if(sb.Length > 0)
                {
                    //MessageBox.Show(sb.ToString());
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex); return;
            }
        }
        /// <summary>
        /// 上传成绩
        /// </summary>
        /// <param name="fsp"></param>
        /// <returns></returns>
        public string UpLoadStudentDataScore(Object obj, int currentRound)
        {
            try
            {
                List<Dictionary<string, string>> successList = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> errorList = new List<Dictionary<string, string>>();
                Dictionary<string, string> localInfos = new Dictionary<string, string>();
                List<Dictionary<string, string>> list0 = helper.ExecuteReaderList("SELECT * FROM localInfos");
                foreach (var item in list0)
                {
                    localInfos.Add(item["key"], item["value"]);
                }
                string[] fusp = obj as string[];
                ///项目名称
                string projectName = string.Empty;
                //组
                string groupName = string.Empty;
                if (fusp.Length > 0)
                    projectName = fusp[0];
                if (fusp.Length > 1)
                    groupName = fusp[1];
                Dictionary<string, string> SportProjectDic = helper.ExecuteReaderOne($"SELECT Id,Type,RoundCount,BestScoreMode,TestMethod," +
                         $"FloatType,TurnsNumber0,TurnsNumber1 FROM SportProjectInfos WHERE Name='{projectName}'");
                string sql0 = $"SELECT Id,ProjectId,Name FROM DbGroupInfos WHERE ProjectId='{SportProjectDic["Id"]}'";
                ///查询本项目已考组
                if (!string.IsNullOrEmpty(groupName))
                {
                    sql0 += $" AND Name = '{groupName}'";
                }
                List<Dictionary<string, string>> sqlGroupsResults = helper.ExecuteReaderList(sql0);
                UploadResultsRequestParameter urrp = new UploadResultsRequestParameter();
                urrp.AdminUserName = localInfos["AdminUserName"];
                urrp.TestManUserName = localInfos["TestManUserName"];
                urrp.TestManPassword = localInfos["TestManPassword"];
                string MachineCode = localInfos["MachineCode"];
                string ExamId = localInfos["ExamId"];
                if (ExamId.IndexOf('_') != -1)
                {
                    ExamId = ExamId.Substring(ExamId.IndexOf('_') + 1);
                }
                urrp.ExamId = ExamId;
                if (MachineCode.IndexOf('_') != -1)
                {
                    MachineCode = MachineCode.Substring(MachineCode.IndexOf('_') + 1);
                }
                StringBuilder messageSb = new StringBuilder();
                StringBuilder logWirte = new StringBuilder();
                ///按组上传
                foreach (var sqlGroupsResult in sqlGroupsResults)
                {
                    string sql = $"SELECT Id,GroupName,Name,IdNumber,SchoolName,GradeName,ClassNumber,State,Sex,BeginTime,FinalScore,uploadState FROM DbPersonInfos" +
                        $" WHERE ProjectId='{SportProjectDic["Id"]}' AND GroupName = '{sqlGroupsResult["Name"]}'";
                    List<Dictionary<string, string>> list = helper.ExecuteReaderList(sql);
                    //轮次
                    urrp.MachineCode = MachineCode;
                    if (list.Count == 0)
                        continue;
                    List<SudentsItem> sudentsItems = new List<SudentsItem>();
                    StringBuilder resultSb = new StringBuilder();
                    //IdNumber 对应Id
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    for (int i = 1; i <=currentRound; i++)
                    {
                        foreach (var stu in list)
                        {
                            ///成绩
                            List<Dictionary<string, string>> resultScoreList1 = helper.ExecuteReaderList(
                                $"SELECT Id,CreateTime,RoundId,State,uploadState,Result FROM ResultInfos WHERE PersonId='{stu["Id"]}' And IsRemoved=0 And RoundId={i} LIMIT 1");
                            #region 查询文件
                            //成绩根目录
                            Dictionary<string, string> dic_images = new Dictionary<string, string>();
                            Dictionary<string, string> dic_viedos = new Dictionary<string, string>();
                            Dictionary<string, string> dic_texts = new Dictionary<string, string>();
                            //string scoreRoot = Application.StartupPath + $"\\Scores\\{projectName}\\{stu["GroupName"]}\\";
                            #endregion 
                            List<RoundsItem> roundsItems = new List<RoundsItem>();
                            foreach (var item in resultScoreList1)
                            {
                                DateTime.TryParse(item["CreateTime"], out DateTime dtBeginTime);
                                string dateStr = dtBeginTime.ToString("yyyyMMdd");
                                string GroupNo = $"{dateStr}_{stu["GroupName"]}_{stu["IdNumber"]}_{item["RoundId"]}";
                                //轮次成绩
                                RoundsItem rdi = new RoundsItem();
                                rdi.RoundId = Convert.ToInt32(item["RoundId"]);
                                rdi.State = ResultStateHelper.Instance.ResultState2Str(item["State"]);
                                rdi.Time = item["CreateTime"];
                                double.TryParse(item["Result"], out double score);
                                int.TryParse(item["State"], out int stateInt32);
                                if (stateInt32 > 1)
                                    score = 0;
                                rdi.Result = score;
                                //string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                                rdi.GroupNo = GroupNo;
                                rdi.Text = dic_texts;
                                rdi.Images = dic_images;
                                rdi.Videos = dic_viedos;
                                roundsItems.Add(rdi);
                            }
                            if (roundsItems.Count > 0)
                            {
                                SudentsItem ssi = new SudentsItem();
                                ssi.SchoolName = stu["SchoolName"];
                                ssi.GradeName = stu["GradeName"];
                                ssi.ClassNumber = stu["ClassNumber"];
                                ssi.Name = stu["Name"];
                                ssi.IdNumber = stu["IdNumber"];
                                ssi.Rounds = roundsItems;
                                if (roundsItems.Count > 0)
                                {
                                    sudentsItems.Add(ssi);
                                    if (!map.Keys.Contains(stu["IdNumber"]))
                                    {
                                        map.Add(stu["IdNumber"], stu["Id"]);
                                    }
                                }
                            }
                        }
                    }
                    #region 上传数据包装
                    if (sudentsItems.Count == 0) continue;
                    urrp.Sudents = sudentsItems;
                    //序列化json
                    string JsonStr = JsonDataHelper.Instance.SerializeObject(urrp);
                    string url = localInfos["Platform"] + RequestUrl.UploadResults;   
                    var formDatas = new List<FormItemModel>();
                    //添加其他字段
                    formDatas.Add(new FormItemModel()
                    {
                        Key = "data",
                        Value = JsonStr
                    });
                    logWirte.AppendLine();
                    logWirte.AppendLine();
                    logWirte.AppendLine(JsonStr);
                    //上传学生成绩
                    string result = HttpUpLoad.Instance.PostForm(url, formDatas);
                    Upload_Result upload_Result =  JsonDataHelper.Instance.DeserializeObject<Upload_Result>(result);
                    string errorStr = "null";
                    List<Dictionary<string, int>> result1 = upload_Result.Result;
                    foreach (var item in sudentsItems)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        //map
                        dic.Add("Id", map[item.IdNumber]);
                        dic.Add("IdNumber", item.IdNumber);
                        dic.Add("Name", item.Name);
                        dic.Add("uploadGroup", item.Rounds[0].GroupNo);
                        dic.Add("RoundId", item.Rounds[0].RoundId.ToString());
                        // dic.Add("uploadGroup", item.Rounds[0].GroupNo);
                        var value = 0;
                        result1.Find(a => a.TryGetValue(item.IdNumber, out value));
                        if (value == 1 || value == -4)
                        {
                            successList.Add(dic);
                        }
                        else if (value != 0)
                        {
                            errorStr = UploadResult.Match(value);
                            dic.Add("error", errorStr);
                            errorList.Add(dic);
                            messageSb.AppendLine($"{sqlGroupsResult["Name"]}组 考号:{item.IdNumber} 姓名:{item.Name}, 第{item.Rounds[0].RoundId}轮错误内容:{errorStr}");
                        }
                    }
                    #endregion 上传数据包装
                }
                LoggerHelper.Info(logWirte.ToString());
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"成功:{successList.Count},失败:{errorList.Count}");
                sb.AppendLine("****************error***********************");
                foreach (var item in errorList)
                {
                    sb.AppendLine($"考号:{item["IdNumber"]} 姓名:{item["Name"]} 错误:{item["error"]}");
                }
                sb.AppendLine("*****************error**********************");
                System.Data.SQLite.SQLiteTransaction sQLiteTransaction = helper.BeginTransaction();
                sb.AppendLine("******************success*********************");
                foreach (var item in successList)
                {
                    //更新成绩上传状态
                    string sql1 = $"UPDATE ResultInfos SET uploadState=1 WHERE PersonId={item["Id"]} and RoundId={item["RoundId"]}";
                    helper.ExecuteNonQuery(sql1);
                    sb.AppendLine($"考号:{item["IdNumber"]} 姓名:{item["Name"]}");
                }
                helper.CommitTransaction(sQLiteTransaction);
                sb.AppendLine("*******************success********************");
                try
                {
                    string txtpath = Application.StartupPath + $"\\Log\\upload\\";
                    if (!Directory.Exists(txtpath))
                    {
                        Directory.CreateDirectory(txtpath);
                    }
                    if (successList.Count != 0 || errorList.Count != 0)
                    {
                        txtpath = Path.Combine(txtpath, $"upload_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");
                        File.WriteAllText(txtpath, sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }

                string outpitMessage = messageSb.ToString();
                return outpitMessage;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="projectID"></param>
        /// <param name="groupName"></param>
        /// <param name="idNumber"></param>
        /// <param name="name"></param>
        /// <param name="currentRound"></param>
        /// <returns></returns>
        public bool ShowModifyStudentDataWindow(string projectName, string projectID, string groupName, string idNumber, string name, int currentRound)
        {
             return   ModifyStudentDataWindowSys.Instance.ShowModifyStudentDataWindow(helper, projectName, projectID, groupName, idNumber, name, currentRound);
        }
        /// <summary>
        /// 修改成绩
        /// </summary>
        /// <param name="score"></param>
        /// <param name="personID"></param>
        /// <param name="state"></param>
        /// <param name="currentRound"></param>
        /// <param name="idNumber"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool FixCurrentChooseStudentGrade(string score, string personID, int state,int currentRound, string idNumber, string name, string id)
        {
            try
            {
                int res = 0;
                if (state > 1)
                {
                    string sql = $"UPDATE ResultInfos SET Result='0',State='{state}' WHERE PersonId='{personID}' AND RoundId='{currentRound}'";
                    res =helper.ExecuteNonQuery(sql);
                }
                else if (state == 1)
                {
                    string sql = $"UPDATE ResultInfos SET Result='{score}' WHERE PersonId='{personID}' AND RoundId='{currentRound}'";
                    res = helper.ExecuteNonQuery(sql);
                }
                if (res== 0)
                {
                    var sortid = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                    string sortid0 = "1";
                    if (sortid != null && sortid.ToString() != "") sortid0 = sortid.ToString();

                    string sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                              $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {sortid0}, 0, '{id}',0,'{name}','{idNumber}',{currentRound},'{score}',{state})";
                    //处理写入数据库
                    res = helper.ExecuteNonQuery(sql);
                }
                if(res>0)
                {
                    File.AppendAllText(@"data/操作日志.txt", $"考号:{idNumber},姓名:{name},修正成绩:第{currentRound}轮成绩为{score}");
                    return true;
                }
                else
                {
                    return false;
                }
                //return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="iDNum"></param>
        /// <param name="roundCount"></param>
        /// <returns></returns>
        public SelectStudentDataModel SelectStudentDataByIDNumber(string projectID, string iDNum )
        {
            try
            {
                List<string> groups = new List<string>();
                SelectStudentDataModel SelectStudentDataModel = null;
                string sql = $"SELECT Name FROM DbGroupInfos WHERE ProjectId='{projectID}'";
                var pl= helper.ExecuteReader(sql);
                while(pl.Read())
                {
                    string groupName = pl.GetString(0);
                    groups.Add(groupName);
                }
                if (groups.Count > 0)
                {
                    foreach (string group in groups)
                    {
                        sql = "Select dpi.id ,dpi.name,dpi.IDNumber,dpi.state  " + $"from DBPersonInfos as dpi where dpi.GroupName='{group}' and dpi.ProjectID='{projectID}' AND dpi.idNumber='{iDNum}'";     
                        var ds = helper.ExecuteReaderList(sql);
                        if (ds!=null&&ds.Count > 0)
                        {
                            foreach (var dic in ds)
                            {
                                string id = dic["Id"];
                                string name = dic["Name"];
                                string idNum = dic["IdNumber"];
                                //string groupName = dic["GroupName"];
                                SelectStudentDataModel = new SelectStudentDataModel()
                                {
                                    Id = id,
                                    Name = name,
                                    idNumber = idNum,
                                    groupName = group,
                                };
                                sql = $"SELECT PersonName,Result,State,uploadState ,roundid FROM ResultInfos WHERE PersonId='{id}'";
                                var sl = helper.ExecuteReaderList(sql);
                                if (sl.Count > 0)
                                {
                                    foreach (var item in sl)
                                    {
                                        string person = item["PersonName"];
                                        double res = double.Parse(item["Result"]);
                                        int state = int.Parse(item["State"]);
                                        int uploadState = int.Parse(item["uploadState"]);
                                        int rounid = int.Parse(item["RoundId"].ToString());
                                        if (sl.Count == 2)
                                        {
                                            if (rounid == 1)
                                            {
                                                if (state != 1)
                                                {
                                                    string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                    SelectStudentDataModel.score = scores;
                                                }
                                                else
                                                {
                                                    SelectStudentDataModel.score = res;

                                                }
                                                SelectStudentDataModel.UpLoadState = uploadState;

                                            }
                                            if (rounid == 2)
                                            {
                                                if (state != 1)
                                                {
                                                    string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                    SelectStudentDataModel.score1 = scores;
                                                }
                                                else
                                                    SelectStudentDataModel.score1 = res;
                                                SelectStudentDataModel.UpLoadState2 = uploadState;
                                            }
                                        }
                                        else
                                        {
                                            if (rounid == 1)
                                            {
                                                if (state != 1)
                                                {
                                                    string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                    SelectStudentDataModel.score = scores;

                                                }
                                                else
                                                {
                                                    SelectStudentDataModel.score = res;

                                                }
                                                SelectStudentDataModel.UpLoadState = uploadState;
                                                SelectStudentDataModel.score1 = "未测试";
                                                SelectStudentDataModel.UpLoadState2 = 0;
                                            }
                                            if (rounid == 2)
                                            {
                                                if (state != 1)
                                                {
                                                    string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                    SelectStudentDataModel.score1 = scores;
                                                }
                                                else
                                                    SelectStudentDataModel.score1 = res;
                                                SelectStudentDataModel.UpLoadState2 = uploadState;
                                                SelectStudentDataModel.score = "未测试";
                                                SelectStudentDataModel.UpLoadState = 0;
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    SelectStudentDataModel.score = "未测试";
                                    SelectStudentDataModel.score1 = "未测试";
                                    SelectStudentDataModel.UpLoadState = 0;
                                    SelectStudentDataModel.UpLoadState2 = 0;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                        
                    }
                }
                else
                {
                    return null;
                }
                return SelectStudentDataModel;
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentDataModels"></param>
        /// <param name="uiDataGridView2"></param>
        public void SetStudentDataInDataView(List<SelectStudentDataModel> studentDataModels, UIDataGridView uiDataGridView1)
        {
            try
            {
               // uiDataGridView2.Rows.Clear();
                int index = 1;
                List<DataGridViewRow> rows = new List<DataGridViewRow>();
                foreach(SelectStudentDataModel model in studentDataModels)
                {
                    DataGridViewRow data= new DataGridViewRow();
                    data.Cells.Add(GetNewDataGridViewCell(index, Color.Black, Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.Name,Color.Black, Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.idNumber, Color.Black, Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.groupName, Color.Black, Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.score, Color.Black, Color.White)); 
                    data.Cells.Add(GetNewDataGridViewCell(model.UpLoadState == 0 ? "未上传" : "已上传" ,Color.Black,Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.score1, Color.Black, Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.UpLoadState2 == 0 ? "未上传" : "已上传", Color.Black, Color.White));
                    data.Cells.Add(GetNewDataGridViewCell(model.Id, Color.Black, Color.White));
                    rows.Add(data);
                    index++;
                }
                if (rows.Count > 0)
                {
                    DataGridViewRow[] dats = new DataGridViewRow[rows.Count];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        dats[i] = rows[i];
                    }
                    uiDataGridView1.Rows.Clear();
                    uiDataGridView1.AllowUserToAddRows = false;
                    uiDataGridView1.Rows.AddRange(dats);

                }

            }
            catch ( Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentDataModels"></param>
        /// <param name="groupName"></param>
        public void SaveDataToChipInfo(List<SelectStudentDataModel> studentDataModels, string groupName)
        {
            try 
            {
                foreach (SelectStudentDataModel model in studentDataModels) 
                {

                    var sort = helper.ExecuteScalar($"SELECT MAX(CHIPSort) + 1 FROM CHIPINFOS");
                    String po=1.ToString();
                    string lp = sort.ToString();
                    if (  !string.IsNullOrEmpty(lp) )
                    {
                        po = lp;
                    }
                    
                    string sql = "INSERT  INTO CHIPINFOS(CHIPNO,GroupName,CHIPSort,personId )"+$"VALUES  ('{model.idNumber}','{groupName}',{po},'{model.Id}')";
                    var res = helper.ExecuteNonQuery(sql);
                    if(res == 1) 
                    {
                        Console.WriteLine(res);
                    }
                    
                }
            }
            catch( Exception ex )
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiComboBox2"></param>
        public void LoadingChipInfos(UIComboBox uiComboBox2)
        {
            try
            {
                List<string > list = new List<string>();
                string sql = "Select GroupName  From chipINFOS";
                var sp=helper.ExecuteReader(sql);
                while (sp.Read())
                {
                    string sl=sp.GetString(0);
                    list.Add(sl);
                }
                if(list.Count > 0)
                {
                    uiComboBox2.Items.Clear();
                    foreach (string s in list)
                    {
                        uiComboBox2.Items.Add(s);
                    }
                    uiComboBox2.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public List<SelectStudentDataModel> LoadingStudentChipData(string groupName)
        {
            try
            {
                List<SelectStudentDataModel> selectStudentDataModels = new List<SelectStudentDataModel>();
                SelectStudentDataModel SelectStudentDataModel =  null;
                string sql = $"Select  dpi.chipNo,dpi.personid From chipINFOS as dpi where dpi.GroupName='{groupName}'";
                var sp=helper.ExecuteReader(sql);
                while (sp.Read())
                {
                    string idNumber= sp.GetString(0);
                    string personid= sp.GetString(1);
                    if(!string.IsNullOrEmpty(idNumber)&&!string.IsNullOrEmpty(personid))
                    {
                        SelectStudentDataModel = new SelectStudentDataModel() 
                        {
                            idNumber = idNumber,
                            Id = personid,
                           
                        };
                        sql = $"Select dpi. name, dpi.groupName from dbpersonInfos as dpi where dpi.id='{personid}' and dpi.idnumber='{idNumber}'";
                        sp = helper.ExecuteReader(sql);
                        while (sp.Read())
                        {
                            string name  = sp.GetString(0);
                            string groupNames= sp.GetString(1);
                            SelectStudentDataModel.Name = name;
                            SelectStudentDataModel.groupName = groupNames;
                            sql = $"SELECT PersonName,Result,State,uploadState ,roundid FROM ResultInfos WHERE PersonId='{personid}'and PersonName='{name}' AND PersonIdNumber='{idNumber}'";
                            var ds = helper.ExecuteReaderList(sql);
                            if(ds.Count > 0)
                            {
                                foreach (var item in ds)
                                {
                                    string person = item["PersonName"];
                                    double res = double.Parse(item["Result"]);
                                    int state = int.Parse(item["State"]);
                                    int uploadState = int.Parse(item["uploadState"]);
                                    int rounid = int.Parse(item["RoundId"].ToString());
                                    if (ds.Count == 2)
                                    {
                                        if (rounid == 1)
                                        {
                                            if (state != 1)
                                            {
                                                string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                SelectStudentDataModel.score = scores;

                                            }
                                            else
                                            {
                                                SelectStudentDataModel.score = res;

                                            }
                                            SelectStudentDataModel.UpLoadState = uploadState;

                                        }
                                        if (rounid == 2)
                                        {
                                            if (state != 1)
                                            {
                                                string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                SelectStudentDataModel.score1 = scores;
                                            }
                                            else
                                                SelectStudentDataModel.score1 = res;
                                            SelectStudentDataModel.UpLoadState2 = uploadState;
                                        }
                                    }
                                    else
                                    {
                                        if (rounid == 1)
                                        {
                                            if (state != 1)
                                            {
                                                string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                SelectStudentDataModel.score = scores;

                                            }
                                            else
                                            {
                                                SelectStudentDataModel.score = res;

                                            }
                                            SelectStudentDataModel.UpLoadState = uploadState;
                                            SelectStudentDataModel.score1 = "未测试";
                                            SelectStudentDataModel.UpLoadState2 = 0;
                                        }
                                        if (rounid == 2)
                                        {
                                            if (state != 1)
                                            {
                                                string scores = ResultStateHelper.Instance.ResultState2Str(state);
                                                SelectStudentDataModel.score1 = scores;
                                            }
                                            else
                                                SelectStudentDataModel.score1 = res;
                                            SelectStudentDataModel.UpLoadState2 = uploadState;
                                            SelectStudentDataModel.score = "未测试";
                                            SelectStudentDataModel.UpLoadState = 0;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                SelectStudentDataModel.score = "未测试";
                                SelectStudentDataModel.score1 = "未测试";
                                SelectStudentDataModel.UpLoadState = 0;
                                SelectStudentDataModel.UpLoadState2 = 0;
                            }
                            
                        }
                        selectStudentDataModels.Add(SelectStudentDataModel);
                    }
                }
                return selectStudentDataModels;
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupNames"></param>
        public void DeleteSelectChipsInfo(string groupNames)
        {
            try
            {
                List<string> list = new List<string>();
                string sql = "Select GroupName  From chipINFOS";
                var sp = helper.ExecuteReader(sql);
                while (sp.Read())
                {
                    string sl = sp.GetString(0);
                    list.Add(sl);
                }
                if(list.Count > 0)
                {
                    sql = $"delete from chipinfos where GroupName='{groupNames}'";
                    helper.ExecuteNonQuery(sql);
                }

            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idnumber"></param>
        /// <param name="personid"></param>
        public void DeleteSelectChipsInfoByChoose(string idnumber, string personid)
        {
            try
            {
                String SQL = $"DELETE from CHIPINFOS WHERE PersonId='{personid}' AND ChipNO='{idnumber}'";
                helper.ExecuteNonQuery(SQL);
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="imgaeDir"></param>
        /// <returns></returns>
        public string SetCurrentGrroupImageDir(string projectName, string groups, string imgaeDir)
        {
            try
            {
                if(!Directory.Exists(imgaeDir))
                {
                    Directory.CreateDirectory(imgaeDir);
                }
                string sp=  Path.Combine(projectName, groups);
                string dir = Path.Combine(imgaeDir, sp);
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory (dir);
                }
                return dir;
            }
            catch(Exception ex)
            {

                LoggerHelper.Debug(ex);
                return null;
            }

        }
    }
}
