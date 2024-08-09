using Core.Features.Deposit;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var command = new DepositCommand
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency
            };

            var result = await _sender.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
