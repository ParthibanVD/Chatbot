using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class ITHelpDeskDialog : IDialog<object>
    {
        public static string EmpData = "";
        public static string EmpCodeFromUser = "";
        public string strStratTime = "";
        public string strTickettype = "";
        public string strITCat = "";
        public string strITSubCat = "";
        public string strITDesc = "";
        public string strITMacno = "";
        public string strITExtnno = "";
        public string strITPriority = "";
        string[] result1 = new string[0];
        string[] result2 = new string[0];
        string strArry = "";
        string strArry1 = "";
        DataSet ds = new DataSet();



        public async Task StartAsync(IDialogContext context)
        {
            var welcomeMessage = context.MakeMessage();
            welcomeMessage.Text = "You have selected ITHelpDesk";
            await context.PostAsync(welcomeMessage);
            DateTime Starttime = DateTime.Now;
            strStratTime = (Starttime.ToString("h:mm:ss tt"));
            ITDetails sDetails = new ITDetails() { EmpCode = EmpCodeFromUser };
            HttpResponseMessage responsePostMethod = GetClientRequest("api/ITHelpDesk/GetITHelpDesk/18", sDetails);

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
            PromptDialog.Choice(context, ITCatAsync, resultArry,
           "Please select Ticket Type", "Sorry, I didn't get that", 3);

        }
        public async Task ITCatAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strTickettype = message.ToString();
            var replyMessage = context.MakeMessage();
            result1 = new string[ds.Tables[2].Rows.Count];
            strArry = "";
            foreach (DataRow dr in ds.Tables[2].Rows)
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

            PromptDialog.Choice(context, ITSubCatAsync, resultArry,
          "Please select Category", "Sorry, I didn't get that", 3);

        }

        private async Task ITSubCatAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strITCat = message.ToString();
            DataTable table = ds.Tables[3];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                table.DefaultView.RowFilter = "CategoryDesc =  '" + message + "' AND TicketType='" + strTickettype + "'";
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

            PromptDialog.Choice(context, ITDescAsync, resultArry1,
          "Please select Sub Category", "Sorry, I didn't get that", 3);
        }

        private async Task ITDescAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strITSubCat = message.ToString();
            PromptDialog.Text(context, ITMacnoAsync, "Please enter the Description  ");

        }

        private async Task ITMacnoAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strITDesc = message.ToString();
            PromptDialog.Text(context, ITExtnAsync, "Please enter your Machine Number");
        }

        public async Task ITExtnAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strITMacno = message.ToString();
            PromptDialog.Text(context, ITPriorityAsync, "Please enter the Extension Number");

        }

        public async Task ITPriorityAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strITExtnno = message.ToString();
            var replyMessage = context.MakeMessage();
            result1 = new string[ds.Tables[4].Rows.Count];
            strArry = "";
            foreach (DataRow dr in ds.Tables[4].Rows)
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

            PromptDialog.Choice(context, ITEndAsync, resultArry,
          "Please select Priority", "Sorry, I didn't get that", 3);
        }


        public async Task ITEndAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            strITPriority = message.ToString();
            await context.PostAsync("IT Help Desk Details \r\n \r\n\r\n Ticket Type: " + strTickettype + " \r\n  Category : " + strITCat + " \r\n Sub Category : " + strITSubCat + " \r\n Machine No : " + strITMacno + " \r\n Extension No : " + strITExtnno + "\r\n Priority  : " + strITPriority + " \r\n");
            PromptDialog.Choice(context, SelectedOptionAsync, new List<string> { "Yes", "No" },
            "Please click 'Yes' to raise this Ticket :", "Sorry, I didn't get that", 3);

        }

        public async Task SelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
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
            string strResponse = ITSavedetails(EmpCodeFromUser, strTickettype, strITCat, strITSubCat, strITDesc, strITMacno, strITExtnno, strITPriority);
            await context.PostAsync("Thanks for using EPMSBot \r\n" + strResponse);
            await context.PostAsync("Say 'hi' to initiate conversation");
            context.Done(this);

        }

        public async Task EndSysNoAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            await context.PostAsync("Thanks for using EPMSBot, your Ticket was not raised, please try again.");
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

        static string ITSavedetails(string empCode, string ITTickettype, string ITCat, string ITSubCat, string ITDesc, string ITMachineNum, string ITExtnNum, string ITPriority)
        {
            string responseString = "";

            ITDetails sDetails = new ITDetails() { EmpCode = empCode, TicketType = ITTickettype, Category = ITCat, SubCategory = ITSubCat, Description = ITDesc, MachineNum = ITMachineNum, ExtensionNum = ITExtnNum, Priority = ITPriority };
            HttpResponseMessage responsePostMethod = ClientPostRequest("api/ITHelpDesk/SaveIT", sDetails);

            if (responsePostMethod.IsSuccessStatusCode)
            {
                responseString = responsePostMethod.Content.ReadAsStringAsync().Result;

            }
            return responseString;
        }


        //-----------------------post----------------------------------------------
        private static HttpResponseMessage ClientPostRequest(string RequestURI, ITDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.PostAsJsonAsync(RequestURI, sDetails).Result;
            return response;
        }

        //-----------------get ----------------------------------------------------
        private static HttpResponseMessage GetClientRequest(string RequestURI, ITDetails sDetails)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://epmschatbotapi.azurewebsites.net");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(RequestURI).Result;

            return response;
        }

        public class ITDetails
        {
            public string EmpCode { get; set; }
            public string TicketType { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Description { get; set; }
            public string MachineNum { get; set; }
            public string ExtensionNum { get; set; }
            public string Priority { get; set; }
        }
    }
}