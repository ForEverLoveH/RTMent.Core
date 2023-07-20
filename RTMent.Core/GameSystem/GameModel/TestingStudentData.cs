using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameModel
{
    public class TestingStudentData
    {
        /// <summary>
        /// 
        /// </summary>
        public int RaceStudentDataId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string idNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int RoundId { get; set; }

        //状态 0:未测试 1:已测试 2:中退 3:缺考 4:犯规
        public int state;
    }
    public class SelectStudentDataModel
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 考号
        /// </summary>
        public string idNumber { get;set; }
       
        /// <summary>
        /// 组号
        /// </summary>
        public string groupName { get; set; }
        /// <summary>
        /// 第一轮成绩
        /// </summary>
        public object score { get; set; }
        
        /// <summary>
        /// 上传状态
        /// </summary>
        public int UpLoadState { get; set; }
        /// <summary>
        /// 第二轮成绩
        /// </summary>
        public object score1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UpLoadState2 { get; set; }


    }
}
