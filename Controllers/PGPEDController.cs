using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using EncryptionSample;
using HSBC_Services.Models;
using HSBC_Services.Helper;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using Newtonsoft.Json;
using System.Reflection;
using RestSharp;
using System.Security.Authentication;
using SPL.Crypto;
using PGPSnippet.PGPDecryption;

namespace HSBC_Services.Controllers
{
    public class PGPEDController : ApiController
    {
        Bank_Ac_Helper objCGT = new Bank_Ac_Helper();
        [HttpPost]
        [Route("API/Bank_Account_No_Validation")]
        //public IHttpActionResult BankAccountValidation(PGPObject.clsBank_AC_Req objBANKupload)
        public IHttpActionResult BankAccountValidation(PGPObject.clsBank_AC_RessPenny targetresult)
        {
            
            PGPObject.clsBank_AC_ResPenny target = new PGPObject.clsBank_AC_ResPenny();
            //PGPObject.clsBank_AC_Ress targetresult = new PGPObject.clsBank_AC_Ress();
            string SYSTIME = System.DateTime.Now.ToString("yyyyMMddHHmmssffff");
            var jsonresult = new JavaScriptSerializer().Serialize(targetresult);
            //string backstring = targetresult.ToString();
            //BR*  To convert JSON text contained in string json into an XML node
           XmlDocument backstringxml = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonresult, "p2aRequest");
           //string Request= backstringxml.InnerXml.ToString().Replace("Root", "p2aRequest");
            //backstringxml.InnerXml.ToString().Replace("Root", "p2aRequest");


            clsLog.WriteLog("Bank_Account_No_Validation Request:" + jsonresult);

            StringBuilder sb = new StringBuilder();

            string strPath = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString()+"_"+ SYSTIME + "_RequestString" + ".txt";
            //sb.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb.Append(backstringxml.InnerXml.ToString() + Environment.NewLine);
            //sb.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw = new StreamWriter(strPath, true);

            sw.Write(sb.ToString());
            sw.Close();

            
            string PGPPath = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_PGPEncoded" + ".asc";
            string bas64path = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_Base64Encoded" + ".txt";
            PGP_ENCRYPTION(strPath, PGPPath, bas64path);
            string Base64 = CryptoHelper.Base64Encode(PGPPath);
            string clientid= ConfigurationManager.AppSettings["clientid"].ToString();
            string clientsecret= ConfigurationManager.AppSettings["clientsecret"].ToString();
            string ProfileId= ConfigurationManager.AppSettings["ProfileId"].ToString();

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol = Tls12;
            string HSBCURL = ConfigurationManager.AppSettings["URL"].ToString();

