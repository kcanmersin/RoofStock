// SetPriceAlertRequest.js
import axios from 'axios';

const setPriceAlertRequest = async (userId, stockSymbol, targetPrice, alertType) => {
  try {
    const response = await axios.post('http://localhost:5244/api/stocks/price-alert', {
      userId: userId,  // Correct userId (fixed)
      stockSymbol: stockSymbol,  // Passed stockSymbol
      targetPrice: targetPrice,  // Passed targetPrice
      alertType: alertType,  // Passed alertType as integer (0 or 1)
    });
    
    return response.data;
  } catch (error) {
    console.error("Error setting price alert:", error);
    throw error;
  }
};

export default setPriceAlertRequest;
