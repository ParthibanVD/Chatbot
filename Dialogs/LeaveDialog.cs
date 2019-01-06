using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SimpleEchoBot.Dialogs
{

    [Serializable]
    public class AdaptiveCardDialog : IDialog<object>
    {
        public string strLeaveType = "";
        public string strLeaveCat = "";
        public string strLeaveSDate = "";
        public string strLeaveSDatet = "";
        public string strEndDate = "";
        public string strStartDate = "";
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";
        public string strLeavetype = "Full Day";

        public object AdaptiveCard { get; private set; }

        public async Task StartAsync(IDialogContext context)
        {
            // context.Wait(this.LeaveCategoryAsync);
            // return Task.CompletedTask;          
            var welcomeMessage = context.MakeMessage();
            welcomeMessage.Text = "You have selected leave booking";
            await context.PostAsync(welcomeMessage);
            PromptDialog.Choice(context, LeaveOnAsync, new List<string> { "CL", "EL", "LOP", "SL", "Leave Sharing", " Marriage Leave", "Maternity Leave", "Paternity Leave", "Sabbatical Leave", "VerySpecial Leave", "Compassionate Leave", "Compensation Leave " },
            "Please select the leave category", "Sorry, I didn't get that",
            3);
        }

        public async Task LeaveOnAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strLeaveCat = message.ToString();
            var welcomeMessage = context.MakeMessage();
            PromptDialog.Choice(context, SelectedAttachmentAsync, new List<string> { "Yesterday", "Tomorrow", "Custom", },
            "Please select leave type", "Sorry, I didn't get that",
            3);
        }
        public async Task SelectedAttachmentAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strLeaveSDate = message.ToString();

            // DateTime strLeaveSDatet = DateTime.Now.AddDays(1);
            // DateTime strLeaveSDatey = DateTime.Now.AddDays(-1);
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Custom":
                    await DatePickerAsync(context);
                    //context.Call(DatePickerAsync, this.ResumeAfterOptionDialog);
                    break;
                case "Yesterday":
                    LeaveTypeAsync(context);
                    break;

                case "Tomorrow":
                    LeaveTypeAsync(context);
                    break;
            }
        }
        public void LeaveTypeAsync(IDialogContext context)
        {

            var message = context.MakeMessage();
            PromptDialog.Choice(context, onedayoptionAsync, new List<string> { "First Half", "Second Half", "Full Day" },
           "Please select leave type", "Sorry, I didn't get that",
           3);
        }

        private async Task DatePickerAsync(IDialogContext context)
        {
            await context.PostAsync("Are you looking for Multiple days leave?");
            context.Wait(this.DatePickerAsync);
        }

        public virtual async Task DatePickerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var temp = await result as Activity;
            DateTime dtStartDate;
            DateTime dtEndDate;
            var datePresent = DateTime.TryParse(temp.Text, out dtStartDate);
            var datePresent1 = DateTime.TryParse(temp.Text, out dtEndDate);
            dtStartDate = DateTime.Now;

            if (!datePresent && temp.Value != null)
            {
                var jObjectValue = temp.Value as JObject;

                strStartDate = jObjectValue.Value<string>("StartDate");
                strEndDate = jObjectValue.Value<string>("EndDate");

                if (!string.IsNullOrEmpty(strStartDate))
                {
                    dtStartDate = DateTime.ParseExact(strStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    strStartDate = dtStartDate.ToString("dd-MMM-yyyy");
                    datePresent = true;
                    dtEndDate = DateTime.ParseExact(strEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    strEndDate = dtEndDate.ToString("dd-MMM-yyyy");
                    datePresent1 = true;
                }
            }

            if (!datePresent)
            {
                //since the user did not send a date, show the card
                AdaptiveCard card = new AdaptiveCard();

                card.Body.Add(new TextBlock()
                {
                    Text = "Please provide start date and end date?",
                    // Size = TextSize.Large,
                    // Weight = TextWeight.Bolder
                });

                DateInput diStartDate = new DateInput()
                {
                    Id = "StartDate",
                    Placeholder = "Start Date",
                    Value = dtStartDate.ToString("yyyy-MM-dd")
                };

                DateInput diEndDate = new DateInput()
                {
                    Id = "EndDate",
                    Placeholder = "End Date",
                    Value = dtStartDate.ToString("yyyy-MM-dd")
                };

                card.Body.Add(diStartDate);
                card.Body.Add(diEndDate);
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
                // await context.PostAsync($"Confirm Your Leavedates: {dtStartDate} {dtEndDate}");
                await context.PostAsync("Please confirm your Leave Details \r\n \r\n\r\n Leave Category : " + strLeaveCat + " \r\n Leave Start Date : " + strStartDate + " \r\n Leave End Date : " + strEndDate + " \r\n");
                PromptDialog.Choice(context, EndoptionAsync, new List<string> { "Yes", "No" },
                "Confirm Your Leavedetails :", "Sorry, I didn't get that",
                3);
            }
        }
        public async Task EndoptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await EndoptionyesAsync(context);
                    break;
                case "No":
                    await EndNoAsync(context);
                    break;
            }
        }
        public async Task EndoptionyesAsync(IDialogContext context )
        {
            var replyMessage = context.MakeMessage();
            string strResponse = LeaveBookCustom(EmpCodeFromUser, strLeaveCat, strLeavetype, strStartDate, strEndDate);
            await context.PostAsync("Thanks for using EPMSBot \r\n" + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }
        public async Task onedayoptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            strLeaveType = message.ToString();

            var replyMessage = context.MakeMessage();
            await context.PostAsync("Please confirm your Leave Details \r\n \r\n\r\n Leave Category : " + strLeaveCat + " \r\n Leave Date : " + strLeaveSDate + " \r\n LeaveType : " + strLeaveType + " \r\n");
            PromptDialog.Choice(context,SelectedOptionAsync, new List<string> { "Yes", "No" },
               "Please click yes to raise this leave :", "Sorry, I didn't get that",
               3);
        }
        public async Task SelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();

            switch (message)
            {
                case "Yes":
                    await EndyesAsync(context);
                    break;
                case "No":
                    await EndNoAsync(context);
                    break;
            }
        }

        public async Task EndyesAsync(IDialogContext context)
        {
            var replyMessage = context.MakeMessage();
            string strResponse = LeaveBook1(EmpCodeFromUser, strLeaveCat, strLeaveType, strLeaveSDate);
            await context.PostAsync("Thanks for using EPMSBot \r\n" + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }
        public async Task EndNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your leave was not raised, please try again.");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
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

        static string LeaveBookCustom(string empCode, string leavecat, string leavetype, string startDate, string endDate)
        {
            string responseString = "";

            LeaveDetails sDetails = new LeaveDetails() { EmpCode = empCode, LeaveCategory = leavecat, LeaveType = leavetype, StartDate = startDate, EndDate = endDate };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/Leave/BookLeave/", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }

        static string LeaveBook1(string empCode, string leavecat, string leaveType, string leaveDate)
        {
            string responseString = "";

            string leaveStardDate = "";
            string leaveEndDate = "";

            if (leaveDate == "Yesterday")
            {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                leaveStardDate = yesterday.ToString("dd-MMM-yyyy");
                leaveEndDate = yesterday.ToString("dd-MMM-yyyy");
            }
            else if (leaveDate == "Tomorrow")
            {
                DateTime Tomorrow = DateTime.Now.AddDays(1);
                leaveStardDate = Tomorrow.ToString("dd-MMM-yyyy");
                leaveEndDate = Tomorrow.ToString("dd-MMM-yyyy");

            }

            LeaveDetails sDetails = new LeaveDetails() { EmpCode = empCode, LeaveCategory = leavecat, LeaveType = leaveType, StartDate = leaveStardDate, EndDate = leaveEndDate };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/Leave/BookLeave/", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }

        private static HttpResponseMessage ClientPostRequest(string RequestURI, LeaveDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.PostAsJsonAsync(RequestURI, sDetails).Result;
            return response;
        }

        public class LeaveDetails
        {
            public string EmpCode { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string LeaveType { get; set; }
            public string LeaveCategory { get; set; }
        }
    }

}
