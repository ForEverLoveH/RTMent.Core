using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameModel
{
    public class GetGroupStudent
    {
        /// <summary>
        /// 
        /// </summary>
        public Results Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Error { get; set; }
        public static string[] CheckJson(string json)
        {
            string[] strs = new string[2];
            var jsObject = JObject.Parse(json);
            string ResultISNull = "0";
            string ResultError = "";
            foreach (JToken child in jsObject.Children())
            {
                var property1 = child as JProperty;
                if (property1.Name == "Error")
                {
                    if (string.IsNullOrEmpty(property1.Value.ToString()))
                    {
                        ResultISNull = "1";
                    }
                    else
                    {
                        ResultISNull = "0";
                        ResultError = property1.Value.ToString();
                    }
                    break;
                }
            }
            strs[0] = ResultISNull;
            strs[1] = ResultError;
            return strs;
        }

    }
    public class Results
    {
        /// <summary>
        /// 
        /// </summary>
        public List<GroupsItem> groups { get; set; }

    }
    public class GroupsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<StudentInfosItem> StudentInfos { get; set; }

    }
    public class StudentInfosItem
    {
        /// <summary>
        /// 高小雅
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// 女
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// 淮安市文通中学
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// 初三
        /// </summary>
        public string GradeName { get; set; }

        /// <summary>
        /// 1班
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ClassNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GroupIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Projects { get; set; }

    }
}
