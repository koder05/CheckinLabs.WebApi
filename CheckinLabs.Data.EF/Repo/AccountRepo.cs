using CheckinLabs.BL.Models;
using CheckinLabs.BL.Repo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.Data.EF.Repo
{
    public class AccountRepo : IAccountRepo
    {
        private readonly AppDbContext _db;
        public AccountRepo(AppDbContext db)
        {
            _db = db;
        }
        public async Task<User> ChangeAccountAsync(UserCheckin checkin, string userName, string userPwd)
        {
            if (checkin.UserProfile.User.AccountState == BL.Enum.AccountState.Disabled)
                throw new UserAccountException("Invalid account.");
            if (_db.Set<User>().Any(u => u.Name == userName && u.Id != checkin.UserProfile.Id))
                throw new UserAccountException("Invalid user name.");
            checkin.UserProfile.User.Name = userName;
            checkin.UserProfile.User.SetPassword(userPwd);
            checkin.UserProfile.User.AccountState = BL.Enum.AccountState.Active;
            checkin.IsUsed = true;
            checkin.CheckinDate = DateTime.Now;
            await _db.SaveChangesAsync();
            return checkin.UserProfile.User;
        }

        public async Task<UserProfile> GetAccountAsync(string userName, string userPwd)
        {
            var profile = await _db.Set<UserProfile>()
                .Include(p => p.User)
                .SingleOrDefaultAsync(p => p.User.Name == userName && p.User.AccountState == BL.Enum.AccountState.Active);
            if(profile == null)
                throw new UserAccountException("user name or password not found.");
            if (profile.User.GeneratePasswordHash(userPwd, profile.User.SecretSalt).SequenceEqual(profile.User.SecretHash))
            {
                return profile;
            }
            else
            {
                throw new UserAccountException("Invalid user name or password.");
            }
        }

        public async Task<UserCheckin> RegisterAccountAsync(UserProfile profile)
        {
            if (_db.Set<User>().Any(u => u.Name == profile.User.Name))
                throw new UserAccountException("Invalid user.");
            profile.User.SetPassword(Guid.NewGuid().ToString());
            var userCheckin = new UserCheckin
            {
                UserProfile = profile,
                CheckinAddr = profile.Email,
                NotifyChannelType = BL.Enum.NotifyChannelType.Email,
                UserCheckinType = BL.Enum.UserCheckinType.RegisterAccount
            };
            userCheckin.Msg = $"Welcome! Confirm your register with code: {userCheckin.Code}";
            await _db.Set<UserCheckin>().AddAsync(userCheckin);
            await _db.SaveChangesAsync();
            return userCheckin;
        }
        public async Task<UserCheckin> RemindAccountAsync(string userName)
        {
            var profile = await _db.Set<UserProfile>().SingleOrDefaultAsync(p => p.User.Name == userName && p.User.AccountState != BL.Enum.AccountState.Disabled);
            if (profile == null)
                throw new UserAccountException("user name not found.");
            foreach(var oldCheckin in _db.Set<UserCheckin>().Where(ch => ch.IsUsed == false && ch.UserProfile.Id == profile.Id))
            {
                oldCheckin.IsUsed = true;
            }
            var userCheckin = new UserCheckin
            {
                UserProfile = profile,
                CheckinAddr = profile.Email,
                NotifyChannelType = BL.Enum.NotifyChannelType.Email,
                UserCheckinType = BL.Enum.UserCheckinType.ChangeAccount
            };
            userCheckin.Msg = $"Welcome! To change your account link with code: {userCheckin.Code}";
            await _db.Set<UserCheckin>().AddAsync(userCheckin);
            await _db.SaveChangesAsync();
            return userCheckin;
        }
        public async Task MarkUserCheckinNotifiedAsync(UserCheckin userCheckin)
        {
            userCheckin = await _db.Set<UserCheckin>().FindAsync(userCheckin.Id);
            if (userCheckin != null && userCheckin.NotifyDate == null)
            {
                userCheckin.NotifyDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
        public async Task<UserCheckin> GetUserCheckinAsync(string checkinCode)
        {
            var checkin = await _db.Set<UserCheckin>()
                .Include(ch => ch.UserProfile)
                .Include(ch => ch.UserProfile.User)
                .SingleOrDefaultAsync(ch => ch.Code == checkinCode && ch.IsUsed == false);
            if (checkin == null || checkin.OutOfDate)
                return null;
            return checkin;
        }
    }
}
