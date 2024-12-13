import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import axios from 'axios';
import Notification from './Notification';

const DepositModal = ({ isOpen, onClose, onSubmit, refreshNavbar }) => {
  const { user } = useAuth();
  const [amount, setAmount] = useState(0);
  const [currency, setCurrency] = useState('TRY');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [notification, setNotification] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const userId = user?.userId;
      if (!userId) {
        throw new Error('User is not logged in or userId not found.');
      }

      const response = await axios.post('http://localhost:5244/api/deposit', {
        userId: userId,
        amount: amount,
        currency: currency,
      });

      const { isSuccess, newBalance, message } = response.data;

      if (isSuccess) {
        setNotification({
          message: `Deposit successful! New balance: $${newBalance.toFixed(2)}`,
          type: 'success',
        });

        if (refreshNavbar) {
          refreshNavbar();
        }

        onSubmit();
        onClose();
      } else {
        throw new Error(message || 'Deposit failed.');
      }
    } catch (err) {
      console.error('Deposit Error:', err);
      setError('An error occurred while making the deposit.');
      setNotification({
        message: 'Deposit failed!',
        type: 'error',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      {notification && (
        <Notification
          message={notification.message}
          type={notification.type}
          onClose={() => setNotification(null)}
        />
      )}
      {isOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
          <div className="max-w-md w-full p-6 rounded-lg bg-white text-gray-900 shadow-lg">
            <h2 className="text-2xl mb-4">Deposit Funds</h2>
            <form onSubmit={handleSubmit}>
              <div className="mb-4">
                <label htmlFor="amount" className="block mb-2 text-gray-900">
                  Amount
                </label>
                <input
                  id="amount"
                  type="number"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  className="w-full p-2 border rounded-md bg-gray-100 border-gray-300"
                  min="1"
                  required
                />
              </div>
              <div className="mb-4">
                <label htmlFor="currency" className="block mb-2 text-gray-900">
                  Currency
                </label>
                <select
                  id="currency"
                  value={currency}
                  onChange={(e) => setCurrency(e.target.value)}
                  className="w-full p-2 border rounded-md bg-gray-100 border-gray-300"
                >
                  <option value="TRY">TRY</option>
                  <option value="USD">USD</option>
                  <option value="EUR">EUR</option>
                </select>
              </div>
              {error && <p className="text-red-500 mb-4">{error}</p>}
              <button
                type="submit"
                className="w-full p-2 rounded-md bg-blue-500 hover:bg-blue-400 text-white"
                disabled={loading}
              >
                {loading ? 'Depositing...' : 'Deposit'}
              </button>
            </form>
            <button
              onClick={onClose}
              className="mt-4 w-full p-2 bg-gray-500 hover:bg-gray-400 text-white rounded-md"
            >
              Close
            </button>
          </div>
        </div>
      )}
    </>
  );
};

export default DepositModal;