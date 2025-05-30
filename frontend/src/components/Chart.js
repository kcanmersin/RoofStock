import React, { useEffect, useState ,useContext} from "react";
import ChartFilter from "./ChartFilter";
import Card from "./Card";
import { Area, XAxis, YAxis, ResponsiveContainer, AreaChart, Tooltip } from "recharts";
import StockContext from "../context/StockContext";
import { fetchHistoricalData } from "../utils/api/stock-api";
import { createDate, convertDateToUnixTimestamp, convertUnixTimestampToDate } from "../utils/helpers/date-helper";
import { chartConfig } from "../constants/config";

const Chart = () => {
  const [filter, setFilter] = useState("1W");
  const { stockSymbol } = useContext(StockContext);
  const [data, setData] = useState([]);

  const formatData = (data) => {
    if (!data || !Array.isArray(data)) {
      console.log("Invalid data format", data);
      return [];
    }

    return data.map((item, index) => {
      const dateStr = item.Datetime || item.Date;
      if (!dateStr) {
        console.warn(`Missing Datetime or Date field in item at index ${index}: ${JSON.stringify(item)}`);
        return { value: item.Close.toFixed(2), date: "Invalid Date" };
      }

      const dateObj = new Date(dateStr);
      if (isNaN(dateObj.getTime())) {
        console.warn(`Invalid Date format for item at index ${index}: ${JSON.stringify(item)}`);
        return { value: item.Close.toFixed(2), date: "Invalid Date" };
      }

      const unixTimestamp = Math.floor(dateObj.getTime() / 1000);
      const formattedDate = convertUnixTimestampToDate(unixTimestamp);
      return { value: item.Close.toFixed(2), date: formattedDate };
    });
  };

  useEffect(() => {
    const getDateRange = () => {
      const { days, weeks, months, years } = chartConfig[filter];

      const endDate = new Date();
      const startDate = createDate(endDate, -days, -weeks, -months, -years);

      const startTimestampUnix = convertDateToUnixTimestamp(startDate);
      const endTimestampUnix = convertDateToUnixTimestamp(endDate);
      return { startTimestampUnix, endTimestampUnix };
    };

    const updateChartData = async () => {
      try {
        const { startTimestampUnix, endTimestampUnix } = getDateRange();
        const resolution = chartConfig[filter].resolution;
        const result = await fetchHistoricalData(stockSymbol, resolution, startTimestampUnix, endTimestampUnix);
        console.log("API Response:", result);
        setData(formatData(result));
      } catch (error) {
        setData([]);
        console.log("Error fetching historical data:", error);
      }
    };

    updateChartData();
  }, [stockSymbol, filter]);

  return (
    <Card>
      <ul className="flex absolute top-2 right-2 z-40">
        {Object.keys(chartConfig).map((item) => (
          <li key={item}>
            <ChartFilter
              text={item}
              active={filter === item}
              onClick={() => {
                setFilter(item);
              }}
            />
          </li>
        ))}
      </ul>
      <ResponsiveContainer>
        <AreaChart data={data}>
          <defs>
            <linearGradient id="chartColor" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="rgb(199 210 254)" stopOpacity={0.8} />
              <stop offset="95%" stopColor="rgb(199 210 254)" stopOpacity={0} />
            </linearGradient>
          </defs>
          <Tooltip contentStyle={{ backgroundColor: "#111827" }} itemStyle={{ color: "#818cf8" }} />
          <Area type="monotone" dataKey="value" stroke="#312e81" fill="url(#chartColor)" fillOpacity={1} strokeWidth={0.5} />
          <XAxis dataKey="date" />
          <YAxis domain={["dataMin", "dataMax"]} />
        </AreaChart>
      </ResponsiveContainer>
    </Card>
  );
};

export default Chart;
