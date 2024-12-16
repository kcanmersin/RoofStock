using Core.Service.PredictionService;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Service.StockTrainingJobService
{
    public class StockTrainingJob : IJob
    {
        private readonly ILogger<StockTrainingJob> _logger;
        private readonly IPredictService _predictService;

        public StockTrainingJob(ILogger<StockTrainingJob> logger, IPredictService predictService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _predictService = predictService ?? throw new ArgumentNullException(nameof(predictService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
                var dataMap = context.MergedJobDataMap; 
                var fileName = dataMap.GetString("FileName") ?? throw new KeyNotFoundException("FileName");
                var daysBack = dataMap.GetInt("DaysBack");
                var epochs = dataMap.GetInt("Epochs");
                var batchSize = dataMap.GetInt("BatchSize");
                var seqLen = dataMap.GetInt("SeqLen");
                var validationSplit = dataMap.GetFloat("ValidationSplit");
                var learningRate = dataMap.GetFloat("LearningRate");
                var dropoutRate = dataMap.GetFloat("DropoutRate");

                _logger.LogInformation("Executing StockTrainingJob with FileName={FileName}, DaysBack={DaysBack}, Epochs={Epochs}, BatchSize={BatchSize}, SeqLen={SeqLen}, ValidationSplit={ValidationSplit}, LearningRate={LearningRate}, DropoutRate={DropoutRate}",
                    fileName, daysBack, epochs, batchSize, seqLen, validationSplit, learningRate, dropoutRate);

                var fileList = new List<string> { fileName };
                var result = await _predictService.TrainMultipleTickersAsync(fileList, daysBack, epochs, batchSize, seqLen, validationSplit, learningRate, dropoutRate);

                _logger.LogInformation("StockTrainingJob completed successfully for FileName={FileName}. Result: {Result}", fileName, result);
        }
    }

}
