using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.Gateways.Interface
{
    public interface INotifyGateway
    {
        Task SendFirstEmail(string[] addresses);
    }
}