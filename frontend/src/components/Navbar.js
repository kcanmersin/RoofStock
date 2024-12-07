import React, { useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import ThemeContext from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext';

const Navbar = () => {
  const { darkMode } = useContext(ThemeContext);
  const { user, setUser, setToken } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    // localStorage'dan token ve kullanıcıyı sil
    localStorage.removeItem('token');
    localStorage.removeItem('user');

    // AuthContext'i sıfırlayın
    setUser(null);
    setToken(null);

    // Kullanıcıyı login sayfasına yönlendir
    navigate('/login');
  };

  return (
    <nav className={`fixed top-0 left-0 w-full p-4 z-50 ${darkMode ? 'bg-gray-800 text-white' : 'bg-blue-600 text-white'}`}>
      <div className="container mx-auto flex justify-between items-center">
        <div className="text-lg font-bold">
          <Link to="/">MyApp</Link>
        </div>
        <ul className="flex space-x-6">
          <li>
            <Link to="/dashboard" className="hover:underline">Dashboard</Link>
          </li>
          {/* Portfolyo linkini userId ile dinamik yap */}
          <li>
            <Link to={`/portfolio/${user?.userId}`} className="hover:underline">
              Portfolio
            </Link>
          </li>
          <li>
            <Link to="/news" className="hover:underline">News</Link>
          </li>
          {/* Eğer kullanıcı giriş yaptıysa Logout, yapmadıysa Login butonunu göster */}
          {user ? (
            <li>
              <button onClick={handleLogout} className="hover:underline">
                Logout
              </button>
            </li>
          ) : (
            <li>
              <Link to="/login" className="hover:underline">Login</Link>
            </li>
          )}
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
