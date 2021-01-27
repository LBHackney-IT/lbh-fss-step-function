using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using Notify.Client;
using Notify.Exceptions;

namespace LbhFssStepFunction.V1.Gateways
{
    public class NotifyGateway : INotifyGateway
    {
        private static string _notifyKey = Environment.GetEnvironmentVariable("NOTIFY_KEY");
        private static string _reverificationFirstEmailTemplate = Environment.GetEnvironmentVariable("REVERIFICATION_FIRST_EMAIL_TEMPLATE");
        private static string _reverificationSecondEmailTemplate = Environment.GetEnvironmentVariable("REVERIFICATION_SECOND_EMAIL_TEMPLATE");
        private static string _reverificationThirdEmailTemplate = Environment.GetEnvironmentVariable("REVERIFICATION_THIRD_EMAIL_TEMPLATE");
        private static string _reverificationPauseEmailTemplate = Environment.GetEnvironmentVariable("REVERIFICATION_PAUSE_EMAIL_TEMPLATE");

        public NotifyGateway()
        {
            
        }

        public async Task SendNotificationEmail(string organisation, string[] addresses, int state)
        {
            var _client = new NotificationClient(_notifyKey);
            var notifyTemplate = string.Empty;
            var personalisation = new Dictionary<string, dynamic>();
            personalisation.Add("CompanyName", organisation ?? "");
            switch (state)
            {
                case 1:
                    notifyTemplate = _reverificationFirstEmailTemplate;
                    break;
                case 2:
                    notifyTemplate = _reverificationSecondEmailTemplate;
                    break;
                case 3:
                    notifyTemplate = _reverificationThirdEmailTemplate;
                    break;
                case 4:
                    notifyTemplate = _reverificationPauseEmailTemplate;
                    break;
            }
            LoggingHandler.LogInfo($"Sending email to organisation contacts.");
            try
            {
                for (int a = 0; a < addresses.Length; a++)
                {
                    await _client.SendEmailAsync(addresses[a], notifyTemplate, personalisation).ConfigureAwait(false);
                    LoggingHandler.LogInfo($"Successfully sent email to organisation recipients");
                }
            }
            catch (NotifyClientException e)
            {
                LoggingHandler.LogError("Gov Notify send error");
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
            }
            catch (Exception e)
            {
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
                throw;
            }
        }
    }
}