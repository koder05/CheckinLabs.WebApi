using CheckinLabs.BL.Enum;
using CheckinLabs.BL.Svc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.Notifications
{
    public sealed class ConsoleNotifyChannel : INotifyChannel
    {
        public NotifyChannelType Type => NotifyChannelType.Trace;

        public Task SendNotificationAsync(string addr, string header, string msgTmp, IDictionary<string, string> tmpParams)
        {
            Console.WriteLine(header);
            Console.WriteLine(msgTmp);
            return Task.CompletedTask;
        }
    }
}
