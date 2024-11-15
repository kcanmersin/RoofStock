using System.Threading.Tasks;
using API.Contracts;
using Core.Features.BuyStock;
using Core.Features.GivePriceAlert;
using Core.Features.SellStock;
using Core.Features.ShowPortfolio;
using Core.Service.KafkaService;
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
        private readonly UserActivityService _userActivityService;

        public StockController(ISender sender, UserActivityService userActivityService)
        {
            _sender = sender;
            _userActivityService = userActivityService;
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> ShowPortfolio([FromQuery] ShowPortfolioRequest request)
        {
            var command = request.Adapt<ShowPortfolioCommand>();
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
            //    await _userActivityService.TrackUserActivityAsync(request.UserId.ToString(), "ViewPortfolio");
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
             //   await _userActivityService.TrackUserActivityAsync(request.UserId.ToString(), "SellStock", new { StockSymbol = request.StockSymbol, Quantity = request.Quantity });
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
              //  await _userActivityService.TrackUserActivityAsync(request.UserId.ToString(), "BuyStock", new { StockSymbol = request.StockSymbol, Quantity = request.Quantity });
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
             //   await _userActivityService.TrackUserActivityAsync(request.UserId.ToString(), "SetPriceAlert", new { StockSymbol = request.StockSymbol, TargetPrice = request.TargetPrice });
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