using System;
using DataDefineLanLib;

namespace ExpDbCp
{
    class Program
    {
        static void Main(string[] args)
        {
            var argS = string.Empty;
            var argD = string.Empty;
            var argU = string.Empty;
            var argP = string.Empty;
            var argO = string.Empty;

            if (args.Length != 5)
            {
                ShowHelp();
                return;
            }

            foreach (var arg in args)
            {
                if (arg.StartsWith("-S=")) argS = arg.Replace("-S=", "");
                if (arg.StartsWith("-d=")) argD = arg.Replace("-d=", "");
                if (arg.StartsWith("-U=")) argU = arg.Replace("-U=", "");
                if (arg.StartsWith("-P=")) argP = arg.Replace("-P=", "");
                if (arg.StartsWith("-O=")) argO = arg.Replace("-O=", "");
            }

            if (string.IsNullOrEmpty(argS)
                || string.IsNullOrEmpty(argD)
                || string.IsNullOrEmpty(argU)
                || string.IsNullOrEmpty(argP)
                || string.IsNullOrEmpty(argO))
            {
                ShowHelp();
                return;
            }

            //argS = "主機"; argD = "資料庫"; argU = "帳號"; argP = "密碼"; argO = "輸出路徑";

            SmoServer oGenerateDdl = new SmoServer(argS, argD, argU, argP);
            oGenerateDdl.ExportFile(argO);
        }

        /// <summary>
        /// 顯示幫助資訊
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine(string.Format(
                "{0}使用說明:{0}-S=主機名 -d=資料庫名 -U=帳號 -P=密碼 -O=輸出基礎路徑{0}例如:{0}ExportSqlDDL.exe -S=SQLDB -d=cc_db -U=access -P=12345678 -O=OutPut{0}請注意參數大小寫!!",
                Environment.NewLine));
        }
    }
}
