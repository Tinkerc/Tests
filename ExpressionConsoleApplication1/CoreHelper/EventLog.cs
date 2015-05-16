
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;
using System.Timers;
using System.Web;

namespace CoreHelper
{
    /// <summary>
    /// 写日志
    /// 不想自动记录Context信息请调用Log(string message, string typeName, false)
    /// </summary>
    public class EventLog
    {
        [System.Serializable]
        public class LogItem
        {
            public System.DateTime Time
            {
                get;
                set;
            }
            public string Title
            {
                get;
                set;
            }
            public string Detail
            {
                get;
                set;
            }
            public string RequestUrl
            {
                get;
                set;
            }
            public string UrlReferrer
            {
                get;
                set;
            }
            public string HostIP
            {
                get;
                set;
            }
            public string UserAgent
            {
                get;
                set;
            }
            public override string ToString()
            {
                string str = this.Time.ToString("yy-MM-dd HH:mm:ss fffff");
                if (string.IsNullOrEmpty(this.Title))
                {
                    this.Title = this.Detail;
                    this.Detail = "";
                }
                if (!string.IsNullOrEmpty(this.Title))
                {
                    str = str + "  " + this.Title;
                }
                if (!string.IsNullOrEmpty(this.RequestUrl))
                {
                    str = str + "\r\nUrl:" + this.RequestUrl;
                }
                if (!string.IsNullOrEmpty(this.UrlReferrer))
                {
                    str = str + "\r\nUrlReferrer:" + this.UrlReferrer;
                }
                if (!string.IsNullOrEmpty(this.HostIP))
                {
                    str = str + "\r\nHostIP:" + this.HostIP;
                }
                if (!string.IsNullOrEmpty(this.UserAgent))
                {
                    str = str + "\r\n" + this.UserAgent;
                }
                if (!string.IsNullOrEmpty(this.Detail))
                {
                    str = str + "\r\n" + this.Detail;
                }
                return str + "\r\n";
            }
        }
        /// <summary>
        /// 项集合
        /// </summary>
        public class LogItemArry
        {
            public string savePath;
            private System.Collections.Generic.List<EventLog.LogItem> logs = new System.Collections.Generic.List<EventLog.LogItem>();
            public void Add(EventLog.LogItem log)
            {
                this.logs.Add(log);
            }
            public override string ToString()
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                foreach (EventLog.LogItem current in this.logs)
                {
                    stringBuilder.Append(current.ToString());
                }
                return stringBuilder.ToString();
            }
        }
        /// <summary>
        /// 是否使用上下文信息写日志
        /// </summary>
        public static bool UseContext = true;
        private static object lockObj = new object();
        private static bool Writing = false;
        private static System.DateTime lastWriteTime = System.DateTime.Now;
        private static System.Collections.Generic.Dictionary<string, EventLog.LogItemArry> logCaches = new System.Collections.Generic.Dictionary<string, EventLog.LogItemArry>();
        private static Timer timer;
        public static string LastError;
       
        private static bool enableSendToServer = true;
        private static string thisDomain = "";
        private static string secondFolder = null;
        private static string rootPath = null;
        /// <summary>
        /// 检查目录并建立
        /// </summary>
        /// <param name="path"></param>
        public static void CreateFolder(string path)
        {
            string text = "";
            string[] array = path.Split(new char[]
			{
				'\\'
			});
            for (int i = 0; i < array.Length; i++)
            {
                text = text + array[i] + "\\";
                if (!System.IO.Directory.Exists(text))
                {
                    System.IO.Directory.CreateDirectory(text);
                }
            }
        }
        /// <summary>
        /// 自定义文件名前辍写入日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="typeName"></param>
        /// <param name="useContext"></param>
        /// <returns></returns>
        public static bool Log(string message, string typeName, bool useContext)
        {
            return EventLog.Log(new EventLog.LogItem
            {
                Detail = message
            }, typeName, useContext);
        }
        public static bool Log(string message, string typeName)
        {
            return EventLog.Log(message, typeName, true);
        }
        /// <summary>
        /// 指定日志类型名生成日志
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool Log(EventLog.LogItem logItem, string typeName)
        {
            return EventLog.Log(logItem, typeName, true);
        }
        /// <summary>
        /// 指定日志类型名生成日志
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="typeName"></param>
        /// <param name="useContext">是否使用当前上下文信息</param>
        /// <returns></returns>
        public static bool Log(EventLog.LogItem logItem, string typeName, bool useContext)
        {
            string text = System.DateTime.Now.ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(typeName))
            {
                text = text + "." + typeName;
            }
          
