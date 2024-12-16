from flask import Flask, request, jsonify
from flask_cors import CORS
import os
from datetime import datetime, timedelta
import pandas as pd
import numpy as np
import yfinance as yf
import pandas_ta as ta
from tensorflow.keras.models import load_model, Sequential
from tensorflow.keras.layers import GRU, Dense, Dropout, Bidirectional, Input
from tensorflow.keras.callbacks import EarlyStopping
from tensorflow.keras import regularizers
from sklearn.preprocessing import MinMaxScaler
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score, mean_absolute_percentage_error
from sklearn.ensemble import RandomForestRegressor
import joblib
import tensorflow as tf
import random

app = Flask(__name__)
CORS(app)

DATA_DIR = 'data'
PREDICTIONS_DIR = 'predictions'

os.makedirs(PREDICTIONS_DIR, exist_ok=True)

def set_seeds(seed=42):
    """Set random seeds for reproducibility."""
    np.random.seed(seed)
    tf.random.set_seed(seed)
    random.seed(seed)

set_seeds()

def calculate_obv(data):
    """Calculate On-Balance Volume (OBV)."""
    obv = [0] 
    for i in range(1, len(data)):
        if data['Close'][i] > data['Close'][i-1]:
            obv.append(obv[-1] + data['Volume'][i]) 
        elif data['Close'][i] < data['Close'][i-1]:
            obv.append(obv[-1] - data['Volume'][i]) 
        else:
            obv.append(obv[-1]) 
    return obv

def calculate_stochastic_oscillator(data, window=14):
    """Calculate Stochastic Oscillator (%K and %D)."""
    low_min = data['Low'].rolling(window=window).min()
    high_max = data['High'].rolling(window=window).max()
    stochastic_k = 100 * (data['Close'] - low_min) / (high_max - low_min)
    stochastic_d = stochastic_k.rolling(window=3).mean()  
    return stochastic_k, stochastic_d

def create_gru_model(units, dropout_rate, learning_rate, input_shape, prediction_horizon):
    """Create a GRU-based Sequential model."""
    model = Sequential()
    model.add(Bidirectional(
        GRU(units=units, activation='tanh', return_sequences=True,
            kernel_regularizer=regularizers.l2(1e-4),
            input_shape=input_shape
        )
    ))
    model.add(Dropout(dropout_rate))
    model.add(GRU(units=units, activation='tanh', kernel_regularizer=regularizers.l2(1e-4)))
    model.add(Dropout(dropout_rate))
    model.add(Dense(prediction_horizon))
    optimizer = tf.keras.optimizers.Adam(learning_rate=learning_rate)
    model.compile(optimizer=optimizer, loss='mean_squared_error', metrics=['mae'])
    return model

