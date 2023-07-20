﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameModel
{
    public  class SoftWareProperty
    {
        /// <summary>
        /// 是否单机
        /// </summary>
        public static string singleMode = "1";
        public SoftWareProperty()
        {
            loadData();
        }
        public static void loadData()
        {
            string path = @"data/singleMode.txt";
            if (File.Exists(path))
            {
                string v = File.ReadAllText(path);
                singleMode = v;
            }
            else
            {
                ///不存在就是单机版
                singleMode = "1";
                File.WriteAllText(path, "1");
            }
        }
    }
}
