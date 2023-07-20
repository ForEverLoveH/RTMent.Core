using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Windows.Forms;
using HZH_Controls;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindow;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class GradeManager:Singleton<GradeManager>
    {
        /// <summary>
        /// 修改成绩
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="name"></param>
        /// <param name="idNumber"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ModifyCurrentChooseStudentGrade(SQLiteHelper helper, string projectName, string groupName, string name, string idNumber, string status )
        {
            try
            {
                int rountid = 0;
                string sql=$"SELECT Id,Type,RoundCount,BestScoreMode,TestMethod," + $"FloatType,TurnsNumber0,TurnsNumber1 FROM SportProjectInfos WHERE Name='{projectName}'";
                var dicData = helper.ExecuteReaderOne(sql);
                int floatType = 0;
                if (dicData.Count > 0)
                {
                    floatType = Convert.ToInt32(dicData["FloatType"]);
                    rountid = Convert.ToInt32(dicData["RoundCount"]);
                }
                ModifyScoreWindow modifyScoreWindow = new ModifyScoreWindow();
                modifyScoreWindow.projectName = projectName;
                modifyScoreWindow.groupName = groupName;
                modifyScoreWindow.Name = name;
                modifyScoreWindow.IdNumber = idNumber;
                modifyScoreWindow.status = status;
                modifyScoreWindow.rountid = rountid;
                if (modifyScoreWindow.ShowDialog() == DialogResult.OK)
                {
                    int roundID = modifyScoreWindow.updaterountId;
                    double updataScore = modifyScoreWindow.updateScore;
                    decimal.Round(decimal.Parse(updataScore.ToString("0.0000")), floatType).ToString();
                    string updatestatus = modifyScoreWindow.status;
                    int Resultinfo_State =ResultStateHelper.Instance.ResultState2Int(updatestatus);
                    sql = $"UPDATE ResultInfos SET Result={updataScore},State={Resultinfo_State} WHERE PersonIdNumber='{idNumber}' AND RoundId={roundID}";
                    int result = helper.ExecuteNonQuery(sql);
                    if (result == 0)
                    {
                        string perid = "";
                        var ds0 = helper.ExecuteReaderOne($"SELECT Id FROM DbPersonInfos WHERE IdNumber='{idNumber}'");
                        if (ds0.Count > 0)
                            perid = ds0["Id"];
                        if (string.IsNullOrEmpty(perid))
                            return false;
                        sql =
                            $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                            $"VALUES (datetime(CURRENT_TIMESTAMP, 'localtime') ,(SELECT MAX(SortId)+1 FROM ResultInfos),0," +
                            $"'{perid}',0,'{name}','{idNumber}',{rountid},{updataScore},{Resultinfo_State})";
                        int result0 = helper.ExecuteNonQuery(sql);
                        if(result0==1)
                            return true;
                        return false;
                    }
                    else  if (result>1)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return false;
            }
        }
        /// <summary>
        /// 删除成绩
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="name"></param>
        /// <param name="personIdNumber"></param>
        /// <returns></returns>
        public bool DeleteCurrentChooseStudentGrade(SQLiteHelper helper,   string personIdNumber)
        {
            try
            {
                string sql = $"DELETE FROM ResultInfos WHERE PersonIdNumber = '{personIdNumber}'";
                int result = helper.ExecuteNonQuery(sql);
                sql = $"update DbPersonInfos SET State=0 where IdNumber='{personIdNumber}'";
                int result1= helper.ExecuteNonQuery(sql);
                if (result == 1 && result1 > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.Debug(exception);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsp"></param>
        /// <returns></returns>
        public string UpLoadStudentGrade(object obj,SQLiteHelper helper,ref int proVal,ref int proMax, ProgressBar progressBar1, System.Windows.Forms.Timer timer1)
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
               string sql0 = $"SELECT Id,ProjectId,Name FROM DbGroupInfos WHERE ProjectId='{SportProjectDic["Id"]}' ";

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

               proMax = sqlGroupsResults.Count;
               proVal = 0;
               progressBar1.Visible = true;
               progressBar1.Value = 0;
               timer1.Start();
               ///按组上传
               foreach (var sqlGroupsResult in sqlGroupsResults)
               {
                   proVal++;
                   string sql = $"SELECT Id,GroupName,Name,IdNumber,SchoolName,GradeName,ClassNumber,State,Sex,BeginTime,FinalScore,uploadState FROM DbPersonInfos" +
                                $" WHERE ProjectId='{SportProjectDic["Id"]}' AND GroupName = '{sqlGroupsResult["Name"]}'";
                   List<Dictionary<string, string>> list = helper.ExecuteReaderList(sql);
                   //轮次
                   int turn = 0;
                   if (list.Count > 0)
                   {
                       Dictionary<string, string> stu = list[0];
                       int.TryParse(SportProjectDic["RoundCount"], out turn);
                       urrp.MachineCode = MachineCode;
                   }
                   else
                   {
                       continue;
                   }

                   List< SudentsItem> sudentsItems = new List< SudentsItem>();
                   //IdNumber 对应Id
                   Dictionary<string, string> map = new Dictionary<string, string>();

                   for (int i = 1; i <= turn; i++)
                   {
                       foreach (var stu in list)
                       {
                           List< RoundsItem> roundsItems = new List<RoundsItem>();
                           ///成绩
                           List<Dictionary<string, string>> resultScoreList1 = helper.ExecuteReaderList(
                               $"SELECT Id,CreateTime,RoundId,State,uploadState,Result FROM ResultInfos WHERE PersonId='{stu["Id"]}' And IsRemoved=0 And RoundId={i} LIMIT 1");

                           #region 查询文件

                           //成绩根目录
                           Dictionary<string, string> dic_images = new Dictionary<string, string>();
                           Dictionary<string, string> dic_viedos = new Dictionary<string, string>();
                           Dictionary<string, string> dic_texts = new Dictionary<string, string>();
                           //string scoreRoot = Application.StartupPath + $"\\Scores\\{projectName}\\{stu["GroupName"]}\\";

                           #endregion 查询文件

                           foreach (var item in resultScoreList1)
                           {
                               //不重复上传
                               if (item["uploadState"] != "0") continue;
                               ///
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
                               {
                                   score = 0;
                               }
                               rdi.Result = score;
                               //string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                               rdi.GroupNo = GroupNo;
                               rdi.Text = dic_texts;
                               rdi.Images = dic_images;
                               rdi.Videos = dic_viedos;
                               roundsItems.Add(rdi);
                           }
                           if (roundsItems.Count == 0) continue;
                           SudentsItem ssi = new SudentsItem();
                           ssi.SchoolName = stu["SchoolName"];
                           ssi.GradeName = stu["GradeName"];
                           ssi.ClassNumber = stu["ClassNumber"];
                           ssi.Name = stu["Name"];
                           ssi.IdNumber = stu["IdNumber"];
                           ssi.Rounds = roundsItems;
                           sudentsItems.Add(ssi);
                           if (!map.Keys.Contains(stu["IdNumber"]))
                           {
                               map.Add(stu["IdNumber"], stu["Id"]);
                           }
                       }
                   }

                   #region #上传成绩包装

                   if (sudentsItems.Count == 0) continue;
                   urrp.Sudents = sudentsItems;
                   //序列化json
                   string JsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(urrp);
                   string url = localInfos["Platform"] +RequestUrl.UploadResults;
                  
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
                   string result =HttpUpLoad.Instance.PostForm(url, formDatas);
                   Upload_Result upload_Result = Newtonsoft.Json.JsonConvert.DeserializeObject< Upload_Result>(result);
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

                   #endregion #上传成绩包装
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
                   string sql1 = $"UPDATE ResultInfos SET uploadState=1 WHERE PersonId={item["Id"]} and RoundId={item["RoundId"]}";
                   helper.ExecuteNonQuery(sql1);
                   //更新成绩上传状态
                   sql1 = $"UPDATE ResultInfos SET uploadState=1 WHERE PersonId={item["Id"]}";
                   helper.ExecuteNonQuery(sql1);
                   sb.AppendLine($"考号:{item["IdNumber"]} 姓名:{item["Name"]}");
               }
               helper.CommitTransaction(sQLiteTransaction);
               sb.AppendLine("*******************success********************");

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
               string outpitMessage = messageSb.ToString();
               return outpitMessage;
           }
           catch (Exception ex)
           {
               LoggerHelper.Debug(ex);
               return ex.Message;
           }
           finally
           {
             
               progressBar1.Visible = false;
               progressBar1.Value = 0;
               timer1.Stop();
             
           }
        }
    }
}