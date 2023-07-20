using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameModel
{
    public class ProjectDataModel
    {
        /// <summary>
        /// 项目名
        /// </summary>
        public string projectName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<GroupDataModel> Groups { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GroupDataModel
    {
        /// <summary>
        /// 项目组名字
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 是否全部完成
        /// </summary>
        public int IsAllTested { get; set; }
    }
}
