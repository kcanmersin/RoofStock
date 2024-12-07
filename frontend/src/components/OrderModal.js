import React, { useState, useContext } from 'react';
import ThemeContext from '../context/ThemeContext';
import { giveOrderRequest } from './GiveOrderRequest';
import { useAuth } from '../context/AuthContext'; // AuthContext'ten user bilgisi almak için import

const OrderModal = ({ isOpen, onClose, stockSymbol }) => {
  const { darkMode } = useContext(ThemeContext);
  const { user } = useAuth(); // user objesini AuthContext'ten al
  const [quantity, setQuantity] = useState(1);
  const [targetPrice, setTargetPrice] = useState(150.0);
  const [orderType, setOrderType] = useState('Buy');
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

      const response = await giveOrderRequest(userId, stockSymbol, quantity, targetPrice, orderType);
      console.log('Order Success:', response);
      onClose();
    } catch (err) {
      console.error('Order Error:', err);
      setError('An error occurred while placing the order.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={`fixed inset-0 bg-black bg-opacity-50 z-50 ${darkMode ? 'text-white' : 'text-black'}`}>
      <div className={`max-w-md mx-auto p-6 rounded-lg mt-20 ${darkMode ? 'bg-gray-800 border-gray-600' : 'bg-white border-gray-300'}`}>
        <h2 className="text-2xl mb-4">Place Order</h2>
        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="quantity" className={`block ${darkMode ? 'text-white' : 'text-black'}`}>Quantity</label>
            <input
              id="quantity"
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              className={`w-full p-2 mb-4 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`}
              min="1"
              required
            />
          </div>
          <div>
            <label htmlFor="targetPrice" className={`block ${darkMode ? 'text-white' : 'text-black'}`}>Target Price</label>
            <input
              id="targetPrice"
              type="number"
              value={targetPrice}
              onChange={(e) => setTargetPrice(e.target.value)}
              className={`w-full p-2 mb-4 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`}
              required
            />
          </div>
          <div>
            <label htmlFor="orderType" className={`block ${darkMode ? 'text-white' : 'text-black'}`}>Order Type</label>
            <select
              id="orderType"
              value={orderType}
              onChange={(e) => setOrderType(e.target.value)}
              className={`w-full p-2 mb-4 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`}
            >
              <option value="Buy">Buy</option>
              <option value="Sell">Sell</option>
            </select>
          </div>
          {error && <p className="text-red-500 mb-4">{error}</p>}
          <button
            type="submit"
            className={`w-full p-2 rounded-md ${darkMode ? 'bg-yellow-600' : 'bg-yellow-500'} text-white`}
            disabled={loading}
          >
            {loading ? 'Placing Order...' : 'Place Order'}
          </button>
        </form>
        <button
          onClick={onClose}
          className={`mt-4 w-full p-2 ${darkMode ? 'bg-gray-500' : 'bg-gray-500'} text-white rounded-md`}
        >
          Close
        </button>
      </div>
    </div>
  );
};

export default OrderModal;