            //var client = new RestClient("https://devcluster.api.p2g.netd2.hsbc.com.hk/cmb-connect-payments-pa-payment-cert-proxy/v1/payments/instant-settlement");
            var client = new RestClient(HSBCURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AddHeader("x-hsbc-client-id", "8d46061475eb4f198edd0d69c359435a");
            //request.AddHeader("x-hsbc-client-secret", "C30A4ba72CAd46D48EaCCabE894178f5");
            request.AddHeader("x-hsbc-client-id", clientid);
            request.AddHeader("x-hsbc-client-secret", clientsecret);
            //request.AddHeader("x-hsbc-profile-id", "PC000023633");
            request.AddHeader("x-hsbc-profile-id", ProfileId);
            request.AddHeader("x-payload-type", "XMLPF");
            request.AddHeader("x-hsbc-unique-ref-id", targetresult.REQUESTID.ToString());
            request.AddHeader("x-hsbc-payment-type", "IMPSRTP");
            request.AddHeader("x-hsbc-country-code", "IN");
           // request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Content-Type", "application/json");

            var body = new JavaScriptSerializer().Serialize("paymentBase64");
            var body1 = new JavaScriptSerializer().Serialize(":");
            var body2 = new JavaScriptSerializer().Serialize(Base64.ToString());
            body = "{" + body.ToString() + ":" + body2.ToString()+"}";
            //body = "{"+body.ToString() + body1.ToString()+ body2.ToString();
            //var Finalbody = new JavaScriptSerializer().Serialize(body.ToString());
            //var body = Base64.ToString();
            //request.AddParameter("application/xml", Request, ParameterType.RequestBody);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            string backstring = response.Content.ToString();

            target.request_id = targetresult.REQUESTID.ToString();
            target.status_Code = "";
            target.request_string = backstringxml.InnerXml.ToString();
            target.response_string = backstring.ToString();
            //DataTable DT1 = objCGT.DTBank_Response(target);

            StringBuilder sb1 = new StringBuilder();

            string strPath1 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME+ "_ResponseString" + ".txt";
            sb1.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb1.Append("Bank_Account_No_Validation Response:" + backstring.ToString() + Environment.NewLine);
            sb1.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw1 = new StreamWriter(strPath1, true);

            sw1.Write(sb1.ToString());
            sw1.Close();

            var ResObj = new PGPObject.GetAccessTokenResponse();
            //var ErrObj = new PGPObject.ErrorResponse();

            
            ResObj = JsonConvert.DeserializeObject<PGPObject.GetAccessTokenResponse>(response.Content);

            var FinalResObj = new PGPObject.FinalGetAccessTokenResponse();
            FinalResObj.access_token = ResObj.access_token;
            FinalResObj.token_type = ResObj.token_type;
            FinalResObj.expires_in = ResObj.expires_in;
            FinalResObj.responseBase64 = ResObj.responseBase64;
            if (FinalResObj.responseBase64 == "")
            {
                PGPObject.clsBank_AC_ResPenny OBJERR = new PGPObject.clsBank_AC_ResPenny();

                OBJERR.result = targetresult;
                OBJERR.request_id = targetresult.REQUESTID.ToString();
                OBJERR.status_Code = "";
                OBJERR.request_string = backstringxml.InnerXml.ToString();
                OBJERR.response_string = "No Response";
                return Ok(OBJERR);
            }
                StringBuilder sb2 = new StringBuilder();

                string strPath2 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString_Base64" + ".txt";
                //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
                sb2.Append(FinalResObj.responseBase64.ToString() + Environment.NewLine);
                //sb2.Append("-------------------------------------------------------" + Environment.NewLine);
                StreamWriter sw2 = new StreamWriter(strPath2, true);

                sw2.Write(sb2.ToString());
                sw2.Close();

                string Base64Decoded = CryptoHelper.Base64Decode(strPath2);


                StringBuilder sb3 = new StringBuilder();

                string strPath3 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString_PGP" + ".txt";
                //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
                sb3.Append(Base64Decoded.ToString() + Environment.NewLine);
                //sb2.Append("-------------------------------------------------------" + Environment.NewLine);
                StreamWriter sw3 = new StreamWriter(strPath3, true);

                sw3.Write(sb3.ToString());
                sw3.Close();



                string strPath4 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_FinalResponseString_Decoded" + ".txt";
                //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
                //sb3.Append(Base64Decoded.ToString() + Environment.NewLine);
                //sb2.Append("-------------------------------------------------------" + Environment.NewLine);


                //PGP_Decryption(strPath3, strPath4);
                PGPDecrypt.Decrypt(strPath3, strPath4);
                string RESS = System.IO.File.ReadAllText(strPath4);
                PGPObject.clsBank_AC_ResPenny OBJ = new PGPObject.clsBank_AC_ResPenny();

                OBJ.result = targetresult;
                OBJ.request_id = targetresult.REQUESTID.ToString();
                OBJ.status_Code = "";
                OBJ.request_string = backstringxml.InnerXml.ToString();
                OBJ.response_string = RESS.ToString();
                return Ok(OBJ);
                //return FinalResObj;
            
        }

