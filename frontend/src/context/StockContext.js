// src/context/StockContext.js
import { createContext, useContext } from 'react';

const StockContext = createContext();

export const useStock = () => {
  return useContext(StockContext);
};

export default StockContext;
