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
        private static string emaFile;
        private static string floatFile;
        private static string pointFile;
        private static string reportFile;
        private static string scaleFile;
        private static string volFile;
        private static string [] tickers;

        public static string currentTicker = string.Empty;

        public static void Main (string[] args) {
            ProcessMultipleTickers ();
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

        private static void ProcessMultipleTickers () {
            int maNumDays1 = 20, maNumDays2 = 120;
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

                DailyReport report = new (maNumDays1, maNumDays2);
                reportFile = $"{dirPath}{DateTime.Now:yyyy-MM-dd}-report.csv";

                foreach (string ticker in tickers) {
                    currentTicker = ticker;
                    DailyReportData data = new ();
                    data.Ticker = ticker;

                    csvFile = $"{dirPath}{ticker}.csv".IsValidFile ().Success;
                    bakFile = $"{dirPath}{ticker}-bak.csv";
                    floatFile = $"{dirPath}{ticker}-float.csv";
                    pointFile = $"{dirPath}{ticker}-point.csv";
                    scaleFile = $"{dirPath}{ticker}-scale.csv";
                    emaFile = $"{dirPath}{ticker}-ema.csv";
                    volFile = $"{dirPath}{ticker}-vol.csv";

                    List<DailyStockData> stockValues = csvFile.ReadStockFile ().Success;

                    long flt = Convert.ToInt64 (appSettings [ticker]);
                    DailyFloatList flist = new DailyFloatList (stockValues, flt);
                    flist.OutputAsCsv (floatFile);
                    data.AddToReport (flist);

                    DailyPointList plist = new DailyPointList (stockValues);
                    plist.OutputAsCsv (pointFile);
                    data.AddToReport (plist);

                    ExponentialAverageList emaList = new(stockValues, ticker, maNumDays1, maNumDays2);
                    emaList.OutputAsCsv(emaFile);
                    data.AddToReport(emaList);

                    DailyVolumeList dvlist = new (stockValues, ticker);
                    AverageVolumeList vlist = new AverageVolumeList(dvlist, ticker, maNumDays1);
                    vlist.OutputAsCsv(volFile);
                    data.AddToReport(vlist);

                    report.Add (data);
                }
                report.OutputAsCsv (reportFile);
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
