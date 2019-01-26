using BTC.Model;
using System;

namespace BTC.Importer
{
    class Program
    {
        public static void Main(string[] args)
        {
            var runner = new Runner();

            try
            {
                runner.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.Message}");
            }
        }
    }

    public class Runner
    {
        SocketListener _listener;
        EventPublisher _publisher;

        public void Start()
        {
            _listener = new SocketListener();
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
