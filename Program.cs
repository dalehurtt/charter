using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

using Rop;

namespace Charts {

    public static class Program {
        private static string bakFile;
        private static string csvFile;
        private static string dirPath;
        private static string floatFile;
        private static string pointFile;
        private static string reportFile;
        private static string scaleFile;
        private static string [] tickers;

        public static string currentTicker = string.Empty;

        public static void Main (string[] args) {
            ProcessMultipleTickers ();
        }

        private static List<DailyFloatData> AddToReport (this List<DailyFloatData> values, ref DailyReport report) {
            DailyFloatData last = values.Last ();
            report.Buy = last.High;
            report.FloatSignal = last.Signal;
            report.Sell = last.Low;
            return values;
        }

        private static List<DailyPointData> AddToReport (this List<DailyPointData> values, ref DailyReport report) {
            DailyPointData last = values.Last ();
            report.Close = last.Close;
            report.Date = last.Date;
            report.HighLow = last.HighLow;
            report.PointSignal = last.Signal;
            report.Reversal = last.Target;
            return values;
        }

        private static Result<string, Exception> IsValidFile (this string filePath) {
            try {
                string dirName = string.Empty, fileName = string.Empty, fullPath = string.Empty;
                FileInfo fi = new FileInfo (filePath);
                if (fi.Exists) {
                    return Result<string, Exception>.Succeeded (filePath);
                }
                else
                    throw new Exception ($"{filePath} does not exist.");
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "IsValidFile");
                return Result<string, Exception>.Failed (ex);
            }
        }

        public static List<DailyReport> OutputReportAsCsv (this List<DailyReport> values, string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine ("Ticker,Date,Close,Point Signal,High/Low,Reversal,Float Signal,Buy,Sell");
                foreach (DailyReport value in values) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {Program.currentTicker}", true, "OutputReportAsCsv");
            }
            return values;
        }

        private static void ProcessMultipleTickers () {
            try {
                var appSettings = ConfigurationManager.AppSettings;
                if (System.Environment.OSVersion.Platform == PlatformID.Unix) {
                    dirPath = appSettings ["dirpathmac"];
                }
                else {
                    dirPath = appSettings ["dirpath"];
                }
                string tickersValue = appSettings ["tickers"];
                tickers = tickersValue.Split (',');
                // tickets = new string [] { 'ABBV' };

                List<DailyReport> reports = new List<DailyReport> (tickers.Length);
                reportFile = $"{dirPath}{DateTime.Now:yyyy-MM-dd}-report.csv";

                foreach (string ticker in tickers) {
                    currentTicker = ticker;
                    DailyReport report = new DailyReport ();
                    report.Ticker = ticker;

                    csvFile = $"{dirPath}{ticker}.csv".IsValidFile ().Success;
                    bakFile = $"{dirPath}{ticker}-bak.csv";
                    floatFile = $"{dirPath}{ticker}-float.csv";
                    pointFile = $"{dirPath}{ticker}-point.csv";
                    scaleFile = $"{dirPath}{ticker}-scale.csv";

                    List<DailyStockData> stockValues = csvFile.ReadStockFile ().Success;

                    long flt = Convert.ToInt64 (appSettings [ticker]);
                    DailyFloatList flist = new DailyFloatList (stockValues, flt);
                    flist.OutputAsCsv (floatFile);
                    flist.AddToReport (ref report);

                    DailyPointList plist = new DailyPointList (stockValues);
                    plist.OutputAsCsv (pointFile);
                    plist.AddToReport (ref report);

                    reports.Add (report);
                }
                reports.OutputReportAsCsv (reportFile);
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {currentTicker}", true, "Main");
            }
        }

        private static Result<List<DailyStockData>, Exception> ReadStockFile (this string inputFile) {
            try {
                return Result <List<DailyStockData>, Exception>.Succeeded (
                    File.ReadAllLines (inputFile)
                    .Skip (1)
                    .Select (v => DailyStockData.FromCsv (v))
                    .ToList ());
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} { currentTicker}", true, "ReadStockFile");
                return Result<List<DailyStockData>, Exception>.Failed (ex);
            }
        }
    }
}
