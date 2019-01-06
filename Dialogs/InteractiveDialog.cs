namespace SimpleEchoBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class InteractiveDialog : IDialog<int>
    {
     
        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("You have selected Shift Booking !");
            // context.Wait(this.OrderStatusFormCallback);
            var orderStatusForm = new FormDialog<OrderStatusDialog>(new OrderStatusDialog(), OrderStatusDialog.BuildForm, FormOptions.PromptInStart);
            context.Call(orderStatusForm, OrderStatusFormCallback);
        }

        private async Task OrderStatusFormCallback(IDialogContext context, IAwaitable<OrderStatusDialog> result)
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


    [Serializable]
    public class OrderStatusDialog
    {
        

        public static IForm<OrderStatusDialog> BuildForm()
        {
                  return new FormBuilder<OrderStatusDialog>()
                    .Message("Glad you like me - I like you too!")
                     .Message("I'm happy to hear from you! !!")
                     .Confirm("No verification will be shown", state => false)
                    .Build();
        }
        
    };
}





