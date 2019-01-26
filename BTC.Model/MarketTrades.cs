using System;
using System.Collections.Generic;

namespace BTC.Model
{
    public class MarketTrades
    {
        public int id { get; set; }
        public long timestamp { get; set; }
        public int marketId { get; set; }
        public string agency { get; set; }
        public string instrument { get; set; }
        public string currency { get; set; }
        public List<List<long>> trades { get; set; }

        public List<MarketTrade> getTrades()
        {
            var result = new List<MarketTrade>();

            foreach (var t in trades)
            {
                var m = new MarketTrade();

                m.ticker = this.instrument + this.currency;
                m.timestamp = t[0];
                m.price = Convert.ToSingle(t[1]) / 100000000;
                m.vol = Convert.ToSingle(t[2]) / 100000000;

                result.Add(m);
            }

            return result;
        }
    }
}
