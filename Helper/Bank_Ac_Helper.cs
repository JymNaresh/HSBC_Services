using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using HSBC_Services.Models;
using System.Data;
using System.Data.SqlClient;

namespace HSBC_Services.Helper
{
    public class Bank_Ac_Helper
    {
        DbContext DB = new DbContext(ConfigurationManager.AppSettings["CONN_STR"]);
        public DataTable DTBank_AC(PGPObject.clsBank_AC_Req objreqBank)
        {
            try
            {

                SqlParameter[] sp = new SqlParameter[4];
                
                sp[1] = new SqlParameter("@FormNo", SqlDbType.VarChar, -1);
                sp[1].Value = objreqBank.FormNo;

                sp[2] = new SqlParameter("@Flag", SqlDbType.VarChar, -1);
                sp[2].Value = "Rq";


                SqlConnection _sqlCon = new SqlConnection(CommonSetting.GetConnectionString(true));
                DataSet ds = SqlHelper.ExecuteDataset(_sqlCon, CommandType.StoredProcedure, "Usp_get_HSBC_Data", sp);

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable DTBank_Response(PGPObject.clsBank_AC_Res objresBank)
        {
            try
            {

                SqlParameter[] sp = new SqlParameter[6];

                
                sp[1] = new SqlParameter("@FormNo", SqlDbType.VarChar, -1);
                sp[1].Value = objresBank.request_id;

                sp[2] = new SqlParameter("@Status", SqlDbType.VarChar, -1);
                sp[2].Value = objresBank.status_Code;

                sp[3] = new SqlParameter("@Flag", SqlDbType.VarChar, -1);
                sp[3].Value = "Rs";

                sp[4] = new SqlParameter("@Req_String", SqlDbType.VarChar, -1);
                sp[4].Value = objresBank.request_string;

                sp[5] = new SqlParameter("@Res_String", SqlDbType.VarChar, -1);
                sp[5].Value = objresBank.response_string;

                SqlConnection _sqlCon = new SqlConnection(CommonSetting.GetConnectionString(true));
                DataSet ds = SqlHelper.ExecuteDataset(_sqlCon, CommandType.StoredProcedure, "Usp_get_HSBC_Data", sp);

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
