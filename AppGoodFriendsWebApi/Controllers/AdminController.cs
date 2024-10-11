using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Models;
using Models.DTO;
using Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]

#if !DEBUG    
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
       Policy = null, Roles = "supusr")]
#endif
    public class AdminController : Controller
    {
        loginUserSessionDto _usr = null;

        IFriendsService _friendService = null;
        ILoginService _loginService = null;
        ILogger<AdminController> _logger;
        
        //GET: api/guest/info
        [HttpGet()]
        [ActionName("Info")]
        [ProducesResponseType(200, Type = typeof(adminInfoDbDto))]
        public IActionResult Info()
        {
            var info = new adminInfoDbDto();

            _logger.LogInformation($"{nameof(Info)}:\n{info.appEnvironment}, {info.dbConnection}, {info.secretSource}");
            return Ok(info);
        }

#if DEBUG
        //GET: api/admin/seed?count={count}
        [HttpGet()]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
          Policy = null, Roles = "supusr")]
        [ActionName("Seed")]
        [ProducesResponseType(200, Type = typeof(gstusrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Seed(string count = "100")
        {
            try
            {
                int _count = int.Parse(count);

                _logger.LogInformation($"{nameof(Seed)}: {nameof(_count)}: {_count}");
                gstusrInfoAllDto _info = await _friendService.SeedAsync(_usr, _count);
                return Ok(_info);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //GET: api/admin/removeseed
        [HttpGet()]
        [ActionName("RemoveSeed")]
        [ProducesResponseType(200, Type = typeof(gstusrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> RemoveSeed(string seeded = "true")
        {
            try
            {
                bool _seeded = bool.Parse(seeded);

                _logger.LogInformation($"{nameof(RemoveSeed)}: {nameof(_seeded)}: {_seeded}");
                gstusrInfoAllDto _info = await _friendService.RemoveSeedAsync(_usr, _seeded);
                return Ok(_info);        
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //GET: api/admin/seeduser?users={count}&superusers={count}
        [HttpGet()]
        [ActionName("SeedUsers")]
        [ProducesResponseType(200, Type = typeof(usrInfoDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> SeedUsers(string countUsr = "32", string countSupUsr = "2")
        {
            try
            {
                int _countUsr = int.Parse(countUsr);
                int _countSupUsr = int.Parse(countSupUsr);

                _logger.LogInformation($"{nameof(SeedUsers)}: {nameof(_countUsr)}: {_countUsr}, {nameof(_countSupUsr)}: {_countSupUsr}");

                usrInfoDto _info = await _loginService.SeedAsync(_countUsr, _countSupUsr);
                return Ok(_info);           
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }       
        }
#endif
        //GET: api/admin/log
        [HttpGet()]
        [ActionName("Log")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<csLogMessage>))]
        public async Task<IActionResult> Log([FromServices] ILoggerProvider _loggerProvider)
        {
            //Note the way to get the LoggerProvider, not the logger from Services via DI
            if (_loggerProvider is csInMemoryLoggerProvider cl)
            {
                return Ok(await cl.MessagesAsync);
            }
            return Ok("No messages in log");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
           //Remember async programming. .Result waits for the Task to complete
            var _token = HttpContext.GetTokenAsync("access_token").Result;
            _usr = csJWTService.DecodeToken(_token);
            base.OnActionExecuting(context);
        }

        #region constructors
        public AdminController(IFriendsService friendService, ILoginService loginService, ILogger<AdminController> logger)
        {
            _friendService = friendService;
            _loginService = loginService;

            _logger = logger;
        }

        /*
        public AdminController(IFriendsService friendService, ILogger<AdminController> logger)
        {
            _friendService = friendService;
            _logger = logger;
        }
        */
        /*
        public AdminController(IFriendsService friendService, ILoginService loginService, ILogger<AdminController> logger)
        {
            _friendService = friendService;
            _loginService = loginService;

            _logger = logger;
        }
        */
        #endregion
    }
}

