import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import DepositModal from './DepositModal';
import WithdrawalModal from './WithdrawalModal';

const Navbar = () => {
  const { user, setUser, setToken } = useAuth();
  const navigate = useNavigate();
  const [isDepositOpen, setIsDepositOpen] = useState(false);
  const [isWithdrawalOpen, setIsWithdrawalOpen] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const [balance, setBalance] = useState(0);

  const fetchNavbarBalance = async () => {
    if (user) {
      try {
        const response = await fetch(`http://localhost:5244/api/stocks/portfolio?UserId=${user.userId}`, {
          headers: { 'Accept': '*/*' },
        });

        if (response.ok) {
          const data = await response.json();
          if (data.isSuccess) {
            setBalance(data.totalBalance || 0);
          }
        }
      } catch (error) {
        console.error('Error fetching balance:', error);
      }
    }
  };

  useEffect(() => {
    fetchNavbarBalance();
  }, [user]);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    setToken(null);
    navigate('/login');
  };

  return (
    <nav className="fixed top-0 left-0 w-full p-4 z-50 bg-gray-500 shadow-md">
      <div className="container mx-auto flex justify-between items-center">
        <div>
          <Link to="/" className="text-lg font-bold text-white hover:text-gray-300 transition-colors duration-200">
            RoofStock
          </Link>
        </div>
        <div className={`lg:flex space-x-6 ${isOpen ? 'flex' : 'hidden'} lg:block`}>
          <ul className="flex space-x-6 items-center">
            {user && (
              <li className="text-white text-sm">
                <strong>Balance:</strong> ${balance.toFixed(2)}
              </li>
            )}
            <li>
              <Link to="/dashboard" className="text-white hover:text-gray-300 transition-colors duration-200">
                Dashboard
              </Link>
            </li>
            <li>
              <Link to={`/portfolio/${user?.userId}`} className="text-white hover:text-gray-300 transition-colors duration-200">
                Portfolio
              </Link>
            </li>
            <li>
              <Link to="/news" className="text-white hover:text-gray-300 transition-colors duration-200">
                News
              </Link>
            </li>
            {user && (
              <>
                <li>
                  <button
                    onClick={() => setIsDepositOpen(true)}
                    className="text-white hover:text-gray-300 transition-colors duration-200"
                  >
                    Deposit
                  </button>
                </li>
                <li>
                  <button
                    onClick={() => setIsWithdrawalOpen(true)}
                    className="text-white hover:text-gray-300 transition-colors duration-200"
                  >
                    Withdrawal
                  </button>
                </li>
              </>
            )}
            {user ? (
              <li>
                <button
                  onClick={handleLogout}
                  className="text-white hover:text-gray-300 transition-colors duration-200"
                >
                  Logout
                </button>
              </li>
            ) : (
              <li>
                <Link to="/login" className="text-white hover:text-gray-300 transition-colors duration-200">
                  Login
                </Link>
              </li>
            )}
          </ul>
        </div>
        <div className="lg:hidden flex items-center">
          <button
            onClick={() => setIsOpen(!isOpen)}
            className="text-white focus:outline-none"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M4 6h16M4 12h16M4 18h16"
              ></path>
            </svg>
          </button>
        </div>
      </div>
      <DepositModal isOpen={isDepositOpen} onClose={() => setIsDepositOpen(false)} onSubmit={fetchNavbarBalance} />
      <WithdrawalModal isOpen={isWithdrawalOpen} onClose={() => setIsWithdrawalOpen(false)} onSubmit={fetchNavbarBalance} />
    </nav>
  );
};

export default Navbar;
