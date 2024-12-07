// src/components/SellModal.js
import React, { useState, useContext } from 'react';
import ThemeContext from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext'; // Import AuthContext to get userId
import axios from 'axios';

const SellModal = ({ isOpen, onClose, onSubmit, stockSymbol }) => {
  const { darkMode } = useContext(ThemeContext);
  const { user } = useAuth(); // Access user data from AuthContext
  const [quantity, setQuantity] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const userId = user?.userId; // Access userId from AuthContext
      if (!userId) {
        throw new Error('User is not logged in or userId not found.');
      }

      const response = await axios.post('http://localhost:5244/api/stocks/sell', {
        UserId: userId, // Use userId from AuthContext
        StockSymbol: stockSymbol,
        Quantity: quantity,
      });

      console.log('Sell Success:', response.data);
      onSubmit(); // Portfolio'yu yenilemek için onSubmit çağırılıyor
      onClose(); // Modalı kapat
    } catch (err) {
      console.error('Sell Error:', err);
      setError('An error occurred while selling the stock.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={`fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center`}>
      <div className={`max-w-md w-full p-6 rounded-lg ${darkMode ? 'bg-gray-800 text-gray-200' : 'bg-white text-gray-900'} shadow-lg`}>
        <h2 className="text-2xl mb-4">Sell {stockSymbol} Stock</h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label htmlFor="quantity" className={`block mb-2 ${darkMode ? 'text-gray-200' : 'text-gray-900'}`}>
              Quantity
            </label>
            <input
              id="quantity"
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              className={`w-full p-2 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`}
              min="1"
              required
            />
          </div>
          {error && <p className="text-red-500 mb-4">{error}</p>}
          <button
            type="submit"
            className={`w-full p-2 rounded-md ${darkMode ? 'bg-red-600 hover:bg-red-500' : 'bg-red-500 hover:bg-red-400'} text-white`}
            disabled={loading}
          >
            {loading ? 'Selling...' : 'Sell'}
          </button>
        </form>
        <button
          onClick={onClose}
          className={`mt-4 w-full p-2 ${darkMode ? 'bg-gray-600 hover:bg-gray-500' : 'bg-gray-500 hover:bg-gray-400'} text-white rounded-md`}
        >
          Close
        </button>
      </div>
    </div>
  );
};

export default SellModal;
