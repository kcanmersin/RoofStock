using Core.Service.PredictionService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly IPredictService _predictService;

    public PredictionController(IPredictService predictService)
    {
        _predictService = predictService;
    }

    [HttpGet("fetch-data")]
    public async Task<IActionResult> FetchData(string ticker, int daysBack = 500)
    {
        var a = "selam";
        try
        {
            var result = await _predictService.FetchDataAsync(ticker, daysBack);
            return Ok(JsonConvert.DeserializeObject(result));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Fetching data failed", error = ex.Message });
        }
    }

    [HttpGet("prepare-data")]
    public async Task<IActionResult> PrepareData(string ticker)
    {
        try
        {
            var result = await _predictService.PrepareDataAsync(ticker);
            return Ok(JsonConvert.DeserializeObject(result));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Preparing data failed", error = ex.Message });
        }
    }

    [HttpPost("train")]
    public async Task<IActionResult> TrainModel(string ticker, int epochs = 50, int batchSize = 32, int seqLen = 60,
                                                 float validationSplit = 0.1f, float learningRate = 0.001f, float dropoutRate = 0.2f)
    {
        try
        {
            var result = await _predictService.TrainModelAsync(ticker, epochs, batchSize, seqLen, validationSplit, learningRate, dropoutRate);
            return Ok(JsonConvert.DeserializeObject(result));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Training model failed", error = ex.Message });
        }
    }

    [HttpPost("complete-training")]
    public async Task<IActionResult> CompleteTraining(string ticker, int daysBack = 500, int epochs = 50, int batchSize = 32,
                                                       int seqLen = 60, float validationSplit = 0.1f, float learningRate = 0.001f, float dropoutRate = 0.2f)
    {
        try
        {
            var result = await _predictService.CompleteTrainingAsync(ticker, daysBack, epochs, batchSize, seqLen, validationSplit, learningRate, dropoutRate);
            return Ok(JsonConvert.DeserializeObject(result));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Complete training failed", error = ex.Message });
        }
    }

    [HttpGet("predict")]
    public async Task<IActionResult> PredictStockPrices(string ticker, int predictDays = 10, int seqLen = 60)
    {
        try
        {
            var result = await _predictService.PredictAsync(ticker, predictDays, seqLen);
            return Ok(JsonConvert.DeserializeObject(result));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Prediction failed", error = ex.Message });
        }
    }
}
