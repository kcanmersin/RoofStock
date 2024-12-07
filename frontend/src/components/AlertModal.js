// src/components/AlertModal.js
import React, { useState, useContext } from 'react';
import ThemeContext from '../context/ThemeContext';
import setPriceAlertRequest from './SetPriceAlertRequest';
import { useAuth } from '../context/AuthContext'; // AuthContext'ten user bilgisini almak için import

const AlertModal = ({ isOpen, onClose, stockSymbol }) => {
  const { darkMode } = useContext(ThemeContext);
  const { user } = useAuth(); // user objesini AuthContext'ten al
  const [targetPrice, setTargetPrice] = useState(100.0);
  const [alertType, setAlertType] = useState(1);
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

      const response = await setPriceAlertRequest(userId, stockSymbol, targetPrice, alertType);
      console.log('Alert Set Success:', response);
      onClose();
    } catch (err) {
      console.error('Alert Error:', err);
      setError('An error occurred while setting the alert.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={`fixed inset-0 bg-black bg-opacity-50 z-50 ${darkMode ? 'text-white' : 'text-black'}`}>
      <div className={`max-w-md mx-auto p-6 rounded-lg mt-20 ${darkMode ? 'bg-gray-800 border-gray-600' : 'bg-white border-gray-300'}`}>
        <h2 className="text-2xl mb-4">Set Price Alert</h2>
        <form onSubmit={handleSubmit}>
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
            <label htmlFor="alertType" className={`block ${darkMode ? 'text-white' : 'text-black'}`}>Alert Type</label>
            <select
              id="alertType"
              value={alertType}
              onChange={(e) => setAlertType(Number(e.target.value))}
              className={`w-full p-2 mb-4 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`}
            >
              <option value={1}>Rise</option>
              <option value={0}>Fall</option>
            </select>
          </div>
          {error && <p className="text-red-500 mb-4">{error}</p>}
          <button
            type="submit"
            className={`w-full p-2 rounded-md ${darkMode ? 'bg-blue-600' : 'bg-blue-500'} text-white`}
            disabled={loading}
          >
            {loading ? 'Setting Alert...' : 'Set Alert'}
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

export default AlertModal;
