import React, { useState, useContext } from 'react';

const Modal = ({ isOpen, onClose, onSubmit, title }) => {
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('TRY');

  if (!isOpen) return null;

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(amount, currency);
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50">
      <div className="max-w-md mx-auto p-6 rounded-lg mt-20">
        <h2 className="text-2xl mb-4">{title}</h2>
        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="amount" className="block">Amount</label>
            <input
              id="amount"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="w-full p-2 mb-4 border rounded-md"
              required
            />
          </div>
          <div>
            <label htmlFor="currency" className="block">Currency</label>
            <select
              id="currency"
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
              className="w-full p-2 mb-4 border rounded-md"
            >
              <option value="TRY">TRY</option>
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
            </select>
          </div>
          <button
            type="submit"
            className="w-full p-2 rounded-md text-white"
          >
            Submit
          </button>
        </form>
        <button
          onClick={onClose}
          className="mt-4 w-full p-2 text-white rounded-md"
        >
          Close
        </button>
      </div>
    </div>
  );
};

export default Modal;
