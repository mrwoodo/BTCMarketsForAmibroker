using System;

namespace BTC.Model
{
    [Serializable]
    public class MarketTicks
    {
        public long volume24h { get; set; }
        public long bestBid { get; set; }
        public long bestAsk { get; set; }
        public long lastPrice { get; set; }
        public long timestamp { get; set; }
        public long snapshotId { get; set; }
        public int marketId { get; set; }
        public string currency { get; set; }
        public string instrument { get; set; }

        public MarketTick tick
        {
            get
            {
                var t = new MarketTick();

                t.ticker = this.instrument + this.currency;
                t.ask = Convert.ToSingle(this.bestAsk) / 100000000;
                t.bid = Convert.ToSingle(this.bestBid) / 100000000;
                t.last = Convert.ToSingle(this.lastPrice) / 100000000;
                t.vol = this.volume24h / 100000000;

                return t;
            }
        }
    }
}
