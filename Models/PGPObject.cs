using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HSBC_Services.Models
{
    public class PGPObject
    {
        public class clsBank_AC_Res
        {
            public clsBank_AC_Ress result { get; set; }
            public string request_id { get; set; }
            public string status_Code { get; set; }
            public string request_string { get; set; }
            public string response_string { get; set; }
        }
        public class clsBank_AC_Ress
        {
            public string SERVICEPROVIDERID { get; set; }
            public string REQUESTID { get; set; }
            public string REQUESTTYPE { get; set; }
            public string REMITTORACCOUNTNO { get; set; }
            public string REMITTORNAME { get; set; }
            public string BENEFICIARYBANKIFSC { get; set; }
            public string BENEFICIARYACCOUNTNO { get; set; }
            public string AMOUNT { get; set; }
            public string CHANNELID { get; set; }
            public string AUTHENTICATIONTOKEN { get; set; }
            public string ADDITIONALINFO7 { get; set; }
            public string BeneficiaryName { get; set; }
        }

        public class clsBank_AC_ResPenny
        {
            public clsBank_AC_RessPenny result { get; set; }
            public string request_id { get; set; }
            public string status_Code { get; set; }
            public string request_string { get; set; }
            public string response_string { get; set; }
        }
        public class clsBank_AC_RessPenny
        {
            public string SERVICEPROVIDERID { get; set; }
            public string REQUESTID { get; set; }
            public string REQUESTTYPE { get; set; }
            public string REMITTORACCOUNTNO { get; set; }
            public string REMITTORNAME { get; set; }
            public string BENEFICIARYBANKIFSC { get; set; }
            public string BENEFICIARYACCOUNTNO { get; set; }
            public string AMOUNT { get; set; }
            public string CHANNELID { get; set; }
            public string AUTHENTICATIONTOKEN { get; set; }
            public string ADDITIONALINFO7 { get; set; }
        }

        public class clsBank_AC_ResEnquiry
        {
            public clsBank_AC_RessEnquiry result { get; set; }
            public string request_id { get; set; }
            public string status_Code { get; set; }
            public string request_string { get; set; }
            public string response_string { get; set; }
        }
        public class clsBank_AC_RessEnquiry
        {
            public string REQUESTID { get; set; }
            public string REQUESTTYPE { get; set; }
            public string REMITTORACCOUNTNO { get; set; }
            public string AMOUNT { get; set; }
            public string CHANNELID { get; set; }
            public string AUTHENTICATIONTOKEN { get; set; }
            public string MERCHANTID { get; set; }
        }

        public class clsBank_AC_Req
        {
            public string FormNo { get; set; }
        }

        public class GetAccessTokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string responseBase64 { get; set; }
        }

        public class FinalGetAccessTokenResponse : ErrorResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string responseBase64 { get; set; }
        }

        public class ErrorResponse
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }
    }
}