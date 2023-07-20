using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTMent.Core.GameSystem.GameModel;

namespace RTMent.Core.GameSystem.GameWindowSys
{
    public class PlatformSettingWindowSys:Singleton<PlatformSettingWindowSys>
    {
        SQLiteHelper helper = null;
        PlatformSettingWindow PlatformSettingWindow=null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        public void ShowPlatformSettingWindow(SQLiteHelper helper)
        {
            this.helper = helper;
            PlatformSettingWindow = new PlatformSettingWindow();
            PlatformSettingWindow.ShowDialog();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  List<Dictionary<string, string>> LoadingInitData()
        {
            return helper.ExecuteReaderList("SELECT * FROM localInfos");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="localValues"></param>
        /// <returns></returns>
        public GetExamList GetExamID(string url, Dictionary<string, string> localValues)
        {
            try
            {
                url += RequestUrl.GetExamListUrl;
                RequestParameter RequestParameter = new RequestParameter();
                RequestParameter.AdminUserName = localValues["AdminUserName"];
                RequestParameter.TestManUserName = localValues["TestManUserName"];
                RequestParameter.TestManPassword = localValues["TestManPassword"];
                string JsonData = JsonDataHelper.Instance.SerializeObject(RequestParameter);
                List<FormItemModel> datas = new List<FormItemModel>();
                datas.Add(new  FormItemModel()
                {
                    Key = "data",
                    Value = JsonData,
                });
                string res = HttpUpLoad.Instance.PostForm(url, datas);
                var result = JsonDataHelper.Instance.DeserializeObject<GetExamList>(res);
                return result;
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
        /// <param name="examId"></param>
        /// <param name="url"></param>
        /// <param name="localValues"></param>
        /// <returns></returns>
        public GetMachineCodeList GetEquipMentID(string examId, string url, Dictionary<string, string> localValues )
        {
            try
            {
                url += RequestUrl.GetMachineCodeListUrl;
                RequestParameter RequestParameter = new RequestParameter();
                RequestParameter.AdminUserName = localValues["AdminUserName"];
                RequestParameter.TestManUserName = localValues["TestManUserName"];
                RequestParameter.TestManPassword = localValues["TestManPassword"];
                RequestParameter.ExamId = examId;
                string jsonData = JsonDataHelper.Instance.SerializeObject(RequestParameter);
                List<FormItemModel> datas = new List<FormItemModel>();
                datas.Add(new FormItemModel()
                {
                    Key = "data",
                    Value = jsonData,
                });
                var res = HttpUpLoad.Instance.PostForm(url, datas);
                GetMachineCodeList result = JsonDataHelper.Instance.DeserializeObject<GetMachineCodeList>(res);
                return result;

            }
            catch (Exception e)
            {
                 LoggerHelper.Debug(e);
                 return null;
            }
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="examId"></param>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public bool SaveDataSetting(string platform, string examId, string machineCode)
        {
            try
            {
                System.Data.SQLite.SQLiteTransaction sQLiteTransaction = helper.BeginTransaction();
                helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{platform}' WHERE key = 'Platform'");
                helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{examId}' WHERE key = 'ExamId'");
                helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{machineCode}' WHERE key = 'MachineCode'");
                helper.CommitTransaction(sQLiteTransaction);
                return true;
            }
            catch (Exception e)
            {
               LoggerHelper.Debug(e);
               return false;
            }
        }
    }
}
