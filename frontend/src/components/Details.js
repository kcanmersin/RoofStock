import React, { useEffect, useState } from "react";
import Card from "./Card";

const Details = ({ details, currentPrice, ticker }) => {
  const detailsList = {
    name: "Name",
    country: "Country",
    currency: "Currency",
    exchange: "Exchange",
    ipo: "IPO Date",
    marketCapitalization: "Market Capitalization",
    finnhubIndustry: "Industry",
  };

  const convertMillionToBillion = (number) => {
    return (number / 1000).toFixed(2);
  };

  const [predictions, setPredictions] = useState([]);
  const [error, setError] = useState(null);

  const fetchPredictions = async () => {
    try {
      const response = await fetch(`http://127.0.0.1:5000/take_predict_dashboard?ticker=${ticker}`);
      const data = await response.json();
      if (response.ok) {
        setPredictions(data.predictions);
      } else {
        setError(data.error || "Failed to fetch predictions");
      }
    } catch (err) {
      setError("An error occurred while fetching predictions.");
    }
  };

  useEffect(() => {
    fetchPredictions();
  }, [ticker]);

  const calculatePercentageChange = (predictedPrice) => {
    return ((predictedPrice - currentPrice) / currentPrice * 100).toFixed(2);
  };

  const getPriceColor = (predictedPrice) => {
    return predictedPrice > currentPrice ? "text-green-500" : "text-red-500";
  };

  return (
    <Card>
      <div className="w-full h-full p-4">
        {/* Details Section */}
        <h3 className="text-xl font-semibold mb-4">Details</h3>
        <table className="w-full text-sm">
          <tbody>
            {Object.keys(detailsList).map((item) => (
              <tr key={item} className="border-b hover:bg-gray-100 transition-colors duration-200">
                <td className="py-2 px-4 text-gray-500 font-medium">{detailsList[item]}</td>
                <td className="py-2 px-4 text-gray-900 font-bold">
                  {item === "marketCapitalization"
                    ? `${convertMillionToBillion(details[item])}B`
                    : details[item]}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Predictions Section */}
        <div className="mt-6">
          <h3 className="text-xl font-semibold mb-4">Predictions</h3>
          {predictions.length > 0 ? (
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50">
                  <th className="py-2 px-4 text-left font-medium">Days</th>
                  <th className="py-2 px-4 text-left font-medium">Predicted Price</th>
                  <th className="py-2 px-4 text-left font-medium">Change (%)</th>
                </tr>
              </thead>
              <tbody>
                {[7, 15, 30].map((day) => {
                  const prediction = predictions[day - 1];
                  if (!prediction) return null;

                  const predictedPrice = parseFloat(prediction.Predicted_Price.toFixed(2));
                  const percentageChange = calculatePercentageChange(predictedPrice);
                  const priceColor = getPriceColor(predictedPrice);

                  return (
                    <tr
                      key={day}
                      className="border-b hover:bg-gray-100 transition-colors duration-200"
                    >
                      <td className="py-2 px-4">{day}-Day</td>
                      <td className="py-2 px-4">${predictedPrice}</td>
                      <td className={`py-2 px-4 font-bold ${priceColor}`}>
                        {percentageChange}%
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          ) : error ? (
            <p className="text-red-500 text-sm">{error}</p>
          ) : (
            <p className="text-gray-500 text-sm">Loading predictions...</p>
          )}
        </div>
      </div>
    </Card>
  );
};

export default Details;
