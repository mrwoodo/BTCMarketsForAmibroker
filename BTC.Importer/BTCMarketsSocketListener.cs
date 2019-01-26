using BTC.Model;
using CsvHelper;
using Newtonsoft.Json;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;

namespace BTC.Importer
{
    /// <summary>
    /// For details on the BTC Markets Public Market API, refer to: https://github.com/BTCMarkets/API/wiki/Websocket
    /// </summary>
    public class BTCMarketsSocketListener
    {
        public event EventHandler<MarketTrade> OnTrade;
        public event EventHandler<MarketTick> OnTick;

        private Socket _socket;
        private string _fileName;

        private TextWriter _writer;
        private CsvWriter _csv;
        private static int count = 0;

        private string[] _pairs;

        public BTCMarketsSocketListener()
        {
            _fileName = "TICKER" + DateTime.Now.ToString("_yyyy_MM_dd_HHmmss") + ".txt";
            _pairs = ConfigurationManager.AppSettings["pairs"].Split(',');
        }

        public void Start()
        {
            _writer = new StreamWriter(_fileName);
            _csv = new CsvWriter(_writer);
            _csv.WriteHeader(typeof(MarketTrade));
            _csv.NextRecord();

            _socket = IO.Socket("wss://socket.btcmarkets.net", new IO.Options()
            {
                Secure = true,
                Transports = ImmutableList.Create<string>("websocket"),
                Upgrade = false
            });

            _socket.On(Socket.EVENT_CONNECT, () =>
            {
                foreach (var p in _pairs)
                {
                    _socket.Emit("join", "TRADE_" + p);
                    _socket.Emit("join", "Ticker-BTCMarkets-" + p.Substring(0, 3) + "-" + p.Substring(3));
                }
            });

            _socket.On(Socket.EVENT_DISCONNECT, () =>
            {
                Console.WriteLine("Socket event disconnent");
            });

            _socket.On(Socket.EVENT_ERROR, () =>
            {
                Console.WriteLine("Socket event error");
            });

            _socket.On("MarketTrade", (data) =>
            {
                var marketTrades = JsonConvert.DeserializeObject<MarketTrades>(data.ToString());
                var trades = marketTrades.getTrades();

                foreach (var t in trades)
                {
                    OnTrade(this, t);

                    _csv.WriteRecord(t);
                    _csv.NextRecord();

                    count++;
                    if (count > 100)
                    {
                        count = 0;
                        _csv.Flush();
                    }
                }
            });

            _socket.On("newTicker", (data) =>
            {
                var marketTicks = JsonConvert.DeserializeObject<MarketTicks>(data.ToString());

                OnTick(this, marketTicks.tick);
            });
        }

        public void Shutdown()
        {
            _socket.Close();
            _csv.Flush();
            _csv.Dispose();
            _writer.Close();
            _writer.Dispose();
        }
    }
}