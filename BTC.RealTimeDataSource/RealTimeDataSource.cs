using AmiBroker;
using AmiBroker.Data;
using BTC.Model;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TinyIpc.Messaging;

namespace BTC.RealTimeDataSource
{
    /// <summary>
    /// Adapted with sample code from .Net for Amibroker http://www.dotnetforab.com/
    /// </summary>
    [ABDataSource("RealTime")]
    public class RealTimeDataSource : DataSourceBase
    {
        internal static Workspace Workspace;
        private string lastLongMessage;
        private int lastLongMessageTime;
        private TickerDataCollection tickers;
        private Periodicity periodicity;
        private bool firstGetQuotesExCall = true;
        private TinyMessageBus _tradeClient;
        private TinyMessageBus _tickClient;
        private BinaryFormatter _binForm = new BinaryFormatter();

        public RealTimeDataSource(string config) : base(config)
        {
            this.tickers = new TickerDataCollection();
        }

        public static new string Configure(string oldSettings, ref InfoSite infoSite)
        {
            var configuration = Configuration.GetConfigObject(oldSettings);

            return oldSettings;
        }

        public override void GetQuotesEx(string ticker, ref QuotationArray quotes)
        {
            if (firstGetQuotesExCall)
            {
                periodicity = quotes.Periodicity;
                firstGetQuotesExCall = false;
            }

            try
            {
                var tickerData = tickers.RegisterTicker(ticker);

                if (tickerData.QuoteDataStatus == QuoteDataStatus.Offline)
                {
                    tickerData.MarkTickerForGetQuotes(periodicity);
                    return;
                }

                if (tickerData.QuoteDataStatus != QuoteDataStatus.Online)
                    return;

                lock (tickerData)
                {
                    quotes.Merge(tickerData.Quotes);
                }
            }
            catch (Exception ex)
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Failed to subscribe to quote update: " + ex);
            }
        }

        public override void GetRecentInfo(string ticker)
        {
            try
            {
                var tickerData = tickers.RegisterTicker(ticker);

                tickerData.MarkTickerForRecentInfo();
            }
            catch (Exception ex)
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Failed to subscribe to real time window update: " + ex);
            }
        }

        public override AmiVar GetExtraData(string ticker, string name, Periodicity periodicity, int arraySize)
        {
            return new AmiVar(ATFloat.Null);
        }

        public override PluginStatus GetStatus()
        {
            var status = new PluginStatus();

            status.Status = StatusCode.OK;
            status.Color = System.Drawing.Color.ForestGreen;
            status.ShortMessage = "Ready";
            status.LongMessage = LogAndMessage.GetMessages();

            if (string.IsNullOrEmpty(status.LongMessage))
            {
                status.LongMessage = status.ShortMessage;
                lastLongMessage = status.ShortMessage;
            }

            if (lastLongMessage != status.LongMessage)
            {
                lastLongMessage = status.LongMessage;
                lastLongMessageTime = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
            }

            status.Status = (StatusCode)((int)status.Status + lastLongMessageTime);

            return status;
        }

        public override bool SetTimeBase(Periodicity timeBase)
        {
            return true;
        }

        public override int GetSymbolLimit()
        {
            return 20;
        }

        public override bool Notify(ref PluginNotification notifyData)
        {
            switch (notifyData.Reason)
            {
                case Reason.DatabaseLoaded:
                    Workspace = notifyData.Workspace;

                    _tradeClient = new TinyMessageBus("trades");
                    _tradeClient.MessageReceived += _tradeClient_MessageReceived;

                    _tickClient = new TinyMessageBus("ticks");
                    _tickClient.MessageReceived += _tickClient_MessageReceived;
                    break;

                case Reason.DatabaseUnloaded:
                    _tradeClient.Dispose();
                    _tickClient.Dispose();
                    break;

                case Reason.RightMouseClick:
                    break;

                default:
                    break;
            }

            return true;
        }

        private void _tickClient_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
        {
            var tick = (MarketTick)ByteArrayToObject(e.Message);
            var tickerData = tickers.GetTickerData(tick.ticker);

            try
            {
                tickerData.RecentInfo.Ask = tick.ask;
                tickerData.RecentInfo.Bid = tick.bid;
                tickerData.RecentInfo.Last = tick.last;
                tickerData.RecentInfo.TotalVol = tick.vol;
                tickerData.RecentInfo.TradeVol = tick.vol;

                var now = DateTime.Now;
                var lastTickDate = now.Year * 10000 + now.Month * 100 + now.Day;
                var lastTickTime = now.Hour * 10000 + now.Minute * 100 + now.Second;

                tickerData.RecentInfo.DateChange = lastTickDate;
                tickerData.RecentInfo.TimeChange = lastTickTime;
                tickerData.RecentInfo.DateUpdate = lastTickDate;
                tickerData.RecentInfo.TimeUpdate = lastTickTime;

                DataSourceBase.NotifyRecentInfoUpdate(tickerData.Ticker, ref tickerData.RecentInfo);
            }
            catch (Exception ex)
            {
                LogAndMessage.LogMessage(MessageType.Error, ex.Message);
            }
        }

        private void _tradeClient_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
        {
            var trade = (MarketTrade)ByteArrayToObject(e.Message);
            var tickerData = tickers.GetTickerData(trade.ticker);
            var result = false;

            try
            {
                var quote = new Quotation();

                lock (tickerData)
                {
                    if (tickerData.Quotes != null)
                        tickerData.Quotes.Clear();

                    quote = new Quotation();

                    var offset = DateTimeOffset.FromUnixTimeMilliseconds(trade.timestamp);

                    quote.DateTime = (AmiDate)offset.LocalDateTime;
                    quote.Price = trade.price;
                    quote.High = trade.price;
                    quote.Low = trade.price;
                    quote.Open = trade.price;
                    quote.Volume = trade.vol;

                    if (tickerData.Quotes != null)
                        tickerData.Quotes.Merge(quote);
                }

                result = true;
            }
            catch (Exception ex)
            {
                LogAndMessage.LogMessage(MessageType.Error, ex.Message);
            }
            finally
            {
                if (tickerData.QuoteDataStatus != QuoteDataStatus.Offline)
                    tickerData.QuoteDataStatus = result ? QuoteDataStatus.Online : QuoteDataStatus.Failed;
            }

            DataSourceBase.NotifyQuotesUpdate();
        }

        private Object ByteArrayToObject(byte[] arrBytes)
        {
            var memStream = new MemoryStream();

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)(_binForm.Deserialize(memStream));

            return obj;
        }
    }
}
