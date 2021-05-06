using System;
using System.Collections.Generic;
using System.IO;

namespace Charts {

    public class DailyReport {

        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public decimal Close { get; set; }
        public PointSignal PointSignal { get; set; }
        public decimal HighLow { get; set; }
        public decimal Reversal { get; set; }
        public FloatSignal FloatSignal { get; set; }
        public decimal Buy { get; set; }
        public decimal Sell { get; set; }

        public string ToCsv () {
            return $"{Ticker},{Date:yyyy-MM-dd},{Close:F2},{PointSignal},{HighLow:F2},{Reversal:F2},{FloatSignal},{Buy:F2},{Sell:F2}";
        }
    }
}