class StockModel:
    def __init__(self, ticker, days_back=500):
        self.ticker = ticker.upper()
        self.days_back = days_back
        self.daily_dir = self.get_daily_subdir()
        self.input_file = os.path.join(self.daily_dir, f"{self.ticker}_data.csv")
        self.indicators_file = os.path.join(self.daily_dir, f"{self.ticker}_data_with_indicators.csv")
        self.scaler_filename = os.path.join(self.daily_dir, f"{self.ticker}_scaler.save")
        self.model_filename = os.path.join(self.daily_dir, f"{self.ticker}_gru_model.keras")

    def get_daily_subdir(self):
        """Generate a daily subdirectory path for the given ticker."""
        today = datetime.now().strftime('%Y-%m-%d')
        subdir = os.path.join(DATA_DIR, self.ticker, today)
        os.makedirs(subdir, exist_ok=True)
        return subdir

    def fetch_data(self):
        """Fetch historical stock data and save to CSV."""
        end_date = (datetime.now() - timedelta(days=1)).strftime('%Y-%m-%d')
        start_date = (datetime.now() - timedelta(days=self.days_back)).strftime('%Y-%m-%d')
        stock_data = yf.download(self.ticker, start=start_date, end=end_date)
        if stock_data.empty:
            raise ValueError(f"No data found for ticker {self.ticker}")
        stock_data = stock_data[['Open', 'High', 'Low', 'Close', 'Adj Close', 'Volume']]
        stock_data.reset_index(inplace=True)
        stock_data.columns = ['Date', 'Open', 'High', 'Low', 'Close', 'Adj Close', 'Volume']
        stock_data.to_csv(self.input_file, index=False)

    def add_indicators(self):
        """Add technical indicators to the stock data."""
        print(f"Adding technical indicators to the stock data for {self.ticker}...")
        if not os.path.exists(self.input_file):
            raise FileNotFoundError(f"File {self.input_file} not found")
        df = pd.read_csv(self.input_file)
        df['SMA_10'] = ta.sma(df['Close'], length=10)
        df['SMA_50'] = ta.sma(df['Close'], length=50)
        df['EMA_10'] = ta.ema(df['Close'], length=10)
        df['EMA_50'] = ta.ema(df['Close'], length=50)
        bbands = ta.bbands(df['Close'], length=20)
        df['Bollinger_Upper'] = bbands['BBU_20_2.0']
        df['Bollinger_Middle'] = bbands['BBM_20_2.0']
        df['Bollinger_Lower'] = bbands['BBL_20_2.0']
        df['ATR_14'] = ta.atr(df['High'], df['Low'], df['Close'], length=14)
        df['RSI_14'] = ta.rsi(df['Close'], length=14)
        macd = ta.macd(df['Close'])
        df['MACD'] = macd['MACD_12_26_9']
        df['MACD_Signal'] = macd['MACDs_12_26_9']
        adx = ta.adx(df['High'], df['Low'], df['Close'], length=14)
        df['ADX_14'] = adx['ADX_14']
        df['DI+_14'] = adx['DMP_14']
        df['DI-_14'] = adx['DMN_14']
        df['CCI_14'] = ta.cci(df['High'], df['Low'], df['Close'], length=14)
        df['OBV'] = calculate_obv(df)
        df['Stochastic_K'], df['Stochastic_D'] = calculate_stochastic_oscillator(df)
        df.dropna(inplace=True)
        df.to_csv(self.indicators_file, index=False)

    def prepare_training_data(self, seq_len, prediction_horizon):
        """Prepare data for training the GRU model."""
        print(f"Preparing data for training the model for {self.ticker}...")
        if not os.path.exists(self.indicators_file):
            raise FileNotFoundError(f"File {self.indicators_file} not found")
        df = pd.read_csv(self.indicators_file)
        feature_columns = df.columns.difference(['Date', 'Adj Close'])
        data = df[feature_columns].values
        self.scaler = MinMaxScaler(feature_range=(0, 1))
        scaled_data = self.scaler.fit_transform(data)
        joblib.dump(self.scaler, self.scaler_filename)
        x_data, y_data = [], []
        close_index = list(feature_columns).index('Close')
        for i in range(seq_len, len(scaled_data) - prediction_horizon + 1):
            x_data.append(scaled_data[i - seq_len:i])
            y_data.append(scaled_data[i:i + prediction_horizon, close_index])
        if not x_data:
            raise ValueError(f"Not enough data to train the model with sequence length {seq_len}")
        return np.array(x_data), np.array(y_data), feature_columns

    def train_model(self, x_train, y_train, x_val, y_val, seq_len, feature_columns,
                    epochs, batch_size, learning_rate, dropout_rate, prediction_horizon):
        """Train the GRU model."""
        print(f"Training the model for {self.ticker}...")
        model = create_gru_model(
            units=50, 
            dropout_rate=dropout_rate,
            learning_rate=learning_rate,
            input_shape=(seq_len, len(feature_columns)),
            prediction_horizon=prediction_horizon
        )
        early_stopping = EarlyStopping(monitor='val_loss', patience=10, restore_best_weights=True)
        history = model.fit(
            x_train, y_train,
            epochs=epochs,
            batch_size=batch_size,
            validation_data=(x_val, y_val),
            callbacks=[early_stopping],
            verbose=0
        )
        model.save(self.model_filename)
        print(f"Model trained and saved for {self.ticker}")
        return history

    def predict_future(self, predict_days, seq_len, prediction_horizon):
        """Make future predictions using the trained GRU model."""
        print(f"Predicting future prices for {self.ticker}...")
        if not os.path.exists(self.model_filename) or not os.path.exists(self.indicators_file):
            raise FileNotFoundError("Model file or input data file not found")
        model = load_model(self.model_filename)
        scaler = joblib.load(self.scaler_filename)
        df = pd.read_csv(self.indicators_file)
        feature_columns = df.columns.difference(['Date', 'Adj Close'])
        data = df[feature_columns].values
        scaled_data = scaler.transform(data)

        if len(scaled_data) < seq_len:
            raise ValueError(f"Not enough data to make predictions with sequence length {seq_len}")
        x_input = scaled_data[-seq_len:]
        predictions = []
        close_index = list(feature_columns).index('Close')
        for _ in range(predict_days):
            x_input_reshaped = np.reshape(x_input, (1, seq_len, len(feature_columns)))
            pred = model.predict(x_input_reshaped, verbose=0)[0, 0]
            predictions.append(pred)
            new_entry = x_input[1:].copy()  
            new_row = x_input[-1].copy()
            new_row[close_index] = pred  
            x_input = np.vstack([new_entry, new_row])  
        predictions_array = np.zeros((len(predictions), len(feature_columns)))
        predictions_array[:, close_index] = predictions
        predictions_original = scaler.inverse_transform(predictions_array)[:, close_index].flatten().tolist()
        print(f"Predictions made for {self.ticker}")
        return predictions_original


    def delete_old_data(self):
        """Delete previous day's data to save space."""
        yesterday = (datetime.now() - timedelta(days=1)).strftime('%Y-%m-%d')
        yesterday_dir = os.path.join(DATA_DIR, self.ticker, yesterday)
        if os.path.exists(yesterday_dir):
            for file in os.listdir(yesterday_dir):
                os.remove(os.path.join(yesterday_dir, file))
            os.rmdir(yesterday_dir)
            print(f"Old data deleted for {self.ticker}")

