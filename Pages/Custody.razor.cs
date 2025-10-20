using ModernAPI.Data;
using System.Data;
using ModernAPI.Pages.Import;
using Microsoft.AspNetCore.Components.Forms;
using ImportExcel;

using System.Data.SQLite;


namespace ModernAPI.Pages
{
    public partial class Custody
    {
        public AddCustodian CustodianModel { get; set; }
        public DownLoadModel downloadModel { get; set; } = new();
        private UpdateAssets UpdateService { get; set; }                // Asset update Service
        public ImportExcelFile ImportExcel { get; set; } = new();
        public bool IsCompletedCustody { get; set; } = false;

        public async Task AddCustody()
        {
            UpdateService = new UpdateAssets(downloadModel.TB_Assets);
            UpdateService.SQLConnectionString = Config.GetConnectionString("SQLServer") ?? "";

            if (UpdateService.GetConnection() != null)
            {
                downloadModel.ShowSpinner = true;
                downloadModel.SpinnerMessage = "Assign Custodian is being processed.....";
                await InvokeAsync(StateHasChanged);
                await Task.Delay(3000);


                await Task.Run(() =>
                {
                    CustodianModel = new(UpdateService.GetConnection(), UpdateService.DBFile);
                    CustodianModel.UpdateCustodian();
                    CustodianModel.IsCompleted = true;
                    // Testing end
                    downloadModel.ShowSpinner = false;
                    downloadModel.SpinnerMessage = "Assign Custodian is completed......";
                });
                
                if (CustodianModel.IsCompleted)
                {
                    IsCompletedCustody = CustodianModel.IsCompleted;
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(5000);
                }
            }
        }

        #region DownLoad Excel file and convert to Data Table
        public async Task GetExcelFile(InputFileChangeEventArgs e)
        {
            try
            {
                downloadModel.ExcelFileName = e.File.Name;
                downloadModel.SQLiteFileName = GetSQLIteFileName(downloadModel.ExcelFileName);



                downloadModel.SpinnerMessage = "Excel file is being loaded.  Wait for some while";
                downloadModel.ShowSpinner = true;
                downloadModel.ExcelFileName = e.File.Name;
                await InvokeAsync(StateHasChanged);

                if (e.File is not null)
                {
                    //ImportExcel = new(e.File);

                    ImportExcel = new(); ;
                    ImportExcel.ExcelFile = e.File;
                    ImportExcel.Headings = ["No", "Code", "Brand", "Model", "Serial Number", "IsPrivate", "Display Name", "Email", "Tag_ID"];
                    ImportExcel.ExportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelFiles");

                    await ImportExcel.ImportDataAsync();        // ImportExcelFile.cs Function
                    downloadModel.IsExcelLoaded = true;                       // Excel file has been loaded successfully.
                }
            }
            catch (Exception error)
            {

                downloadModel.IsError = true;
                downloadModel.ErrorMessage = error.Message;

            }
            finally
            {
                //ImportExcel ??= new();
                downloadModel.TB_Assets = await ImportDataTable();        // Import Data from DB file into DataTable
                downloadModel.ShowSpinner = false;
                await InvokeAsync(StateHasChanged);
            }

        }

        private string GetSQLIteFileName(string excelFileName)
        {
            var FileName = Path.GetFileNameWithoutExtension(excelFileName);
            var FileExtension = Path.GetExtension(excelFileName);
            downloadModel.ExcelDataSheet = "Data";
            return FileName + ".db";
        }

        public async Task<DataTable> ImportDataTable()
        {
            downloadModel.IsDataTableLoaded = false;
            //string _Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DB", downloadModel.SQLiteFileName);
            var ConnectionClass = new Connections(downloadModel.SQLiteFileName);

            if (ImportExcel.ImportDataSet.Tables.Count == 0) { return new(); }           // Data Not Found return new DataTable


            string FirstDataSheet = ImportExcel.ImportDataSet.Tables[0].TableName;
            SQLiteCommand _Command = new(); ;
            _Command.Connection = ConnectionClass.MyConnection;
            _Command.CommandText = $"SELECT * FROM [{FirstDataSheet}]";
            SQLiteDataAdapter _Adapter = new(_Command);
            DataSet _DataSet = new();

            await Task.Run(() =>
            {
                _Adapter.Fill(_DataSet);
            });

            if (_DataSet is not null)
            {
                if (_DataSet.Tables.Count > 0)
                {
                    downloadModel.IsDataTableLoaded = true;
                    return _DataSet.Tables[0];
                }
                else
                {
                    downloadModel.IsError = true;
                    downloadModel.ErrorMessage = "No data found in the ImportAssets.db file.";
                }
            }
            return null;

        }
        #endregion
    }
}

