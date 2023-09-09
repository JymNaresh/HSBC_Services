using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;


    public class Exceptions : ApiController
    {
       
   
        public static void Publish(string exception, NameValueCollection AdditionalInfo)
        {

            string m_LogName = AppDomain.CurrentDomain.BaseDirectory + "ErrorList//ErrorLog.txt";
            
            StringBuilder strInfo = new StringBuilder();

            
            if (AdditionalInfo != null)
            {
                 
                strInfo.AppendFormat("{0}General Information{0}", Environment.NewLine);
                strInfo.AppendFormat("{0}Additonal Info:", Environment.NewLine);
                foreach (string i in AdditionalInfo)
                {
                    strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, i, AdditionalInfo.Get(i));
                }
            }
             
            strInfo.AppendFormat("{0}{0}Exception Information{0}{1}", Environment.NewLine, exception.ToString());

         

            string str = "";
            using (FileStream fs = File.OpenRead(m_LogName))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    str = sr.ReadToEnd();
                }
            }

            using (FileStream fs = File.Open(m_LogName, FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(str + strInfo.ToString());
                }
            }
            // send notification email if operatorMail attribute was provided
            //			if (m_OpMail.Length > 0)
            //			{
            //				string subject = "Exception Notification";
            //				string body = strInfo.ToString();
            //SmtpClient o = new SmtpClient();
            //o.Send(
            //SmtpMail.Send("CustomPublisher@mycompany.com", m_OpMail, subject, body);
            //			}
        }

       
    }