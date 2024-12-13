// src/pages/PortfolioPage.js
import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Pie } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';
import BuyModal from '../components/BuyModal';
import SellModal from '../components/SellModal';
import { useAuth } from '../context/AuthContext';

ChartJS.register(
  ArcElement,
  Tooltip,
  Legend
);

const PortfolioPage = () => {
  const { user } = useAuth();
  const [portfolio, setPortfolio] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showBuyModal, setShowBuyModal] = useState(false);
  const [showSellModal, setShowSellModal] = useState(false);
  const [selectedStock, setSelectedStock] = useState(null);
  const [showOrders, setShowOrders] = useState(false);
  const [showAlerts, setShowAlerts] = useState(false);
  const [showTransactions, setShowTransactions] = useState(false);
  const [predictions, setPredictions] = useState({});

  const effectiveUserId = user?.userId || '1e33ce27-d2a6-412a-8789-73b5640fa4e1';

  const fetchPortfolio = async () => {
    try {
      const response = await axios.get('http://localhost:5244/api/stocks/portfolio', {
        params: { userId: effectiveUserId },
      });
      setPortfolio(response.data);
      setLoading(false);
    } catch (err) {
      setError('Error fetching portfolio data.');
      setLoading(false);
    }
  };

  const fetchPredictions = async (stockSymbol) => {
    try {
      const response = await axios.get(`http://127.0.0.1:5000/take_predict_dashboard?ticker=${stockSymbol}`);
      return response.data.predictions;
    } catch {
      return null;
    }
  };

  const fetchAllPredictions = async () => {
    if (!portfolio || !portfolio.stockHoldingItems) return;

    const predictionPromises = portfolio.stockHoldingItems.map((item) =>
      fetchPredictions(item.stockSymbol).then((predictions) => ({
        stockSymbol: item.stockSymbol,
        predictions,
      }))
    );

    const results = await Promise.all(predictionPromises);
    const predictionsBySymbol = results.reduce((acc, { stockSymbol, predictions }) => {
      acc[stockSymbol] = predictions;
      return acc;
    }, {});
    setPredictions(predictionsBySymbol);
  };

  useEffect(() => {
    fetchPortfolio();
  }, [effectiveUserId]);

  useEffect(() => {
    if (portfolio) {
      fetchAllPredictions();
    }
  }, [portfolio]);

  if (loading) return <div className="text-center mt-10">Loading...</div>;
  if (error) return <div className="text-center mt-10 text-red-500">{error}</div>;

  const {
    stockHoldingItems = [],
    orders = [],
    stockPriceAlerts = [],
    transactions = [],
    totalPortfolioValue,
    change,
    totalBalance,
    availableBalance
  } = portfolio || {};

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right',
        labels: {
          color: '#000',
        }
      },
      title: {
        display: false,
        text: '',
        color: '#000',
      }
    },
    maintainAspectRatio: false,
  };

  const handleCancelOrder = async (orderId) => {
    try {
      await axios.post('http://localhost:5244/api/orders/cancel', {
        orderId,
        userId: effectiveUserId
      });
      fetchPortfolio();
    } catch (err) {
      setError('Error canceling order.');
    }
  };

  const handleDeletePriceAlert = async (alertId) => {
    if (!alertId) {
      setError('Alert ID is missing.');
      return;
    }
    try {
      await axios.delete(`http://localhost:5244/api/stocks/price-alert?UserId=${effectiveUserId}&AlertId=${alertId}`);
      fetchPortfolio();
    } catch (err) {
      setError('Error deleting price alert.');
    }
  };

  const stockSymbols = stockHoldingItems.map(item => item.stockSymbol);
  const holdingValues = stockHoldingItems.map(item => item.holdingValue);

  const pieData = {
    labels: stockSymbols,
    datasets: [
      {
        data: holdingValues,
        backgroundColor: [
          '#34D399',
          '#60A5FA',
          '#FBBF24',
          '#F87171',
          '#A78BFA',
          '#F472B6',
          '#818CF8'
        ],
        borderWidth: 1,
      },
    ],
  };

  const activePriceAlerts = stockPriceAlerts.filter(alert => !alert.isTriggered);

  const handleBuySellSubmit = () => {
    fetchPortfolio();
  };


  const getOrderTypeLabel = (orderType) => {
    return orderType === 0 ? 'Buy' : orderType === 1 ? 'Sell' : 'Unknown';
  };

  const getOrderStatusLabel = (status) => {
    switch (status) {
      case 0:
        return 'Pending';
      case 1:
        return 'Completed';
      case 2:
        return 'Failed';
      case 3:
        return 'Canceled';
      case 4:
        return 'In Progress';
      default:
        return 'Unknown';
    }
  };


  return (
    <div className="container mx-auto p-6 bg-white text-gray-900">
      <h2 className="text-3xl mb-6 font-bold text-center">Portfolio Overview</h2>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-6 mb-10 bg-gray-100 p-6 rounded-lg shadow">
        <div className="flex flex-col items-center">
          <span className="text-lg font-semibold">Total Portfolio Value</span>
          <span className="text-2xl font-bold">${totalPortfolioValue?.toFixed(2)}</span>
        </div>
        <div className="flex flex-col items-center">
          <span className="text-lg font-semibold">Change</span>
          <span className={`text-2xl font-bold ${change >= 0 ? 'text-green-500' : 'text-red-500'}`}>
            ${change?.toFixed(2)}
          </span>
        </div>
        <div className="flex flex-col items-center">
          <span className="text-lg font-semibold">Total Balance</span>
          <span className="text-2xl font-bold">${totalBalance?.toFixed(2)}</span>
        </div>
        <div className="flex flex-col items-center">
          <span className="text-lg font-semibold">Available Balance</span>
          <span className="text-2xl font-bold">${availableBalance?.toFixed(2)}</span>
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-8 mb-10">
        <div className="p-4 rounded-lg shadow bg-white">
          <h3 className="text-2xl font-semibold mb-4">Portfolio Distribution</h3>
          {stockHoldingItems.length > 0 ? (
            <div className="w-full h-48">
              <Pie data={pieData} options={options} />
            </div>
          ) : (
            <p className="text-center">No holdings to display.</p>
          )}
        </div>

        <div className="p-4 rounded-lg shadow bg-white">
          <h3 className="text-2xl font-semibold mb-4">Predictions</h3>
          {stockHoldingItems.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse table-auto text-sm">
                <thead>
                  <tr className="border-b bg-gray-50">
                    <th className="py-2 px-4 text-left font-medium">Stock Name</th>
                    <th className="py-2 px-4 text-left font-medium">7-Day</th>
                    <th className="py-2 px-4 text-left font-medium">15-Day</th>
                    <th className="py-2 px-4 text-left font-medium">30-Day</th>
                  </tr>
                </thead>
                <tbody>
                  {stockHoldingItems.map((item) => {
                    const stockPredictions = predictions[item.stockSymbol];
                    if (!stockPredictions) return null;

                    const prediction7 = stockPredictions[6]?.Predicted_Price.toFixed(2) || "N/A";
                    const prediction15 = stockPredictions[14]?.Predicted_Price.toFixed(2) || "N/A";
                    const prediction30 = stockPredictions[29]?.Predicted_Price.toFixed(2) || "N/A";

                    return (
                      <tr key={item.stockSymbol} className="border-b hover:bg-gray-100">
                        <td className="py-2 px-4 font-bold">{item.stockSymbol}</td>
                        <td className={`py-2 px-4 ${prediction7 >= item.unitPrice ? 'text-green-500' : 'text-red-500'}`}>
                          ${prediction7}
                        </td>
                        <td className={`py-2 px-4 ${prediction15 >= item.unitPrice ? 'text-green-500' : 'text-red-500'}`}>
                          ${prediction15}
                        </td>
                        <td className={`py-2 px-4 ${prediction30 >= item.unitPrice ? 'text-green-500' : 'text-red-500'}`}>
                          ${prediction30}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-center text-gray-500">No predictions available.</p>
          )}
        </div>

      </div>

      <div className="mb-6">
  <h3 className="text-2xl font-semibold mb-4">Stock Holdings</h3>
  {stockHoldingItems.length > 0 ? (
    <div className="overflow-x-auto">
      <table className="min-w-full border-collapse table-auto text-sm">
        <thead>
          <tr className="border-b bg-gray-50">
            <th className="py-2 px-4 text-left font-medium">Stock Symbol</th>
            <th className="py-2 px-4 text-left font-medium">Quantity</th>
            <th className="py-2 px-4 text-left font-medium">Unit Price</th>
            <th className="py-2 px-4 text-left font-medium">Holding Value</th>
            <th className="py-2 px-4 text-left font-medium">Change</th>
          </tr>
        </thead>
        <tbody>
          {stockHoldingItems.map((item) => (
            <tr key={item.stockSymbol} className="border-b hover:bg-gray-100">
              <td className="py-2 px-4 font-bold">{item.stockSymbol}</td>
              <td className="py-2 px-4">{item.quantity}</td>
              <td className="py-2 px-4">${item.unitPrice.toFixed(2)}</td>
              <td className="py-2 px-4">${item.holdingValue.toFixed(2)}</td>
              <td className={`py-2 px-4 ${item.change >= 0 ? 'text-green-500' : 'text-red-500'}`}>${item.change.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  ) : (
    <p className="text-center text-gray-500">No holdings available.</p>
  )}
</div>


<div className="mb-6">
  <button
    onClick={() => setShowOrders(!showOrders)}
    className="w-full text-left px-4 py-2 rounded-md bg-gray-100 text-gray-900 focus:outline-none"
  >
    <span className="text-2xl font-semibold">Orders</span>
  </button>
  <div
    className={`transition-max-height duration-500 overflow-hidden ${showOrders ? 'max-h-screen' : 'max-h-0'}`}
  >
    {showOrders && (
      <div className="mt-4 p-4 rounded-lg shadow bg-white">
        {orders.length > 0 ? (
          <table className="min-w-full table-auto text-sm">
            <thead>
              <tr className="border-b">
                <th className="py-2 px-4 text-left font-medium">Stock Symbol</th>
                <th className="py-2 px-4 text-left font-medium">Type</th>
                <th className="py-2 px-4 text-left font-medium">Status</th>
                <th className="py-2 px-4 text-left font-medium">Quantity</th>
                <th className="py-2 px-4 text-left font-medium">Target Price</th>
                <th className="py-2 px-4 text-left font-medium">Date</th>
                <th className="py-2 px-4 text-left font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.orderId} className="border-b hover:bg-gray-200">
                  <td className="py-2 px-4">{order.stockSymbol || 'N/A'}</td>
                  <td className="py-2 px-4">{getOrderTypeLabel(order.orderType)}</td>
                  <td className="py-2 px-4">{getOrderStatusLabel(order.status)}</td>
                  <td className="py-2 px-4">{order.quantity || 0}</td>
                  <td className="py-2 px-4">${order.targetPrice?.toFixed(2) || 'N/A'}</td>
                  <td className="py-2 px-4">{new Date(order.createdDate).toLocaleString() || 'N/A'}</td>
                  <td className="py-2 px-4">
                    <button
                      onClick={() => handleCancelOrder(order.orderId)}
                      className="px-3 py-1 rounded-md bg-red-500 hover:bg-red-400 text-white"
                    >
                      Cancel
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p className="text-center">No orders available.</p>
        )}
      </div>
    )}
  </div>
</div>



      <div className="mb-6">
        <button
          onClick={() => setShowAlerts(!showAlerts)}
          className="w-full text-left px-4 py-2 rounded-md bg-gray-100 text-gray-900 focus:outline-none"
        >
          <span className="text-2xl font-semibold">Active Price Alerts</span>
        </button>
        <div className={`transition-max-height duration-500 overflow-hidden ${showAlerts ? 'max-h-screen' : 'max-h-0'}`}>
          {showAlerts && (
            <div className="mt-4 p-4 rounded-lg shadow bg-white">
              {activePriceAlerts.length > 0 ? (
                <table className="min-w-full table-auto text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 px-4 text-left font-medium">Stock Symbol</th>
                      <th className="py-2 px-4 text-left font-medium">Target Price</th>
                      <th className="py-2 px-4 text-left font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activePriceAlerts.map((alert) => (
                      <tr key={alert.alertId} className="border-b hover:bg-gray-200">
                        <td className="py-2 px-4">{alert.stockSymbol}</td>
                        <td className="py-2 px-4">${alert.targetPrice.toFixed(2)}</td>
                        <td className="py-2 px-4">
                          <button
                            onClick={() => handleDeletePriceAlert(alert.alertId)}
                            className="px-3 py-1 rounded-md bg-red-500 hover:bg-red-400 text-white"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="text-center">No active price alerts.</p>
              )}
            </div>
          )}
        </div>
      </div>

      <div className="mb-6">
        <button
          onClick={() => setShowTransactions(!showTransactions)}
          className="w-full text-left px-4 py-2 rounded-md bg-gray-100 text-gray-900 focus:outline-none"
        >
          <span className="text-2xl font-semibold">Transactions</span>
        </button>
        <div
          className={`transition-max-height duration-500 overflow-hidden ${showTransactions ? 'max-h-screen' : 'max-h-0'}`}
        >
          {showTransactions && (
            <div className="mt-4 p-4 rounded-lg shadow bg-white">
              {transactions.length > 0 ? (
                <table className="min-w-full table-auto text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 px-4 text-left font-medium">Description</th>
                      <th className="py-2 px-4 text-left font-medium">Amount</th>
                      <th className="py-2 px-4 text-left font-medium">Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {transactions.map((transaction) => (
                      <tr key={transaction.id} className="border-b hover:bg-gray-200">
                        <td className="py-2 px-4">{transaction.description || 'N/A'}</td>
                        <td className="py-2 px-4">${transaction.amount?.toFixed(2) || 'N/A'}</td>
                        <td className="py-2 px-4">{new Date(transaction.createdDate).toLocaleString() || 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="text-center">No transactions available.</p>
              )}
            </div>
          )}
        </div>
      </div>

      {showBuyModal && (
        <BuyModal
          selectedStock={selectedStock}
          onClose={() => setShowBuyModal(false)}
          onSubmit={handleBuySellSubmit}
        />
      )}
      {showSellModal && (
        <SellModal
          selectedStock={selectedStock}
          onClose={() => setShowSellModal(false)}
          onSubmit={handleBuySellSubmit}
        />
      )}
    </div>
  );
};

export default PortfolioPage;
