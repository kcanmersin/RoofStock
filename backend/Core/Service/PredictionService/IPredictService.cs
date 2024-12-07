using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.PredictionService
{

    public interface IPredictService
    {
        Task<string> FetchDataAsync(string ticker, int daysBack);
        Task<string> PrepareDataAsync(string ticker);
        Task<string> TrainModelAsync(string ticker, int epochs, int batchSize, int seqLen, float validationSplit, float learningRate, float dropoutRate);
        Task<string> CompleteTrainingAsync(string ticker, int daysBack, int epochs, int batchSize, int seqLen, float validationSplit, float learningRate, float dropoutRate);
        Task<string> PredictAsync(string ticker, int predictDays, int seqLen);

        Task<string> TrainMultipleTickersAsync(List<string> fileList, int daysBack, int epochs, int batchSize, int seqLen, float validationSplit, float learningRate, float dropoutRate);

    }
}
