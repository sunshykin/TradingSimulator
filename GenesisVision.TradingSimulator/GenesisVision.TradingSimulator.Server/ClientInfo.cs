using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenesisVision.TradingSimulator.Server
{
    public class ClientInfo
    {
        /// <summary>
        /// Уникальный идентификатор клиента
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Поток для передачи информации клиенту
        /// </summary>
        public StreamWriter Stream { get; private set; }

        /// <summary>
        /// Тип отображения котировок у клиента
        /// </summary>
        public DisplayInformationType DisplayType { get; set; }

        public ClientInfo(Guid guid, StreamWriter sr, DisplayInformationType dt)
        {
            Guid = guid;
            Stream = sr;
            DisplayType = dt;
        }
    }
}
