import React, { useContext } from "react";
import StockContext from "../context/StockContext";

const SearchResults = ({ results, onClear }) => {
  const { setStockSymbol } = useContext(StockContext);

  const handleSelection = (symbol) => {
    setStockSymbol(symbol);
    onClear();
  };

  return (
    <div className="absolute z-50 mt-2 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-64 overflow-y-auto">
      <ul className="divide-y divide-gray-200">
        {results.map((item) => (
          <li
            key={item.symbol}
            className="p-4 flex justify-between items-center cursor-pointer hover:bg-indigo-100 transition duration-300"
            onClick={() => handleSelection(item.symbol)}
          >
            <div className="text-gray-800 font-semibold">{item.symbol}</div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default SearchResults;
