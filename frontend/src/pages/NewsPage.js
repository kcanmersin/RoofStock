import React, { useState, useEffect, useContext } from 'react';
import axios from 'axios';
import ThemeContext from '../context/ThemeContext';

const NewsPage = () => {
  const { darkMode } = useContext(ThemeContext);
  const [newsData, setNewsData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [newsType, setNewsType] = useState('company');
  const [symbol, setSymbol] = useState('AAPL'); 
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const stockSymbols = ['AAPL', 'MSFT', 'GOOGL', 'AMZN', 'TSLA', 'NVDA', 'META'];

  const today = new Date();
  const twoYearsAgo = new Date();
  twoYearsAgo.setFullYear(today.getFullYear() - 2);
  const fromDate = twoYearsAgo.toISOString().split('T')[0];
  const toDate = today.toISOString().split('T')[0];

  const fetchCompanyNews = async (symbol) => {
    const companyRequest = {
      Symbol: symbol,
      From: fromDate,
      To: toDate,
      Page: page,
      PageSize: 9
    };

    try {
      const response = await axios.get('http://localhost:5244/api/StockApi/company-news', { params: companyRequest });
      setNewsData(response.data.data);
      setTotalPages(response.data.totalPages);
      setLoading(false);
    } catch (err) {
      setError('Error fetching company news.');
      setLoading(false);
    }
  };

  const fetchMarketNews = async () => {
    const marketRequest = {
      Category: 'general',
      MinId: 0,
      Page: page,
      PageSize: 9
    };

    try {
      const response = await axios.get('http://localhost:5244/api/StockApi/market-news', { params: marketRequest });
      setNewsData(response.data.data);
      setTotalPages(response.data.totalPages);
      setLoading(false);
    } catch (err) {
      setError('Error fetching market news.');
      setLoading(false);
    }
  };

  // Fetch news when the component loads or when news type, page, or symbol changes
  useEffect(() => {
    setLoading(true);
    if (newsType === 'company') {
      // Fetch company news for a random symbol
      const randomSymbol = stockSymbols[Math.floor(Math.random() * stockSymbols.length)];
      setSymbol(randomSymbol); // Update the symbol state
      fetchCompanyNews(randomSymbol);
    } else if (newsType === 'market') {
      fetchMarketNews();
    }
  }, [newsType, page]); // Track newsType and page for triggering the fetch

  const handleNewsTypeChange = (type) => {
    setNewsType(type);
    setPage(1); // Reset to first page when changing news type
  };

  const handlePageChange = (newPage) => {
    setPage(newPage);
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  return (
    <div className={`container mx-auto p-6 ${darkMode ? 'bg-gray-900 text-gray-200' : 'bg-white text-gray-900'}`}>
      <h2 className="text-3xl mb-6">News Feed</h2>

      {/* Toggle buttons for news type */}
      <div className="mb-6">
        <button
          className={`px-6 py-2 mr-4 rounded-full ${newsType === 'company' ? 'bg-blue-600 text-white' : 'bg-gray-300'}`}
          onClick={() => handleNewsTypeChange('company')}
        >
          Company News
        </button>
        <button
          className={`px-6 py-2 rounded-full ${newsType === 'market' ? 'bg-blue-600 text-white' : 'bg-gray-300'}`}
          onClick={() => handleNewsTypeChange('market')}
        >
          Market News
        </button>
      </div>

      {/* Display news articles */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {newsData.length > 0 ? (
          newsData.map((newsItem, index) => (
            <div key={index} className="p-4 border rounded-lg shadow-lg bg-white dark:bg-gray-800">
              <h3 className="text-xl font-semibold mb-2">{newsItem.headline || newsItem.title}</h3>
              <p className="text-sm mb-2">{newsItem.summary}</p>
              <a href={newsItem.url} target="_blank" rel="noopener noreferrer" className="text-blue-500">
                Read More
              </a>
            </div>
          ))
        ) : (
          <div>No news found for this category.</div>
        )}
      </div>

      {/* Pagination */}
      <div className="mt-6 flex justify-between items-center">
        <button
          onClick={() => handlePageChange(page > 1 ? page - 1 : page)}
          disabled={page <= 1}
          className="px-4 py-2 bg-gray-300 rounded-lg"
        >
          Previous
        </button>
        <span className="px-4 py-2">{`Page ${page} of ${totalPages}`}</span>
        <button
          onClick={() => handlePageChange(page < totalPages ? page + 1 : page)}
          disabled={page >= totalPages}
          className="px-4 py-2 bg-gray-300 rounded-lg"
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default NewsPage;
