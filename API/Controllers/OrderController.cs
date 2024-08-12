using API.Contracts;
using Core.Features.CancelOrder;
using Core.Features.GiveOrder;
using Core.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ISender _sender;

        public OrderController(ISender sender)
        {
            _sender = sender;
        }
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest request)
        {
            var command = new CancelOrderCommand
            {
                OrderId = request.OrderId,
                UserId = request.UserId
            };

            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }

        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] GiveOrderRequest request)
        {
            var command = request.Adapt<OrderCommand>();
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(new
            {
                code = result.Error.Code,
                message = result.Error.Message
            });
        }

    }
}
