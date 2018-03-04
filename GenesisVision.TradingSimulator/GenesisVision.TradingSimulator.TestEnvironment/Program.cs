using System;
using System.Diagnostics;
using System.Threading;

namespace GenesisVision.TradingSimulator.TestEnvironment
{
    class Program
    {
        /// <summary>
        /// Количество клиентов для теста
        /// </summary>
        private static int _clientCount = 4;

        static void Main(string[] args)
        {
            string path = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName;
            string clientPath = path + @"\GenesisVision.TradingSimulator.Client\bin\Release\netcoreapp2.0\GenesisVision.TradingSimulator.Client.dll",
                serverPath = path + @"\GenesisVision.TradingSimulator.Server\bin\Release\netcoreapp2.0\GenesisVision.TradingSimulator.Server.dll";

            ProcessStartInfo server = new ProcessStartInfo("dotnet", serverPath),
                client = new ProcessStartInfo("dotnet", clientPath);
            server.UseShellExecute = true;
            client.UseShellExecute = true;

            // Запускаем сервер
            var servPrc = new Process();
            servPrc.StartInfo = server;
            servPrc.Start();
            Thread.Sleep(100);

            // Запускаем 4 клиента
            var clientPrc = new Process[_clientCount];
            for (int i = 0; i < _clientCount; i++)
            {
                clientPrc[i].StartInfo = client;
                clientPrc[i].Start();
                Thread.Sleep(300);
            }

            Console.WriteLine("1 Server and 4 Clients are started");
            Console.WriteLine("Press Any Key To Exit");
            Console.ReadKey();

            // Закрываем все процессы
            servPrc.Kill();
            for (int i = 0; i < _clientCount; i++)
                clientPrc[i].Kill();
        }
    }
}
