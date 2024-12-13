import React, { useEffect, useState } from "react";

const Notification = ({ message, type, onClose }) => {
  const [isVisible, setIsVisible] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(false);
      onClose();
    }, 4000); // Dismiss after 4 seconds

    return () => clearTimeout(timer);
  }, [onClose]);

  if (!isVisible) return null;

  return (
    <div
      className={`fixed right-4 top-4 p-4 rounded-md ${type === "success" ? "bg-green-500" : type === "error" ? "bg-red-500" : "bg-yellow-500"} text-white shadow-lg`}
    >
      <p>{message}</p>
    </div>
  );
};

export default Notification;
