from flask import Flask, request, jsonify
from langchain_community.chatgroq import ChatGroq
import requests

app = Flask(__name__)

STOCKFLOW_BACKEND_URL = "http://localhost:5244/api"

chatgroq = ChatGroq(model="gpt-3.5-turbo")

def fetch_portfolio(user_id):
    url = f"{STOCKFLOW_BACKEND_URL}/portfolio/{user_id}"
    response = requests.get(url)
    return response.json() if response.status_code == 200 else {"error": response.text}

def fetch_ticker_price(ticker):
    url = f"{STOCKFLOW_BACKEND_URL}/stocks/price/{ticker}"
    response = requests.get(url)
    return response.json() if response.status_code == 200 else {"error": response.text}

def process_buy_request(user_id, ticker, quantity):
    url = f"{STOCKFLOW_BACKEND_URL}/stocks/buy"
    payload = {"userId": user_id, "ticker": ticker, "quantity": quantity}
    response = requests.post(url, json=payload)
    return response.json() if response.status_code == 200 else {"error": response.text}

def process_user_request(prompt, user_id):
    system_prompt = """
    You are an assistant for the StockFlow system. Users can ask to:
    - View their portfolio.
    - Check the price of a stock.
    - Buy stocks.
    Based on the user's request, decide which action to perform and return the required parameters.
    Respond in this JSON format:
    {"action": "action_name", "params": {...}}
    Actions:
    - "show_portfolio": Requires {"user_id": "string"}
    - "get_ticker_price": Requires {"ticker": "string"}
    - "buy_stock": Requires {"user_id": "string", "ticker": "string", "quantity": "int"}
    """

    response = chatgroq.run(system_prompt + f"\nUser request: {prompt}")

    try:
        parsed_response = response.json()
    except ValueError:
        return {"error": "Unable to process user request."}

    action = parsed_response.get("action")
    params = parsed_response.get("params", {})

    if action == "show_portfolio":
        return fetch_portfolio(params.get("user_id"))
    elif action == "get_ticker_price":
        return fetch_ticker_price(params.get("ticker"))
    elif action == "buy_stock":
        return process_buy_request(params.get("user_id"), params.get("ticker"), params.get("quantity"))
    else:
        return {"error": "Invalid action or missing parameters."}

@app.route("/process", methods=["POST"])
def process_request():
    data = request.json
    user_id = data.get("user_id")
    prompt = data.get("prompt")

    if not prompt or not user_id:
        return jsonify({"error": "User ID and prompt are required."}), 400

    result = process_user_request(prompt, user_id)
    return jsonify(result)

if __name__ == "__main__":
    app.run(debug=True, port=8000)
