// src/components/BuyModal.js
import React, { useState, useContext } from 'react';
import ThemeContext from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext'; // AuthContext'ten user bilgisi almak için import
import axios from 'axios';

const BuyModal = ({ isOpen, onClose, onSubmit, stockSymbol }) => {
  const { darkMode } = useContext(ThemeContext);
  const { user } = useAuth(); // user objesini AuthContext'ten al
  const [quantity, setQuantity] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const userId = user?.userId; // AuthContext'ten gelen user objesinden userId al
      if (!userId) {
        throw new Error('User is not logged in or userId not found.');
      }

      const response = await axios.post('http://localhost:5244/api/stocks/buy', {
        UserId: userId,
        StockSymbol: stockSymbol,
        Quantity: quantity,
      });

      console.log('Buy Success:', response.data);
      onSubmit(); // Portfolio'yu yenilemek için onSubmit çağırılıyor
      onClose(); // Modalı kapat
    } catch (err) {
      console.error('Buy Error:', err);
      setError('An error occurred while buying the stock.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={`fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center`}>
      <div className={`max-w-md w-full p-6 rounded-lg ${darkMode ? 'bg-gray-800 text-gray-200' : 'bg-white text-gray-900'} shadow-lg`}>
        <h2 className="text-2xl mb-4">Buy {stockSymbol} Stock</h2>
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
            className={`w-full p-2 rounded-md ${darkMode ? 'bg-blue-600 hover:bg-blue-500' : 'bg-blue-500 hover:bg-blue-400'} text-white`}
            disabled={loading}
          >
            {loading ? 'Buying...' : 'Buy'}
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

export default BuyModal;
