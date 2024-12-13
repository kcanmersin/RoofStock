import React, { useState, useEffect } from "react";
import { SearchIcon, XIcon } from "@heroicons/react/solid";
import SearchResults from "./SearchResults";
import nasdaqTxt from "./nasdaq.txt";

const Search = () => {
  const [input, setInput] = useState("");
  const [nasdaqSymbols, setNasdaqSymbols] = useState([]);
  const [bestMatches, setBestMatches] = useState([]);

  // NASDAQ sembollerini dosyadan yükle
  useEffect(() => {
    const loadNasdaqSymbols = async () => {
      const response = await fetch(nasdaqTxt);
      const textData = await response.text();
      const symbols = textData.split("\n").map((line) => line.trim());
      setNasdaqSymbols(symbols);
    };

    loadNasdaqSymbols();
  }, []);

  const updateBestMatches = () => {
    if (input) {
      // Girdi ile eşleşen sembolleri filtrele
      const filteredResults = nasdaqSymbols.filter((symbol) =>
        symbol.toLowerCase().startsWith(input.toLowerCase())
      );

      const formattedResults = filteredResults.map((symbol) => ({
        symbol,
        displaySymbol: symbol,
      }));

      setBestMatches(formattedResults);
    } else {
      setBestMatches([]);
    }
  };

  const clear = () => {
    setInput("");
    setBestMatches([]);
  };

  return (
    <div className="relative w-full max-w-lg mx-auto">
      <div className="flex items-center border-2 border-gray-300 rounded-lg shadow-md bg-white">
        <input
          type="text"
          value={input}
          onChange={(event) => setInput(event.target.value)}
          onKeyPress={(event) => {
            if (event.key === "Enter") {
              updateBestMatches();
            }
          }}
          placeholder="Search stocks..."
          className="w-full px-4 py-2 text-gray-700 focus:outline-none rounded-l-lg"
        />
        {input && (
          <button
            onClick={clear}
            className="p-2 bg-gray-200 hover:bg-gray-300 rounded-full transition duration-200"
          >
            <XIcon className="h-5 w-5 text-gray-500" />
          </button>
        )}
        <button
          onClick={updateBestMatches}
          className="p-2 bg-indigo-600 hover:bg-indigo-700 rounded-r-lg transition duration-200"
        >
          <SearchIcon className="h-5 w-5 text-white" />
        </button>
      </div>
      {input && bestMatches.length > 0 && (
        <SearchResults results={bestMatches} onClear={clear} />
      )}
    </div>
  );
};

export default Search;
