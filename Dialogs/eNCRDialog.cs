using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class eNCRDialog : IDialog<object>
    {
        public string strNcrDetail = "";
        public string strProsubCat = "0";
        public string strProeNcrfoud = "";
        public string strProDesc = "";
        public string strProLoc = "0";
        public string strProDepart = "0";
        public string strProCat = "0";
        public string strProSubCat1 = "0";
        public string strProHolCon = "";
        public string strProSubCon = "";
        public string strSyssubCat = "";
        public string strStartDate = "";
        public string strSyseNCRFound = "";
        public string strSysDesc = "";
        public string strSysLoc = "";
        public string strSysDepart = "";
        public string strSysCat = "";
        public string strSysSubCat1 = "";
        public string strSysHolCon = "";
        public string strSysSubCon = "";
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";
        public string strStratTime = "";
        public string strSysEndTime = "";
        public string strProEndTime = "";
        string[] result1 = new string[0];
        string[] result2 = new string[0];
        string strArry = "";
        string strArry1 = "";
        DataSet ds = new DataSet();
        public string StrSysattachmenturl = "";
        public string StrSysattachmentname = "";
        public string StrProattachmenturl = "";
        public string StrProattachmentname = "";

        public async Task StartAsync(IDialogContext context)
        {
            var welcomeMessage = context.MakeMessage();
            welcomeMessage.Text = "You have selected eNCR";
            await context.PostAsync(welcomeMessage);
            DateTime Starttime = DateTime.Now;
            strStratTime = (Starttime.ToString("h:mm:ss tt"));
            eNCRDetails sDetails = new eNCRDetails() { EmpCode = EmpCodeFromUser };
            HttpResponseMessage responsePostMethod = GetClientRequest("api/eNCR/GeteNCRCombo/18", sDetails);

            string responseString = "";
            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;
            }
            ds = JsonConvert.DeserializeObject<DataSet>(responseString);

            result1 = new string[ds.Tables[0].Rows.Count];

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (strArry == "")
                {
                    strArry = dr[1].ToString();
                }
                else
                {
                    strArry = strArry + "," + dr[1].ToString();
                }

            }
            List<string> resultArry = strArry.Split(',').ToList();
            PromptDialog.Choice(context, SelectedAttachmentAsync, resultArry,
           "Please select Category", "Sorry, I didn't get that", 3);

        }
        public async Task SelectedAttachmentAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strNcrDetail = message.ToString();

            // DateTime strLeaveSDatet = DateTime.Now.AddDays(1);
            // DateTime strLeaveSDatey = DateTime.Now.AddDays(-1);
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Product NCR":
                    ProductNcrAsync(context);
                    //context.Call(DatePickerAsync, this.ResumeAfterOptionDialog);
                    break;
                case "System NCR":
                    SystemNcrAsync(context);
                    break;
            }
        }
        public void ProductNcrAsync(IDialogContext context)
        {
            var message = context.MakeMessage();

            strArry1 = "";
            DataTable table = ds.Tables[2];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "NCRDescription =  '" + strNcrDetail + "'";
                dtNew = table.DefaultView.ToTable();
            }

            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "," + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split(',').ToList();

            PromptDialog.Choice(context, FoundAsync, resultArry1,
            "eNCR Found At", "Sorry, I didn't get that", 3);
        }
        private async Task FoundAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strProeNcrfoud = message.ToString();
            PromptDialog.Text(context, HoldingAsync, "Please enter the Description  ");
        }

        private async Task HoldingAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strProDesc = message.ToString();
            result1 = new string[ds.Tables[3].Rows.Count];
            strArry = "";
            foreach (DataRow dr in ds.Tables[3].Rows)
            {
                if (strArry == "")
                {
                    strArry = dr[1].ToString();
                }
                else
                {
                    strArry = strArry + "," + dr[1].ToString();
                }

            }
            List<string> resultArry = strArry.Split(',').ToList();
            PromptDialog.Choice(context, SubconAsync, resultArry,
            "Please select Holding Contract", "Sorry, I didn't get that", 3);
        }

        private async Task SubconAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strProHolCon = message.ToString();

            DataTable table = ds.Tables[4];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "Project =  '" + message + "'";
                dtNew = table.DefaultView.ToTable();
            }
            strArry1 = "";
            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "," + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split(',').ToList();


            PromptDialog.Choice(context, UploadconfirmAsync, resultArry1,
            "Please select Sub Contract", "Sorry, I didn't get that", 3);
        }

        public async Task UploadconfirmAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strProSubCon = message.ToString();
            PromptDialog.Choice(context, SelectedProuploadAsync, new List<string> { "Yes", "No" },
           "Do you want to attach a file?", "Sorry, I didn't get that",
           3);

        }
        public async Task SelectedProuploadAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await ProductUploadAsync(context);

                    break;
                case "No":
                    await EndproductAttachAsync(context);
                    break;
            }
        }
        public async Task ProductUploadAsync(IDialogContext context)
        {
            var replyMessage = context.MakeMessage();
            await context.PostAsync("Please upload your file \r\n (Preferred file format: PDF)");
            context.Wait(this.MessageReceivedAsync);
        }
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            DateTime Endtime = DateTime.Now;
            strProEndTime = (Endtime.ToString("h:mm:ss tt"));

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                StrProattachmenturl = attachment.ContentUrl;
                StrProattachmentname = attachment.Name;

                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.       
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                    //await context.PostAsync($"Attachment of type :{attachment.ContentType} name :  {attachment.Name}  and size of :{contentLenghtBytes} bytes received.");
                    await context.PostAsync("eNCR Details \r\n \r\n\r\n NCR Type: " + strNcrDetail + " \r\n  eNCR FountAt : " + strProeNcrfoud + " \r\n Holding Contract : " + strProHolCon + " \r\n Sub Contract : " + strProSubCon + " \r\n");
                    PromptDialog.Choice(context, SelectedProOptionAsync, new List<string> { "Yes", "No" },
                    "Please click yes to raise this eNCR :", "Sorry, I didn't get that", 3);


                }
            }
            //  context.Done(this);
        }


        public void SystemNcrAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            strArry = "";
            result1 = new string[ds.Tables[1].Rows.Count];

            foreach (DataRow dr in ds.Tables[1].Rows)
            {
                if (strArry == "")
                {
                    strArry = dr[1].ToString();
                }
                else
                {
                    strArry = strArry + "," + dr[1].ToString();
                }

            }
            List<string> resultArry = strArry.Split(',').ToList();

            PromptDialog.Choice(context, FoundAsyncSys, resultArry,
            "Please select Sub Category", "Sorry, I didn't get that", 3);
        }

        private async Task FoundAsyncSys(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSyssubCat = message.ToString();
            strArry1 = "";
            DataTable table = ds.Tables[2];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "NCRDescription =  '" + strNcrDetail + "'";
                dtNew = table.DefaultView.ToTable();
            }

            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "," + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split(',').ToList();

            PromptDialog.Choice(context, SysDesc, resultArry1,
            "eNCR Found At", "Sorry, I didn't get that", 3);
        }

        private async Task SysDesc(IDialogContext context, IAwaitable<string> argument)
        {

            var message = await argument;
            strSyseNCRFound = message.ToString();
            PromptDialog.Text(context, SysLocAsync, "Please enter the Description ");

        }
        private async Task SysLocAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysDesc = message.ToString();
            result1 = new string[ds.Tables[5].Rows.Count];
            strArry = "";
            foreach (DataRow dr in ds.Tables[5].Rows)
            {
                if (strArry == "")
                {
                    strArry = dr[1].ToString();
                }
                else
                {
                    strArry = strArry + "," + dr[1].ToString();
                }

            }
            List<string> resultArry = strArry.Split(',').ToList();

            PromptDialog.Choice(context, SysDepAsync, resultArry,
            "Please select Location", "Sorry, I didn't get that", 3);
        }

        private async Task SysDepAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysLoc = message.ToString();
            DataTable table = ds.Tables[6];

            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "CompanyName =  '" + message + "'";
                dtNew = table.DefaultView.ToTable();
            }
            strArry1 = "";
            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "," + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split(',').ToList();

            PromptDialog.Choice(context, SysCatAsync, resultArry1,
            "Please select Department", "Sorry, I didn't get that", 3);
        }

        private async Task SysCatAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysDepart = message.ToString();
            var dtNew = new DataTable();
            DataTable table = ds.Tables[7];
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "SubCategory =  '" + strSyssubCat + "'  ";
                dtNew = table.DefaultView.ToTable();
            }
            strArry1 = "";
            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "`" + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split('`').ToList();

            PromptDialog.Choice(context, SysCatsubAsync, resultArry1,
            "Please select Category", "Sorry, I didn't get that", 3);
        }

        private async Task SysCatsubAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysCat = message.ToString();
            DataTable table = ds.Tables[8];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "NCRCategory =  '" + message + "' AND SubCategory='" + strSyssubCat + "'";
                dtNew = table.DefaultView.ToTable();
            }
            strArry1 = "";
            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "`" + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split('`').ToList();


            PromptDialog.Choice(context, SysHoldingAsync, resultArry1,
            "Please select Sub Category ", "Sorry, I didn't get that", 3);
        }

        private async Task SysHoldingAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysSubCat1 = message.ToString();
            result1 = new string[ds.Tables[3].Rows.Count];
            strArry = "";
            foreach (DataRow dr in ds.Tables[3].Rows)
            {
                if (strArry == "")
                {
                    strArry = dr[1].ToString();
                }
                else
                {
                    strArry = strArry + "," + dr[1].ToString();
                }

            }
            List<string> resultArry = strArry.Split(',').ToList();
            PromptDialog.Choice(context, SysSubconAsync, resultArry,
            "Please select Holding Contract", "Sorry, I didn't get that", 3);
        }

        private async Task SysSubconAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysHolCon = message.ToString();
            DataTable table = ds.Tables[4];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "Project =  '" + message + "'";
                dtNew = table.DefaultView.ToTable();
            }
            strArry1 = "";
            result2 = new string[dtNew.Rows.Count];
            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[1].ToString()))
                {
                    strArry1 = strArry1 + "," + dr[1].ToString();
                }
            }
            strArry1 = strArry1.Substring(1, strArry1.Length - 1);
            List<string> resultArry1 = strArry1.Split(',').ToList();

            PromptDialog.Choice(context, UploadSysconfirmAsync, resultArry1,
            "Please select Sub Contract", "Sorry, I didn't get that", 3);
        }

        public async Task UploadSysconfirmAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strSysSubCon = message.ToString();
            PromptDialog.Choice(context, SelecteduploadAsync, new List<string> { "Yes", "No" },
           "Do you want to attach a file?", "Sorry, I didn't get that",
           3);

        }
        public async Task SelecteduploadAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await SysUploadAsync(context);

                    break;
                case "No":
                    await EndNoSysAttachAsync(context);
                    break;
            }
        }

        public async Task SysUploadAsync(IDialogContext context)
        {
            await context.PostAsync("Please upload your file \r\n (Preferred file format: PDF)");
            context.Wait(this.SysMessageReceivedAsync);
        }
        public virtual async Task SysMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            DateTime Endtime = DateTime.Now;
            strSysEndTime = (Endtime.ToString("h:mm:ss tt"));

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                StrSysattachmenturl = attachment.ContentUrl;
                StrSysattachmentname = attachment.Name;

                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.       
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                    await context.PostAsync($"Attachment of type :{attachment.ContentType} name :  {attachment.Name}  and size of :{contentLenghtBytes} bytes received.");
                    await context.PostAsync("eNCR Details \r\n \r\n\r\n NCR Type: " + strNcrDetail + " \r\n  Sub Category : " + strSyssubCat + " \r\n  eNCR Found At  : " + strSyseNCRFound + " \r\n Location   : " + strSysLoc + " \r\n Department   : " + strSysDepart + " \r\n Category  : " + strSysCat + " \r\n Sub Category  : " + strSysSubCat1 + " \r\n Holding Contract : " + strSysHolCon + " \r\n Sub Contract  : " + strSysSubCon + " \r\n");
                    PromptDialog.Choice(context, SelectedSysOptionAsync, new List<string> { "Yes", "No" },
                    "Please click 'Yes' to raise this eNCR :", "Sorry, I didn't get that", 3);
                }
            }

            //  context.Done(this);
        }
        public async Task SelectedSysOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await EndSystemYesAsync(context);
                    break;
                case "No":
                    await EndSysNoAsync(context);
                    break;
            }
        }

        public async Task EndSystemYesAsync(IDialogContext context)
        {
            var replyMessage = context.MakeMessage();
            string strResponse = SystemNcr(EmpCodeFromUser, strNcrDetail, strSyssubCat, strSyseNCRFound, strSysDesc, strSysLoc, strSysDepart, strSysCat, strSysSubCat1, strSysHolCon, strSysSubCon, StrSysattachmentname, StrSysattachmenturl, strStratTime, strSysEndTime);
            await context.PostAsync("Thanks for using EPMSBot \r\n" + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);

        }

        public async Task EndSysNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your eNCR was not raised, please try again.");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
            //  context.Wait(this.LeaveCategoryAsync);
        }

        public async Task SelectedProOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await EndproductYesAsync(context);
                    break;
                case "No":
                    await EndProNoAsync(context);
                    break;
            }
        }


        public async Task EndproductAttachAsync(IDialogContext context)
        {
            var replyMessage = context.MakeMessage();
            await context.PostAsync("eNCR Details \r\n \r\n\r\n NCR Type: " + strNcrDetail + " \r\n  eNCR FountAt : " + strProeNcrfoud + " \r\n Holding Contract : " + strProHolCon + " \r\n Sub Contract : " + strProSubCon + " \r\n");
            PromptDialog.Choice(context, SelectedProOptionAsync, new List<string> { "Yes", "No" },
          "Please click 'Yes' to raise this eNCR :", "Sorry, I didn't get that",
               3);
        }

        public async Task EndproductYesAsync(IDialogContext context)
        {
            var replyMessage = context.MakeMessage();
            string strResponse = ProductNcr(EmpCodeFromUser, strNcrDetail, strProsubCat, strProeNcrfoud, strProDesc, strProLoc, strProDepart, strProCat, strProSubCat1, strProHolCon, strProSubCon, StrProattachmentname, StrProattachmenturl, strStratTime, strProEndTime);
            await context.PostAsync("Thanks for using EPMSBot \r\n" + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }

        public async Task EndProNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your eNCR was not raised, please try again.");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
            //  context.Wait(this.LeaveCategoryAsync);
        }
        public async Task EndNoSysAttachAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("eNCR Details \r\n \r\n \r\n NCR Type : " + strNcrDetail + " \r\n Sub Category : " + strSyssubCat + " \r\n eNCR Found At : " + strSyseNCRFound + " \r\n  Location   : " + strSysLoc + " \r\n Department : " + strSysDepart + " \r\n Category : " + strSysCat + " \r\n  Sub Category : " + strSysSubCat1 + " \r\n Holding Contract : " + strSysHolCon + " \r\n  Sub Contract : " + strSysSubCon + " \r\n");
          // await context.PostAsync("eNCR Details \r\n \r\n\r\n NCR Type : " + strNcrDetail + " \r\n  Sub Category : " + strSyssubCat + " \r\n  eNCR Found At  : " + strSyseNCRFound + " \r\n Location  : " + strSysLoc + " \r\n Department   : " + strSysDepart + " \r\n Category  : " + strSysCat + " \r\n Sub Category  : " + strSysSubCat1 + " \r\n Holding Contract : " + strSysHolCon + " \r\n Sub Contract  : " + strSysSubCon + " \r\n");
            PromptDialog.Choice(context, SelectedSysOptionAsync, new List<string> { "Yes", "No" },
            "Please click 'Yes' to raise this eNCR :", "Sorry, I didn't get that", 3);

        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Done(this);
            }
        }

        static string SystemNcr(string empCode, string Sysncrtype, string SyseventCat, string Sysfoundat, string Sysdesc, string Syslocation, string Sysdept, string Syscat, string Syssubcat, string Sysholcon, string Syssubcon, string SysAttachmentfileName, string SysAttachmentfileurl, string SysStarttime, string SysEndtime)
        {
            string responseString = "";

            eNCRDetails sDetails = new eNCRDetails() { EmpCode = empCode, NCRType = Sysncrtype, EventCategory = SyseventCat, FountAt = Sysfoundat, Description = Sysdesc, Location = Syslocation, Department = Sysdept, Category = Syscat, SubCategory = Syssubcat, HoldingContract = Sysholcon, SubContract = Syssubcon, AttFileName = SysAttachmentfileName, AttFileDataURL = SysAttachmentfileurl, StartTime = SysStarttime, EndTime = SysEndtime };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/eNCR/SaveeNCR/", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }


        static string ProductNcr(string empCode, string ProncrType, string ProeventCat, string Profoundat, string ProDesc, string Proloc, string ProDept, string ProCat, string ProSubcat, string ProHolcon, string ProSubcon, string ProAttfilename, string ProAtturl, string ProStarttime, string ProEndtime)
        {
            string responseString = "";
            eNCRDetails sDetails = new eNCRDetails() { EmpCode = empCode, NCRType = ProncrType, EventCategory = ProeventCat, FountAt = Profoundat, Description = ProDesc, Location = Proloc, Department = ProDept, Category = ProCat, SubCategory = ProSubcat, HoldingContract = ProHolcon, SubContract = ProSubcon, AttFileName = ProAttfilename, AttFileDataURL = ProAtturl, StartTime = ProStarttime, EndTime = ProEndtime };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/eNCR/SaveeNCR/", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }


        //-----------------------post----------------------------------------------
        private static HttpResponseMessage ClientPostRequest(string RequestURI, eNCRDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.PostAsJsonAsync(RequestURI, sDetails).Result;
            return response;
        }

        //-----------------get ----------------------------------------------------
        private static HttpResponseMessage GetClientRequest(string RequestURI, eNCRDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(RequestURI).Result;

            return response;
        }

        public class eNCRDetails
        {
            public string EmpCode { get; set; }
            public string NCRType { get; set; }
            public string EventCategory { get; set; }
            public string FountAt { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string HoldingContract { get; set; }
            public string SubContract { get; set; }
            public string AttFileName { get; set; }
            public string AttFileDataURL { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }
    }
}