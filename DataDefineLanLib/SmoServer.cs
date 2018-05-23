using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DataDefineLanLib
{
    /// <summary>
    /// Class SmoServer.
    /// </summary>
    public class SmoServer
    {
        /// <summary>
        /// SmoDB DB SMO物件
        /// </summary>
        private Database _smoDb;

        /// <summary>
        /// 預設呼叫SMO產生Scripting的選項設定.
        /// </summary>
        private ScriptingOptions _defScriptingOptions = new ScriptingOptions();

        private SmoServer()
        {
            _defScriptingOptions.ExtendedProperties = true;
            _defScriptingOptions.Indexes = true;
            _defScriptingOptions.Triggers = true;
            _defScriptingOptions.DriAll = true;
            _defScriptingOptions.ScriptSchema = true;
        }

        /// <summary>
        /// GenerateDDL 建構子
        /// </summary>
        /// <param name="host">主機位址</param>
        /// <param name="db">資料庫名稱</param>
        /// <param name="id">驗證帳號</param>
        /// <param name="password">驗證密碼</param>
        public SmoServer(string host, string db, string id, string password)
            : this()
        {
            string databaseConnectionString = $"Data Source={host};Initial Catalog={db};User ID={id};Password={password}";

            //一個資料庫連接字串
            //ServerConnection conn = new ServerConnection(new System.Data.SqlClient.SqlConnection(smoConnStr));
            ServerConnection srvConn = new ServerConnection
            {
                ServerInstance = host,
                LoginSecure = false,
                Login = id,
                Password = password,
                DatabaseName = db
            };
            Server srv = new Server(srvConn);
            _smoDb = srv.Databases[db];
        }

        /// <summary>
        /// ObjList 抓取出來的物件定義清單.
        /// </summary>
        public Dictionary<DbObjectTypeList, List<DataDefineObject>> ObjList = new Dictionary<DbObjectTypeList, List<DataDefineObject>>() {
            { DbObjectTypeList.Function , new List<DataDefineObject>() } ,
            { DbObjectTypeList.StoredProcedure , new List<DataDefineObject>() } ,
            { DbObjectTypeList.Table , new List<DataDefineObject>() } ,
            { DbObjectTypeList.MarkDown , new List<DataDefineObject>() } ,
            { DbObjectTypeList.View , new List<DataDefineObject>() }
        };

        /// <summary>
        /// NoDescList 資料表註解資訊不完整資料
        /// </summary>
        public List<string> NoDescList = new List<string>();

        /// <summary>
        /// ReadMe 資料
        /// </summary>
        public List<string> MakrDownReadMe = new List<string>();

        /// <summary>
        /// GetDefinition 取得資料庫所有物件相關定義
        /// </summary>
        public void GetDefinition()
        {
            //清除舊資料.
            foreach (var key in ObjList.Keys) ObjList[key].Clear();
            NoDescList.Clear();

            GetViewsDefinition();
            GetStoredProceduresDefinition();
            GetFunctionDefinition();
            GetTableDefinition();
        }

        /// <summary>
        /// 儲存檔案
        /// </summary>
        /// <param name="basePath">匯出檔案路徑</param>
        public void ExportFile(string basePath)
        {
            GetDefinition();

            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

            foreach (var file in new DirectoryInfo(basePath).GetFiles("*.sql", SearchOption.AllDirectories))
            {
                file.Delete();
            }

            var summary = new StringBuilder("轉出資料統計").AppendLine();

            foreach (var objectType in ObjList.Keys)
            {
                var filePath = basePath + "\\" + objectType.ToString();

                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                summary.AppendFormat($"\t{objectType}: {ObjList[objectType].Count} 個").AppendLine();
                var ext = objectType == DbObjectTypeList.MarkDown ? ".md" : ".sql";
                foreach (var ddo in ObjList[objectType])
                {
                    File.WriteAllText(filePath + "\\" + ddo.ObjectName + ext, ddo.Objectddl.ToString(), new UTF8Encoding(objectType != DbObjectTypeList.MarkDown));
                }

            }

            summary.AppendLine($"尚有 {NoDescList.Count} 個 Table 資料描述尚不完整 詳細清單如下");
            summary.AppendLine("--------------------------------------------");
            foreach (string s in NoDescList) summary.AppendFormat("\t{0}", s).AppendLine();

            File.WriteAllText($"{basePath}\\摘要資訊.txt", summary.ToString(), new UTF8Encoding(false));
            File.WriteAllText($"{basePath}\\ReadMe.md", $"| 表單名稱 | 表單說明 | 補充 |{Environment.NewLine}|---|---|---|{Environment.NewLine}", new UTF8Encoding(false));
            File.AppendAllLines($"{basePath}\\ReadMe.md", MakrDownReadMe);
        }

        /// <summary>
        /// 取得資料庫 View 相關資訊
        /// </summary>
        private void GetViewsDefinition()
        {
            foreach (View dbview in _smoDb.Views)
            {
                if (dbview.IsSystemObject) continue;
                var objectName = dbview.Schema + "." + dbview.Name;
                ObjList[DbObjectTypeList.View].Add(new DataDefineObject(objectName, dbview.Script()));
                ShowProMsg(objectName + " -->> 處理中...");
            }
        }

        /// <summary>
        /// 取得資料庫 StoredProcedure 相關資訊
        /// </summary>
        private void GetStoredProceduresDefinition()
        {
            foreach (StoredProcedure dbsp in _smoDb.StoredProcedures)
            {
                if (dbsp.IsSystemObject) continue;
                var objectName = dbsp.Schema + "." + dbsp.Name;
                ObjList[DbObjectTypeList.StoredProcedure].Add(new DataDefineObject(objectName, dbsp.Script()));
                ShowProMsg(objectName + " -->> 處理中...");
            }
        }

        /// <summary>
        /// 取得資料庫 Function 相關資訊
        /// </summary>
        private void GetFunctionDefinition()
        {
            foreach (UserDefinedFunction dbfn in _smoDb.UserDefinedFunctions)
            {
                if (dbfn.IsSystemObject) continue;
                var objectName = dbfn.Schema + "." + dbfn.Name;
                ObjList[DbObjectTypeList.Function].Add(new DataDefineObject(objectName, dbfn.Script()));
                ShowProMsg(objectName + " -->> 處理中...");
            }
        }

        /// <summary>
        /// GetTableDefinition 取得資料庫 Table 相關資訊
        /// </summary>
        private void GetTableDefinition()
        {
            foreach (Table table in _smoDb.Tables)
            {
                if (table.IsSystemObject) continue;
                var objectName = table.Schema + "." + table.Name;
                var desc = table.Script(this._defScriptingOptions);
                var markDown = new StringCollection();

                GetTableSummary(table, out var txtSummary, out var txtMarkDown);
                desc.Insert(0, txtSummary);
                ObjList[DbObjectTypeList.Table].Add(new DataDefineObject(objectName, desc));
                markDown.Add(txtMarkDown);
                ObjList[DbObjectTypeList.MarkDown].Add(new DataDefineObject(objectName, markDown));
            }
        }

        /// <summary>
        /// GetTableSummary 取得資料表摘要資訊
        /// </summary>
        /// <param name="table">摘要資訊描述</param>
        /// <param name="txtSummary">The text summary.</param>
        /// <param name="txtMarkDown">The text mark down.</param>
        private void GetTableSummary(Table table, out string txtSummary, out string txtMarkDown)
        {
            // tableDescription 資料表註解說明
            string tableDescription = "";

            StringBuilder oSb = new StringBuilder();
            StringBuilder markDown = new StringBuilder();

            // 是否缺表單說明
            Boolean isNoTableDesc = false;

            // 缺說明的欄位個數
            int columnNoDescCount = 0;

            tableDescription = GetExtendedProperties(table.ExtendedProperties, "MS_Description", "Memo");
            isNoTableDesc = string.IsNullOrEmpty(tableDescription);

            // write header
            oSb.AppendLine(string.Format(
                "/* ================================================================================================{0}資料表名稱:{1}.{2}{0}資料表說明:{3}{0}---------------------------------------------------------------------------------------------------",
                Environment.NewLine, table.Schema, table.Name, tableDescription));
            // write markdown header
            markDown.AppendLine(string.Format(
                @"{0}* **資料表名稱** {1}.{2}{0}* **資料表說明** {3}{0}* [回清單](../ReadMe.md){0}{0}---{0}{0}| 欄位名稱 | 型態 | PK | 是否Null | 說明 |{0}|---|---|---|---|---|",
                Environment.NewLine, table.Schema, table.Name,
                tableDescription.Replace("\r", " ").Replace("\n", " ").Trim()));

            foreach (Column col in table.Columns)
            {
                var dataType = string.Format("{0}({1})", col.DataType.Name, col.DataType.MaximumLength <= -1 ? "max" : col.DataType.MaximumLength.ToString());
                var isNull = col.Nullable ? "NULL" : "NOT NULL";

                // msDescription 註解說明
                var msDescription = GetExtendedProperties(col.ExtendedProperties, "MS_Description", "Memo").Replace("\r", " ").Replace("\n", " ").Trim();

                if (string.IsNullOrEmpty(msDescription)) columnNoDescCount++;

                oSb.AppendFormat("{0,-60}{1,-25}{2,-10}{3}", col.Name, dataType, isNull, msDescription).AppendLine();

                msDescription = col.EnumForeignKeys().Rows.Cast<DataRow>().Aggregate(msDescription, (current, row) => current + string.Format("<br/>[{0}.{1}]({0}.{1}.md)", row[0], row[1]));

                markDown.AppendFormat("| {0} | {1} | {2} | {3} | {4} |",
                    col.Name, dataType,
                    col.InPrimaryKey ? "v" : "",
                    isNull, msDescription).AppendLine();
            }

            oSb.AppendLine("================================================================================================ */");

            string msg = string.Format("{0,-60} -->> 欄位共{1,3}個{2}{3}",
                    table.Schema + "." + table.Name.Trim(),
                    table.Columns.Count,
                    (columnNoDescCount > 0) ? string.Format("、{0,3}個未填說明", columnNoDescCount) : "",
                    isNoTableDesc ? "、資料表說明未填寫" : "");

            ShowProMsg(msg);

            if (isNoTableDesc || columnNoDescCount > 0) NoDescList.Add(msg);
            txtSummary = oSb.ToString();

            MakrDownReadMe.Add(string.Format(
                "| [{0}]({1}/{0}.md) | {2} | {3} |",
                table.Schema + "." + table.Name,
                DbObjectTypeList.MarkDown.ToString(),
                tableDescription,
                string.Format("欄位共{0,3}個{1}{2}",
                    table.Columns.Count,
                    (columnNoDescCount > 0) ? string.Format("、{0,3}個未填說明", columnNoDescCount) : "",
                    isNoTableDesc ? "、資料表說明未填寫" : "")
                ));
            txtMarkDown = markDown.ToString();
        }

        /// <summary>
        /// GetExtendedProperties 取得資料庫延伸屬性的值.
        /// </summary>
        /// <param name="extendedPropertyCollection">The _ extended property collection.</param>
        /// <param name="names">The _name.</param>
        /// <returns></returns>
        private static string GetExtendedProperties(ExtendedPropertyCollection extendedPropertyCollection, params string[] names)
        {
            StringBuilder oSb = new StringBuilder();
            foreach (var name in names)
            {
                if (extendedPropertyCollection.Contains(name) && extendedPropertyCollection[name].Value != null)
                {
                    oSb.Append(extendedPropertyCollection[name].Value.ToString().Trim());
                }
            }
            return oSb.ToString();
        }

        /// <summary>
        /// ShowProMsg 顯示處理訊息
        /// </summary>
        /// <param name="msg">要顯示的訊息.</param>
        private static void ShowProMsg(string msg)
        {
            Console.Write(msg.Replace(" ", "") + "\r\n");
        }
    }
}