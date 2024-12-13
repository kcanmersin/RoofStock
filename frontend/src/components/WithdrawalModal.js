import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import axios from 'axios';
import Notification from './Notification';

const WithdrawalModal = ({ isOpen, onClose, onSubmit, refreshNavbar }) => {
  const { user } = useAuth();
  const [amountInUSD, setAmountInUSD] = useState(0);
  const [targetCurrency, setTargetCurrency] = useState('TRY');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [notification, setNotification] = useState(null);

  const fetchUpdatedBalance = async () => {
    try {
      const response = await axios.get(
        `http://localhost:5244/api/stocks/portfolio?UserId=${user?.userId}`
      );
      if (response.data.isSuccess) {
        return response.data.totalBalance || 0;
      }
    } catch (err) {
      console.error('Error fetching updated balance:', err);
    }
    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const userId = user?.userId;
      if (!userId) {
        throw new Error('User is not logged in or userId not found.');
      }

      const response = await axios.post('http://localhost:5244/api/withdrawal', {
        userId: userId,
        amountInUSD: amountInUSD,
        targetCurrency: targetCurrency,
      });

      const updatedBalance = await fetchUpdatedBalance();

      setNotification({
        message: `Withdrawal successful! Remaining balance: $${updatedBalance?.toFixed(2)}`,
        type: 'success',
      });

      if (refreshNavbar) {
        refreshNavbar();
      }

      onSubmit(response.data);
      onClose();
    } catch (err) {
      console.error('Withdrawal Error:', err);
      setError('An error occurred while processing the withdrawal.');
      setNotification({
        message: 'Withdrawal failed!',
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
            <h2 className="text-2xl mb-4">Withdraw Funds</h2>
            <form onSubmit={handleSubmit}>
              <div className="mb-4">
                <label htmlFor="amountInUSD" className="block mb-2 text-gray-900">
                  Amount (in USD)
                </label>
                <input
                  id="amountInUSD"
                  type="number"
                  value={amountInUSD}
                  onChange={(e) => setAmountInUSD(e.target.value)}
                  className="w-full p-2 border rounded-md bg-gray-100 border-gray-300"
                  min="1"
                  required
                />
              </div>
              <div className="mb-4">
                <label htmlFor="targetCurrency" className="block mb-2 text-gray-900">
                  Target Currency
                </label>
                <select
                  id="targetCurrency"
                  value={targetCurrency}
                  onChange={(e) => setTargetCurrency(e.target.value)}
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
                {loading ? 'Withdrawing...' : 'Withdraw'}
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

export default WithdrawalModal;
