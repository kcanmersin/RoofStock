using Core.Features.Withdrawal;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/withdrawal")]
    public class WithdrawalController : ControllerBase
    {
        private readonly ISender _sender;

        public WithdrawalController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequest request)
        {
            var command = request.Adapt<WithdrawalCommand>();
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
