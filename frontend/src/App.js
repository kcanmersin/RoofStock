import React, { useState } from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import StockContext from "./context/StockContext";
import Navbar from "./components/Navbar";
import Dashboard from "./components/Dashboard";
import LoginForm from "./components/LoginForm";
import RegisterForm from "./components/RegisterForm";
import PortfolioPage from "./pages/PortfolioPage";
import NewsPage from "./pages/NewsPage";
import PrivateRoute from "./components/PrivateRoute";
import BuyModal from "./components/BuyModal";

function App() {
  const [refreshTrigger, setRefreshTrigger] = useState(false);
  const [stockSymbol, setStockSymbol] = useState("AAPL");
  const [isBuyModalOpen, setIsBuyModalOpen] = useState(false);

  const triggerNavbarRefresh = () => {
    setRefreshTrigger((prev) => !prev);
  };

  return (
    <AuthProvider>
      <StockContext.Provider value={{ stockSymbol, setStockSymbol }}>
        <Router>
          <div className="bg-white text-gray-900">
            <Navbar refreshTrigger={refreshTrigger} />
            <Routes>
              <Route path="/login" element={<LoginForm />} />
              <Route path="/register" element={<RegisterForm />} />
              <Route path="/dashboard" element={<PrivateRoute element={<Dashboard />} />} />
              <Route path="/portfolio/:userId" element={<PortfolioPage />} />
              <Route path="/news" element={<NewsPage />} />
              <Route path="/" element={<LoginForm />} />
            </Routes>
            <BuyModal
              isOpen={isBuyModalOpen}
              onClose={() => setIsBuyModalOpen(false)}
              onSubmit={triggerNavbarRefresh}
              stockSymbol={stockSymbol}
            />
          </div>
        </Router>
      </StockContext.Provider>
    </AuthProvider>
  );
}

export default App;
