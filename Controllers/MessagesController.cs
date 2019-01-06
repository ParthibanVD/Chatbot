using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Net;
using SimpleEchoBot.Dialogs;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            
            if (activity.GetActivityType() == ActivityTypes.Message)
            {

                string temEmpData = activity.From.Id.ToString() + " = " + activity.From.Name.ToString();
                ShiftDialog1.EmpData = temEmpData;
                ShiftDialog1.EmpCodeFromUser = activity.From.Id.ToString();
                ShiftDialog1.Companycode = activity.From.Name.ToString();
                AdaptiveCardDialog.EmpData = temEmpData;
                AdaptiveCardDialog.EmpCodeFromUser = activity.From.Id.ToString();
                SoftwareDialog.EmpData = temEmpData;
                SoftwareDialog.EmpCodeFromUser = activity.From.Id.ToString();
                TqDialog.EmpData = temEmpData;
                TqDialog.EmpCodeFromUser = activity.From.Id.ToString();
                eNCRDialog.EmpData = temEmpData;
                eNCRDialog.EmpCodeFromUser = activity.From.Id.ToString();
                ITHelpDeskDialog.EmpData = temEmpData;
                ITHelpDeskDialog.EmpCodeFromUser = activity.From.Id.ToString();
                EchoDialog.EmpData = temEmpData;
                EchoDialog.EmpCodeFromUser = activity.From.Name.ToString();
                await Conversation.SendAsync(activity, () => new EchoDialog());

            }
            else 
            {
                await HandleSystemMessageAsync(activity);
                //string temcountry = activity.From.Name.ToString();
                //await Conversation.SendAsync(activity, () => new EchoDialog());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            string messageType = message.GetActivityType();
            if (messageType == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (messageType == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
               // IConversationUpdateActivity iConversationUpdated = message as IConversationUpdateActivity;
                //if (iConversationUpdated != null)
               // {
                 //   ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));
                 //   foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                  //  {
                        // if the bot is added, then   
                   //     if (member.Id == iConversationUpdated.Recipient.Id)
                    //    {
                     //       var reply = ((Activity)iConversationUpdated).CreateReply($"Top Sugeestions!\n1.Shift Booking!\n \n2.Leave Booking!\n");
                       //     await connector.Conversations.ReplyToActivityAsync(reply);

                       // }
                  //  }
              //  }
            }
            else if (messageType == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (messageType == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (messageType == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}