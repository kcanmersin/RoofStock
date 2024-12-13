import axios from 'axios';

const setPriceAlertRequest = async (userId, stockSymbol, targetPrice, alertType) => {
  try {
    const response = await axios.post('http://localhost:5244/api/stocks/price-alert', {
      userId: userId,
      stockSymbol: stockSymbol,
      targetPrice: targetPrice,
      alertType: alertType,
    });

    return response.data;
  } catch (error) {
    console.error("Error setting price alert:", error);
    throw error;
  }
};

export default setPriceAlertRequest;
