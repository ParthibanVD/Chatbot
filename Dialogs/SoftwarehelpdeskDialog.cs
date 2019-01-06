namespace SimpleEchoBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    [Serializable]
    public class SoftwarehelpdeskDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("You have selected Software HelpDesk!");
            // context.Wait(this.OrderStatusFormCallback);
            var orderStatusForm = new FormDialog<HelpdeskDialog>(new HelpdeskDialog(), HelpdeskDialog.BuildForm, FormOptions.PromptInStart);
            context.Call(orderStatusForm, FormCallback);
        }

        private async Task FormCallback(IDialogContext context, IAwaitable<HelpdeskDialog> result)
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

        [Serializable]
        public class HelpdeskDialog
        {
            //  [Prompt(" Please select from the List{||}")]
            //   public ListOptions? List;
            [Prompt(" Please select your shift {||}")]
            public ShiftOptions? Shift;
            [Prompt(" Please select the Booking Month{||}")]
            public MonthOptions? Month;
            public static string EmpData = "";
            public static string EmpCodeFromUser = "";














        }
}
