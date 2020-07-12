using CheckinLabs.BL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckinLabs.BL.Repo
{
    public interface IAccountRepo
    {
        Task<UserCheckin> RegisterAccountAsync(UserProfile profile);
        Task<UserCheckin> RemindAccountAsync(string userName);
        Task MarkUserCheckinNotifiedAsync(UserCheckin userCheckin);
        Task<UserCheckin> GetUserCheckinAsync(string checkinCode);
        Task<User> ChangeAccountAsync(UserCheckin checkin, string userName, string userPwd);
        Task<UserProfile> GetAccountAsync(string username, string userPwd);
    }
}
