using System;
using System.Collections.Generic;
using System.Text;
using GenesisVision.TradingSimulator.DataLayer.QuoteGenerator;

namespace GenesisVision.TradingSimulator.Server
{
    class Init
    {
        static void Main(string[] args)
        {
            var gen = new Generator();
            gen.Start();

            var serv = new Server(gen);
            serv.Start();
        }
    }
}
