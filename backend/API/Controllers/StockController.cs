using System.Threading.Tasks;
using API.Contracts;
using Core.Features.BuyStock;
using Core.Features.DeletePriceAlert;
using Core.Features.GivePriceAlert;
using Core.Features.SellStock;
using Core.Features.ShowPortfolio;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockController : ControllerBase
    {
        private readonly ISender _sender;

        public StockController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> ShowPortfolio([FromQuery] ShowPortfolioRequest request)
        {
            var command = request.Adapt<ShowPortfolioCommand>();
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

        [HttpPost("sell")]
        public async Task<IActionResult> SellStock([FromBody] SellStockRequest request)
        {
            var command = request.Adapt<SellStockCommand>();
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

        [HttpPost("buy")]
        public async Task<IActionResult> BuyStock([FromBody] BuyStockRequest request)
        {
            var command = request.Adapt<BuyStockCommand>();
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

        [HttpPost("price-alert")]
        public async Task<IActionResult> SetPriceAlert([FromBody] SetPriceAlertRequest request)
        {
            var command = request.Adapt<SetPriceAlertCommand>();
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
        [HttpDelete("price-alert")]
        public async Task<IActionResult> DeletePriceAlert([FromQuery] DeletePriceAlertRequest request)
        {
            var command = request.Adapt<DeletePriceAlertCommand>();
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new { message = "Price alert deleted successfully." });
            }

            return BadRequest(new
            {
                code = result.Error.Code,
                message = result.Error.Message
            });
        }
    }
}