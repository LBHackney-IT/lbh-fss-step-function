using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.Gateways.Interface
{
    public interface INotifyGateway
    {
        Task SendNotificationEmail(string organisation, string[] addresses, int state);
    }
}