using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace AppliedAccounts.Models
{
    public class ImportExcelFile
    {
        public IBrowserFile? ExcelFile { get; set; }
        public DataSet ImportDataSet { get; set; }
        public bool IsImported { get; set; } = false;
        public string MyMessages { get; set; }
        public string FileName { get; set; }


        public ImportExcelFile()
        {
            ExcelFile = null;
            ImportDataSet = new DataSet();
            IsImported = false;
            MyMessages = string.Empty;
            FileName = string.Empty;

        }


        public ImportExcelFile(IBrowserFile excelFile)
        {

            ExcelFile = excelFile;
            ImportDataSet = new DataSet();
            IsImported = false;
            MyMessages = string.Empty;
            FileName = string.Empty;


            if (excelFile is not null)
            {
                FileName = Path.GetFileNameWithoutExtension(excelFile.Name);
            }

        }



        #region Import Data From Excel file into DataSet

        public async Task ImportDataAsync()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);          // Enable Page encoders for Excel files.
            try
            {
                MyMessages = string.Empty;
                var _Directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelFiles");
                if (!Directory.Exists(_Directory)) { Directory.CreateDirectory(_Directory); }

                var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };

                var _ExcelFile = Path.Combine(_Directory, ExcelFile.Name);

                if (File.Exists(_ExcelFile)) { File.Delete(_ExcelFile); }

                using (FileStream fs = new(_ExcelFile, FileMode.Create, FileAccess.ReadWrite))
                { await ExcelFile.OpenReadStream().CopyToAsync(fs); }


                using (var stream = File.Open(_ExcelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    ImportDataSet = reader.AsDataSet(conf);

                    if (ImportDataSet is not null)
                    {
                        SaveInTable(ImportDataSet);
                    }
                }


                if (File.Exists(_ExcelFile)) { File.Delete(_ExcelFile); }
            }
            catch (Exception error)
            {
                MyMessages = error.Message;

            }
            finally
            {
                if (MyMessages.Length == 0) { IsImported = true; } else { IsImported = false; }
                //ImportDataSet = new();
            }
        }

        #endregion


        #region Save Imported DataSet into SQL Lite DB Temp with GUID Name.

        internal bool SaveInTable(DataSet importDataSet)
        {
            #region Validate the Header of DataTable

            if (importDataSet.Tables.Count > 0)
            {
                var Headings = new[] { "No", "Code", "Brand", "Model", "Serial Number", "IsPrivate", "Display Name", "Email", "Tag_ID" };
                var _Columns = importDataSet.Tables[0].Columns.Cast<DataColumn>().ToList();
                var _Valid = true;
                foreach (DataColumn Column in _Columns)
                {
                    if (Headings.Any(e => e.ToLower() == Column.ColumnName.ToLower())) { continue; }
                    _Valid = false;
                }

                if (!_Valid) { return false; }
            }
            #endregion

            bool _Result = false;
            int _Records = 0;
            string _Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DB");
            string _GUID = Guid.NewGuid().ToString();
            string _Title = $"Import file {ExcelFile} dated {DateTime.Now}";

            try
            {
                if (FileName.Length > 0)
                {
                    var _OldFilePath = Path.Combine(_Path, FileName + ".db");
                    if (File.Exists(_OldFilePath)) { File.Delete(_OldFilePath); }
                }
            }
            catch (Exception error)
            {
                MyMessages = error.Message;
            }

            string _ConnText = $"";
            string _ImportDBPath = Path.Combine(_Path, FileName + ".db");
            SQLiteConnection.CreateFile(_ImportDBPath);
            SQLiteConnection _TempDBConnection = new($"Data Source={_ImportDBPath}");

            if (_TempDBConnection.State != ConnectionState.Open) { _TempDBConnection.Open(); }

            if (importDataSet is not null)
            {
                foreach (DataTable _Table in importDataSet.Tables)
                {
                    var _Text = new StringBuilder();
                    _Text.Append($"CREATE TABLE [{_Table.TableName}] (");

                    string _TableName = _Table.TableName;
                    string _LastColumn = _Table.Columns[_Table.Columns.Count - 1].ColumnName;

                    foreach (DataColumn _Column in _Table.Columns)
                    {
                        _Text.Append($"[{_Column.ColumnName}] ");
                        _Text.Append($"NVARCHAR");

                        if (_Column.ColumnName != _LastColumn) { _Text.Append(','); }
                    }

                    _Text.Append(')');
                    SQLiteCommand _Command = new(_Text.ToString(), _TempDBConnection);
                    _Command.ExecuteNonQuery();

                    foreach (DataRow _Row in _Table.Rows)
                    {

                        _Text.Clear();
                        _Text.Append($"INSERT INTO [{_Table.TableName}] VALUES (");
                        foreach (DataColumn _Column in _Table.Columns)
                        {
                            string RowValue = _Row[_Column.ColumnName].ToString() ?? "";
                            if (RowValue.Contains("'"))
                            {
                                RowValue = RowValue.Replace("'", ",");
                            }

                            _Text.AppendLine($"'{RowValue}'");
                            if (_Column.ColumnName != _LastColumn) { _Text.Append(','); }
                        }
                        _Text.Append(')');

                        _Command.CommandText = _Text.ToString();
                        _Records += _Command.ExecuteNonQuery();
                        _Result = true;

                    }
                }
            }

            _TempDBConnection.Close();
            return _Result;

        }
        #endregion
    }
}