def get_param(request_json, param_name, default):
    """Helper function to get parameters with default if missing or invalid."""
    value = request_json.get(param_name, default)
    if isinstance(value, (int, float)) and value <= 0:
        return default
    return value

@app.route('/train_multiple_tickers', methods=['POST'])
def train_multiple_tickers():
    request_json = request.get_json()
    if not request_json:
        return jsonify({"error": "Invalid JSON payload"}), 400

    file_list = request_json.get('fileList') or request_json.get('file_list', [])
    days_back = get_param(request_json, 'daysBack', 500)
    epochs = get_param(request_json, 'epochs', 100)
    batch_size = get_param(request_json, 'batchSize', 32)
    seq_len = get_param(request_json, 'seqLen', 60)
    validation_split = get_param(request_json, 'validationSplit', 0.1)
    learning_rate = get_param(request_json, 'learningRate', 0.001)
    dropout_rate = get_param(request_json, 'dropoutRate', 0.2)
    prediction_horizon = get_param(request_json, 'prediction_horizon', 30)

    results = []

    if not file_list:
        return jsonify({"error": "fileList is required and cannot be empty"}), 400

    for file_name in file_list:
        if not os.path.exists(file_name):
            results.append({"file": file_name, "error": "File not found"})
            continue

        try:
            with open(file_name, 'r') as f:
                tickers = f.read().splitlines()

            for ticker in tickers:
                if not ticker.strip():
                    continue  
                try:
                    print(f"Processing ticker: {ticker}")
                    stock_model = StockModel(ticker, days_back)
                    stock_model.fetch_data()
                    stock_model.add_indicators()
                    x_data, y_data, feature_columns = stock_model.prepare_training_data(seq_len, prediction_horizon)
                    
                    train_size = int(len(x_data) * (1 - validation_split))
                    x_train, x_val = x_data[:train_size], x_data[train_size:]
                    y_train, y_val = y_data[:train_size], y_data[train_size:]
                    
                    stock_model.train_model(
                        x_train, y_train, x_val, y_val, seq_len, feature_columns,
                        epochs, batch_size, learning_rate, dropout_rate, prediction_horizon
                    )

                    predictions = stock_model.predict_future(
                        predict_days=prediction_horizon,
                        seq_len=seq_len,
                        prediction_horizon=prediction_horizon
                    )

                    ticker_predictions_dir = os.path.join(PREDICTIONS_DIR, ticker)
                    os.makedirs(ticker_predictions_dir, exist_ok=True)

                    today = datetime.now().date()
                    predicted_dates = [today + timedelta(days=i) for i in range(1, prediction_horizon + 1)]
                    prediction_df = pd.DataFrame({
                        "Date": predicted_dates,
                        "Predicted_Price": predictions
                    })

                    prediction_csv_filename = os.path.join(ticker_predictions_dir, f"{ticker}_30_day_predictions.csv")
                    prediction_df.to_csv(prediction_csv_filename, index=False)
                    print(f"Predictions saved to {prediction_csv_filename}")

                    stock_model.delete_old_data()

                    results.append({
                        "ticker": ticker,
                        "status": "success",
                        "prediction_file": prediction_csv_filename
                    })
                except Exception as e:
                    print(f"Error processing ticker {ticker}: {e}")
                    results.append({"ticker": ticker, "error": str(e)})

        except Exception as e:
            print(f"Error reading file {file_name}: {e}")
            results.append({"file": file_name, "error": str(e)})

    return jsonify({"results": results})

