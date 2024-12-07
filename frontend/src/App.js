// src/App.js
import React, { useState } from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";  // Import AuthProvider
import Dashboard from "./components/Dashboard";
import LoginForm from "./components/LoginForm";
import RegisterForm from "./components/RegisterForm";
import DepositForm from "./components/DepositForm";
import WithdrawalForm from "./components/WithdrawalForm";
import StockContext from "./context/StockContext";
import ThemeContext from "./context/ThemeContext";
import PortfolioPage from './pages/PortfolioPage';
import Navbar from "./components/Navbar";
import NewsPage from './pages/NewsPage';
import PrivateRoute from './components/PrivateRoute';  

function App() {
  const [darkMode, setDarkMode] = useState(true);
  const [stockSymbol, setStockSymbol] = useState("MSFT");

  return (
    <AuthProvider> {/* Wrap everything in AuthProvider */}
      <ThemeContext.Provider value={{ darkMode, setDarkMode }}>
        <StockContext.Provider value={{ stockSymbol, setStockSymbol }}>
          <Router>
            <div className={`${darkMode ? 'bg-gray-900 text-gray-200' : 'bg-white text-gray-900'}`}>
              <Navbar />
              <Routes>
                <Route path="/login" element={<LoginForm />} />
                <Route path="/register" element={<RegisterForm />} />
                <Route path="/dashboard" element={<PrivateRoute element={<Dashboard />} />} />
                <Route path="/deposit" element={<DepositForm />} />
                <Route path="/withdrawal" element={<WithdrawalForm />} />
                <Route path="/portfolio/:userId" element={<PortfolioPage />} />
                <Route path="/news" element={<NewsPage />} />
                <Route path="/" element={<LoginForm />} />
              </Routes>
            </div>
          </Router>
        </StockContext.Provider>
      </ThemeContext.Provider>
    </AuthProvider>
  );
}

export default App;
