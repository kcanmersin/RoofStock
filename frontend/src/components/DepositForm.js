import React, { useState, useContext } from 'react';
import ThemeContext from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext'; // AuthContext'ten user bilgisi almak için import

const DepositForm = () => {
  const { darkMode } = useContext(ThemeContext);
  const { user } = useAuth(); // user objesini AuthContext'ten al
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('TRY');
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const handleDeposit = async (e) => {
    e.preventDefault();
    const userId = user?.userId; // AuthContext'ten gelen user objesinden userId al
    if (!userId) {
      setError('User is not logged in or userId not found.');
      return;
    }

    const depositData = {
      userId,
      amount,
      currency,
    };

    try {
      const response = await fetch('http://localhost:5244/api/deposit', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(depositData),
      });

      const data = await response.json();
      if (response.ok) {
        setSuccess('Deposit successful');
        setError(null);
      } else {
        setError(data.message || 'Something went wrong');
        setSuccess(null);
      }
    } catch (error) {
      setError('Failed to communicate with the server');
      setSuccess(null);
    }
  };

  return (
    <div className={`mb-4 ${darkMode ? 'bg-gray-800 text-white' : 'bg-white text-gray-800'}`}>
      <h4 className="font-semibold text-lg">Deposit Funds</h4>
      <form onSubmit={handleDeposit}>
        <div>
          <label htmlFor="amount" className="block">Amount</label>
          <input
            id="amount"
            type="number"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            className={`w-full p-2 border ${darkMode ? 'border-gray-600' : 'border-gray-300'} rounded-md`}
            required
          />
        </div>
        <div className="mt-2">
          <label htmlFor="currency" className="block">Currency</label>
          <select
            id="currency"
            value={currency}
            onChange={(e) => setCurrency(e.target.value)}
            className={`w-full p-2 border ${darkMode ? 'border-gray-600' : 'border-gray-300'} rounded-md`}
          >
            <option value="TRY">TRY</option>
            <option value="USD">USD</option>
            <option value="EUR">EUR</option>
          </select>
        </div>
        <button type="submit" className={`mt-4 w-full ${darkMode ? 'bg-blue-600' : 'bg-blue-500'} text-white p-2 rounded-md`}>
          Deposit
        </button>
      </form>
      {error && <p className="text-red-500">{error}</p>}
      {success && <p className="text-green-500">{success}</p>}
    </div>
  );
};

export default DepositForm;
