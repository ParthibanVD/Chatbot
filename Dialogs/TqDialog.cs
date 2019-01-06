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
    public class TqDialog : IDialog<object>
    {
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";
        public string strStartDate = "";
        public string Strattachmenturl = "";
        public string Strattachmentname = "";
        DataSet ds = new DataSet();
        string[] result1 = new string[0];
        string[] result2 = new string[0];
        string strArry = "";
        string strArry1 = "";
        public string strTqCat = "";
        public string strTqSubCat = "";
        public string strTqClientRef = "";
        public string strDrawingRef = "";
        public string strClosureType = "";
        public string strTqDesc = "";
        public string strHolCon = "";
        public string strConNo = "";
        public string strIssued = "";
        public string strStratTime = "";
        public string strEndTime = "";


        public async Task StartAsync(IDialogContext context)
        {
            // context.Wait(this.MessageReceivedAsync);
            // return Task.CompletedTask;

            var welcomeMessage = context.MakeMessage();
            welcomeMessage.Text = "You have selected TqManager";
            await context.PostAsync(welcomeMessage);

            TQDetails sDetails = new TQDetails() { EmpCode = EmpCodeFromUser };
            HttpResponseMessage responsePostMethod = GetClientRequest("api/TQ/GetTQCombo/18", sDetails);

            string responseString = "";
            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;
            }
            ds = JsonConvert.DeserializeObject<DataSet>(responseString);

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

            PromptDialog.Choice(context, NextOnAsync, resultArry,
             "Please select Category", "Sorry, I didn't get that", 3);
        }

        public async Task NextOnAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strTqCat = message.ToString();
            DateTime Starttime = DateTime.Now;
            strStratTime = (Starttime.ToString("h:mm:ss tt"));
            strArry1 = "";
            DataTable table = ds.Tables[2];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "Category =  '" + message + "'";
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

            PromptDialog.Choice(context, ClientOnAsync, resultArry1,
             "Please select sub category", "Sorry, I didn't get that",
             3);

        }

        private async Task ClientOnAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strTqSubCat = message.ToString();
            PromptDialog.Text(context, DrawingAsync, "Please enter the Client Ref ");
        }

        private async Task DrawingAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strTqClientRef = message.ToString();
            PromptDialog.Text(context, TqDesAsync, "Please enter the Drawing Ref ");
        }

        private async Task TqDesAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strDrawingRef = message.ToString();

            PromptDialog.Text(context, ClosureAsync, "Please enter the TQ Description ");

        }
        private async Task ClosureAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strTqDesc = message.ToString();

            PromptDialog.Choice(context, SelectedAttachmentAsync, new List<string> { "Close TQ by assumption after 2 working days", "Hold Work" },
            "Please select Closure Type", "Sorry, I didn't get that",
            3);
        }
        public async Task SelectedAttachmentAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strClosureType = message.ToString();
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Close TQ by assumption after 2 working days":
                    await Dateget2daysAsync(context);
                    break;
                case "Hold Work":
                    await DategetAsync(context);
                    break;
            }
        }

        private async Task Dateget2daysAsync(IDialogContext context)
        {
            //var message = await argument;
            //strClosureType = message.ToString();
            DateTime dtclosingafterDate;
            dtclosingafterDate = DateTime.Now.AddDays(2);
            strStartDate = dtclosingafterDate.ToString("dd-MMM-yyyy");
            await context.PostAsync("Do you want to extend closure date? \r\n" + strStartDate);
            context.Wait(this.DatePick2daysAsync);
        }

        public virtual async Task DatePick2daysAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {

            var temp = await argument as Activity;
            DateTime dtStartDate;
            var datePresent = DateTime.TryParse(temp.Text, out dtStartDate);
            DateTime today = DateTime.Today;
            dtStartDate = DateTime.Now.AddDays(2);
            // if (temp.Value == null) {
            //  if (TQClosureType != "Hold Work")
            //  {
            //      dtStartDate = DateTime.Now.AddDays(2);
            //      datePresent = true;
            //      strStartDate = dtStartDate.ToString("dd-MMM-yyyy");
            //   }
            //  }


            if (!datePresent && temp.Value != null)
            {
                var jObjectValue = temp.Value as JObject;

                strStartDate = jObjectValue.Value<string>("StartDate");

                if (!string.IsNullOrEmpty(strStartDate))
                {

                    dtStartDate = DateTime.ParseExact(strStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    strStartDate = dtStartDate.ToString("dd-MMM-yyyy");
                    datePresent = true;
                }
            }

            if (!datePresent)
            {
                //since the user did not send a date, show the card
                AdaptiveCard card = new AdaptiveCard();

                card.Body.Add(new TextBlock()
                {
                    Text = "Please provide \r\n Expected date of response ",
                    // Size = TextSize.Large,
                    // Weight = TextWeight.Bolder
                });

                DateInput diStartDate = new DateInput()
                {
                    Id = "StartDate",
                    Placeholder = "Start Date",
                    Value = dtStartDate.ToString("yyyy-MM-dd")
                };
                //dtStartDate = DateTime.Now;
                //if (temp.Value == null)
                //{
                //    if (TQClosureType != "Hold Work")
                //    {
                //        dtStartDate = DateTime.Now.AddDays(2);
                //        datePresent = true;
                //        strStartDate = dtStartDate.ToString("dd-MMM-yyyy");
                //    }
                //}
                card.Body.Add(diStartDate);

                card.Actions.Add(new SubmitAction()
                {
                    Title = "OK"
                });

                Attachment cardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card
                };

                var message = context.MakeMessage();

                message.Attachments = new List<Attachment>();
                message.Attachments.Add(cardAttachment);
                await context.PostAsync(message);

            }
            else
            {
                //  await context.PostAsync($"your selected date ", strStartDate);
                result1 = new string[ds.Tables[0].Rows.Count];
                strArry = "";
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

                PromptDialog.Choice(context, ContractnoAsync, resultArry,
                "Please select Holding Contract ", "Sorry, I didn't get that",
                3);
            }
        }

        private async Task DategetAsync(IDialogContext context)
        {
            //var message = await argument;
            //strClosureType = message.ToString();
            DateTime dtholdDate;
            dtholdDate = DateTime.Now;
            strStartDate = dtholdDate.ToString("dd-MMM-yyyy");
            await context.PostAsync("Do you want to extend the closure date?\r\n" + strStartDate);
            context.Wait(this.DatePickAsync);
        }

        public virtual async Task DatePickAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {

            var temp = await argument as Activity;
            DateTime dtStartDate;
            var datePresent = DateTime.TryParse(temp.Text, out dtStartDate);
            DateTime today = DateTime.Today;
            dtStartDate = DateTime.Now;

            if (!datePresent && temp.Value != null)
            {
                var jObjectValue = temp.Value as JObject;

                strStartDate = jObjectValue.Value<string>("StartDate");

                if (!string.IsNullOrEmpty(strStartDate))
                {

                    dtStartDate = DateTime.ParseExact(strStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    strStartDate = dtStartDate.ToString("dd-MMM-yyyy");
                    datePresent = true;
                }
            }

            if (!datePresent)
            {
                //since the user did not send a date, show the card
                AdaptiveCard card = new AdaptiveCard();

                card.Body.Add(new TextBlock()
                {
                    Text = "Please provide \r\n Expected date of response ",
                    // Size = TextSize.Large,
                    // Weight = TextWeight.Bolder
                });

                DateInput diStartDate = new DateInput()
                {
                    Id = "StartDate",
                    Placeholder = "Start Date",
                    Value = dtStartDate.ToString("yyyy-MM-dd")
                };
                //dtStartDate = DateTime.Now;
                //if (temp.Value == null)
                //{
                //    if (TQClosureType != "Hold Work")
                //    {
                //        dtStartDate = DateTime.Now.AddDays(2);
                //        datePresent = true;
                //        strStartDate = dtStartDate.ToString("dd-MMM-yyyy");
                //    }
                //}
                card.Body.Add(diStartDate);

                card.Actions.Add(new SubmitAction()
                {
                    Title = "OK"
                });

                Attachment cardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card
                };

                var message = context.MakeMessage();

                message.Attachments = new List<Attachment>();
                message.Attachments.Add(cardAttachment);
                await context.PostAsync(message);

            }
            else
            {
                //  await context.PostAsync($"your selected date ", strStartDate);
                result1 = new string[ds.Tables[0].Rows.Count];
                strArry = "";
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

                PromptDialog.Choice(context, ContractnoAsync, resultArry,
                "Please select Holding Contract ", "Sorry, I didn't get that",
                3);
            }
        }

        private async Task ContractnoAsync(IDialogContext context, IAwaitable<string> argument)
        {

            var message = await argument;
            strHolCon = message.ToString();

            DataTable table = ds.Tables[5];
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

            PromptDialog.Choice(context, issuedAsync, resultArry1,
            "Please select Contract No ", "Sorry, I didn't get that",
            3);
        }

        private async Task issuedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strConNo = message.ToString();

            result1 = new string[ds.Tables[4].Rows.Count];
            strArry = "";
            foreach (DataRow dr in ds.Tables[4].Rows)
            {
                if (strArry == "")
                {
                    strArry = dr[2].ToString();
                }
                else
                {
                    strArry = strArry + "," + dr[2].ToString();
                }
            }
            List<string> resultArry = strArry.Split(',').ToList();
            PromptDialog.Choice(context, UploadconfirmAsync, resultArry,
            "Issued to ", "Sorry, I didn't get that",
            3);
        }
        public async Task UploadconfirmAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strIssued = message.ToString();
            PromptDialog.Choice(context, SelecteduploadAsync, new List<string> { "Yes", "No" },
           "Do you want to attach a file?", "Sorry, I didn't get that",
           2);

        }
        public async Task SelecteduploadAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await UploadAsync(context);

                    break;
                case "No":
                    await EndNoAttachAsync(context);
                    break;
            }
        }

        public async Task UploadAsync(IDialogContext context)
        {

            var replyMessage = context.MakeMessage();
            await context.PostAsync("Please upload your file \r\n (Preferred file format: PDF)");
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            DateTime Endtime = DateTime.Now;
            strEndTime = (Endtime.ToString("h:mm:ss tt"));

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                Strattachmenturl = attachment.ContentUrl;
                Strattachmentname = attachment.Name;

                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.       
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                    //await context.PostAsync($"Attachment of type :{attachment.ContentType} name :  {attachment.Name}  and size of :{contentLenghtBytes} bytes received.");
                    await context.PostAsync("TQ Details \r\n \r\n\r\n TQ Category : " + strTqCat + " \r\n TQ SubCategory : " + strTqSubCat + " \r\n Client Ref No : " + strTqClientRef + " \r\n  Drawing Ref No : " + strDrawingRef + " \r\n Closure Type : " + strClosureType + " \r\n Expected Response : " + strStartDate + " \r\n  Holding Contract : " + strHolCon + " \r\n Sub Contract : " + strConNo + " \r\n  Issued To : " + strIssued + " \r\n");
                    PromptDialog.Choice(context, SelectedOptionAsync, new List<string> { "Yes", "No" },
                       "Please click 'Yes' to raise this TQ :", "Sorry, I didn't get that",
                       3);
                }
            }
        }
        public async Task SelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await EndYesAsync(context);
                    break;
                case "No":
                    await EndNoAsync(context);
                    break;
            }
        }
        public async Task EndYesAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            string strResponse = SaveTqDetails(EmpCodeFromUser, strTqCat, strTqSubCat, strTqClientRef, strDrawingRef, strTqDesc, strClosureType, strStartDate, strHolCon, strConNo, strIssued, Strattachmentname, Strattachmenturl, strStratTime, strEndTime);
            await context.PostAsync("Thanks for using EPMSBot \r\n " + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }

        public async Task EndNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your TQ was not raised, please try again.");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }
        public async Task EndNoAttachAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("TQ Details \r\n \r\n\r\n TQ Category : " + strTqCat + " \r\n TQ SubCategory : " + strTqSubCat + " \r\n Client Ref No : " + strTqClientRef + " \r\n  Drawing Ref No : " + strDrawingRef + " \r\n Closure Type : " + strClosureType + " \r\n Expected Response : " + strStartDate + " \r\n  Holding Contract : " + strHolCon + " \r\n Sub Contract : " + strConNo + " \r\n  Issued To : " + strIssued + " \r\n");
            PromptDialog.Choice(context, SelectedOptionAsync, new List<string> { "Yes", "No" },
            "Please click 'Yes' to raise this TQ :", "Sorry, I didn't get that", 3);
        }

        //-------------------post------------------
        static string SaveTqDetails(string empCode, string tqCat, string tqSubCat, string tqClientref, string tqDrawingref, string tqDesc, string tqClosure, string tqExpected, string tqHoldCon, string tqSubCon, string tqissued, string tqAttachmentName, string tqAttachmentUrl, string tqStarttime, string tqEndtime)
        {
            string responseString = "";

            TQDetails sDetails = new TQDetails() { EmpCode = empCode, TQCategory = tqCat, TQSubCategory = tqSubCat, ClientRefNo = tqClientref, DrawingRefNo = tqDrawingref, TQDescription = tqDesc, ClosureType = tqClosure, ExpectedResponse = tqExpected, HoldingContract = tqHoldCon, SubContract = tqSubCon, IssuedTo = tqissued, AttFileName = tqAttachmentName, AttFileDataURL = tqAttachmentUrl, TQStartTime = tqStarttime, TQEndTime = tqEndtime };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/TQ/SaveTQ", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }

        private static HttpResponseMessage ClientPostRequest(string RequestURI, TQDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.PostAsJsonAsync(RequestURI, sDetails).Result;
            return response;
        }

        //-----------------get -------------------
        private static HttpResponseMessage GetClientRequest(string RequestURI, TQDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(RequestURI).Result;
            return response;
        }

        public class TQDetails
        {
            public string EmpCode { get; set; }
            public string TQCategory { get; set; }
            public string TQSubCategory { get; set; }
            public string ClientRefNo { get; set; }
            public string DrawingRefNo { get; set; }
            public string TQDescription { get; set; }
            public string ClosureType { get; set; }
            public string ExpectedResponse { get; set; }
            public string HoldingContract { get; set; }
            public string SubContract { get; set; }
            public string DrawingRange { get; set; }
            public string VONo { get; set; }
            public string IssuedTo { get; set; }
            public string AttFileName { get; set; }
            public string AttFileDataURL { get; set; }
            public string TQStartTime { get; set; }
            public string TQEndTime { get; set; }
        }

    }
}