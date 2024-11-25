class StockTransaction:
    def __init__(self, user_id, stock_symbol, quantity, action):
        self.user_id = user_id
        self.stock_symbol = stock_symbol
        self.quantity = quantity
        self.action = action
