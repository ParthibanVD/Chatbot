using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using SimpleEchoBot.Dialogs;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        private const string ShiftOption = "Shift Booking";

        private const string LeaveOption = "Leave Booking";

        private const string SoftwareOption = " Software HelpDesk ";

        private const string TqOption = " TQ Manager ";

        private const string NcrOption = " eNCR";

        private const string ITOption = "IT HelpDesk";

        public static string EmpData = "";

        public static string EmpCodeFromUser = "";



        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower().Contains("issue") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else if  (message.Text.ToLower().Contains("shiftbooking") || message.Text.ToLower().Contains("shift booking") || message.Text.ToLower().Contains("Shift Booking") || message.Text.ToLower().Contains("ShiftBooking"))
            {
                // await context.Forward(new ShiftDialog(), this.ResumeAfterOptionDialog, message, CancellationToken.None);
                context.Call(new ShiftDialog1(), ResumeAfterOptionDialog);
            }
            else if (message.Text.ToLower().Contains("leavebooking") || message.Text.ToLower().Contains("Leave Booking") || message.Text.ToLower().Contains("leave booking") || message.Text.ToLower().Contains("LeaveBooking"))
            {
               // await context.Forward(new Leavefrom(), this.ResumeAfterOptionDialog, message, CancellationToken.None);
                context.Call(new AdaptiveCardDialog(), ResumeAfterOptionDialog);
            }
            else if (message.Text.ToLower().Contains("i like you") || message.Text.ToLower().Contains("i love you") || message.Text.ToLower().Contains("ilikeyou, iloveyou"))
            {
                // await context.Forward(new Leavefrom(), this.ResumeAfterOptionDialog, message, CancellationToken.None);
                context.Call(new InteractiveDialog(), ResumeAfterfinalDialogAsync);
            }
            else if (Convert.ToInt32(EmpCodeFromUser) == 5)
            {
                await this.ShowOptionsUKAsync (context);
            }
            else
            {
                await this.ShowOptionsAsync(context);
            }
        }

        private async Task ResumeAfterfinalDialogAsync(IDialogContext context, IAwaitable<int> result)
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
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ShowOptionsUKAsync(IDialogContext context)
        {

            await context.PostAsync($"Hi, Iam EPMS Virtual Assistant, How can I help you today!");
            PromptDialog.Choice(context, SelectedAttachmentAsync, new List<string> { "Project Master", "Support", },
           "Select one from the List ", "Not a valid option", 3);

        }

        private async Task ShowOptionsAsync(IDialogContext context)
          {

            await context.PostAsync($"Hi, Iam EPMS Virtual Assistant, How can I help you today!");
            PromptDialog.Choice(context, SelectedAttachmentAsync, new List<string> { "Plan & Timesheets", "Project Master", "Support", },
           "Select one from the List ", "Not a valid option", 3);

          }

        public async Task SelectedAttachmentAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            switch (message)
            {
                case "Plan & Timesheets":
                    PlanTimesheetAsync(context);       
                    break;
                case "Project Master":
                    ProjectmasterAsync(context);
                    break;
                case "Support":
                    SupportAsync(context);
                    break;
            }
        }
        public void PlanTimesheetAsync(IDialogContext context)
        {

            var message = context.MakeMessage();
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { ShiftOption, LeaveOption}, "Select one from the List ", "Not a valid option", 3);
        }

        public void ProjectmasterAsync(IDialogContext context)
        {

            var message = context.MakeMessage();
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { TqOption, NcrOption, }, "Select one from the List ", "Not a valid option", 3);
        }

        public void SupportAsync(IDialogContext context)
        {

            var message = context.MakeMessage();
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { SoftwareOption, ITOption }, "Select one from the List ", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case ShiftOption:
                        context.Call(new ShiftDialog1(), this.ResumeAfterOptionDialog);
                        break;

                    case LeaveOption:
                        context.Call(new AdaptiveCardDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case SoftwareOption:
                        context.Call(new SoftwareDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case ITOption:
                        context.Call(new ITHelpDeskDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case TqOption:
                       context.Call(new TqDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case NcrOption:
                        context.Call(new eNCRDialog(), this.ResumeAftereNCRDialog);
                        break;

                }
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync($"Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!");
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
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
                context.Wait(this.MessageReceivedAsync);
            }

        }

        private async Task ResumeAftereNCRDialog(IDialogContext context, IAwaitable<object> result)
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
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}