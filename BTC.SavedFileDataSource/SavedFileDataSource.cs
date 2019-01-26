using AmiBroker.Data;
using AmiBroker.Utils.Data.DataSourceOffline;
using BTC.Model;
using CsvHelper;
using System;
using System.IO;

namespace BTC.SavedFileDataSource
{
    /// <summary>
    /// Adapted with sample code from .Net for Amibroker http://www.dotnetforab.com/
    /// </summary>
    [ABDataSource("Saved File")]
    internal class SavedFileDataSource : DataSourceOffline
    {
        private PluginStatus pluginStatus = new PluginStatus(StatusCode.OK, System.Drawing.Color.Green, "OK", "OK");

        public SavedFileDataSource(string settings) : base(settings)
        {
        }

        public override void Ticker_GetQuotes(TickerData tickerData)
        {
            var dataPath = @"C:\Users\Chris\OneDrive\trading\BTC XRP ticker data\"; //TODO: Make configurable
            var dataFiles = Directory.GetFiles(dataPath, "*.txt");

            tickerData.Quotes.Clear();

            for (int i = 0; i < dataFiles.Length; i++)
            {
                using (var sr = new StreamReader(dataFiles[i]))
                {
                    var reader = new CsvReader(sr);
                    var records = reader.GetRecords<MarketTrade>();

                    foreach (var t in records)
                    {
                        try
                        {
                            if (tickerData.Ticker.Equals(t.ticker, StringComparison.CurrentCultureIgnoreCase))
                            {
                                var offset = DateTimeOffset.FromUnixTimeMilliseconds(t.timestamp);
                                var quote = new Quotation
                                {
                                    DateTime = (AmiDate)offset.LocalDateTime,
                                    Open = t.price,
                                    High = t.price,
                                    Low = t.price,
                                    Price = t.price,
                                    Volume = t.vol
                                };

                                tickerData.Quotes.Merge(quote);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public override void Ticker_Ready(TickerData tickerData)
        {
            if (tickerData.Quotes != null)
                tickerData.Quotes.Clear();
        }

        public override PluginStatus GetStatus()
        {
            return pluginStatus;
        }

        public override bool SetTimeBase(Periodicity timeBase)
        {
            return true;
        }
    }
}