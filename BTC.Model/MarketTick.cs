using System;

namespace BTC.Model
{
    [Serializable]
    public class MarketTick
    {
        public string ticker { get; set; }
        public Single bid { get; set; }
        public Single ask { get; set; }
        public Single last { get; set; }
        public long vol { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1} : bid = {2} ask = {3} last = {4} vol = {5}", DateTime.Now.ToLongTimeString(), ticker, bid.ToString(), ask.ToString(), last.ToString(), vol.ToString());
        }
    }
}
