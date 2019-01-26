using BTC.Model;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TinyIpc.Messaging;

namespace BTC.Importer
{
    public class EventPublisher
    {
        TinyMessageBus _trades;
        TinyMessageBus _ticks;
        BinaryFormatter _bf = new BinaryFormatter();

        public EventPublisher()
        {
            _trades = new TinyMessageBus("trades");
            _ticks = new TinyMessageBus("ticks");
        }

        public void SendTrade(MarketTrade trade)
        {
            _trades.PublishAsync(ObjectToByteArray(trade));
        }

        public void SendTick(MarketTick tick)
        {
            _ticks.PublishAsync(ObjectToByteArray(tick));
        }

        public void Shutdown()
        {
            _trades.Dispose();
            _ticks.Dispose();
        }

        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            var ms = new MemoryStream();
            _bf.Serialize(ms, obj);

            return ms.ToArray();
        }
    }
}
