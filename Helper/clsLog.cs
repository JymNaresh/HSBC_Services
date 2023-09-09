using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Configuration;


namespace HSBC_Services.Helper
{
    public static class clsLog
    {
        public static void WriteLog(Exception ex)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string strPath = ConfigurationManager.AppSettings["LogFile"].ToString() + System.DateTime.Now.ToLongDateString() + ".txt";
                sb.Append(System.DateTime.Now.ToString() + Environment.NewLine);
                sb.Append("Msg:" + ex.Message + Environment.NewLine);
                sb.Append("StackTrace: " + ex.InnerException.StackTrace + Environment.NewLine);
                sb.Append("-------------------------------------------------------");


                StreamWriter sw = new StreamWriter(strPath);

                sw.Write(sb.ToString());

                sw.Close();
            }
            catch (Exception ex1)
            { Console.WriteLine(ex1.Message); }
            finally
            { }
        }

        public static void WriteLog(string strLog)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string strPath = ConfigurationManager.AppSettings["LogFile"].ToString() + System.DateTime.Now.ToLongDateString() + ".txt";
                sb.Append(System.DateTime.Now.ToString() + Environment.NewLine);
                sb.Append("Msg:" + strLog + Environment.NewLine);
                sb.Append("-------------------------------------------------------" + Environment.NewLine);
                StreamWriter sw = new StreamWriter(strPath, true);

                sw.Write(sb.ToString());
                sw.Close();
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
            finally
            { }
        }
    }
}
