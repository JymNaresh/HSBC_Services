using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;


public class CommonSetting
{
    //public static string SQLConnectionString = ReadConfigFileValue("CONN_STR");

    
    public CommonSetting()
    {

    }


    public static string GetMasterDBName()
    {
        try
        {
            return "eFINO";
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    public static string GetConnectionString(bool IsMasterDataBase)
    {
        try
        {
           

            string SQLString = ReadConfigFileValue("CONN_STR");
            if (IsMasterDataBase == true)
            {
                SQLString = SQLString.Replace("#DBNAME#", CommonSetting.GetMasterDBName().ToString());
            }
           
            return SQLString;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

   

    public static string ReadConfigFileValue(string Key)
    {
        try
        {
            string Value = "";
            Value = ConfigurationManager.AppSettings[Key];
            return Value;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


}

