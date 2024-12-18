const basePath = "https://finnhub.io/api/v1";

export const searchSymbol = async (query) => {
  const url = `${basePath}/search?q=${query}&token=${process.env.REACT_APP_API_KEY}`;
  const response = await fetch(url);

  if (!response.ok) {
    const message = `An error has occured: ${response.status}`;
    throw new Error(message);
  }
  console.log('Response:', response);
  return await response.json();
};

export const fetchStockDetails = async (stockSymbol) => {
  const url = `${basePath}/stock/profile2?symbol=${stockSymbol}&token=${process.env.REACT_APP_API_KEY}`;
  const response = await fetch(url);

  if (!response.ok) {
    const message = `An error has occured: ${response.status}`;
    throw new Error(message);
  }

  return await response.json();
};


export const fetchQuote = async (stockSymbol) => {
  const url = `${basePath}/quote?symbol=${stockSymbol}&token=${process.env.REACT_APP_API_KEY}`;
  const response = await fetch(url);

  if (!response.ok) {
    const message = `An error has occured: ${response.status}`;
    throw new Error(message);
  }

  return await response.json();
};


export const fetchHistoricalData = async (
  stockSymbol,
  resolution,
  from,
  to
) => {
  // Flask server endpoint URL
  const basePath = 'http://127.0.0.1:5000';  // Flask backend base URL
  
  // `from` tarihini 4 gün geri al
  const fromDate = new Date(from * 1000);  // `from` parametresi Unix timestamp (saniye cinsinden), onu milisaniyeye çevir
  fromDate.setDate(fromDate.getDate() - 4);  // `from` tarihini 4 gün geri al
  const fromTimestamp = Math.floor(fromDate.getTime() / 1000);  // Yeni `from` timestamp'i (Unix zaman damgası)

  // `to` tarihini 4 gün geri al
  const toDate = new Date();
  toDate.setDate(toDate.getDate() - 4);  // 4 gün geri git
  const toTimestamp = Math.floor(toDate.getTime() / 1000);  // `to` tarihini Unix timestamp (epoch)

  // URL'yi oluştur
  const url = `${basePath}/stock/candle?symbol=${stockSymbol}&resolution=${resolution}&from=${fromTimestamp}&to=${toTimestamp}`;
  
  try {
    const response = await fetch(url);

    //log response and request
    console.log('Request:', url);
    console.log('Response:', response);
    if (!response.ok) {
      const message = `An error has occurred: ${response.status}`;
      throw new Error(message);
    }

    // Veriyi JSON olarak döndürüyoruz
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching historical data:', error);
    throw error;
  }
};
