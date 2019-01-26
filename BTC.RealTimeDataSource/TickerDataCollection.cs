using System.Collections.Generic;

namespace BTC.RealTimeDataSource
{
    internal class TickerDataCollection
    {
        private SortedDictionary<string, TickerData> mapTickerTickerData;

        internal TickerDataCollection()
        {
            mapTickerTickerData = new SortedDictionary<string, TickerData>();
        }

        internal TickerData RegisterTicker(string ticker)
        {
            lock (mapTickerTickerData)
            {
                TickerData tickerData;

                if (mapTickerTickerData.TryGetValue(ticker, out tickerData))
                    return tickerData;

                tickerData = new TickerData(ticker);

                mapTickerTickerData.Add(ticker, tickerData);

                return tickerData;
            }
        }

        internal TickerData GetTickerData(string ticker)
        {
            TickerData result;

            lock (mapTickerTickerData)
            {
                mapTickerTickerData.TryGetValue(ticker, out result);
            }

            return result;
        }

        internal string[] GetAllTickers()
        {
            lock (mapTickerTickerData)
            {
                string[] result = new string[mapTickerTickerData.Count];

                mapTickerTickerData.Keys.CopyTo(result, 0);

                return result;
            }
        }
    }
}