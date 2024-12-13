import React, { useState, useEffect } from "react";
import axios from "axios";

const NewsPage = () => {
  const [newsData, setNewsData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [newsType, setNewsType] = useState("company");
  const [symbol, setSymbol] = useState("AAPL");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const stockSymbols = ["AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "NVDA", "META"];

  const today = new Date();
  const twoYearsAgo = new Date(today.setFullYear(today.getFullYear() - 2));
  const fromDate = twoYearsAgo.toISOString().split("T")[0];
  const toDate = new Date().toISOString().split("T")[0];

  const fetchCompanyNews = async (symbol) => {
    const companyRequest = {
      Symbol: symbol,
      From: fromDate,
      To: toDate,
      Page: page,
      PageSize: 9,
    };

    try {
      const response = await axios.get("http://localhost:5244/api/StockApi/company-news", {
        params: companyRequest,
      });
      setNewsData(response.data.data);
      setTotalPages(response.data.totalPages);
      setLoading(false);
    } catch (err) {
      setError("Error fetching company news.");
      setLoading(false);
    }
  };

  const fetchMarketNews = async () => {
    const marketRequest = {
      Category: "general",
      MinId: 0,
      Page: page,
      PageSize: 9,
    };

    try {
      const response = await axios.get("http://localhost:5244/api/StockApi/market-news", {
        params: marketRequest,
      });
      setNewsData(response.data.data);
      setTotalPages(response.data.totalPages);
      setLoading(false);
    } catch (err) {
      setError("Error fetching market news.");
      setLoading(false);
    }
  };

  useEffect(() => {
    setLoading(true);
    if (newsType === "company") {
      const randomSymbol = stockSymbols[Math.floor(Math.random() * stockSymbols.length)];
      setSymbol(randomSymbol);
      fetchCompanyNews(randomSymbol);
    } else if (newsType === "market") {
      fetchMarketNews();
    }
  }, [newsType, page]);

  const handleNewsTypeChange = (type) => {
    setNewsType(type);
    setPage(1);
  };

  const handlePageChange = (newPage) => {
    setPage(newPage);
  };

  if (loading) return <div className="text-center mt-10">Loading...</div>;
  if (error) return <div className="text-center text-red-500 mt-10">{error}</div>;

  return (
    <div className="container mx-auto p-6 bg-gray-50 text-gray-900">
      {/* Header */}
      <h2 className="text-3xl font-bold mb-6 text-center">News</h2>

      {/* Toggle Buttons with Pagination */}
      <div className="flex justify-center items-center mb-6 space-x-4">
        <button
          onClick={() => handlePageChange(page > 1 ? page - 1 : page)}
          disabled={page <= 1 || loading}
          className="px-4 py-2 bg-gray-300 rounded-lg hover:bg-gray-400 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Previous
        </button>

        <button
          className={`px-6 py-2 rounded-full ${
            newsType === "company" ? "bg-blue-600 text-white" : "bg-gray-300"
          }`}
          onClick={() => handleNewsTypeChange("company")}
        >
          Company News
        </button>
        <button
          className={`px-6 py-2 rounded-full ${
            newsType === "market" ? "bg-blue-600 text-white" : "bg-gray-300"
          }`}
          onClick={() => handleNewsTypeChange("market")}
        >
          Market News
        </button>

        <button
          onClick={() => handlePageChange(page < totalPages ? page + 1 : page)}
          disabled={page >= totalPages || loading}
          className="px-4 py-2 bg-gray-300 rounded-lg hover:bg-gray-400 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Next
        </button>
      </div>

      {/* News Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {newsData.length > 0 ? (
          newsData.map((newsItem, index) => (
            <div
              key={index}
              className="p-4 border rounded-lg shadow-lg bg-white transition-transform transform hover:scale-105"
            >
              <h3 className="text-xl font-semibold mb-2">{newsItem.headline || newsItem.title}</h3>
              <p className="text-sm mb-2 text-gray-700">{newsItem.summary}</p>
              <a
                href={newsItem.url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-blue-600 font-medium"
              >
                Read More
              </a>
            </div>
          ))
        ) : (
          <div className="col-span-full text-center text-gray-500">No news found for this category.</div>
        )}
      </div>

      {/* Page Info */}
      <div className="mt-4 text-center">
        <span>{`Page ${page} of ${totalPages}`}</span>
      </div>
    </div>
  );
};

export default NewsPage;
