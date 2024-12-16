import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';

const OrderModal = ({ isOpen, onClose, stockSymbol, currentPrice }) => {
  const { user } = useAuth();
  const [quantity, setQuantity] = useState(1);
  const [targetPrice, setTargetPrice] = useState(currentPrice || 150.0);
  const [orderType, setOrderType] = useState('Buy');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setTargetPrice(currentPrice);
  }, [currentPrice]);

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    if (orderType === 'Sell' && targetPrice < currentPrice) {
      setError(`You cannot sell at a price lower than the current price of ${currentPrice}.`);
      setLoading(false);
      return;
    }

    try {
      const userId = user?.userId;
      if (!userId) {
        throw new Error('User is not logged in or userId not found.');
      }

      const response = await axios.post('http://localhost:5244/api/orders/place', {
        userId,               
        stockSymbol,            
        quantity,              
        targetPrice,         
        orderType: orderType === 'Buy' ? 0 : 1,  
      });

      console.log('Order Success:', response.data);
      onClose();  
    } catch (err) {
      console.error('Order Error:', err);
      setError('An error occurred while placing the order.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
      <div className="max-w-md w-full p-6 rounded-lg bg-white shadow-lg">
        <h2 className="text-2xl font-bold mb-6 text-center">Place Order</h2>
        <form onSubmit={handleSubmit}>
          {/* Quantity Input */}
          <div className="mb-6">
            <label htmlFor="quantity" className="block text-lg font-medium mb-2 text-gray-800">
              Quantity
            </label>
            <input
              id="quantity"
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              className="w-full p-3 border rounded-md bg-gray-50 border-gray-300 focus:outline-none focus:ring-2 focus:ring-yellow-400"
              min="1"
              required
            />
          </div>

          {/* Target Price Input */}
          <div className="mb-6">
            <label htmlFor="targetPrice" className="block text-lg font-medium mb-2 text-gray-800">
              Target Price
            </label>
            <input
              id="targetPrice"
              type="number"
              value={targetPrice}
              onChange={(e) => setTargetPrice(e.target.value)}
              className="w-full p-3 border rounded-md bg-gray-50 border-gray-300 focus:outline-none focus:ring-2 focus:ring-yellow-400"
              required
            />
          </div>

          {/* Order Type Selection */}
          <div className="mb-6">
            <label className="block text-lg font-medium mb-2 text-gray-800">Order Type</label>
            <div className="flex space-x-4">
              <button
                type="button"
                className={`flex-1 p-3 text-center rounded-md border ${
                  orderType === 'Buy'
                    ? 'bg-green-500 text-white border-green-500'
                    : 'bg-gray-100 text-gray-800 border-gray-300'
                }`}
                onClick={() => setOrderType('Buy')}
              >
                Buy
              </button>
              <button
                type="button"
                className={`flex-1 p-3 text-center rounded-md border ${
                  orderType === 'Sell'
                    ? 'bg-red-500 text-white border-red-500'
                    : 'bg-gray-100 text-gray-800 border-gray-300'
                }`}
                onClick={() => setOrderType('Sell')}
              >
                Sell
              </button>
            </div>
          </div>

          {/* Error Message */}
          {error && <p className="text-red-500 mb-4">{error}</p>}

          {/* Submit Button */}
          <button
            type="submit"
            className="w-full p-3 rounded-md bg-yellow-500 hover:bg-yellow-400 text-white font-medium"
            disabled={loading}
          >
            {loading ? 'Placing Order...' : 'Place Order'}
          </button>
        </form>

        {/* Close Button */}
        <button
          onClick={onClose}
          className="mt-4 w-full p-3 bg-gray-500 hover:bg-gray-400 text-white rounded-md font-medium"
        >
          Close
        </button>
      </div>
    </div>
  );
};

export default OrderModal;