        [HttpPost]
        [Route("API/PGP_Encryption")]
        public IHttpActionResult PGP_ENCRYPTION(string inputpath,string outputpath, string bas64path)
        {
            // note: all key info is in app.config

            // pass in a string encrypted data to decrypt
            //string decrypted = CryptoHelper.DecryptPgpData("-----BEGIN PGP MESSAGE----- some pgp-wrapped encrypted string that the private key and password will open");

            // pass in 2 file paths to generate the encrypted file
            // (IoHelper.BasePath is just the path where the executable is running)
            //CryptoHelper.EncryptPgpFile(inputpath, outputpath);
            PGPDecrypt.EncryptAndSign(inputpath, outputpath);
            string Base64= CryptoHelper.Base64Encode(outputpath, bas64path);
            
            // if you need to convert a private key from a pgp to xml format:
            //string xmlPPrivateKey = CryptoHelper.GetPrivateKeyXml("-----BEGIN PGP PRIVATE KEY BLOCK----- a pgp private key");
            PGPObject.clsBank_AC_Res OBJ = new PGPObject.clsBank_AC_Res();
            OBJ.status_Code = Base64;
            return Ok(OBJ);
        }

        

        [HttpPost]
        [Route("API/PGP_Decryption")]
        public IHttpActionResult PGP_Decryption(string inputpath, string outputpath)
        {
            // note: all key info is in app.config

            // pass in a string encrypted data to decrypt
            string decrypted = CryptoHelper.DecryptPgpData(inputpath);

            StringBuilder sb4 = new StringBuilder();
            sb4.Append(decrypted.ToString() + Environment.NewLine);

            StreamWriter sw4 = new StreamWriter(outputpath, true);

            sw4.Write(sb4.ToString());
            sw4.Close();


            // pass in 2 file paths to generate the encrypted file
            // (IoHelper.BasePath is just the path where the executable is running)
            //CryptoHelper.EncryptPgpFile(IoHelper.BasePath + @"\plain-text.txt", IoHelper.BasePath + @"\pgp-encrypted.asc");

            // if you need to convert a private key from a pgp to xml format:
            //string xmlPPrivateKey = CryptoHelper.GetPrivateKeyXml("-----BEGIN PGP PRIVATE KEY BLOCK----- a pgp private key");
            PGPObject.clsBank_AC_Res OBJ = new PGPObject.clsBank_AC_Res();
            return Ok(OBJ);
        }

        [HttpPost]
        [Route("API/EnquiryStatus")]
        //public IHttpActionResult BankAccountValidation(PGPObject.clsBank_AC_Req objBANKupload)
        public IHttpActionResult EnquiryStatus(PGPObject.clsBank_AC_RessEnquiry targetresult)
        {

            PGPObject.clsBank_AC_ResEnquiry target = new PGPObject.clsBank_AC_ResEnquiry();
            //PGPObject.clsBank_AC_Ress targetresult = new PGPObject.clsBank_AC_Ress();
            string SYSTIME = System.DateTime.Now.ToString("yyyyMMddHHmmssffff");
            var jsonresult = new JavaScriptSerializer().Serialize(targetresult);
            //string backstring = targetresult.ToString();
            //BR*  To convert JSON text contained in string json into an XML node
            XmlDocument backstringxml = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonresult, "p2aRequest");
            //string Request= backstringxml.InnerXml.ToString().Replace("Root", "p2aRequest");
            //backstringxml.InnerXml.ToString().Replace("Root", "p2aRequest");


            clsLog.WriteLog("EnquiryStatus Request:" + jsonresult);

            StringBuilder sb = new StringBuilder();

            string strPath = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_RequestString" + ".txt";
            //sb.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb.Append(backstringxml.InnerXml.ToString() + Environment.NewLine);
            //sb.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw = new StreamWriter(strPath, true);

            sw.Write(sb.ToString());
            sw.Close();


