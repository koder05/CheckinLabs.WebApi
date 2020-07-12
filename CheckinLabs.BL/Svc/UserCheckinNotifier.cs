using CheckinLabs.AppBase;
using CheckinLabs.BL.Enum;
using CheckinLabs.BL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckinLabs.BL.Svc
{
    public class UserCheckinNotifier : INotifier<UserCheckin>
    {
        private readonly IEnumerable<INotifyChannel> _channels;
        private readonly IConfiguration _cfg;
        private readonly ILogger<UserCheckinNotifier> _logger;
        public UserCheckinNotifier(IEnumerable<INotifyChannel> channels, IConfiguration cfg, ILogger<UserCheckinNotifier> logger)
        {
            _channels = channels;
            _logger = logger;
            _cfg = cfg;
        }
        public async Task CreateNotificationAsync(UserCheckin subj)
        {
            var tmpParams = new Dictionary<string, string>();
            tmpParams.Add($"{{{nameof(UserCheckin.UserProfile)}.{nameof(UserProfile.DisplayName)}}}", subj.UserProfile.DisplayName);
            tmpParams.Add($"{{{nameof(UserCheckin.Code)}}}", subj.Code);

            var msgTmplFile = string.Empty;
            var header = string.Empty;
            switch(subj.UserCheckinType)
            {
                case UserCheckinType.RegisterAccount:
                    header = "Подтверждение регистрации в системе CheckinLabs";
                    msgTmplFile = "register_msg_tmp.html";
                    break;
                case UserCheckinType.ChangeAccount:
                    header = "Изменение учётных данных в системе CheckinLabs";
                    msgTmplFile = "remind_msg_tmp.html";
                    break;
            }

            try
            {
                var msgTmpl = File.ReadAllText(Path.Combine(CApp.AssemblyDirectory, msgTmplFile));
                var msgBuilder = new StringBuilder(msgTmpl);
                foreach (var kvp in tmpParams)
                    msgBuilder.Replace(kvp.Key, kvp.Value);
                subj.Msg = msgBuilder.ToString();

                var traces = GetChannels(NotifyChannelType.Trace);
                foreach (var channel in traces)
                    await channel.SendNotificationAsync(subj.CheckinAddr, header, subj.Msg, tmpParams);
                var emails = GetChannels(NotifyChannelType.Email);
                foreach (var channel in emails)
                    await channel.SendNotificationAsync(subj.CheckinAddr, header, subj.Msg, tmpParams);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"error {nameof(UserCheckinNotifier.CreateNotificationAsync)} {this.GetType().Name}");
            }
        }
        private IEnumerable<INotifyChannel> GetChannels(NotifyChannelType type)
        {
            if (_channels == null)
                return new INotifyChannel[0];
            return _channels.Where(i => i.Type == type);
        }
    }
}
