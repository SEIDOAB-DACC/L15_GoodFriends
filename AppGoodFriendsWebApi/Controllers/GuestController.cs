using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Configuration;
using Models;
using Models.DTO;

using Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GuestController : Controller
    {
        IFriendsService _friendService = null;
        ILoginService _loginService = null;
        ILogger<GuestController> _logger = null;

       //GET: api/guest/info
        [HttpGet()]
        [ActionName("Info")]
        [ProducesResponseType(200, Type = typeof(gstusrInfoAllDto))]
        public async Task<IActionResult> Info()
        {
            var info = await _friendService.InfoAsync;
            return Ok(info);
        }


        //POST: api/guest/LoginUser
        [HttpPost]
        [ActionName("LoginUser")]
        [ProducesResponseType(200, Type = typeof(loginUserSessionDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> LoginUser([FromBody] loginCredentialsDto userCreds)
        {
            _logger.LogInformation("LoginUser initiated");

            try
            {
                // Note: Validate userCreds to avoid sql injection
                // UserName and password - Allow only Only a-z or A-Z or 0-9 between 4-12 characters
                var pSimple = @"^([a-z]|[A-Z]|[0-9]){4,12}$";

                //RFC2822 email pattern from regexr.com
                var pEmail = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

                //UserNameOrEmail
                var pUNoE = @$"({pSimple})|({pEmail})";

                // Match the regular expression pattern against a text string.
                Regex r = new Regex(pUNoE, RegexOptions.IgnoreCase);
                if (!r.Match(userCreds.UserNameOrEmail).Success) throw new ArgumentException("Wrong username format");

                // Match the regular expression pattern against a text string.
                r = new Regex(pSimple, RegexOptions.IgnoreCase);
                if (!r.Match(userCreds.Password).Success) throw new ArgumentException("Wrong password format");

                //With validated credentials proceed to login
                var _usr = await _loginService.LoginUserAsync(userCreds);
                 _logger.LogInformation($"{_usr.UserName} logged in");
                    return Ok(_usr);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Login Error: {ex.Message}");
                return BadRequest($"Login Error: {ex.Message}");
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        #region constructors
        public GuestController(IFriendsService friendService, ILoginService loginService, ILogger<GuestController> logger)
        {
            _friendService = friendService;
            _loginService = loginService;

            _logger = logger;
        }
        
        /*
        public GuestController(IFriendsService friendService, ILogger<GuestController> logger)
        {
            _friendService = friendService;
            _logger = logger;
        }
        */
        /*
        public GuestController(IFriendsService friendService, ILoginService loginService, ILogger<GuestController> logger)
        {
            _friendService = friendService;
            _loginService = loginService;

            _logger = logger;
        }
        */
        #endregion
    }
}

