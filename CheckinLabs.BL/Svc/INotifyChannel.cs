using CheckinLabs.BL.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckinLabs.BL.Svc
{
    public interface INotifyChannel
    {
        NotifyChannelType Type { get; }
        Task SendNotificationAsync(string addr, string header, string msgTmp, IDictionary<string, string> tmpParams);
    }
}
