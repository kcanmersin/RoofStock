import React, { useContext, useEffect, useState } from "react";
import Card from "./Card";
import ThemeContext from "../context/ThemeContext";

const Details = ({ details, currentPrice, ticker }) => {
  const { darkMode } = useContext(ThemeContext);

  const detailsList = {
    name: "Name",
    country: "Country",
    currency: "Currency",
    exchange: "Exchange",
    ipo: "IPO Date",
    marketCapitalization: "Market Capitalization",
    finnhubIndustry: "Industry",
  };
  
  // Function to convert market capitalization from million to billion
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
      <ul className={`w-full h-full flex flex-col justify-between divide-y-1 ${darkMode ? "divide-gray-800" : ""}`}>
        {Object.keys(detailsList).map((item) => (
          <li key={item} className="flex-1 flex justify-between items-center">
            <span>{detailsList[item]}</span>
            <span className="font-bold">
              {item === "marketCapitalization"
                ? `${convertMillionToBillion(details[item])}B`
                : details[item]}
            </span>
          </li>
        ))}
        <div className="mt-4">
          <h3 className="text-lg font-semibold mb-2">Predictions</h3>
          {predictions.length > 0 ? (
            <div className="space-y-2">
              {[7, 15, 30].map((day) => {
                const prediction = predictions[day - 1];
                if (!prediction) return null;

                const predictedPrice = parseFloat(prediction.Predicted_Price.toFixed(2));
                const percentageChange = calculatePercentageChange(predictedPrice);
                const priceColor = getPriceColor(predictedPrice);

                return (
                  <div
                    key={day}
                    className={`flex justify-between items-center p-2 rounded-md ${darkMode ? "bg-gray-800" : "bg-gray-100"}`}
                  >
                    <span>{day}-Day Prediction</span>
                    <span className={`font-bold ${priceColor}`}>
                      ${predictedPrice} ({percentageChange}%)
                    </span>
                  </div>
                );
              })}
            </div>
          ) : error ? (
            <p className="text-red-500 text-sm">{error}</p>
          ) : (
            <p>Loading predictions...</p>
          )}
        </div>
      </ul>
    </Card>
  );
};

export default Details;