            logItem.Time = System.DateTime.Now;
            
            return EventLog.WriteLog(EventLog.GetLogFolder(), logItem, text);
        }
        /// <summary>
        /// 生成日志,默认文件名
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sendToServer">是否发送到服务器</param>
        /// <returns></returns>
        public static bool Log(string message, bool sendToServer)
        {
            if (sendToServer)
            {
                EventLog.SendToServer(message, "");
            }
            return EventLog.WriteLog(message);
        }
        /// <summary>
        /// 生成日志,默认文件名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Log(string message)
        {
            return EventLog.WriteLog(message);
        }
        /// <summary>
        /// 生成日志,文件名以Error开头
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Error(string message)
        {
            return EventLog.Log(message, "Error");
        }
        /// <summary>
        /// 生成日志,文件名以Info开头
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Info(string message)
        {
            return EventLog.Log(message, "Info");
        }
        /// <summary>
        /// 生成日志,文件名以Debug开头
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Debug(string message)
        {
            return EventLog.Log(message, "Debug");
        }
        /// <summary>
        /// 在当前网站目录生成日志
        /// </summary>
        /// <param name="message"></param>
        public static bool WriteLog(string message)
        {
            return EventLog.Log(message, "");
        }
        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!EventLog.Writing)
            {
                EventLog.WriteLogFromCache();
            }
        }
        /// <summary>
        /// 指定路径,文件名,写入日志
        /// </summary>
        /// <param name="path"></param>
        /// <param name="logItem"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool WriteLog(string path, EventLog.LogItem logItem, string fileName)
        {
            bool result;
            try
            {
                if (EventLog.timer == null)
                {
                    EventLog.timer = new Timer();
                    EventLog.timer.Interval = 2000.0;
                    EventLog.timer.Elapsed += new ElapsedEventHandler(EventLog.timer_Elapsed);
                    EventLog.timer.Start();
                }
                if (!System.IO.Directory.Exists(path))
                {
                    EventLog.CreateFolder(path);
                }
                string text = path + fileName + ".txt";
                if (!EventLog.logCaches.ContainsKey(text))
                {
                    EventLog.logCaches.Add(text, new EventLog.LogItemArry
                    {
                        savePath = text
                    });
                }
                EventLog.logCaches[text].Add(logItem);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public static void WriteLogFromCache()
        {
            lock (EventLog.lockObj)
            {
                EventLog.Writing = true;
                if (EventLog.logCaches.Count > 0)
                {
                    System.Collections.Generic.Dictionary<string, EventLog.LogItemArry> dictionary = new System.Collections.Generic.Dictionary<string, EventLog.LogItemArry>(EventLog.logCaches);
                    foreach (System.Collections.Generic.KeyValuePair<string, EventLog.LogItemArry> current in dictionary)
                    {
                        EventLog.LogItemArry value = current.Value;
                        EventLog.LastError = null;
                        try
                        {
                            EventLog.WriteLine(value.ToString(), current.Key);
                        }
                        catch (System.Exception ex)
                        {
                            EventLog.LastError = ex.ToString();
                        }
                        EventLog.logCaches.Remove(current.Key);
                    }
                }
                EventLog.Writing = false;
            }
        }
        /// <summary>
        /// 把日志发送到消息服务端
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="requestUrl"></param>
        /// <param name="detail"></param>
        /// <param name="logName"></param>
        public static void SendToServer(string domain, string requestUrl, string detail, string logName)
        {
            
        }
        /// <summary>
        /// 使用当前Context信息把日志发送到服务端
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="logName"></param>
        public static void SendToServer(string detail, string logName)
        {
           
        }
        private static void WriteLine(string message, string filePath)
        {
            using (System.IO.FileStream fileStream = System.IO.File.OpenWrite(filePath))
            {
                System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(fileStream, System.Text.Encoding.GetEncoding("gb2312"));
                streamWriter.BaseStream.Seek(0L, System.IO.SeekOrigin.End);
                streamWriter.Write(message);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }
        /// <summary>
        /// 获取日志二级目录
        /// </summary>
        /// <returns></returns>
        public static string GetSecondFolder()
        {
            
            return EventLog.secondFolder;
        }
        /// <summary>
        /// 获取日志绝对目录
        /// </summary>
        /// <returns></returns>
        public static string GetLogFolder()
        {
            
            return EventLog.rootPath;
        }
    }
}
