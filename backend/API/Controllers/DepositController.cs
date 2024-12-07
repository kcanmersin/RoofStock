using Core.Features.Deposit;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Core.Features.User.Deposit
{
    [ApiController]
    [Route("api/deposit")]
    public class DepositController : ControllerBase
    {
        private readonly ISender _sender;

        public DepositController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        //rate limit defaultx
        [EnableRateLimiting("default")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var command = request.Adapt<DepositCommand>();

            var result = await _sender.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
