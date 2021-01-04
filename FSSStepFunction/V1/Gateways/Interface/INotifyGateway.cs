using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.Gateways.Interface
{
    public interface INotifyGateway
    {
        Task SendNotificationEmail(string[] addresses, int state);
    }
}