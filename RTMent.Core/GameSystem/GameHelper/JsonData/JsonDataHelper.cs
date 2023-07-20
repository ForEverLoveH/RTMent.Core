using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class JsonDataHelper:Singleton<JsonDataHelper>
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public T DeserializeObject<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string SerializeObject<T>(T obj) where T : class
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
