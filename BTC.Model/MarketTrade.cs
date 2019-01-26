using System;

namespace BTC.Model
{
    [Serializable]
    public struct MarketTrade
    {
        public string ticker { get; set; }
        public long timestamp { get; set; }
        public Single price { get; set; }
        public Single vol { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1} : {2} @ {3}", DateTime.Now.ToLongTimeString(), ticker, vol.ToString(), price.ToString());
        }
    }
}
