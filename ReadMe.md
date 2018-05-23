# SMO Project

連接 **MS SQL Server** 資料庫，取得資料結構資料

可以透過 [dotnet](https://docs.microsoft.com/zh-tw/dotnet/core/tools/?tabs=netcore2x) 工具來 publish ，範例語法如下

``` cmd
dotnet publish -c Release -r win10-x64
dotnet publish -c Release -r ubuntu.16.10-x64
```

專案改採用 **.netcore** 進行改寫，所以 release 的元件很多，建議直接使用以下的 **batch** 內容呼叫執行檔案執行即可

使用方式為複製以下的 batch 內容，建立 .bat 檔案，然後執行即可(注意執行檔案的路徑與輸出目錄的路徑)

``` bat
rem ExpDbCp.exe -S=主機 -d=資料庫 -U=帳號 -P=密碼 -O=輸出目錄

echo %date% %time%

D:\netcoreapp2.0\win10-x64\publish\ExpDbCp.exe -S=DatabaseServer -d=DataBaseName -U=loginName -P=loginPassword -O=D:\Projects\DataBaseDDL

echo %date% %time%
pause
```

## 注意事項

開發版本為 **.NET Framework 4.7** 與 **.NET Standard 2.0**

所以使用此 command 程式的電腦，必須依照其符合的 SQL Server 版本進行重制，可以透過 **nuget** 進行

詳細情詳可以參考 [如何在 Visual Studio.NET 中建立 Visual C# SMO 專案](https://docs.microsoft.com/zh-tw/sql/relational-databases/server-management-objects-smo/how-to-create-a-visual-csharp-smo-project-in-visual-studio-net)