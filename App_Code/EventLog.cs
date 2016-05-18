using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Net;

/// <summary>
/// EventLog 的摘要描述
/// </summary>
public static class EventLog
{
    public static string FilePath { get; set; }
    public static string m_bDebugMode { get; set; }

    public static void Write(string format, params object[] arg)
    {
        Write(string.Format(format, arg));
    }

    public static void Write(string message)
    {
        m_bDebugMode = System.Web.Configuration.WebConfigurationManager.AppSettings["Debug_Mode"];
        if (m_bDebugMode == "True")
        {
            string FilePath = Path.Combine(HttpRuntime.AppDomainAppPath, DateTime.Now.ToString("yyyy_MMdd") + "_WebService.txt");
            FileInfo finfo = new FileInfo(FilePath);
            if (finfo.Directory.Exists == false)
            {
                finfo.Directory.Create();
            }
            string writeString = string.Format("{0:yyyy/MM/dd HH:mm:ss.fff} {1}",
                DateTime.Now, message) + Environment.NewLine;
            File.AppendAllText(FilePath, writeString, Encoding.Unicode);
        }
    }
}