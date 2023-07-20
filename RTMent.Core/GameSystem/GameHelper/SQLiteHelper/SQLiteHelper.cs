using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class SQLiteHelper
    {
        /// <summary>
        /// 数据库列表
        /// </summary>
        public static Dictionary<string, SQLiteHelper> DataBaceList = new Dictionary<string, SQLiteHelper>();

        private SQLiteConnection dbConnection;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filename">数据库文件名</param>
        public SQLiteHelper(string filename = null)
        {
            if (filename == null)
            {
                filename = Common.DbPath;
            }

            DataSource = filename;

            GetSQLiteConnection();
        }

        /// <summary>
        /// 数据库地址
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 创建数据库，如果数据库文件存在则忽略此操作
        /// </summary>
        public void CreateDataBase()
        {
            string path = Path.GetDirectoryName(DataSource);
            if ((!string.IsNullOrWhiteSpace(path)) && (!Directory.Exists(path))) Directory.CreateDirectory(path);
            if (!File.Exists(DataSource)) SQLiteConnection.CreateFile(DataSource);
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        public SQLiteTransaction BeginTransaction()
        {
            SQLiteTransaction sQLiteTransaction = null;
            try
            {
                sQLiteTransaction = dbConnection.BeginTransaction();
            }
            catch (Exception)
            {
                sQLiteTransaction = null;
            }
            return sQLiteTransaction;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction(SQLiteTransaction sQLiteTransaction)
        {
            try
            {
                if (sQLiteTransaction != null) sQLiteTransaction.Commit();
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// 获得连接对象
        /// </summary>
        /// <returns>SQLiteConnection</returns>
        public void GetSQLiteConnection()
        {
            string connStr = string.Format("Data Source={0};Version=3;Max Pool Size=10;Journal Mode=Off;", DataSource);
            dbConnection = new SQLiteConnection(connStr);
        }

        /// <summary>
        /// 准备操作命令参数
        /// </summary>
        /// <param name="cmd">SQLiteCommand</param>
        /// <param name="conn">SQLiteConnection</param>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string cmdText, Dictionary<String, String> data)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            if (data != null && data.Count >= 1)
            {
                foreach (KeyValuePair<String, String> val in data)
                {
                    cmd.Parameters.AddWithValue(val.Key, val.Value);
                }
            }
        }

        /// <summary>
        /// 查询，返回DataSet
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataset(string cmdText, Dictionary<string, string> data = null)
        {
            var ds = new DataSet();
            var command = new SQLiteCommand();
            PrepareCommand(command, dbConnection, cmdText, data);
            var da = new SQLiteDataAdapter(command);
            da.Fill(ds);
            return ds;
        }

        /// <summary>
        /// 查询，返回DataTable
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteDataTable(string cmdText, Dictionary<string, string> data = null)
        {
            var dt = new DataTable();
            var command = new SQLiteCommand();
            PrepareCommand(command, dbConnection, cmdText, data);
            SQLiteDataReader reader = command.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        /// <summary>
        /// 返回一行数据
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataRow</returns>
        public DataRow ExecuteDataRow(string cmdText, Dictionary<string, string> data = null)
        {
            DataSet ds = ExecuteDataset(cmdText, data);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return ds.Tables[0].Rows[0];
            return null;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string cmdText, Dictionary<string, string> data = null)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine($"数据库操作:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                sb.AppendLine(cmdText);
                LoggerHelper.Info(sb.ToString());
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                // throw;
            }
            int result = 0;
            var command = new SQLiteCommand();
            PrepareCommand(command, dbConnection, cmdText, data);
            result = command.ExecuteNonQuery();

            return result;
        }

        /// <summary>
        /// 返回SqlDataReader对象
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>SQLiteDataReader</returns>
        public SQLiteDataReader ExecuteReader(string cmdText, Dictionary<string, string> data = null)
        {
            var command = new SQLiteCommand();

            try
            {
                PrepareCommand(command, dbConnection, cmdText, data);
                SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception e)
            {
                command.Dispose();
                throw;
            }
        }

        public List<Dictionary<string, string>> ExecuteReaderList(string cmdText)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            var ds = ExecuteReader(cmdText);
            int columcount = ds.FieldCount;
            while (ds.Read())
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                for (int i = 0; i < columcount; i++)
                {
                    object obj = ds.GetValue(i);
                    if (obj == null)
                    {
                        dic.Add(ds.GetName(i), "");
                    }
                    else
                    {
                        dic.Add(ds.GetName(i), obj.ToString());
                    }
                }
                list.Add(dic);
            }

            return list;
        }

        public Dictionary<string, string> ExecuteReaderOne(string cmdText)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            var ds = ExecuteReader(cmdText);
            int columcount = ds.FieldCount;
            while (ds.Read())
            {
                for (int i = 0; i < columcount; i++)
                {
                    object obj = ds.GetValue(i);
                    if (obj == null)
                    {
                        dic.Add(ds.GetName(i), "");
                    }
                    else
                    {
                        dic.Add(ds.GetName(i), obj.ToString());
                    }
                }
                break;
            }

            return dic;
        }

        /// <summary>
        /// 返回结果集中的第一行第一列，忽略其他行或列
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>object</returns>
        public object ExecuteScalar(string cmdText, Dictionary<string, string> data = null)
        {
            var cmd = new SQLiteCommand();
            PrepareCommand(cmd, dbConnection, cmdText, data);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="recordCount">总记录数</param>
        /// <param name="pageIndex">页牵引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="countText">查询总记录数的Sql文本</param>
        /// <param name="data">命令参数</param>
        /// <returns>DataSet</returns>
        public DataSet ExecutePager(ref int recordCount, int pageIndex, int pageSize, string cmdText, string countText, Dictionary<string, string> data = null)
        {
            if (recordCount < 0)
                recordCount = int.Parse(ExecuteScalar(countText, data).ToString());
            var ds = new DataSet();
            var command = new SQLiteCommand();
            PrepareCommand(command, dbConnection, cmdText, data);
            var da = new SQLiteDataAdapter(command);
            da.Fill(ds, (pageIndex - 1) * pageSize, pageSize, "result");
            return ds;
        }

        /// <summary>
        /// 重新组织数据库：VACUUM 将会从头重新组织数据库
        /// </summary>
        public void ResetDataBass(SQLiteConnection conn)
        {
            var cmd = new SQLiteCommand();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = "vacuum";
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            cmd.ExecuteNonQuery();
        }

        public void CloseDb()
        {
            if (dbConnection.State == ConnectionState.Open)
                dbConnection.Close();

            dbConnection = null;
        }

        /// <summary>
        /// 初始化清空数据库
        /// </summary>
        public bool InitDb()
        {
            try
            {
                var dss = ExecuteReaderList("SELECT name,seq FROM sqlite_sequence");
                var transaction = BeginTransaction();
                foreach (var ds in dss)
                {
                    string tableName = ds["name"];
                    if (tableName == "localInfos")
                    {
                        continue;
                    }
                    ExecuteNonQuery($"DELETE FROM {tableName}");
                    ExecuteNonQuery($"UPDATE sqlite_sequence SET seq=0 where name='{tableName}'");
                }
                CommitTransaction(transaction);
                return true;
            }catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        public bool BackDb()
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(DataSource);
                File.Copy(DataSource, $"./db/backup/{fileNameWithoutExtension}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.db");
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;

            }
        }
    }

}


