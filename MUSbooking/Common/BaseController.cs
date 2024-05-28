using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MUSbooking.Common
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class BaseController : ControllerBase
    {
        private IMediator? mediator;
        private protected IMediator Mediator => mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;
        

        protected ActionResult<T> HandleResult<T>(ValidationResult<T> obj)
        {
            if (obj is null) return NotFound();
            if (obj.IsSuccess && obj.Value is not null)
                return Ok(obj.Value);
            if (obj.IsSuccess && obj.Value is null)
                return NotFound();
            return BadRequest(obj.Error);
        }
    }
}