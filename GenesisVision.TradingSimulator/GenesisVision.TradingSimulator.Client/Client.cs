using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GenesisVision.TradingSimulator.Client
{
    class Client
    {
        /// <summary>
        /// Клиент
        /// </summary>
        private TcpClient _client;

        /// <summary>
        /// Адрес сервера
        /// </summary>
        private string _host;

        /// <summary>
        /// Порт сервера
        /// </summary>
        private int _port;

        /// <summary>
        /// Состояние соединения с сервером
        /// </summary>
        private bool _isConnected;

        public Client()
        {
            _host = "localhost";
            _port = 3333;

            _client = new TcpClient();
            _client.Connect(_host, _port);

            _isConnected = true;

            HandleCommunication();

            while (_isConnected)
            {

            }
            Console.WriteLine("Вы отключились от сервера");
            Console.ReadKey();
        }

        public void HandleCommunication()
        {
            StreamReader sr = new StreamReader(_client.GetStream(), Encoding.ASCII);
            StreamWriter sw = new StreamWriter(_client.GetStream(), Encoding.ASCII);

            // Поток для передачи информации серверу
            Thread writerThread = new Thread(() =>
            {
                while (_isConnected)
                {
                    var writeData = Console.ReadLine();

                    sw.WriteLine(writeData);
                    sw.Flush();

                    if (writeData == "q")
                        _isConnected = false;
                }
            });
            writerThread.Start();

            // Поток для приема информации с сервера
            Thread readerThread = new Thread(() =>
            {
                while (_isConnected)
                {
                    var readData = sr.ReadLine();
                    Console.WriteLine(readData);
                }
            });
            readerThread.Start();
        }
    }
}
