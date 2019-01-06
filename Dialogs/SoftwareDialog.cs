using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class SoftwareDialog : IDialog<object>
    {

        public string strCat = "";
        public string strTicketType = "";
        public string strDesc = "";
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";
        public string Strattachmenturl = "";
        public string Strattachmentname = "";
        public string strStratTime = "";
        public string strEndTime = "";

        public object AdaptiveCard { get; private set; }
        public string Strstream = "";
        public async Task StartAsync(IDialogContext context)
        {
            var welcomeMessage = context.MakeMessage();
            welcomeMessage.Text = "You have selected SoftwareHelpDesk";
            await context.PostAsync(welcomeMessage);
            DateTime Starttime = DateTime.Now;
            strStratTime = (Starttime.ToString("h:mm:ss tt"));
            string strResponse = GetCategory(EmpCodeFromUser);
            string str1 = strResponse.ToString();
            // string new1 = JsonConvert.DeserializeObject<string>(strResponse);
            //var result = JsonConvert.DeserializeObject<string>(strResponse);
            PromptDialog.Choice(context, LeaveOnAsync, new List<string> { "EPMS", "Revit", "Tekla", "UK Estimation" },
            "Please select the Category ", "Sorry, I didn't get that",
            3);

        }
        public async Task LeaveOnAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            strTicketType = message.ToString();

            var welcomeMessage = context.MakeMessage();
            PromptDialog.Choice(context, catAsync, new List<string> { "Development", "Support" },
            "Please select Ticket Type", "Sorry, I didn't get that",
            3);


        }
        public async Task catAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            strCat = message.ToString();

            try
            {

                PromptDialog.Text(context, UploadconfirmAsync, "Please enter the Description? ");
            }
            catch (Exception e)
            {
                await context.PostAsync("Error is <br/> " + e.ToString());
                context.Done(this);
            }
        }
        public async Task UploadconfirmAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strDesc = message.ToString();
            DateTime Endtime = DateTime.Now;
            strEndTime = (Endtime.ToString("h:mm:ss tt"));
            strDesc = message.ToString();
            PromptDialog.Choice(context, SelectedAttachmentAsync, new List<string> { "Yes", "No" },
           "Do you want to attach a file?", "Sorry, I didn't get that",
           3);

        }
        public async Task SelectedAttachmentAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await UploadAsync(context);

                    break;
                case "No":
                    await EndAsync(context);
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
                    await context.PostAsync($"Attachment of type : {attachment.ContentType} name :{attachment.Name} and size of : {contentLenghtBytes} received.");
                    await context.PostAsync("Ticket Details \r\n \r\n\r\n Category : " + strTicketType + " \r\n Ticket Type : " + strCat + " \r\n");
                    PromptDialog.Choice(context, SelectedOptionAsync, new List<string> { "Yes", "No" },
                       "Please click 'Yes' to raise this Ticket request:", "Sorry, I didn't get that", 2);

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
            string strResponse = SaveSoftwareHelpDesk(EmpCodeFromUser, strTicketType, strCat, strDesc, Strattachmentname, Strattachmenturl, strStratTime, strEndTime);

            await context.PostAsync("Thanks for using EPMSBot \r\n Software request saved successfully");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }

        public async Task EndNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your Ticket request was not raised, please try again.");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }

        public async Task EndAsync(IDialogContext context)
        {
            var replyMessage = context.MakeMessage();
            await context.PostAsync("Ticket Details \r\n \r\n\r\n Category : " + strTicketType + " \r\n Ticket Type : " + strCat + " \r\n");
            PromptDialog.Choice(context, SelectedOptionAsync, new List<string> { "Yes", "No" },
            "Please click 'Yes' to raise this Ticket request :", "Sorry, I didn't get that", 2);
        }

        //----------get method---------------
        static string GetCategory(string empCode)
        {
            string responseString = "";

            SoftwareDeatils sDetails = new SoftwareDeatils() { EmpCode = empCode };
            HttpResponseMessage responsePostMethod = GetClientRequest("api/SoftwareHelpDesk/GetCategory", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }

        private static HttpResponseMessage GetClientRequest(string RequestURI, SoftwareDeatils sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(RequestURI).Result;

            return response;
        }
        //----post method-------------------
        static string SaveSoftwareHelpDesk(string empCode, string tickettype, string cat, string dec, string attFName, string attFData, string Softwarestarttime, string Softwareendtime)
        {
            string responseString = "";

            SoftwareDeatils sDetails = new SoftwareDeatils() { EmpCode = empCode, TicketType = tickettype, Category = cat, Description = dec, AttFileName = attFName, AttFileDataURL = attFData, ActionStartTime = Softwarestarttime, ActionEndTime = Softwareendtime };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/SoftwareHelpDesk/SaveSoftwareHelpDesk", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }

        private static HttpResponseMessage ClientPostRequest(string RequestURI, SoftwareDeatils sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.PostAsJsonAsync(RequestURI, sDetails).Result;
            return response;
        }

        public class SoftwareDeatils
        {
            public string EmpCode { get; set; }
            public string TicketType { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public string AttFileName { get; set; }
            public string AttFileDataURL { get; set; }
            public string ActionStartTime { get; set; }
            public string ActionEndTime { get; set; }
        }

    }
}