using System;
using System.Threading.Tasks;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using Notify.Client;
using Notify.Exceptions;

namespace LbhFssStepFunction.V1.Gateways
{
    public class NotifyGateway : INotifyGateway
    {
        private readonly NotificationClient _client;
        private static string _notifyKey = Environment.GetEnvironmentVariable("NOTIFY_KEY");
        private static string _reverificationFirstEmailTemplate = Environment.GetEnvironmentVariable("REVERIFICATION_FIRST_EMAIL_TEMPLATE");

        public NotifyGateway()
        {
            _client = new NotificationClient(_notifyKey);
        }
        public async Task SendFirstEmail(string[] addresses)
        {
            LoggingHandler.LogInfo($"Sending first email to {addresses[0]}");
            try
            {
                for (int a = 0; a < addresses.Length; a++)
                {
                    await _client.SendEmailAsync(addresses[a], _reverificationFirstEmailTemplate).ConfigureAwait(false);
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