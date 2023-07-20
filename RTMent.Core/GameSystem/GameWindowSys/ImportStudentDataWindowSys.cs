using MiniExcelLibs;
using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameModel;
using RTMent.Core.GameSystem.GameWindow;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RTMent.Core.GameSystem.GameWindowSys
{
    public class ImportStudentDataWindowSys : Singleton<ImportStudentDataWindowSys>
    {
        private  SQLiteHelper helper = null;
        private ImportStudentDataWindow importStudentDataWindow=null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, string>> LoadingInitData()
        {
            try
            {
                List<Dictionary<string, string>> locals = new List<Dictionary<string, string>>();
                locals = helper.ExecuteReaderList("SELECT * FROM localInfos");
                if (locals.Count > 0)
                    return locals;
                else
                    return null;
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
        /// <param name="projectName"></param>
        /// <param name="helper"></param>
        /// <exception cref="NotImplementedException"></exception>
        public bool  ShowImportStudentDataWindow(string projectName, SQLiteHelper helper)
        {
            this.helper = helper;
            importStudentDataWindow = new ImportStudentDataWindow();
            importStudentDataWindow.ProjectName=projectName;
            if (importStudentDataWindow.ShowDialog() == DialogResult.OK)
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
        public void ShowPlatformSettingWindow()
        {
            PlatformSettingWindowSys.Instance.ShowPlatformSettingWindow(helper);
        }
        /// <summary>
        /// 数据库初始化
        /// </summary>
        /// <returns></returns>
        public bool InitDataBase()
        {
           return  helper.InitDb();
        }

        public bool BackDataBase()
        {
             return helper.BackDb();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string OpenLocalXlsxFile()
        {
            string path = string.Empty;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;      //该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";     //弹窗的标题
            dialog.InitialDirectory = Application.StartupPath + "\\";    //默认打开的文件夹的位置
            dialog.Filter = "MicroSoft Excel文件(*.xlsx)|*.xlsx";       //筛选文件
            dialog.ShowHelp = true;     //是否显示“帮助”按钮
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = dialog.FileName;
            }
            return path;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="path"></param>
        /// <param name="workSheetName"></param>
        /// <param name="proVal"></param>
        /// <param name="proMax"></param>
        public List<InputData> LoadingLocalImportStudentData(string projectName, string path, string workSheetName )
        {
            try
            {
                string   projectID = helper.ExecuteScalar($"select Id from SportProjectInfos where name='{projectName}'").ToString();
                var rows = MiniExcel.Query<InputData>(path, workSheetName).ToList();
                if (rows.Count > 0)
                    return rows;
                else
                {
                    return null;
                }
               
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
        /// <param name="data"></param>
        /// <param name="listView1"></param>
        /// <param name="projectName"></param>
        public void ShowLocalStudentDataView(List<InputData> data, ListView listView1,string projectName)
        {
            if(data.Count > 0)
            {
                listView1.Items.Clear();
                listView1.BeginUpdate();
                InitListViewData(listView1 );
                int index = 0;
                foreach(InputData row in data) 
                {
                    ListViewItem listViewItem = new ListViewItem()
                    {
                        UseItemStyleForSubItems = false,
                        Text = index.ToString(),
                    };
                    listViewItem.SubItems.Add(projectName);
                    listViewItem.SubItems.Add(row.School);
                    listViewItem.SubItems.Add(row.GradeName);  
                    listViewItem.SubItems.Add(row.ClassName);
                    listViewItem.SubItems.Add(row.Name);
                    listViewItem.SubItems.Add(row.Sex);
                    listViewItem.SubItems.Add(row.IdNumber);
                    listViewItem.SubItems.Add(row.GroupName);
                    listView1.Items.Insert(listView1.Items.Count, listViewItem);
                    index++;
                }
                TreeViewHelper.Instance.AutoResizeColumnWidth(listView1);
                listView1.EndUpdate();
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listView1"></param>
        private void InitListViewData(ListView listView1)
        {
            listView1.View = View.Details;
            ColumnHeader[] Header = new ColumnHeader[100];
            int sp = 0;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "序号";
            Header[sp].Width = 40;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "项目名";
            Header[sp].Width = 100;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "学校";
            Header[sp].Width = 80;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "年级";
            Header[sp].Width = 40;

            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "班级";
            Header[sp].Width = 100;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "姓名";
            Header[sp].Width = 100;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "性别";
            Header[sp].Width = 100;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "准考证号";
            Header[sp].Width = 100;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "组别";
            Header[sp].Width = 100;
            sp++;
            ColumnHeader[] Header1 = new ColumnHeader[sp];
            listView1.Columns.Clear();
            for (int i = 0; i < Header1.Length; i++)
            {
                Header1[i] = Header[i];
            }
            listView1.Columns.AddRange(Header1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataList"></param>
        public bool ImportCurrentDataToDataBase(List<InputData> dataList,String projectName,ref  int proVal,ref int proMax)
        {
            try
            {
                string projectid =  helper.ExecuteScalar($"select Id from SportProjectInfos where name='{projectName}'").ToString();
                HashSet<string> hast = new HashSet<string>();
                foreach (InputData data in dataList)
                {
                    hast.Add(data.GroupName);
                }
                List<string> roles = new List<string>();
                roles.AddRange(hast);
                var trans = helper.BeginTransaction();
                for (int i = 0; i < roles.Count; i++)
                {
                    string GroupName = roles[i];
                    string countstr = helper.ExecuteScalar($"SELECT COUNT(*) FROM DbGroupInfos where ProjectId='{projectid}' and Name='{GroupName}'").ToString();
                    int.TryParse(countstr, out int count);
                    if(count== 0)
                    {
                        string groupsortidstr =helper.ExecuteScalar("select MAX( SortId ) + 1 from DbGroupInfos").ToString();
                        int groupsortid = 1;
                        int.TryParse(groupsortidstr, out groupsortid);
                        string insertsql = $"INSERT INTO DbGroupInfos(CreateTime,SortId,IsRemoved,ProjectId,Name,IsAllTested) " +
                            $"VALUES(datetime(CURRENT_TIMESTAMP, 'localtime'),{groupsortid},0,'{projectid}','{GroupName}',0)";
                        //插入组
                       helper.ExecuteNonQuery(insertsql);
                    }
                }
                proVal = 0;
                proMax = dataList.Count;
                for (int i = 0;i < dataList.Count;i++)
                {
                    
                    InputData data = dataList[i];
                    string idNum = data.IdNumber.ToString();
                    string name = data.Name.ToString();
                    int Sex = data.Sex == "男" ? 0 : 1;
                    string schoolName = data.School;
                    string grade = data.GradeName;
                    string classNum = data.ClassName;
                    string groupName=data.GroupName;
                    string countstr = helper.ExecuteScalar($"SELECT COUNT(*) FROM DbPersonInfos WHERE ProjectId='{projectid}' AND IdNumber='{idNum}'").ToString();
                    int.TryParse(countstr, out int count);
                    if (count == 0)
                    {
                        int personsortid = 1;
                        string personsortidstr = helper.ExecuteScalar("select MAX( SortId ) + 1 from DbPersonInfos").ToString();
                        int.TryParse(personsortidstr, out personsortid);
                        string insertsql = $"INSERT INTO DbPersonInfos(CreateTime,SortId,IsRemoved,ProjectId,SchoolName,GradeName,ClassNumber,GroupName,Name,IdNumber,Sex,State,FinalScore,uploadState) " +
                            $"VALUES(datetime(CURRENT_TIMESTAMP, 'localtime'),{personsortid},0,'{projectid}','{schoolName}','{grade}','{classNum}','{groupName}'," +
                            $"'{name}','{idNum}',{Sex},0,-1,0)";
                       helper.ExecuteNonQuery(insertsql);
                    }
                    proVal++;
                }
                helper.CommitTransaction(trans);
                return true;
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
        /// <param name="num"></param>
        public string  LoadingPlatformStudentDataView(string num, Dictionary<string, string> localValues )
        {
            try
            {
                RequestParameter RequestParameter = new RequestParameter();
                RequestParameter.AdminUserName = localValues["AdminUserName"];
                RequestParameter.TestManUserName = localValues["TestManUserName"];
                RequestParameter.TestManPassword = localValues["TestManPassword"];
                string ExamId0 = localValues["ExamId"];
                ExamId0 = ExamId0.Substring(ExamId0.IndexOf('_') + 1);
                string MachineCode0 = localValues["MachineCode"];
                MachineCode0 = MachineCode0.Substring(MachineCode0.IndexOf('_') + 1);
                RequestParameter.ExamId = ExamId0;
                RequestParameter.MachineCode = MachineCode0;
                RequestParameter.GroupNums = num+ "";
                string jsonData = JsonDataHelper.Instance.SerializeObject(RequestParameter);
                string url = localValues["Platform"] + RequestUrl.GetGroupStudentUrl;
                var datas = new List<FormItemModel>();
                datas.Add(new FormItemModel()
                {
                    Key = "data",
                    Value = jsonData,
                });
                string res = HttpUpLoad.Instance.PostForm(url, datas);
                string[] strs = GetGroupStudent.CheckJson(res);
                GetGroupStudent getGroupStudent = null;
                GetGroupStudent studentList = new GetGroupStudent();
                studentList.Results = new Results();
                studentList.Results.groups = new List<GroupsItem>();
                if (strs[0] == "1")
                {
                    getGroupStudent = JsonDataHelper.Instance.DeserializeObject<GetGroupStudent>(res);
                    if(getGroupStudent != null &&getGroupStudent.Results!=null&&getGroupStudent.Results.groups!=null&&getGroupStudent.Results.groups.Count!=0 ) 
                        studentList.Results.groups.AddRange(getGroupStudent.Results.groups);         
                }
                else
                {
                    getGroupStudent = new GetGroupStudent();
                    getGroupStudent.Error = strs[1];
                    UIMessageBox.ShowWarning(string.Format("提交错误,错误码:[{0}]", getGroupStudent.Error));
                    return  null ;
                }
                if (studentList.Results.groups.Count > 0)
                {
                    return DownLoadingOutputExcelData(studentList);
                }
                else
                {
                    return null;
                }


            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return  null ;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentList"></param>
        private string DownLoadingOutputExcelData(GetGroupStudent studentList )
        {
            try
            {
                List<GroupsItem> groups = studentList.Results.groups;
                List<InputData> datas = new List<InputData>();
                int step = 1;
                
                foreach(var group in groups)
                {
                    string groupId = group.GroupId;
                    string groupName = group.GroupName;
                    foreach (var StudentInfo in group.StudentInfos)
                    {
                        InputData idata = new InputData();
                        idata.Id = step;
                        idata.School = StudentInfo.SchoolName;
                        idata.GradeName = StudentInfo.GradeName;
                        idata.ClassName = StudentInfo.ClassName;
                        idata.Name = StudentInfo.Name;
                        idata.Sex = StudentInfo.Sex;
                        idata.IdNumber = StudentInfo.IdNumber;
                        idata.GroupName = groupId;
                        datas.Add(idata);
                        step++;
                    }
                }
                string path = Application.StartupPath + $"\\模板\\下载名单\\downList{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                MiniExcel.SaveAs(path, datas);
                return path;
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return null ;
            }
        }

        
    }
}
