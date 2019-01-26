using BTC.Model;
using System;

namespace BTC.Importer
{
    /// <summary>
    /// Refer to app.config for which BTCMarkets currency pairs we are listening for
    /// e.g. <add key="pairs" value="BTCAUD,XRPAUD" />
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            var importer = new Importer();

            try
            {
                importer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.Message}");
            }
        }
    }

    public class Importer
    {
        private BTCMarketsSocketListener _listener;
        private EventPublisher _publisher;

        public void Start()
        {
            _listener = new BTCMarketsSocketListener();
            _publisher = new EventPublisher();

            _listener.OnTrade += _listener_OnTrade;
            _listener.OnTick += _listener_OnTick;
            _listener.Start();

            ConsoleKeyInfo kb;
            do
            {
                kb = Console.ReadKey();
                if (kb.Key == ConsoleKey.Z) break;
            } while (true);

            _listener.Shutdown();
            _publisher.Shutdown();

            Console.WriteLine("Importer has shutdown");
        }

        private void _listener_OnTick(object sender, MarketTick e)
        {
            _publisher.SendTick(e);

            Console.WriteLine(e.ToString());
        }

        private void _listener_OnTrade(object sender, MarketTrade e)
        {
            _publisher.SendTrade(e);

            Console.WriteLine(e.ToString());
        }
    }
}
