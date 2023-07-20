using HZH_Controls.Controls;
using HZH_Controls.Forms;
using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindow;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RTMent.Core.GameSystem.GameWindowSys
{
    public class PersonImportWindowSys : Singleton<PersonImportWindowSys>
    {
        PersonImportWindow personImportWindow= null;
        SQLiteHelper helper=    null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public bool ShowPersonImportWindow(SQLiteHelper helper)
        {
            personImportWindow = new PersonImportWindow();
            this.helper = helper;
            if( personImportWindow.ShowDialog()==System.Windows.Forms.DialogResult.OK )
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ProjectDataModel> LoadingProjectView()
        {
            List<ProjectDataModel> projects = new List<ProjectDataModel>();
            try
            {
                projects.Clear();

                string sql = $"SELECT Id,Name FROM SportProjectInfos";
                var sl = helper.ExecuteReader(sql);
                while (sl.Read())
                {
                    string projectID = sl.GetValue(0).ToString();
                    string projectName = sl.GetString(1);
                    sql = $"SELECT Name,IsAllTested FROM DbGroupInfos WHERE ProjectId='{projectID}'";
                    var ds = helper.ExecuteReader(sql);
                    projects.Add(new ProjectDataModel()
                    {
                        projectName = projectName,
                        Groups = new List<GroupDataModel>()
                    });
                    while (ds.Read())
                    {
                        string groupName = ds.GetString(0);
                        int isAllTest = ds.GetInt32(1);
                        ProjectDataModel projectDataModel = projects.FirstOrDefault(a => a.projectName == projectName);
                        if (projectDataModel != null)
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
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="projectID"></param>
        /// <param name="txt_Type"></param>
        /// <param name="txt_RoundCount"></param>
        /// <param name="txt_TestMethod"></param>
        /// <param name="txt_projectName"></param>
        /// <param name="txt_BestScoreMode"></param>
        /// <param name="txt_FloatType"></param>
        public void LoadingProjectAttribute(string projectName, ref string projectID, UIComboBox txt_Type, UIComboBox txt_RoundCount, UIComboBox txt_TestMethod, UITextBox txt_projectName, UIComboBox txt_BestScoreMode, UIComboBox txt_FloatType)
        {
            try
            {
                var ds = helper.ExecuteReader("SELECT spi.Name,spi.Type,spi.RoundCount,spi.BestScoreMode,spi.TestMethod,spi.FloatType,spi.Id " +
                $"FROM SportProjectInfos AS spi WHERE spi.Name='{projectName}'");

                while (ds.Read())
                {
                    string Name = ds.GetString(0);
                    int Type = ds.GetInt16(1);
                    int RoundCount = ds.GetInt16(2);
                    int BestScoreMode = ds.GetInt16(3);
                    int TestMethod = ds.GetInt16(4);
                    int FloatType = ds.GetInt16(5);
                    projectID = ds.GetValue(6).ToString();
                    txt_projectName.Text = Name;
                    txt_Type.SelectedIndex = 0;
                    txt_RoundCount.SelectedIndex = RoundCount;
                    txt_BestScoreMode.SelectedIndex = BestScoreMode;
                    txt_TestMethod.SelectedIndex = TestMethod;
                    txt_FloatType.SelectedIndex = FloatType;

                    break;
                }
            }
            catch(Exception e)
            {
                LoggerHelper.Debug(e);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="groupName"></param>
        /// <param name="ucDataGridView1"></param>
        public void LoadingProjectStudentData(string projectID, string groupName, UCDataGridView ucDataGridView1)
        {
            try
            {
                List<DataGridViewColumnEntity> lstCulumns = new List<DataGridViewColumnEntity>();
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "ID", HeadText = "序号", Width = 5, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "GroupName", HeadText = "组别名称", Width = 20, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "School", HeadText = "学校", Width = 20, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Grade", HeadText = "年级", Width = 5, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Class", HeadText = "班级", Width = 5, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Name", HeadText = "姓名", Width = 20, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Sex", HeadText = "性别", Width = 5, WidthType = SizeType.AutoSize, Format = (a) => { return ((int)a) == 0 ? "男" : "女"; } });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "IdNumber", HeadText = "准考证号", Width = 20, WidthType = SizeType.AutoSize });
                ucDataGridView1.Columns=lstCulumns;
                ucDataGridView1.IsShowCheckBox = true;
                List<Object> list = new List<Object>();
                string sql = $"SELECT d.GroupName,d.SchoolName,d.GradeName,d.ClassNumber,d.Name,d.Sex,d.IdNumber " + $"FROM DbPersonInfos AS d WHERE d.GroupName='{groupName}' AND d.ProjectId='{projectID}'";
                var sl = helper.ExecuteReaderList(sql);
                int index = 1;
                foreach(var item in sl)
                {
                    DataGridViewModel model = new DataGridViewModel()
                    {
                        ID = index.ToString(),
                        GroupName = item["GroupName"],
                        School = item["SchoolName"],
                        Grade = item["GradeName"],
                        Class = item["ClassNumber"],
                        Name = item["Name"],
                        Sex = Convert.ToInt32(item["Sex"]),
                        IdNumber = item["IdNumber"],

                    };
                    list.Add(model);
                    index++;
                }
                ucDataGridView1.DataSource = list;
                ucDataGridView1.ReloadSource();


            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex); return;
            }
        }
        /// <summary>
        /// 删除当前项目
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <returns></returns>
        public bool DeleteCurrentProject(string projectName )
        {
            try
            {
                var value = helper.ExecuteScalar($"SELECT Id FROM SportProjectInfos WHERE Name='{projectName}'");
                string projectId = value.ToString();

                int result = helper.ExecuteNonQuery($"DELETE FROM SportProjectInfos WHERE Id = '{projectId}'");
                if (result == 1)
                {
                    helper.ExecuteNonQuery($"DELETE FROM DbGroupInfos WHERE ProjectId = '{projectId}'");
                    helper.ExecuteNonQuery($"DELETE FROM DbPersonInfos WHERE ProjectId = '{projectId}'");
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
        /// <summary>
        /// 新建项目
        /// </summary>
        /// <param name="projects">项目名</param>
        /// <returns></returns>
        public bool CreateNewProject(string projects)
        {
            try
            {
                string sql = $"select Id from SportProjectInfos where Name='{projects}' LIMIT 1";
                var existProject = helper.ExecuteScalar(sql);
                int si = 1;
                var ds = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM SportProjectInfos").ToString();
                int.TryParse(ds, out si);
                if (existProject != null)
                {
                     UIMessageBox.ShowWarning( $"项目:{projects}已存在");
                    return false;
                }
                else
                {

                    sql = $"INSERT INTO SportProjectInfos (CreateTime, SortId, IsRemoved, Name, Type, RoundCount, BestScoreMode, TestMethod, FloatType ) " +
                        $"VALUES(datetime(CURRENT_TIMESTAMP, 'localtime'),{si}," + $"0,'{projects}',0,2,0,0,2)";
                    int result = helper.ExecuteNonQuery(sql);
                    if(result == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
        /// <summary>
        /// 删除当前选中
        /// </summary>
        /// <param name="ucDataGridView1"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool DeleteCurrentChooseStudentData(UCDataGridView ucDataGridView1, string projectName)
        {
            try
            {
                var value = helper.ExecuteScalar($"SELECT Id FROM SportProjectInfos WHERE Name='{projectName}'");
                string projectId = value.ToString();
                if (!string.IsNullOrEmpty(projectId))
                {
                    for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
                    {
                        DataGridViewModel osoure = ucDataGridView1.SelectRows[i].DataSource as DataGridViewModel;
                        var vpersonId = helper.ExecuteScalar($"SELECT  Id FROM DbPersonInfos WHERE ProjectId='{projectId}' and Name='{osoure.Name}' and IdNumber='{osoure.IdNumber}'");
                        //删除人
                        helper.ExecuteNonQuery($"DELETE FROM DbPersonInfos WHERE Id='{vpersonId}'");
                        //删除成绩
                        helper.ExecuteNonQuery($"DELETE FROM ResultInfos WHERE PersonId='{vpersonId}'");
                    }
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
        /// <summary>
        /// 删除当前组
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public  bool DeleteCurrentGroupStudent(string groupName, string projectName)
        {
            try
            {
                var value = helper.ExecuteScalar($"SELECT Id FROM SportProjectInfos WHERE Name='{projectName}'");
                string projectId = value.ToString();
                if (!string.IsNullOrEmpty(projectId))
                {
                    //删除组
                    helper.ExecuteNonQuery($"DELETE FROM DbGroupInfos WHERE ProjectId='{projectId}' and Name='{groupName}'");
                    var ds = helper.ExecuteReader($"SELECT Id FROM DbPersonInfos WHERE ProjectId='{projectId}' AND GroupName='{groupName}'");
                    while (ds.Read())
                    {
                        var vpersonId = ds.GetValue(0).ToString(); ;
                        //删除成绩
                        helper.ExecuteNonQuery($"DELETE FROM ResultInfos WHERE PersonId='{vpersonId}'");
                    }
                    //删除人
                    helper.ExecuteNonQuery($"DELETE FROM DbPersonInfos WHERE ProjectId='{projectId}' AND GroupName='{groupName}'");
                    return true;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        public  bool ShowImportStudentDataWindow(string projectName)
        {
             return  ImportStudentDataWindowSys.Instance.ShowImportStudentDataWindow(projectName, helper);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name0"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="roundCount"></param>
        /// <param name="bestScoreMode"></param>
        /// <param name="testMethod"></param>
        /// <param name="floatType"></param>
        /// <returns></returns>
        public bool SaveCurrentProjectSetting(string name0, string name, int type, int roundCount, int bestScoreMode, int testMethod, int floatType)
        {
            try
            {
                string projectID = helper.ExecuteScalar($"select Id from SportProjectInfos where Name='{name0}'").ToString();

                string sql = $"UPDATE SportProjectInfos SET Name='{name}', Type={type},RoundCount={roundCount},BestScoreMode={bestScoreMode},TestMethod={testMethod},FloatType={floatType} where Id='{projectID}'";
                int result = helper.ExecuteNonQuery(sql);
                if (result == 1)
                {
                    return true;
                }
                else
                    return false;
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
    }
}
