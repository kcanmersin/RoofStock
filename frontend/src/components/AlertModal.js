import React, { useState, useEffect } from "react";
import setPriceAlertRequest from "./SetPriceAlertRequest";
import { useAuth } from "../context/AuthContext";

const AlertModal = ({ isOpen, onClose, stockSymbol, currentPrice }) => {
  const { user } = useAuth();
  const [targetPrice, setTargetPrice] = useState(100.0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [alertType, setAlertType] = useState(null);

  useEffect(() => {
    if (targetPrice > currentPrice) {
      setAlertType(1); // Rise
    } else if (targetPrice < currentPrice) {
      setAlertType(0); // Fall
    }
  }, [targetPrice, currentPrice]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const userId = user?.userId;
      if (!userId) {
        throw new Error('User is not logged in or userId not found.');
      }

      const response = await setPriceAlertRequest(userId, stockSymbol, targetPrice, alertType);
      console.log('Alert Set Success:', response);

      if (Notification.permission === 'granted') {
        new Notification('Price Alert Set', {
          body: `Your price alert for ${stockSymbol} at $${targetPrice} has been successfully set.`,
          icon: '/icon.png',
        });
      }

      onClose();
    } catch (err) {
      console.error('Alert Error:', err);
      setError('An error occurred while setting the alert.');

      if (Notification.permission === 'granted') {
        new Notification('Error', {
          body: 'An error occurred while setting the price alert. Please try again.',
          icon: '/icon.png',
        });
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (Notification.permission !== 'granted' && Notification.permission !== 'denied') {
      Notification.requestPermission();
    }
  }, []);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
      <div className="max-w-md w-full p-6 rounded-lg bg-white shadow-lg">
        <h2 className="text-2xl font-bold mb-6 text-center">Set Price Alert</h2>
        <form onSubmit={handleSubmit}>
          {/* Target Price */}
          <div className="mb-6">
            <label htmlFor="targetPrice" className="block text-lg font-medium mb-2 text-gray-800">
              Target Price
            </label>
            <input
              id="targetPrice"
              type="number"
              value={targetPrice}
              onChange={(e) => setTargetPrice(parseFloat(e.target.value))}
              className="w-full p-3 border rounded-md bg-gray-50 border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-400"
              required
            />
          </div>

          {/* Error Message */}
          {error && <p className="text-red-500 mb-4">{error}</p>}

          {/* Submit Button */}
          <button
            type="submit"
            className="w-full p-3 rounded-md bg-blue-500 hover:bg-blue-400 text-white font-medium"
            disabled={loading}
          >
            {loading ? "Setting Alert..." : "Set Alert"}
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

export default AlertModal;
