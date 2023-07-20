using RTMent.Core.GameSystem.GameHelper;
using RTMent.Core.GameSystem.GameWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace RTMent.Core.GameSystem.GameWindowSys
{
    public class ModifyStudentDataWindowSys:Singleton<ModifyStudentDataWindowSys>
    {
        private ModifyStudentDataWindow  modifyStudentDataWindow = null;
        private SQLiteHelper helper= null;
        public static string scores;
        public static string personID;
        public static int state;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="projectName"></param>
        /// <param name="projectID"></param>
        /// <param name="groupName"></param>
        /// <param name="name"></param>
        /// <param name="currentRound"></param>
        /// <returns></returns>
        public bool ShowModifyStudentDataWindow(SQLiteHelper helper, string projectName, string projectID, string groupName, string idNumber, string name, int currentRound)
        {
           this.helper = helper;
            modifyStudentDataWindow = new ModifyStudentDataWindow();
            modifyStudentDataWindow.projectName=    projectName;
            modifyStudentDataWindow.projectId= projectID;
            modifyStudentDataWindow.groupName = groupName ;
            modifyStudentDataWindow.IdNumber= idNumber;
            modifyStudentDataWindow.stuName = name;
            modifyStudentDataWindow.CurrentRound= currentRound;
            if(modifyStudentDataWindow.ShowDialog()==System.Windows.Forms.DialogResult.OK )
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
        /// <param name="projectId"></param>
        /// <param name="idNumber"></param>
        /// <param name="currentRound"></param>
        /// <param name="personId"></param>
        /// <param name="score"></param>
        /// <param name="state"></param>
        public void LoadingInitData(string projectId, string idNumber, int currentRound, ref string personId, ref string score, ref string state)
        {
            string sql = $"SELECT Id FROM DbPersonInfos WHERE ProjectId='{projectId}' AND IdNumber='{idNumber}'";
            Dictionary<string, string> dic0 =helper.ExecuteReaderOne(sql);
            if (dic0.Count > 0)
            {
                personId = dic0["Id"];
            }

            sql = $"SELECT * FROM ResultInfos WHERE RoundId='{currentRound}'  AND PersonId='{personId}'";
            Dictionary<string, string> dic1 = helper.ExecuteReaderOne(sql);
            if (dic1.Count > 0)
            {
                score = dic1["Result"];
                state = dic1["State"];
            }
        }

        public void SetModifyData(string score, string personId, int iState)
        {
            scores= score;
            personID= personId;
            state =iState;
        }
    }
}
