// src/pages/PortfolioPage.js
import React, { useState, useEffect, useContext } from 'react';
import axios from 'axios';
import { Pie } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';
import ThemeContext from '../context/ThemeContext';
import BuyModal from '../components/BuyModal';
import SellModal from '../components/SellModal';
import { useAuth } from '../context/AuthContext'; // AuthContext'i ekleyin

ChartJS.register(
  ArcElement,
  Tooltip,
  Legend
);

const PortfolioPage = () => {
  const { darkMode } = useContext(ThemeContext);
  const { user } = useAuth(); // AuthContext'ten kullanıcı bilgisi alın
  const [portfolio, setPortfolio] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showBuyModal, setShowBuyModal] = useState(false);
  const [showSellModal, setShowSellModal] = useState(false);
  const [selectedStock, setSelectedStock] = useState(null);
  const [showOrders, setShowOrders] = useState(false);
  const [showAlerts, setShowAlerts] = useState(false);
  const [showTransactions, setShowTransactions] = useState(false);

  const effectiveUserId = user?.userId || '1e33ce27-d2a6-412a-8789-73b5640fa4e1'; // Kullanıcı ID'sini AuthContext'ten al veya varsayılanı kullan

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

  useEffect(() => {
    fetchPortfolio();
  }, [effectiveUserId]);

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
          color: darkMode ? '#fff' : '#000'
        }
      },
      title: {
        display: false,
        text: '',
        color: darkMode ? '#fff' : '#000'
      }
    },
    maintainAspectRatio: false,
  };

  const handleCancelOrder = async (orderId) => {
    console.log('handleCancelOrder called with:', { userId: effectiveUserId, orderId });
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
    console.log('handleDeletePriceAlert called with:', { userId: effectiveUserId, alertId });
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
  const activeOrders = orders.filter(order => order.status === 0);
  const activeTransactions = transactions;

  const handleBuySellSubmit = () => {
    fetchPortfolio();
  };

  return (
    <div className={`container mx-auto p-6 ${darkMode ? 'bg-gray-900 text-gray-200' : 'bg-white text-gray-900'}`}>
      <h2 className="text-3xl mb-6 font-bold text-center">Portfolio Overview</h2>

      {/* Summary Card */}
      <div className={`grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-6 mb-10 ${darkMode ? 'bg-gray-800' : 'bg-gray-100'} p-6 rounded-lg shadow`}>
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

      {/* Charts */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-8 mb-10">
        <div className={`p-4 rounded-lg shadow ${darkMode ? 'bg-gray-800' : 'bg-white'} transition-all duration-300`}>
          <h3 className="text-2xl font-semibold mb-4">Portfolio Distribution</h3>
          {stockHoldingItems.length > 0 ? (
            <div className="w-full h-48">
              <Pie data={pieData} options={options} />
            </div>
          ) : (
            <p className="text-center">No holdings to display.</p>
          )}
        </div>
      </div>

      {/* Active Orders */}
      <div className="mb-6">
        <button
          onClick={() => setShowOrders(!showOrders)}
          className={`w-full text-left px-4 py-2 rounded-md ${darkMode ? 'bg-gray-800 text-white' : 'bg-gray-100 text-gray-900'} focus:outline-none`}
        >
          <span className="text-2xl font-semibold">Active Orders</span>
        </button>
        <div className={`transition-max-height duration-500 overflow-hidden ${showOrders ? 'max-h-screen' : 'max-h-0'}`}>
          {showOrders && (
            <div className={`mt-4 p-4 rounded-lg shadow ${darkMode ? 'bg-gray-800' : 'bg-white'}`}>
              {activeOrders.length > 0 ? (
                <table className="min-w-full table-auto text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 px-4 text-left font-medium">Stock Symbol</th>
                      <th className="py-2 px-4 text-left font-medium">Target Price</th>
                      <th className="py-2 px-4 text-left font-medium">Status</th>
                      <th className="py-2 px-4 text-left font-medium">Created Date</th>
                      <th className="py-2 px-4 text-left font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activeOrders.map((order, idx) => (
                      <tr key={order.orderId} className={`border-b hover:bg-gray-200 ${idx % 2 === 0 ? '' : 'bg-gray-50'}`}>
                        <td className="py-2 px-4">{order.stockSymbol}</td>
                        <td className="py-2 px-4">${order.targetPrice.toFixed(2)}</td>
                        <td className="py-2 px-4 text-yellow-500 font-bold">Pending</td>
                        <td className="py-2 px-4">{new Date(order.createdDate).toLocaleString()}</td>
                        <td className="py-2 px-4">
                          <button
                            onClick={() => handleCancelOrder(order.orderId)}
                            className={`px-3 py-1 rounded-md bg-red-500 hover:bg-red-400 text-white`}
                          >
                            Cancel
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="text-center">No active orders.</p>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Active Price Alerts */}
      <div className="mb-6">
        <button
          onClick={() => setShowAlerts(!showAlerts)}
          className={`w-full text-left px-4 py-2 rounded-md ${darkMode ? 'bg-gray-800 text-white' : 'bg-gray-100 text-gray-900'} focus:outline-none`}
        >
          <span className="text-2xl font-semibold">Active Price Alerts</span>
        </button>
        <div className={`transition-max-height duration-500 overflow-hidden ${showAlerts ? 'max-h-screen' : 'max-h-0'}`}>
          {showAlerts && (
            <div className={`mt-4 p-4 rounded-lg shadow ${darkMode ? 'bg-gray-800' : 'bg-white'}`}>
              {activePriceAlerts.length > 0 ? (
                <table className="min-w-full table-auto text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 px-4 text-left font-medium">Stock Symbol</th>
                      <th className="py-2 px-4 text-left font-medium">Alert Price</th>
                      <th className="py-2 px-4 text-left font-medium">Alert Type</th>
                      <th className="py-2 px-4 text-left font-medium">Status</th>
                      <th className="py-2 px-4 text-left font-medium">Created Date</th>
                      <th className="py-2 px-4 text-left font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activePriceAlerts.map((alert, idx) => {
                      const alertTypeMap = {
                        0: 'Fall',
                        1: 'Rise'
                      };
                      const alertStatus = alert.isTriggered ? 'Triggered' : 'Pending';

                      return (
                        <tr
                          key={alert.alertId}
                          className={`border-b hover:bg-gray-200 ${idx % 2 === 0 ? '' : 'bg-gray-50'}`}
                        >
                          <td className="py-2 px-4">{alert.stockSymbol}</td>
                          <td className="py-2 px-4">${alert.targetPrice}</td>
                          <td className="py-2 px-4">{alertTypeMap[alert.alertType]}</td>
                          <td className={`py-2 px-4 ${alert.isTriggered ? 'text-green-500' : 'text-yellow-500'}`}>
                            {alertStatus}
                          </td>
                          <td className="py-2 px-4">{new Date(alert.createdDate).toLocaleString()}</td>
                          <td className="py-2 px-4">
                            <button
                              onClick={() => handleDeletePriceAlert(alert.alertId)}
                              className={`px-3 py-1 rounded-md bg-red-500 hover:bg-red-400 text-white`}
                            >
                              Delete
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              ) : (
                <p className="text-center">No active price alerts.</p>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Transaction History */}
      <div className="mb-6">
        <button
          onClick={() => setShowTransactions(!showTransactions)}
          className={`w-full text-left px-4 py-2 rounded-md ${darkMode ? 'bg-gray-800 text-white' : 'bg-gray-100 text-gray-900'} focus:outline-none`}
        >
          <span className="text-2xl font-semibold">Transaction History</span>
        </button>
        <div className={`transition-max-height duration-500 overflow-hidden ${showTransactions ? 'max-h-screen' : 'max-h-0'}`}>
          {showTransactions && (
            <div className={`mt-4 p-4 rounded-lg shadow ${darkMode ? 'bg-gray-800' : 'bg-white'}`}>
              {activeTransactions.length > 0 ? (
                <table className="min-w-full table-auto text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 px-4 text-left font-medium">ID</th>
                      <th className="py-2 px-4 text-left font-medium">Amount</th>
                      <th className="py-2 px-4 text-left font-medium">Type</th>
                      <th className="py-2 px-4 text-left font-medium">Description</th>
                      <th className="py-2 px-4 text-left font-medium">Created Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activeTransactions.map((transaction, idx) => (
                      <tr key={transaction.id} className={`border-b hover:bg-gray-200 ${idx % 2 === 0 ? '' : 'bg-gray-50'}`}>
                        <td className="py-2 px-4">{transaction.id}</td>
                        <td className={`py-2 px-4 ${transaction.amount < 0 ? 'text-red-500' : 'text-green-500'}`}>
                          ${transaction.amount}
                        </td>
                        <td className="py-2 px-4">{transaction.type}</td>
                        <td className="py-2 px-4">{transaction.description}</td>
                        <td className="py-2 px-4">{new Date(transaction.createdDate).toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="text-center">No transactions found.</p>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Stock Holdings */}
      <div className={`overflow-x-auto shadow-lg rounded-lg p-4 mb-6 ${darkMode ? 'bg-gray-800' : 'bg-white'}`}>
        <h3 className="text-2xl mb-4 font-semibold">Stock Holdings</h3>
        <table className="min-w-full table-auto text-sm">
          <thead>
            <tr className="border-b">
              <th className="py-2 px-4 text-left font-medium">Stock Symbol</th>
              <th className="py-2 px-4 text-left font-medium">Quantity</th>
              <th className="py-2 px-4 text-left font-medium">Current Price</th>
              <th className="py-2 px-4 text-left font-medium">Change</th>
              <th className="py-2 px-4 text-left font-medium">Holding Value</th>
              <th className="py-2 px-4 text-left font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {stockHoldingItems.map((item, idx) => (
              <tr key={item.stockSymbol} className={`border-b hover:bg-gray-200 ${idx % 2 === 0 ? '' : 'bg-gray-50'}`}>
                <td className="py-2 px-4">{item.stockSymbol}</td>
                <td className="py-2 px-4">{item.quantity}</td>
                <td className="py-2 px-4">${item.unitPrice}</td>
                <td className={`py-2 px-4 ${item.change < 0 ? 'text-red-500' : 'text-green-500'}`}>
                  {item.change >= 0 ? `+${item.change}%` : `${item.change}%`}
                </td>
                <td className="py-2 px-4">${item.holdingValue.toFixed(2)}</td>
                <td className="py-2 px-4">
                  <button
                    onClick={() => {
                      console.log('Buy button clicked for:', item);
                      setSelectedStock(item);
                      setShowBuyModal(true);
                    }}
                    className={`px-3 py-1 rounded-md bg-green-500 hover:bg-green-400 text-white mr-2`}
                  >
                    Buy
                  </button>
                  <button
                    onClick={() => {
                      console.log('Sell button clicked for:', item);
                      setSelectedStock(item);
                      setShowSellModal(true);
                    }}
                    className={`px-3 py-1 rounded-md bg-red-500 hover:bg-red-400 text-white`}
                  >
                    Sell
                  </button>
                </td>
              </tr>
            ))}
            {stockHoldingItems.length === 0 && (
              <tr>
                <td colSpan={6} className="py-2 px-4 text-center">No stock holdings found.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Modals */}
      {showBuyModal && (
        <BuyModal
          isOpen={showBuyModal}
          onClose={() => setShowBuyModal(false)}
          onSubmit={handleBuySellSubmit}
          stockSymbol={selectedStock.stockSymbol}
        />
      )}
      {showSellModal && (
        <SellModal
          isOpen={showSellModal}
          onClose={() => setShowSellModal(false)}
          onSubmit={handleBuySellSubmit}
          stockSymbol={selectedStock.stockSymbol}
        />
      )}
    </div>
  );
};

export default PortfolioPage;
