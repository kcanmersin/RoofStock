import axios from 'axios';

const giveOrderRequest = async (userId, stockSymbol, quantity, targetPrice, orderType) => {
  const orderTypeInt = orderType === 'Buy' ? 0 : 1;
  try {
    const response = await axios.post('http://localhost:5244/api/orders/place', {
      userId: userId,
      stockSymbol: stockSymbol,
      quantity: quantity,
      targetPrice: targetPrice,
      orderType: orderTypeInt,
    });
    return response.data;
  } catch (error) {
    console.error("Error placing order:", error);
    throw error;
  }
};

export { giveOrderRequest };
