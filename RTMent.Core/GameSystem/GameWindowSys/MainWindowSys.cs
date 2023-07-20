using RTMent.Core.GameSystem.GameHelper;
 
using RTMent.Core.GameSystem.GameModel;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.GameWindowSys
{
    public class MainWindowSys : Singleton<MainWindowSys>
    {   
        /// <summary>
        /// 
        /// </summary>
        private  SQLiteHelper helper = new SQLiteHelper(); 
        /// <summary>
        /// 加载项目信息
        /// </summary>
        /// <param name="uiTreeView1"></param>
        public List<ProjectDataModel> LoadingCurrentProjectData( )
        {
            List<ProjectDataModel> projects = new List<ProjectDataModel>();
            try
            {
                projects.Clear();
                
                string sql = $"SELECT Id,Name FROM SportProjectInfos";
                var sl = helper.ExecuteReader(sql);
                while(sl.Read())
                {
                    string projectID = sl.GetValue(0).ToString();
                    string projectName=sl.GetString(1);
                    sql = $"SELECT Name,IsAllTested FROM DbGroupInfos WHERE ProjectId='{projectID}'";
                    var ds= helper.ExecuteReader(sql);
                    projects.Add(new ProjectDataModel()
                    {
                        projectName = projectName,
                        Groups = new List<GroupDataModel>()
                    });
                    while (ds.Read())
                    {
                        string groupName = ds.GetString(0);
                        int isAllTest=ds.GetInt32(1);
                        ProjectDataModel projectDataModel = projects.FirstOrDefault(a => a.projectName == projectName);
                        if(projectDataModel != null)
                        {
                            projectDataModel.Groups.Add(new GroupDataModel()
                            {
                                GroupName = groupName,
                                IsAllTested = isAllTest,
                            });
                        }
                        else
                        {
                            projects.Add(new ProjectDataModel()
                            {
                                Groups = new List<GroupDataModel>()
                                {
                                    new GroupDataModel()
                                    {
                                        GroupName = groupName,
                                        IsAllTested = isAllTest,
                                    } ,
                                },
                                projectName = projectName,

                            });
                        }
                    }

                }
                return projects;
            }
            catch(Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }
        /// <summary>
        /// 加载当前学生信息
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="listView1"></param>
        public void LoadingCurrentStudentData(string projectName, string groupName, ListView listView1, ref string projectId)
        {
            if (string.IsNullOrEmpty(projectName) && string.IsNullOrEmpty(groupName)) return;
            TreeViewHelper.Instance.LoadingCurrentStudentData(helper, projectName, groupName, listView1, ref projectId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns> 
        public bool ShowPersonImportWindow()
        {
            return PersonImportWindowSys.Instance.ShowPersonImportWindow(helper);
        }

        public void ShowPlatformSettingWindow()
        {
            PlatformSettingWindowSys.Instance.ShowPlatformSettingWindow(helper);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="name"></param>
        /// <param name="idNumber"></param>
        /// <param name="status"></param>
        /// <param name="rountid"></param>
        /// <returns></returns>
        public bool ModifyCurrentChooseStudentGrade(string projectName, string groupName, string name, string idNumber, string status )
        {
            return GradeManager.Instance.ModifyCurrentChooseStudentGrade(helper, projectName, groupName, name, idNumber,
                status);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="personIdNumber"></param>
        /// <returns></returns>
        public bool DeleteCurrentChooseStudentGrade( string personIdNumber)
        {
            return GradeManager.Instance.DeleteCurrentChooseStudentGrade(helper,  personIdNumber);
        }

        public bool InitDataBase()
        {
            return helper.InitDb();
        }

        public bool BackDataBase()
        {
            return helper.BackDb();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsp"></param>
        /// <returns></returns>
       
        public string UpLoadStudentGrade(string[] fsp, ref int proVal, ref int proMax, ProgressBar progressBar1, Timer timer1)
        {
            return GradeManager.Instance.UpLoadStudentGrade(fsp,helper,ref  proVal,ref  proMax,progressBar1,timer1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsp"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool ShowRunningTestingWindow(string[] fsp)
        {
            try
            {
                List<Dictionary<string, string>> list = helper.ExecuteReaderList($"SELECT Id,Type,RoundCount,BestScoreMode,TestMethod," +
                   $"FloatType,TurnsNumber0,TurnsNumber1 FROM SportProjectInfos WHERE Name='{fsp[0]}'");
                if (list.Count == 1)
                {
                    Dictionary<string, string> dic = list[0];
                    int.TryParse(dic["Type"], out int state);
                    return RunningTestingWindowSys.Instance.ShowRunningTestingWindow(helper,fsp, dic);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
    }
}
