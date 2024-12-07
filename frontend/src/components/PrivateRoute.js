// src/components/PrivateRoute.js
import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const PrivateRoute = ({ element }) => {
  const { user, loading } = useAuth();

  if (loading) {
    // Veriler hala yükleniyor, bekleyin
    return <div>Loading...</div>;
  }

  if (!user) {
    // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
    return <Navigate to="/login" />;
  }

  return element;
};

export default PrivateRoute;
