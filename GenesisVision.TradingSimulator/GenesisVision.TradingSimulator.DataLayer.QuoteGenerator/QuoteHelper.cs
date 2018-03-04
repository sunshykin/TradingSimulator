using System;
using System.Collections.Generic;
using System.Text;

namespace GenesisVision.TradingSimulator.DataLayer.QuoteGenerator
{
    public enum QuoteType
    {
        RUB_EUR = 0,
        EUR_USD,
        USD_CHG,
        USD_AUD,
        USD_GBP
    }

    public static class QuoteExtention
    {
        public static string GetName(this QuoteType type)
        {
            switch (type)
            {
                case QuoteType.RUB_EUR:
                    return "RUB_EUR";
                case QuoteType.EUR_USD:
                    return "EUR_USD";
                case QuoteType.USD_CHG:
                    return "USD_CHG";
                case QuoteType.USD_AUD:
                    return "USD_AUD";
                case QuoteType.USD_GBP:
                    return "USD_GBP";
                default:
                    return String.Empty;
            }
        }
    }
}
