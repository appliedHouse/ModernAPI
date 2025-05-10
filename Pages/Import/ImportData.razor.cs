using AppliedAccounts.Models;
using Microsoft.AspNetCore.Components.Forms;
using ModernAPI.Data;
using System.Data;
using System.Data.SQLite;

namespace ModernAPI.Pages.Import
{
    public partial class ImportData
    {
        public DownLoadModel MyModel { get; set; } = new();

        public ImportExcelFile ImportExcel { get; set; } = new();
        
        public async Task GetExcelFile(InputFileChangeEventArgs e)
        {
            try
            {
                MyModel.ExcelFileName = e.File.Name;
                MyModel.SQLiteFileName = GetSQLIteFileName(MyModel.ExcelFileName);



                MyModel.SpinnerMessage = "Excel file is being loaded.  Wait for some while";
                MyModel.ShowSpinner = true;
                MyModel.ExcelFileName = e.File.Name;
                await InvokeAsync(StateHasChanged);

                if (e.File is not null)
                {
                    ImportExcel = new(e.File);
                    await ImportExcel.ImportDataAsync();        // ImportExcelFile.cs Function
                    MyModel.IsExcelLoaded = true;                       // Excel file has been loaded successfully.
                }
            }
            catch (Exception error)
            {

                MyModel.IsError = true;
                MyModel.ErrorMessage = error.Message;

            }
            finally
            {
                //ImportExcel ??= new();
                MyModel.TB_Assets =  await ImportDataTable();        // Import Data from DB file into DataTable
                MyModel.ShowSpinner = false;
                await InvokeAsync(StateHasChanged);
            }

        }

        private string GetSQLIteFileName(string excelFileName)
        {
            var FileName = Path.GetFileNameWithoutExtension(excelFileName);
            var FileExtension = Path.GetExtension(excelFileName);
            MyModel.ExcelDataSheet = "Data";
            return FileName + ".db";
        }

        public async Task<DataTable> ImportDataTable()
        {
            MyModel.IsDataTableLoaded = false;
            //string _Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DB", MyModel.SQLiteFileName);
            var ConnectionClass = new Connections(MyModel.SQLiteFileName);

            if(ImportExcel.ImportDataSet.Tables.Count == 0) { return new(); }           // Data Not Found return new DataTable


            string FirstDataSheet = ImportExcel.ImportDataSet.Tables[0].TableName;
            SQLiteCommand _Command = new(); ;
            _Command.Connection = ConnectionClass.MyConnection;
            _Command.CommandText = $"SELECT * FROM [{FirstDataSheet}]";
            SQLiteDataAdapter _Adapter = new(_Command);
            DataSet _DataSet = new();

            await Task.Run(() => {
                _Adapter.Fill(_DataSet);
            });
            
            if (_DataSet is not null)
            {
                if (_DataSet.Tables.Count > 0)
                {
                    MyModel.IsDataTableLoaded = true;
                    return _DataSet.Tables[0];
                }
                else
                {
                    MyModel.IsError = true;
                    MyModel.ErrorMessage = "No data found in the ImportAssets.db file.";
                }
            }
            return null;

        }
    }

    public class DownLoadModel
    {
        public string SpinnerMessage { get; set; } = string.Empty;
        public string ExcelFileName { get; set; } = string.Empty;
        public string ExcelDataSheet { get; set; } = string.Empty;
        public string SQLiteFileName { get; set; } = string.Empty;
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsExcelLoaded { get; set; }
        public bool IsDataTableLoaded { get; set; } = false;
        public bool ShowSpinner { get; set; }
        public DataTable TB_Assets { get; set; }
    }
}
