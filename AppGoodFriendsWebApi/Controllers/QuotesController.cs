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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Data;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QuotesController : Controller
    {
        loginUserSessionDto _usr = null;

        IFriendsService _service = null;
        ILogger<QuotesController> _logger = null;

        //GET: api/quotes/read
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
           Policy = null, Roles = "usr, supusr")]
        [HttpGet()]
        [ActionName("Read")]
        [ProducesResponseType(200, Type = typeof(csRespPageDto<IQuote>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Read(string seeded = "true", string flat = "true",
            string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                bool _seeded = bool.Parse(seeded);
                bool _flat = bool.Parse(flat);
                int _pageNr = int.Parse(pageNr);
                int _pageSize = int.Parse(pageSize);

                _logger.LogInformation($"{nameof(Read)}: {nameof(_seeded)}: {_seeded}, {nameof(_flat)}: {_flat}, " +
                    $"{nameof(_pageNr)}: {_pageNr}, {nameof(_pageSize)}: {_pageSize}");
                var _resp = await _service.ReadQuotesAsync(_usr, _seeded, _flat, filter?.Trim()?.ToLower(), _pageNr, _pageSize);     
                return Ok(_resp);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(Read)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        //GET: api/quotes/readitem
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            Policy = null, Roles = "usr, supusr")]
        [HttpGet()]
        [ActionName("ReadItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItem(string id = null, string flat = "false")
        {
            try
            {
                var _id = Guid.Parse(id);
                bool _flat = bool.Parse(flat);


                _logger.LogInformation($"{nameof(ReadItem)}: {nameof(_id)}: {_id}, {nameof(_flat)}: {_flat}");

                var item = await _service.ReadQuoteAsync(_usr, _id, false);
                if (item == null) throw new Exception ($"Item with id {id} does not exist");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
        //DELETE: api/quotes/deleteitem/id
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
           Policy = null, Roles = "supusr")]
        [HttpDelete("{id}")]
        [ActionName("DeleteItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {   
            try
            {
                var _id = Guid.Parse(id);
                
                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(_id)}: {_id}");

                var item = await _service.DeleteQuoteAsync(_usr, _id);
                if (item == null) throw new Exception ($"Item with id {id} does not exist");
        
                _logger.LogInformation($"item {_id} deleted");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(DeleteItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        //GET: api/quotes/readitemdto
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            Policy = null, Roles = "usr, supusr")]
        [HttpGet()]
        [ActionName("ReadItemDto")]
        [ProducesResponseType(200, Type = typeof(csQuoteCUdto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItemDto(string id = null)
        {
            try
            {
                var _id = Guid.Parse(id);
  
                _logger.LogInformation($"{nameof(ReadItemDto)}: {nameof(_id)}: {_id}");
                
                var item = await _service.ReadQuoteAsync(_usr, _id, false);
                if (item == null) throw new Exception ($"Item with id {id} does not exist");

                var dto = new csQuoteCUdto(item);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(ReadItemDto)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }


        //PUT: api/quotes/updateitem/id
        //Body: csQuoteCUdto in Json
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
           Policy = null, Roles = "usr, supusr")]
        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] csQuoteCUdto item)
        {
            try
            {
                var _id = Guid.Parse(id);

                _logger.LogInformation($"{nameof(UpdateItem)}: {nameof(_id)}: {_id}");
                
                if (item.QuoteId != _id) throw new Exception("Id mismatch");

                var _item = await _service.UpdateQuoteAsync(_usr, item);
                _logger.LogInformation($"item {_id} updated");
               
                return Ok(_item);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(UpdateItem)}: {ex.Message}");
                return BadRequest($"Could not update. Error {ex.Message}");
            }
        }

        //POST: api/quotes/createitem
        //Body: csQuoteCUdto in Json
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            Policy = null, Roles = "usr, supusr")]
        [HttpPost()]
        [ActionName("CreateItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateItem([FromBody] csQuoteCUdto item)
        {
            try
            {
                _logger.LogInformation($"{nameof(CreateItem)}:");
               
                var _item = await _service.CreateQuoteAsync(_usr, item);
                _logger.LogInformation($"item {_item.QuoteId} created");

                return Ok(_item);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{nameof(CreateItem)}: {ex.Message}");
                return BadRequest($"Could not create. Error {ex.Message}");
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
           //Remember async programming. .Result waits for the Task to complete
            var _token = HttpContext.GetTokenAsync("access_token").Result;
            _usr = csJWTService.DecodeToken(_token);
            base.OnActionExecuting(context);
        }

        #region constructors
        public QuotesController(IFriendsService service, ILogger<QuotesController> logger)
        {
            _service = service;
            _logger = logger;
        }
        /*
        public QuotesController(ILogger<QuotesController> logger)
        {
            _logger = logger;
        }
        public QuotesController(IFriendsService service, ILogger<QuotesController> logger)
        {
            _service = service;
            _logger = logger;
        }
        */
        #endregion
    }
}