            string PGPPath = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_PGPEncoded" + ".asc";
            string bas64path = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_Base64Encoded" + ".txt";
            PGP_ENCRYPTION(strPath, PGPPath, bas64path);
            string Base64 = CryptoHelper.Base64Encode(PGPPath);
            string clientid = ConfigurationManager.AppSettings["clientid"].ToString();
            string clientsecret = ConfigurationManager.AppSettings["clientsecret"].ToString();
            string ProfileId = ConfigurationManager.AppSettings["ProfileId"].ToString();

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol = Tls12;
            string HSBCURL = ConfigurationManager.AppSettings["EnquiryURL"].ToString();

            var client = new RestClient(HSBCURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AddHeader("x-hsbc-client-id", "8d46061475eb4f198edd0d69c359435a");
            //request.AddHeader("x-hsbc-client-secret", "C30A4ba72CAd46D48EaCCabE894178f5");
            request.AddHeader("x-hsbc-client-id", clientid);
            request.AddHeader("x-hsbc-client-secret", clientsecret);
            //request.AddHeader("x-hsbc-profile-id", "PC000023633");
            request.AddHeader("x-hsbc-profile-id", ProfileId);
            request.AddHeader("x-payload-type", "XMLPF");
            request.AddHeader("x-hsbc-unique-ref-id", targetresult.REQUESTID.ToString());
            request.AddHeader("x-hsbc-payment-type", "IMPSRTP");
            request.AddHeader("x-hsbc-country-code", "IN");
            // request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Content-Type", "application/json");

            var body = new JavaScriptSerializer().Serialize("paymentEnquiryBase64");
            var body1 = new JavaScriptSerializer().Serialize(":");
            var body2 = new JavaScriptSerializer().Serialize(Base64.ToString());
            body = "{" + body.ToString() + ":" + body2.ToString() + "}";
            //body = "{"+body.ToString() + body1.ToString()+ body2.ToString();
            //var Finalbody = new JavaScriptSerializer().Serialize(body.ToString());
            //var body = Base64.ToString();
            //request.AddParameter("application/xml", Request, ParameterType.RequestBody);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            string backstring = response.Content.ToString();

            target.request_id = targetresult.REQUESTID.ToString();
            target.status_Code = "";
            target.request_string = backstringxml.InnerXml.ToString();
            target.response_string = backstring.ToString();
            //DataTable DT1 = objCGT.DTBank_Response(target);

            StringBuilder sb1 = new StringBuilder();

            string strPath1 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString" + ".txt";
            sb1.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb1.Append("EnquiryStatus Response:" + backstring.ToString() + Environment.NewLine);
            sb1.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw1 = new StreamWriter(strPath1, true);

            sw1.Write(sb1.ToString());
            sw1.Close();

            var ResObj = new PGPObject.GetAccessTokenResponse();
            //var ErrObj = new PGPObject.ErrorResponse();


            ResObj = JsonConvert.DeserializeObject<PGPObject.GetAccessTokenResponse>(response.Content);

            var FinalResObj = new PGPObject.FinalGetAccessTokenResponse();
            FinalResObj.access_token = ResObj.access_token;
            FinalResObj.token_type = ResObj.token_type;
            FinalResObj.expires_in = ResObj.expires_in;
            FinalResObj.responseBase64 = ResObj.responseBase64;
            if (FinalResObj.responseBase64 == "")
            {
                PGPObject.clsBank_AC_ResEnquiry OBJERR = new PGPObject.clsBank_AC_ResEnquiry();

                OBJERR.result = targetresult;
                OBJERR.request_id = targetresult.REQUESTID.ToString();
                OBJERR.status_Code = "";
                OBJERR.request_string = backstringxml.InnerXml.ToString();
                OBJERR.response_string = "No Response";
                return Ok(OBJERR);
            }
            StringBuilder sb2 = new StringBuilder();

            string strPath2 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString_Base64" + ".txt";
            //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb2.Append(FinalResObj.responseBase64.ToString() + Environment.NewLine);
            //sb2.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw2 = new StreamWriter(strPath2, true);

            sw2.Write(sb2.ToString());
            sw2.Close();

            string Base64Decoded = CryptoHelper.Base64Decode(strPath2);


            StringBuilder sb3 = new StringBuilder();

            string strPath3 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString_PGP" + ".txt";
            //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb3.Append(Base64Decoded.ToString() + Environment.NewLine);
            //sb2.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw3 = new StreamWriter(strPath3, true);

            sw3.Write(sb3.ToString());
            sw3.Close();



            string strPath4 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_FinalResponseString_Decoded" + ".txt";
            //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            //sb3.Append(Base64Decoded.ToString() + Environment.NewLine);
            //sb2.Append("-------------------------------------------------------" + Environment.NewLine);


            //PGP_Decryption(strPath3, strPath4);
            PGPDecrypt.Decrypt(strPath3, strPath4);
            string RESS = System.IO.File.ReadAllText(strPath4);
            PGPObject.clsBank_AC_ResEnquiry OBJ = new PGPObject.clsBank_AC_ResEnquiry();

            OBJ.result = targetresult;
            OBJ.request_id = targetresult.REQUESTID.ToString();
            OBJ.status_Code = "";
            OBJ.request_string = backstringxml.InnerXml.ToString();
            OBJ.response_string = RESS.ToString();
            return Ok(OBJ);
            //return FinalResObj;

        }

