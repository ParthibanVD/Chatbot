namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class ShiftDialog : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("You have selected Shift Booking !");
            // context.Wait(this.OrderStatusFormCallback);
            var orderStatusForm = new FormDialog<ShiftStatusDialog>(new ShiftStatusDialog(), ShiftStatusDialog.BuildForm, FormOptions.PromptInStart);
            context.Call(orderStatusForm, OrderStatusFormCallback);
        }


        private async Task OrderStatusFormCallback(IDialogContext context, IAwaitable<ShiftStatusDialog> result)
        {
            // TODO: Handle FormCanceledException
            try
            {
                var order = await result;
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the Existing Dialog";
                }
                else
                {
                    reply = $"Oops ! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }
    }
    public enum ListOptions { TimeSheetBooking };
    public enum ShiftOptions { General, Second };
    public enum MonthOptions { November, December };

    [Serializable]
    public class ShiftStatusDialog
    {
        [Prompt(" Please select your shift {||}")]
        public ShiftOptions? Shift;
        [Prompt(" Please select the Booking Month{||}")]
        public MonthOptions? Month;
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";

        public static IForm<ShiftStatusDialog> BuildForm()
        {
            OnCompletionAsyncDelegate<ShiftStatusDialog> processOrder = async (context, state) =>
            {
                // context.PrivateConversationData.SetValue<string>(
                // "List", state.List.ToString());
                context.PrivateConversationData.SetValue<string>(
                    "EmployeeNumber", EmpData);
                context.PrivateConversationData.SetValue<string>(
                    "Shifts", state.Shift.ToString());
                context.PrivateConversationData.SetValue<string>(
                   "Month", state.Month.ToString());
                // Tell the user that the form is complete 
                string strResponse = BookTimeSheet(EmpCodeFromUser, state.Month.ToString(), state.Shift.ToString());
                await context.PostAsync(" Your !!" + strResponse);
                // await context.PostAsync("Glad you asked!!We will help to do your admin activites quickly and recommended best for you");
                await context.PostAsync("Thanks for using EPMSbot !");
                await context.PostAsync("Say 'hi' to initiate conversation");
            };
            return new FormBuilder<ShiftStatusDialog>()
                    .Field(nameof(ShiftStatusDialog.Shift))
                   .Field(nameof(ShiftStatusDialog.Month))
                    .OnCompletion(processOrder)
                    .AddRemainingFields()
                    .Confirm("Please confirm your shift details \r\n Shift : {Shift} \r\n  Month : {Month} \r\n{||}")
                    .Build();
        }
        static string BookTimeSheet(string empCode, string monthName, string strShift)
        {
            string responseString = "";

            ShiftDetails sDetails = new ShiftDetails() { EmpCode = empCode, monthName = monthName, shift = strShift };
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
        }
    };
}