@app.route('/take_predict_dashboard', methods=['GET'])
def take_predict_dashboard():
    ticker = request.args.get('ticker') 
    day = request.args.get('day', type=int) 

    if not ticker:
        return jsonify({"error": "Ticker parameter is required"}), 400

    ticker_folder = os.path.join("predictions", ticker)
    prediction_file = os.path.join(ticker_folder, f"{ticker}_30_day_predictions.csv")

    if not os.path.exists(ticker_folder):
        return jsonify({"error": f"Folder for ticker '{ticker}' does not exist"}), 404

    if not os.path.exists(prediction_file):
        return jsonify({"error": f"Prediction file for ticker '{ticker}' not found"}), 404

    prediction_df = pd.read_csv(prediction_file)

    if day:
        if day < 1 or day > 30:
            return jsonify({"error": "Day parameter must be between 1 and 30"}), 400
        
        prediction_day = prediction_df.iloc[day - 1]
        return jsonify({"ticker": ticker, "day": day, "prediction": prediction_day.to_dict()})

    predictions = prediction_df.to_dict(orient='records')
    return jsonify({"ticker": ticker, "predictions": predictions})

@app.route('/stock/candle', methods=['GET'])
def fetch_historical_data():
    stock_symbol = request.args.get('symbol')
    resolution = request.args.get('resolution')
    from_timestamp = int(request.args.get('from'))
    to_timestamp = int(request.args.get('to'))
    
    try:
        stock = yf.Ticker(stock_symbol)
        
        from_date = datetime.utcfromtimestamp(from_timestamp).strftime('%Y-%m-%d')
        to_date = datetime.utcfromtimestamp(to_timestamp).strftime('%Y-%m-%d')
        
        interval_mapping = {
            '1': '1m',
            '5': '5m',
            '15': '15m',
            '30': '30m',
            '60': '60m',
            'D': '1d',
            'W': '1wk',
            'M': '1mo'
        }
        
        if resolution not in interval_mapping:
            return jsonify({'error': 'Invalid resolution'}), 400
        
        interval = interval_mapping[resolution]
        
        historical_data = stock.history(period='1d', start=from_date, end=to_date, interval=interval)
        
        if historical_data.empty:
            return jsonify({'error': 'No data found for the given parameters'}), 404
        
        data = historical_data.reset_index().to_dict(orient='records')
        
        return jsonify(data)
    
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
