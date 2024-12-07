// src/components/Dashboard.js
import React, { useState, useContext, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import ThemeContext from "../context/ThemeContext";
import Overview from "./Overview";
import Details from "./Details";
import Chart from "./Chart";
import Header from "./Header";
import StockContext from "../context/StockContext";
import { fetchStockDetails, fetchQuote } from "../utils/api/stock-api";

const Dashboard = () => {
  const { darkMode } = useContext(ThemeContext);
  const { stockSymbol } = useContext(StockContext);
  const { user } = useAuth();  // user bilgilerini AuthContext'ten alıyoruz

  const [stockDetails, setStockDetails] = useState({});
  const [quote, setQuote] = useState({});

  useEffect(() => {
    const updateStockDetails = async () => {
      try {
        const result = await fetchStockDetails(stockSymbol);
        setStockDetails(result);
      } catch (error) {
        setStockDetails({});
        console.error("Error fetching stock details:", error);
      }
    };

    const updateStockOverview = async () => {
      try {
        const result = await fetchQuote(stockSymbol);
        setQuote(result);
      } catch (error) {
        setQuote({});
        console.error("Error fetching stock quote:", error);
      }
    };

    if (stockSymbol) {
      updateStockDetails();
      updateStockOverview();
    }
  }, [stockSymbol]);

  if (!user) {
    return <p>Loading...</p>;  // Kullanıcı bilgisi henüz gelmediyse, yükleme mesajı göster
  }

  return (
    <div
      className={`h-screen grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 grid-rows-8 md:grid-rows-7 xl:grid-rows-5 auto-rows-fr gap-6 p-10 font-quicksand ${darkMode ? 'bg-gray-900 text-gray-300' : 'bg-white text-gray-900'}`}
    >
      {/* Header Section */}
      <div className="col-span-1 md:col-span-2 xl:col-span-3 row-span-1 flex justify-start items-center">
        <Header name={stockDetails.name} />
      </div>

      {/* Chart Section */}
      <div className="md:col-span-2 row-span-4">
        <Chart />
      </div>

      {/* Overview Section */}
      <div>
        <Overview
          symbol={stockSymbol}
          price={quote.pc}
          change={quote.d}
          changePercent={quote.dp}
          currency={stockDetails.currency}
        />
      </div>

      {/* Details Section */}
      <div className="row-span-2 xl:row-span-3">
        <Details details={stockDetails} ticker={stockSymbol} currentPrice={quote.pc} />
      </div>

      {/* Conditional Button Display */}
      {user && (
        <div className="fixed bottom-10 right-10">
          <button className="btn btn-primary">Buy</button>
          <button className="btn btn-secondary">Sell</button>
          <button className="btn btn-warning">Set Alert</button>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
