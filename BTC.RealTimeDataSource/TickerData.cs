using AmiBroker.Data;
using System.ComponentModel;

namespace BTC.RealTimeDataSource
{
    public enum QuoteDataStatus : int
    {
        [Description("Offline")]
        Offline,
        [Description("Failed")]
        Failed,
        [Description("New")]
        New,
        [Description("Online")]
        Online
    }

    public class TickerData
    {
        public string Ticker;
        public QuoteDataStatus QuoteDataStatus;
        internal QuotationList Quotes;
        public RecentInfo RecentInfo;

        public TickerData(string ticker)
        {
            Ticker = ticker;
            QuoteDataStatus = QuoteDataStatus.Offline;
        }

        internal void MarkTickerForGetQuotes(Periodicity periodicity)
        {
            lock (this)
            {
                Quotes = new QuotationList(periodicity);
                QuoteDataStatus = QuoteDataStatus.New;
            }
        }

        internal void MarkTickerForRecentInfo()
        {
            lock (this)
            {
                RecentInfo = new RecentInfo();
                RecentInfo.Bitmap = RecentInfoField.Last |
                    RecentInfoField.Bid |
                    RecentInfoField.Ask |
                    RecentInfoField.TradeVol |
                    RecentInfoField.TotalVol |
                    RecentInfoField.DateChange |
                    RecentInfoField.DateUpdate;
            }
        }
    }
}