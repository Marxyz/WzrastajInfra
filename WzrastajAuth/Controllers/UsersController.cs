using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WzrastajAuth.Controllers.Requests;

namespace WzrastajAuth.Controllers
{
    [ApiController]
    [Route("[controller]/api")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger Logger;
        private readonly IUserActionHandler UserActionHandler;

        public UsersController(ILogger<UsersController> logger, IUserActionHandler userActionHandler)
        {
            Logger = logger;
            UserActionHandler = userActionHandler;
        }


        [AllowAnonymous]
        [HttpPost("/register")]
        [ValidateModel]
        public async Task<IActionResult> Create(RegistrationRequest request)
        {
            var res = await UserActionHandler.CreateAsync(request);
            return res.ToPresentable();
        }


        [Authorize(Policy = "User")]
        [HttpGet("/me/{id}")]
        [ValidateModel]
        public async Task<IActionResult> GetUserInfo(Guid id)
        {
            var responsePresenter = await UserActionHandler.GetById(id);
            return responsePresenter.ToPresentable();
        }
    }
}