        [HttpPost]
        [Route("API/NEFT")]
        //public IHttpActionResult BankAccountValidation(PGPObject.clsBank_AC_Req objBANKupload)
        public IHttpActionResult NEFT(PGPObject.clsBank_AC_Ress targetresult)
        {

            PGPObject.clsBank_AC_Res target = new PGPObject.clsBank_AC_Res();
            //PGPObject.clsBank_AC_Ress targetresult = new PGPObject.clsBank_AC_Ress();
            string SYSTIME = System.DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string ProfileId = ConfigurationManager.AppSettings["ProfileId"].ToString();
            string URL = ConfigurationManager.AppSettings["URL"].ToString();
            string CreditTime = string.Empty;
            CreditTime = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss");




            var ReqString = @"<Document xmlns=""urn:iso:std:iso:20022:tech:xsd:pain.001.001.03"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
" + @"<CstmrCdtTrfInitn>
" + @"<GrpHdr>
" + @"<MsgId>" + targetresult.REQUESTID + @"</MsgId>
" + @"<CreDtTm>" + CreditTime + @"</CreDtTm>
" + @"<NbOfTxs>" + "1" + @"</NbOfTxs>
" + @"<CtrlSum>" + targetresult.AMOUNT + @"</CtrlSum>
" + @"<InitgPty>
" + @"<Id>
" + @"<OrgId>
" + @"<Othr>
" + @"<Id>" + ProfileId + @"</Id>
" + @"</Othr>
" + @"</OrgId>
" + @"</Id>
" + @"</InitgPty>
" + @"</GrpHdr>
" + @"<PmtInf>
" + @"<PmtInfId>" + targetresult.REQUESTID + @"</PmtInfId>
" + @"<PmtMtd>" + "TRF" + @"</PmtMtd>
" + @"<PmtTpInf>
" + @"<SvcLvl>
" + @"<Cd>" + "URNS" + @"</Cd>
" + @"</SvcLvl>
" + @"</PmtTpInf>
" + @"<ReqdExctnDt>" + DateTime.Now.ToString("yyyy-MM-dd") + @"</ReqdExctnDt>
" + @"<Dbtr>
" + @"<Nm>" + "SPANDANA SPHOORTY FINANCIAL LIMITED" + @"</Nm>
" + @"<PstlAdr>
" + @"<StrtNm>" + "GALAXY WING B 16TH FLOOR PLOT NO.1" + @"</StrtNm>
" + @"<PstCd>" + "500081" + @"</PstCd>
" + @"<TwnNm>" + "HYDERABAD" + @"</TwnNm>
" + @"<Ctry>" + "IN" + @"</Ctry>
" + @"</PstlAdr>
" + @"</Dbtr>
" + @"<DbtrAcct>
" + @"<Id>
" + @"<Othr>
" + @"<Id>" + targetresult.REMITTORACCOUNTNO + @"</Id>
" + @"</Othr>
" + @"</Id>
" + @"<Ccy>" + "INR" + @"</Ccy>
" + @"</DbtrAcct>
" + @"<DbtrAgt>
" + @"<FinInstnId>
" + @"<BIC>" + "HSBCINBB" + @"</BIC>
" + @"<PstlAdr>
" + @"<Ctry>" + "IN" + @"</Ctry>
" + @"</PstlAdr>
" + @"</FinInstnId>
" + @"</DbtrAgt>
" + @"<CdtTrfTxInf>
" + @"<PmtId>
" + @"<InstrId>" + targetresult.REQUESTID + @"</InstrId>
" + @"<EndToEndId>" + targetresult.REQUESTID + @"</EndToEndId>
" + @"</PmtId>
" + @"<Amt>
" + @"<InstdAmt Ccy=""INR"">" + targetresult.AMOUNT + @"</InstdAmt>
" + @"</Amt>
" + @"<ChrgBr>" + "DEBT" + @"</ChrgBr>
" + @"<CdtrAgt>
" + @"<FinInstnId>
" + @"<ClrSysMmbId>
" + @"<MmbId>" + targetresult.BENEFICIARYBANKIFSC + @"</MmbId>
" + @"</ClrSysMmbId>
" + @"<PstlAdr>
" + @"<Ctry>" + "IN" + @"</Ctry>
" + @"</PstlAdr>
" + @"</FinInstnId>
" + @"</CdtrAgt>
" + @"<Cdtr>
" + @"<Nm>" + targetresult.BeneficiaryName + @"</Nm>
" + @"<PstlAdr>
" + @"<Ctry>" + "IN" + @"</Ctry>
" + @"</PstlAdr>
" + @"</Cdtr>
" + @"<CdtrAcct>
" + @"<Id>
" + @"<Othr>
" + @"<Id>" + targetresult.BENEFICIARYACCOUNTNO + @"</Id>
" + @"</Othr>
" + @"</Id>
" + @"</CdtrAcct>
" + @"<RmtInf>
" + @"<Ustrd>" + "Testing Narration" + @"</Ustrd>
" + @"</RmtInf>
" + @"</CdtTrfTxInf>
" + @"</PmtInf>
" + @"</CstmrCdtTrfInitn>
" + @"</Document>";
            //ReqString = ReqString.Replace(" ", "");
            clsLog.WriteLog("HSBC NEFT ReqString:" + ReqString);



            StringBuilder sb = new StringBuilder();

            string strPath = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_RequestString" + ".txt";
            //sb.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb.Append(ReqString.ToString() + Environment.NewLine);
            //sb.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw = new StreamWriter(strPath, true);

            sw.Write(sb.ToString());
            sw.Close();


            string PGPPath = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_PGPEncoded" + ".asc";
            string bas64path = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_Base64Encoded" + ".txt";
            PGP_ENCRYPTION(strPath, PGPPath, bas64path);
            string Base64 = CryptoHelper.Base64Encode(PGPPath);
            string clientid = ConfigurationManager.AppSettings["clientid"].ToString();
            string clientsecret = ConfigurationManager.AppSettings["clientsecret"].ToString();
            //string ProfileId= ConfigurationManager.AppSettings["ProfileId"].ToString();

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol = Tls12;
            string HSBCURL = ConfigurationManager.AppSettings["NEFTURL"].ToString();

            var client = new RestClient(HSBCURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AddHeader("x-hsbc-client-id", "8d46061475eb4f198edd0d69c359435a");
            //request.AddHeader("x-hsbc-client-secret", "C30A4ba72CAd46D48EaCCabE894178f5");
            request.AddHeader("x-hsbc-client-id", clientid);
            request.AddHeader("x-hsbc-client-secret", clientsecret);
            request.AddHeader("x-hsbc-profile-id", ProfileId);
            request.AddHeader("x-payload-type", "pain.001.001.03");
            //request.AddHeader("x-hsbc-unique-ref-id", targetresult.FormNumber.ToString());
            request.AddHeader("x-hsbc-country-code", "IN");

            var body = new JavaScriptSerializer().Serialize("paymentBase64");
            var body1 = new JavaScriptSerializer().Serialize(":");
            var body2 = new JavaScriptSerializer().Serialize(Base64.ToString());
            body = "{" + body.ToString() + ":" + body2.ToString() + "}";
            //body = "{"+body.ToString() + body1.ToString()+ body2.ToString();
            //var Finalbody = new JavaScriptSerializer().Serialize(body.ToString());
            //var body = Base64.ToString();
            //request.AddParameter("application/xml", Request, ParameterType.RequestBody);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            string backstring = response.Content.ToString();

            target.request_id = targetresult.REQUESTID.ToString();
            target.status_Code = "";
            target.request_string = ReqString.ToString();
            target.response_string = backstring.ToString();
            DataTable DT1 = objCGT.DTBank_Response(target);

            StringBuilder sb1 = new StringBuilder();

            string strPath1 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString" + ".txt";
            sb1.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb1.Append("Bank_Account_No_Validation Response:" + backstring.ToString() + Environment.NewLine);
            sb1.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw1 = new StreamWriter(strPath1, true);

            sw1.Write(sb1.ToString());
            sw1.Close();

            var ResObj = new PGPObject.GetAccessTokenResponse();
            //var ErrObj = new PGPObject.ErrorResponse();


            ResObj = JsonConvert.DeserializeObject<PGPObject.GetAccessTokenResponse>(response.Content);

            var FinalResObj = new PGPObject.FinalGetAccessTokenResponse();
            FinalResObj.access_token = ResObj.access_token;
            FinalResObj.token_type = ResObj.token_type;
            FinalResObj.expires_in = ResObj.expires_in;
            FinalResObj.responseBase64 = ResObj.responseBase64;
            if (FinalResObj.responseBase64 == "")
            {
                PGPObject.clsBank_AC_Res OBJERR = new PGPObject.clsBank_AC_Res();

                OBJERR.result = targetresult;
                OBJERR.request_id = targetresult.REQUESTID.ToString();
                OBJERR.status_Code = "";
                OBJERR.request_string = ReqString.ToString();
                OBJERR.response_string = "No Response";
                return Ok(OBJERR);
            }
            StringBuilder sb2 = new StringBuilder();

            string strPath2 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString_Base64" + ".txt";
            //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb2.Append(FinalResObj.responseBase64.ToString() + Environment.NewLine);
            //sb2.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw2 = new StreamWriter(strPath2, true);

            sw2.Write(sb2.ToString());
            sw2.Close();

            string Base64Decoded = CryptoHelper.Base64Decode(strPath2);


            StringBuilder sb3 = new StringBuilder();

            string strPath3 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_ResponseString_PGP" + ".txt";
            //sb2.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb3.Append(Base64Decoded.ToString() + Environment.NewLine);
            //sb2.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw3 = new StreamWriter(strPath3, true);

            sw3.Write(sb3.ToString());
            sw3.Close();



            string strPath4 = ConfigurationManager.AppSettings["LogFile"].ToString() + targetresult.REQUESTID.ToString() + "_" + SYSTIME + "_FinalResponseString_Decoded" + ".txt";

            //PGP_Decryption(strPath3, strPath4);
            PGPDecrypt.Decrypt(strPath3, strPath4);
            string RESS = System.IO.File.ReadAllText(strPath4);
            PGPObject.clsBank_AC_Res OBJ = new PGPObject.clsBank_AC_Res();

            OBJ.result = targetresult;
            OBJ.request_id = targetresult.REQUESTID.ToString();
            OBJ.status_Code = "";
            OBJ.request_string = ReqString.ToString();
            OBJ.response_string = RESS.ToString();
            return Ok(OBJ);
            //return FinalResObj;

        }
    }
}
