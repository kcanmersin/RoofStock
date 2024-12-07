import React, { useState, useContext } from 'react';
import ThemeContext from '../context/ThemeContext'; // Tema kontrolü

const Modal = ({ isOpen, onClose, onSubmit, title }) => {
  const { darkMode } = useContext(ThemeContext); // Tema kontrolü
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('TRY');

  if (!isOpen) return null; // Eğer modal açık değilse hiçbir şey render edilmesin

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(amount, currency); // Veri gönderimi
  };

  return (
    <div
      className={`fixed inset-0 bg-black bg-opacity-50 z-50 ${darkMode ? 'text-white' : 'text-black'}`} // Arka plan ve metin rengi
    >
      <div
        className={`max-w-md mx-auto p-6 rounded-lg mt-20 ${darkMode ? 'bg-gray-800 border-gray-600' : 'bg-white border-gray-300'}`} // Modal'ın arka planı ve border renkleri
      >
        <h2 className={`text-2xl mb-4 ${darkMode ? 'text-white' : 'text-black'}`}>{title}</h2> {/* Başlık rengi */}
        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="amount" className={`block ${darkMode ? 'text-white' : 'text-black'}`}>Amount</label>
            <input
              id="amount"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className={`w-full p-2 mb-4 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`} // Input alanları renkleri
              required
            />
          </div>
          <div>
            <label htmlFor="currency" className={`block ${darkMode ? 'text-white' : 'text-black'}`}>Currency</label>
            <select
              id="currency"
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
              className={`w-full p-2 mb-4 border rounded-md ${darkMode ? 'bg-gray-700 border-gray-600' : 'bg-gray-100 border-gray-300'}`} // Select alanları renkleri
            >
              <option value="TRY">TRY</option>
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
            </select>
          </div>
          <button
            type="submit"
            className={`w-full p-2 rounded-md ${darkMode ? 'bg-blue-600' : 'bg-blue-500'} text-white`} // Buton rengi
          >
            Submit
          </button>
        </form>
        <button
          onClick={onClose}
          className={`mt-4 w-full p-2 ${darkMode ? 'bg-gray-500' : 'bg-gray-500'} text-white rounded-md`} // Close butonu rengi
        >
          Close
        </button>
      </div>
    </div>
  );
};

export default Modal;
