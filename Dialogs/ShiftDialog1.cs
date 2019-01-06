using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class ShiftDialog1: IDialog<object>
    {

        public string strShiftCat = "";
        public string strMonth = "";
        public string strStratTime = "";
        public string strEndTime = "";
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";
        public static string Companycode = "";
        string strArry = "";

        public async Task StartAsync(IDialogContext context)
        {
            var welcomeMessage = context.MakeMessage();
            DateTime Starttime = DateTime.Now;
            strStratTime = (Starttime.ToString("h:mm:ss"));
            welcomeMessage.Text = "You have selected Shift Booking !";
            if (Convert.ToInt32(Companycode) == 1)
            {
                await context.PostAsync(welcomeMessage);
                PromptDialog.Choice(context, ShiftOnAsync, new List<string> { "General", "Second", },
                "Please select your shift ", "Sorry, I didn't get that",
                3);
            }
            if (Convert.ToInt32(Companycode) == 2)
            {
                await context.PostAsync(welcomeMessage);
                PromptDialog.Choice(context, ShiftOnAsync, new List<string> { "Regular", "First", "Second" },
                "Please select your shift ", "Sorry, I didn't get that",
                3);
            }
            if (Convert.ToInt32(Companycode) == 5)
            {
                await context.PostAsync(welcomeMessage);
                PromptDialog.Choice(context, ShiftOnAsync, new List<string> { "General 1", "General 2" },
                "Please select your shift ", "Sorry, I didn't get that",
                3);
            }
        }

        public async Task ShiftOnAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strShiftCat = message.ToString();
            var welcomeMessage = context.MakeMessage();
            DateTime now = DateTime.Now;

            for (int i = 0; i < 5; i++)
            {
                now = now.AddMonths(1);

                if (strArry == "")
                {
                    strArry = now.ToString("MMMM");
                }
                else
                {
                    strArry = strArry + "," + now.ToString("MMMM");
                }

            }
            List<string> Currentmonth = strArry.Split(',').ToList();
            PromptDialog.Choice(context, onedayoptionAsync, Currentmonth,
            " Please select the Booking Month", "Sorry, I didn't get that", 3);

        }

        public async Task onedayoptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strMonth = message.ToString();
            await context.PostAsync("Please confirm your Shift Details \r\n \r\n\r\n Shift : " + strShiftCat + " \r\n Month : " + strMonth + " \r\n");
            PromptDialog.Choice(context, SelectedOptionAsync, new List<string> { "Yes", "No" },
               "Please click 'yes' to raise this booking ", "Sorry, I didn't get that",
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
            DateTime Endtime = DateTime.Now;
            strEndTime = (Endtime.ToString("h:mm:ss"));
            string strResponse = BookTimeSheet(EmpCodeFromUser, strMonth, strShiftCat, strStratTime, strEndTime);
            await context.PostAsync("Thanks for using EPMSBot \r\n" + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }
        public async Task EndNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your shift was not raised, please try again.");
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);
        }

        static string BookTimeSheet(string empCode, string monthName, string strShift, string strstarttime, string strendtime)
        {
            string responseString = "";
            ShiftDetails sDetails = new ShiftDetails() { EmpCode = empCode, monthName = monthName, shift = strShift, Starttime = strstarttime, Endtime = strendtime };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/Shift/BookShift/", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;
            }
            return responseString;
        }

        private static HttpResponseMessage ClientPostRequest(string RequestURI, ShiftDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.PostAsJsonAsync(RequestURI, sDetails).Result;
            return response;
        }

        public class ShiftDetails
        {
            public string EmpCode { get; set; }
            public string shift { get; set; }
            public string monthName { get; set; }
            public string Starttime { get; set; }
            public string Endtime { get; set; }
        }
    }

};
