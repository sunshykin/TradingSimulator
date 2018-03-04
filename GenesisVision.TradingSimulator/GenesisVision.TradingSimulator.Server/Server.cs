using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GenesisVision.TradingSimulator.DataLayer.QuoteGenerator;

namespace GenesisVision.TradingSimulator.Server
{
    class Server
    {
        #region Properties

        /// <summary>
        /// Время между тиками таймера
        /// </summary>
        private int _timeDelay;

        /// <summary>
        /// Словарь с ключем "название котировки" и значением состоящим из
        /// значения котировки и маркера обновлена ли она
        /// </summary>
        private Dictionary<string, Tuple<double, bool>> _quotes;

        /// <summary>
        /// Генератор значений котировок
        /// </summary>
        private Generator _generator;

        /// <summary>
        /// Лисенер подключений
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// Запущен ли сервер
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Порт сервера
        /// </summary>
        private int _port;

        /// <summary>
        /// Список клиентов, подключенных к серверу
        /// </summary>
        private List<ClientInfo> _clientList;

        #endregion

        public Server(Generator gen)
        {
            _timeDelay = 2000;
            _generator = gen;
            _quotes = new Dictionary<string, Tuple<double, bool>>();
            _clientList = new List<ClientInfo>();
        }
        
        /// <summary>
        /// Запуск сервера
        /// </summary>
        public void Start()
        {
            _port = 3333;

            #region Start Listener
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            _isRunning = true;
            #endregion

            #region Start reading generated information

            Thread genCheckThread = new Thread(CheckGenerator);
            genCheckThread.Start();

            #endregion

            #region Start Timer

            Timer timer = new Timer(TimerTick, null, _timeDelay, _timeDelay);

            #endregion

            // Ожидаем клиентов
            WaitClients();
        }

        #region Generator methods

        /// <summary>
        /// Проверка информации, поступающей с генератора и ее обработка
        /// </summary>
        private void CheckGenerator()
        {
            while (true)
            {
                if (_generator.Updated)
                {
                    var data = _generator.GeneratedValue;
                    lock (_quotes)
                    {
                        if (_quotes.ContainsKey(data.Item1))
                        {
                            _quotes[data.Item1] = new Tuple<double, bool>(data.Item2, true);
                        }
                        else
                        {
                            _quotes.Add(data.Item1, new Tuple<double, bool>(data.Item2, true));
                        }
                    }
                }
            }
        }

        #endregion

        #region Timer methods

        /// <summary>
        /// Обновление словаря. 
        /// Устанавливает все маркеры обновления котировок равными false
        /// </summary>
        private void UpdateDict()
        {
            lock (_quotes)
            {
                _quotes = _quotes.ToDictionary(p => p.Key, p => new Tuple<double, bool>(p.Value.Item1, false));
            }
        }

        /// <summary>
        /// Действие, происходящее по тику таймера
        /// </summary>
        /// <param name="state"></param>
        private void TimerTick(object state)
        {
            lock (_quotes)
            {
                var changed = _quotes.Where(q => q.Value.Item2);
                var info = changed.Select(q => String.Format("{0}={1:F2}", q.Key, q.Value.Item1));

                // Отправляем каждому клиенту
                lock (_clientList)
                {
                    var disconnected = new List<Guid>();
                    foreach (var c in _clientList)
                    {
                        try
                        {
                            SendToClient(c, info);
                        }
                        catch (Exception e)
                        {
                            disconnected.Add(c.Guid);
                        }
                    }

                    if (disconnected.Count > 0)
                        _clientList.RemoveAll(c => disconnected.Contains(c.Guid));
                }
            }

            UpdateDict();
        }

        #endregion

        #region TCP Server methods

        /// <summary>
        /// Ожидает подключения клиентов
        /// </summary>
        public void WaitClients()
        {
            while (_isRunning)
            {
                // Ожидаем подключение клиента
                TcpClient client = _listener.AcceptTcpClient();

                // Когда клиент найден, создаем поток для работы с ним
                Thread t = new Thread(HandleClient);
                t.Start(client);
            }
        }

        /// <summary>
        /// Обработчик запросов клиента
        /// </summary>
        /// <param name="obj"></param>
        public void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            var stream = client.GetStream();
            StreamWriter sWriter = new StreamWriter(stream, Encoding.ASCII);
            StreamReader sReader = new StreamReader(stream, Encoding.ASCII);

            ClientInfo cInfo = new ClientInfo(Guid.NewGuid(), sWriter, DisplayInformationType.Row);

            // Добавляем клиента в список
            lock (_clientList)
            {
                _clientList.Add(cInfo);
                Console.WriteLine("Клиент {0} подключился", cInfo.Guid);
            }

            // Отправляем клиенту информацию о всех котировках на данный момент
            lock (_quotes)
            {
                SendToClient(cInfo, _quotes.Select(q => String.Format("{0}={1:F2}", q.Key, q.Value.Item1)));
            }

            bool isConnected = true;

            while (isConnected)
            {
                try
                {
                    string command = sReader.ReadLine();

                    switch (command)
                    {
                        case "r":
                            cInfo.DisplayType = DisplayInformationType.Row;
                            break;
                        case "c":
                            cInfo.DisplayType = DisplayInformationType.Column;
                            break;
                        case "q":
                            lock (_clientList)
                            {
                                _clientList.Remove(cInfo);
                                cInfo.Stream.Dispose();
                                Console.WriteLine("Клиент {0} отключился", cInfo.Guid);
                            }

                            isConnected = false;
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Клиент {0} отключился", cInfo.Guid);
                    return;
                }
            }
        }

        /// <summary>
        /// Отправка информации о котировках клиенту
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="quotes">Информация о котировках формата "{котировка}={значение}"</param>
        private void SendToClient(ClientInfo client, IEnumerable<string> quotes)
        {
            client.Stream.WriteLine(String.Join(client.DisplayType == DisplayInformationType.Row ? ';' : '\n', quotes));
            client.Stream.WriteLine();
            client.Stream.Flush();
        }

        #endregion


    }
}