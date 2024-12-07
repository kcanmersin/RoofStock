import React, { useState, useEffect, useContext } from 'react';
import axios from 'axios';
import { useParams } from 'react-router-dom';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import ThemeContext from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext'; // Import useAuth to get userId

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

const PortfolioPage = () => {
  const { darkMode } = useContext(ThemeContext); // Tema seçimini almak
  const { user } = useAuth(); // Access user data from AuthContext
  const [portfolio, setPortfolio] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const { userId: urlUserId } = useParams();
  const effectiveUserId = urlUserId || user?.userId; // Use userId from URL or AuthContext

  // Portföy verisini çekme
  const fetchPortfolio = async () => {
    if (!effectiveUserId) {
      setError('No userId found.');
      setLoading(false);
      return;
    }
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

  // Chart seçenekleri
  const options = {
    responsive: true,
    scales: {
      x: { beginAtZero: true },
      y: { beginAtZero: true },
    },
  };

  // Emir iptal işlemi
  const handleCancelOrder = async (orderId) => {
    try {
      await axios.post('http://localhost:5244/api/stocks/cancel-order', { orderId });
      fetchPortfolio();
    } catch (err) {
      setError('Error canceling order.');
    }
  };

  // Fiyat alarmı silme işlemi
  const handleDeletePriceAlert = async (alertId) => {
    try {
      await axios.delete(`http://localhost:5244/api/stocks/price-alert/${alertId}`);
      fetchPortfolio();
    } catch (err) {
      setError('Error deleting price alert.');
    }
  };

  // Yükleniyor veya hata durumunu göster
  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  // Güvenli varsayılan değerler
  const stockHoldingItems = portfolio?.stockHoldingItems || [];
  const priceAlerts = portfolio?.priceAlerts || [];

  return (
    <div
      className={`container mx-auto p-6 ${darkMode ? 'bg-gray-900 text-gray-200' : 'bg-white text-gray-900'}`}
    >
      <h2 className="text-3xl mb-6">Portfolio Overview</h2>

      {/* Stock Chart */}
      {stockHoldingItems.length > 0 && (
        <div className="mb-6">
          <Line
            data={{
              labels: stockHoldingItems.map((item) => item.stockSymbol),
              datasets: [
                {
                  label: 'Stock Price',
                  data: stockHoldingItems.map((item) => item.currentPrice),
                  borderColor: darkMode ? 'rgba(75,192,192,1)' : 'rgba(75,192,192,0.8)', // Dark mode'da daha belirgin bir renk
                  backgroundColor: darkMode
                    ? 'rgba(75,192,192,0.2)'
                    : 'rgba(75,192,192,0.3)', // Arka plan rengini koyulaştır
                  fill: true,
                },
              ],
            }}
            options={options}
          />
        </div>
      )}

      {/* Portfolio Table */}
      <div
        className={`overflow-x-auto shadow-lg rounded-lg p-4 mb-6 ${darkMode ? 'bg-gray-800' : 'bg-white'}`}
      >
        <h3 className="text-2xl mb-4">Stock Holdings</h3>
        <table className="min-w-full table-auto">
          <thead>
            <tr>
              <th className="py-2 px-4 border-b">Stock Symbol</th>
              <th className="py-2 px-4 border-b">Quantity</th>
              <th className="py-2 px-4 border-b">Current Price</th>
              <th className="py-2 px-4 border-b">Actions</th>
            </tr>
          </thead>
          <tbody>
            {stockHoldingItems.map((item) => (
              <tr key={item.stockSymbol} className={darkMode ? 'bg-gray-700' : ''}>
                <td className="py-2 px-4 border-b">{item.stockSymbol}</td>
                <td className="py-2 px-4 border-b">{item.quantity}</td>
                <td className="py-2 px-4 border-b">${item.currentPrice}</td>
                <td className="py-2 px-4 border-b">
                  <button
                    onClick={() => handleCancelOrder(item.orderId)}
                    className={`px-4 py-2 rounded-full ${darkMode ? 'bg-red-600' : 'bg-red-500'} text-white`}
                  >
                    Cancel Order
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Price Alerts Table */}
      <div
        className={`overflow-x-auto shadow-lg rounded-lg p-4 ${darkMode ? 'bg-gray-800' : 'bg-white'}`}
      >
        <h3 className="text-2xl mb-4">Price Alerts</h3>
        <table className="min-w-full table-auto">
          <thead>
            <tr>
              <th className="py-2 px-4 border-b">Stock Symbol</th>
              <th className="py-2 px-4 border-b">Alert Price</th>
              <th className="py-2 px-4 border-b">Actions</th>
            </tr>
          </thead>
          <tbody>
            {priceAlerts.map((alert) => (
              <tr key={alert.alertId} className={darkMode ? 'bg-gray-700' : ''}>
                <td className="py-2 px-4 border-b">{alert.stockSymbol}</td>
                <td className="py-2 px-4 border-b">${alert.alertPrice}</td>
                <td className="py-2 px-4 border-b">
                  <button
                    onClick={() => handleDeletePriceAlert(alert.alertId)}
                    className={`px-4 py-2 rounded-full ${darkMode ? 'bg-red-600' : 'bg-red-500'} text-white`}
                  >
                    Delete Alert
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default PortfolioPage;
