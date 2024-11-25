using System.Net.Http.Json;

namespace Core.Service.PredictionService
{
    public class PredictService : IPredictService
    {
        private readonly HttpClient _httpClient;

        public PredictService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> FetchDataAsync(string ticker, int daysBack)
        {
            var url = $"/fetch_data?ticker={ticker}&days_back={daysBack}";
            return await SendRequestAsync(url);
        }

        public async Task<string> PrepareDataAsync(string ticker)
        {
            var url = $"/add_indicators?ticker={ticker}";
            return await SendRequestAsync(url);
        }

        public async Task<string> TrainModelAsync(string ticker, int epochs, int batchSize, int seqLen, float validationSplit, float learningRate, float dropoutRate)
        {
            var url = $"/train_lstm_model?ticker={ticker}&epochs={epochs}&batch_size={batchSize}&seq_len={seqLen}&validation_split={validationSplit}&learning_rate={learningRate}&dropout_rate={dropoutRate}";
            return await SendRequestAsync(url);
        }

        public async Task<string> CompleteTrainingAsync(string ticker, int daysBack, int epochs, int batchSize, int seqLen, float validationSplit, float learningRate, float dropoutRate)
        {
            var url = $"/complete_training?ticker={ticker}&days_back={daysBack}&epochs={epochs}&batch_size={batchSize}&seq_len={seqLen}&validation_split={validationSplit}&learning_rate={learningRate}&dropout_rate={dropoutRate}";
            return await SendRequestAsync(url);
        }

        public async Task<string> PredictAsync(string ticker, int predictDays, int seqLen)
        {
            var url = $"/predict_lstm_model?ticker={ticker}&predict_days={predictDays}&seq_len={seqLen}";
            return await SendRequestAsync(url);
        }
        public async Task<string> TrainMultipleTickersAsync(List<string> fileList, int daysBack, int epochs, int batchSize, int seqLen, float validationSplit, float learningRate, float dropoutRate)
        {
            var payload = new
            {
                file_list = fileList,
                days_back = daysBack,
                epochs,
                batch_size = batchSize,
                seq_len = seqLen,
                validation_split = validationSplit,
                learning_rate = learningRate,
                dropout_rate = dropoutRate
            };

            var response = await _httpClient.PostAsJsonAsync("/train_multiple_tickers", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request to Flask API failed: {response.StatusCode}, {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        private async Task<string> SendRequestAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request to Flask API failed: {response.StatusCode}, {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
