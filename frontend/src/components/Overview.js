import React, { useState, useContext } from "react";
import Card from "./Card";
import BuyModal from "./BuyModal";
import SellModal from "./SellModal";
import OrderModal from "./OrderModal";
import AlertModal from "./AlertModal";
import ThemeContext from "../context/ThemeContext";

const Overview = ({ symbol, price, change, changePercent, currency }) => {
  const { darkMode } = useContext(ThemeContext);
  const [isBuyModalOpen, setIsBuyModalOpen] = useState(false);
  const [isSellModalOpen, setIsSellModalOpen] = useState(false);
  const [isOrderModalOpen, setIsOrderModalOpen] = useState(false);
  const [isAlertModalOpen, setIsAlertModalOpen] = useState(false);
  const [notifications, setNotifications] = useState([]);

  const addNotification = (message, backgroundColor) => {
    const id = Date.now();
    setNotifications((prev) => [...prev, { id, message, backgroundColor, visible: true }]);
    setTimeout(() => {
      setNotifications((prev) =>
        prev.map((n) => (n.id === id ? { ...n, visible: false } : n))
      );
      setTimeout(() => {
        setNotifications((prev) => prev.filter((n) => n.id !== id));
      }, 1000);
    }, 3000);
  };

  const handleBuySubmit = (quantity) => {
    addNotification("Purchase successful", "bg-green-500");
    setTimeout(() => {
      handleModalClose();
    }, 1000);
  };

  const handleSellSubmit = (quantity) => {
    addNotification("Sale successful", "bg-red-500");
    setTimeout(() => {
      handleModalClose();
    }, 1000);
  };

  const handleOrderSubmit = (orderDetails) => {
    addNotification("Order placed successfully", "bg-yellow-500");
    setTimeout(() => {
      handleModalClose();
    }, 1000);
  };

  const handleAlertSubmit = (alertDetails) => {
    addNotification("Alert set successfully", "bg-blue-500");
    setTimeout(() => {
      handleModalClose();
    }, 1000);
  };

  const handleBuyModalOpen = () => setIsBuyModalOpen(true);
  const handleSellModalOpen = () => setIsSellModalOpen(true);
  const handleOrderModalOpen = () => setIsOrderModalOpen(true);
  const handleAlertModalOpen = () => setIsAlertModalOpen(true);

  const handleModalClose = () => {
    setIsBuyModalOpen(false);
    setIsSellModalOpen(false);
    setIsOrderModalOpen(false);
    setIsAlertModalOpen(false);
  };

  return (
    <div className="relative">
      <Card>
        <span className="absolute left-4 top-4 text-neutral-400 text-lg xl:text-xl 2xl:text-2xl">
          {symbol}
        </span>
        <div className="w-full h-full flex items-center justify-around">
          <span className="text-2xl xl:text-4xl 2xl:text-5xl flex items-center">
            ${price}
            <span className="text-lg xl:text-xl 2xl:text-2xl text-neutral-400 m-2">
              {currency}
            </span>
          </span>
          <span className={`text-lg xl:text-xl 2xl:text-2xl ${change > 0 ? "text-lime-500" : "text-red-500"}`}>
            {change} <span>({changePercent}%)</span>
          </span>
        </div>
        <div className="mt-4 flex justify-between">
          <button
            onClick={handleBuyModalOpen}
            className={`p-2 w-1/4 ${darkMode ? 'bg-green-700 hover:bg-green-600' : 'bg-green-500 hover:bg-green-400'} text-white rounded-md transition-colors`}
          >
            Buy
          </button>
          <button
            onClick={handleSellModalOpen}
            className={`p-2 w-1/4 ${darkMode ? 'bg-red-700 hover:bg-red-600' : 'bg-red-500 hover:bg-red-400'} text-white rounded-md transition-colors`}
          >
            Sell
          </button>
          <button
            onClick={handleOrderModalOpen}
            className={`p-2 w-1/4 ${darkMode ? 'bg-yellow-700 hover:bg-yellow-600' : 'bg-yellow-500 hover:bg-yellow-400'} text-white rounded-md transition-colors`}
          >
            Place Order
          </button>
          <button
            onClick={handleAlertModalOpen}
            className={`p-2 w-1/4 ${darkMode ? 'bg-blue-700 hover:bg-blue-600' : 'bg-blue-500 hover:bg-blue-400'} text-white rounded-md transition-colors`}
          >
            Set Alert
          </button>
        </div>
      </Card>
      <BuyModal
        isOpen={isBuyModalOpen}
        onClose={handleModalClose}
        onSubmit={handleBuySubmit}
        stockSymbol={symbol}
      />
      <SellModal
        isOpen={isSellModalOpen}
        onClose={handleModalClose}
        onSubmit={handleSellSubmit}
        stockSymbol={symbol}
      />
      <OrderModal
        isOpen={isOrderModalOpen}
        onClose={handleModalClose}
        onSubmit={handleOrderSubmit}
        stockSymbol={symbol}
      />
      <AlertModal
        isOpen={isAlertModalOpen}
        onClose={handleModalClose}
        onSubmit={handleAlertSubmit}
        stockSymbol={symbol}
      />
      <div className="fixed bottom-4 right-4 space-y-2 z-50">
  {notifications.map((n) => (
    <div
      key={n.id}
      className={`transition-opacity duration-1000 p-8 rounded-lg font-semibold text-center shadow-lg ${n.backgroundColor} ${n.visible ? 'opacity-100' : 'opacity-0'} text-2xl`}
    >
      {n.message}
    </div>
  ))}
</div>


    </div>
  );
};

export default Overview;
