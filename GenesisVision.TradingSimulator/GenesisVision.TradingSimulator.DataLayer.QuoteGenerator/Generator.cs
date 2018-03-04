using System;
using System.Threading;
using System.Threading.Tasks;

namespace GenesisVision.TradingSimulator.DataLayer.QuoteGenerator
{
    public class Generator
    {
        #region Properties

        /// <summary>
        /// Время между тиками таймера
        /// </summary>
        private int _timeDelay;

        /// <summary>
        /// Рандомайзер
        /// </summary>
        private Random _randomizer;

        /// <summary>
        /// Состояние выдаваемого значения
        /// </summary>
        public bool Updated { get; set; }

        private Tuple<string, double> _generatedValue;

        /// <summary>
        /// Выдаваемое значение
        /// </summary>
        public Tuple<string, double> GeneratedValue
        {
            get
            {
                Updated = false;
                return _generatedValue;
            }
            set { _generatedValue = value; }
        }

        #endregion
        
        public Generator()
        {
            _timeDelay = 600;
            _randomizer = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            Updated = false;
            GeneratedValue = new Tuple<string, double>(String.Empty, 0);
        }

        #region Methods

        public void Start()
        {
            Timer timer = new Timer(TimerTick, null, 0, _timeDelay);
        }

        private void TimerTick(object state)
        {
            _generatedValue = new Tuple<string, double>(RandomizeQuote(), RandomizeValue());
            Updated = true;
        }

        private double RandomizeValue()
        {
            return Math.Round(_randomizer.NextDouble() * 10, 2);
        }

        private string RandomizeQuote()
        {
            return ((QuoteType)_randomizer.Next(0, 5)).GetName();
        }

        #endregion
    }
}
