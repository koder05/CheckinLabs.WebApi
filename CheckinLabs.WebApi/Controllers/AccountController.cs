using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckinLabs.BL.Models;
using CheckinLabs.BL.Repo;
using CheckinLabs.BL.Svc;
using CheckinLabs.WebApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CheckinLabs.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : MyControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountRepo _repo;
        public AccountController(IAccountRepo repo, ILogger<AccountController> logger)
        {
            _repo = repo;
            _logger = logger;
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] AccountRegisterRequest request, [FromServices] INotifier<UserCheckin> notifier)
        {
            try
            {
                var profile = new UserProfile
                {
                    User = new User
                    {
                        Name = request.Email,
                        CreatedOver = request.UIClientId
                    },
                    Email = request.Email,
                    DisplayName = request.Email,
                    CompanyName = request.Company,
                    ManagerName = request.ManagerName,
                };
                var userCheckin = await _repo.RegisterAccountAsync(profile);
                await notifier.CreateNotificationAsync(userCheckin);
                await _repo.MarkUserCheckinNotifiedAsync(userCheckin);
                return Ok();
            }
            catch (UserAccountException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error Register {this.GetType().Name}");
                return InternalServerError(ex);
            }
        }
        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] AccountUpdateRequest request)
        {
            try
            {
                var userCheckin = await _repo.GetUserCheckinAsync(request.Code);
                if (userCheckin == null)
                    return NotFound($"Actual user checkin with code={request.Code} not found");
                await _repo.ChangeAccountAsync(userCheckin, request.UserName, request.UserPassword);
                return Ok();
            }
            catch (UserAccountException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error {nameof(AccountController.UpdateAsync)} {this.GetType().Name}");
                return InternalServerError(ex);
            }
        }
        [HttpPost]
        [Route("remind")]
        public async Task<IActionResult> RemindAsync([FromBody] string userName, [FromServices] INotifier<UserCheckin> notifier)
        {
            try
            {
                var userCheckin = await _repo.RemindAccountAsync(userName);
                await notifier.CreateNotificationAsync(userCheckin);
                await _repo.MarkUserCheckinNotifiedAsync(userCheckin);
                return Ok();
            }
            catch (UserAccountException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error {nameof(AccountController.RemindAsync)} {this.GetType().Name}");
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("check-in/{checkinCode}")]
        public async Task<IActionResult> GetUserCheckinAsync(string checkinCode)
        {
            try
            {
                var userCheckin = await _repo.GetUserCheckinAsync(checkinCode);
                if (userCheckin == null)
                    return NotFound($"Actual user checkin with code={checkinCode} not found");
                return Ok(new {  
                    userCheckin.Code,
                    UserCheckinType = userCheckin.UserCheckinType.ToString(),
                    userCheckin.UserProfile.DisplayName,
                    userCheckin.UserProfile.Email,
                    Login = userCheckin.UserProfile.User.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error {nameof(AccountController.GetUserCheckinAsync)} {this.GetType().Name}");
                return InternalServerError(ex);
            }
        }
    }
}
