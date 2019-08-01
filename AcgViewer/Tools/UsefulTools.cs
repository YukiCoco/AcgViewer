using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgViewer.Tools
{
    public class UsefulTools
    {

    }

    public class ExcepitonLog
    {
        private static object m_Lock = new object();
        public static void WriteLog(Exception ce)
        {
            string dic = @"Logs";
            if (!Directory.Exists(dic))//判断是否存在
            {
                Directory.CreateDirectory(dic);//创建新路径
            }
            lock (m_Lock)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(dic + "\\" + "Log.log", true))
                {
                    file.WriteLine("------------------------------" + DateTime.Now.ToString() + "------------------------------");
                    file.WriteLine(ce.StackTrace);
                    file.WriteLine(ce.Source);
                    file.WriteLine(ce.TargetSite);
                    file.WriteLine(ce.Message);
                }
            }
        }
    }
}
