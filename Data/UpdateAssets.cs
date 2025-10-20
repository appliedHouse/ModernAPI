using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ModernAPI.Data
{
    public class UpdateAssets
    {
        public DataTable DBFile { get; set; }
        public string SQLConnectionString { get; set; }
        public SqlConnection SQLConnection { get; set; }
        public SqlCommand Command { get; set; }
        public ProgressBarClass MyProgressBar { get; set; }

        public int TotalRows { get; set; } = 0;
        public int Counter { get; set; } = 0;
        public int Saved { get; set; } = 0;
        public int Skiped { get; set; } = 0;

        public List<string> TagSaved { get; set; }
        public List<string> TagSkiped { get; set; }



        public bool HasNoError { get; set; } = true;
        public string EmployeeGUID { get; set; } = string.Empty;
        public string DepartmentGUID { get; set; } = string.Empty;
        public bool IsTransaction { get; set; } = false;
        public int DepartmentID { get; set; } = 0;

        public string val_Manufacturer { get; set; }
        public string val_Model { get; set; }
        public string val_Private { get; set; }

        public int _Def_ManufactureID = 0;         // Class [CustomFieldsDefinition] Manufacture ID
        public int _Def_Model = 0;                 // Class [CustomFieldsDefinition] Model ID
        public int _Def_Private = 0;            // Class [CustomFieldsDefinition] Private / Public

        public bool IsCompleted { get; set; } = false;


        public UpdateAssets(DataTable _DataTable)
        {
            TotalRows = _DataTable.Rows.Count;
            DBFile = _DataTable;

            MyProgressBar = new();
            MyProgressBar.Counter = 0;
            MyProgressBar.Min = 0;
            MyProgressBar.Max = _DataTable.Rows.Count;

            TagSaved = [];
            TagSkiped = [];

        }

        public SqlConnection GetConnection()
        {
            if (SQLConnection == null)
            {
                if (!string.IsNullOrEmpty(SQLConnectionString))
                {
                    SQLConnection = new(SQLConnectionString);
                    if (SQLConnection.State != ConnectionState.Open) { SQLConnection.Open(); }
                }
            }
            return SQLConnection;
        }


        public async Task<ProgressBarClass> UpdateAssetAsync(DataRow ExcelRow)
        {
            // Get Asset Data from AssetInspector by TagID.
            // Edit / Updata Asset Data from Excel Sheet Data
            // Save Asset Data to AssetInspector

            var excluded = new[] { "No", "Code", "Brand", "Model", "Serial Number", "IsPrivate", "Display Name", "Email", "Tag_ID" };

            bool NoHeaderFound = !ExcelRow.Table.Columns.Cast<DataColumn>()
                .Any(c => !excluded.Any(e => c.ColumnName.Contains(e)));

            if (NoHeaderFound) { return null; }
            {
                var _Query = new StringBuilder();
                _Query.Append("SELECT * FROM [AssetTag] [T] ");
                _Query.Append("LEFT JOIN [Assets] [A] ON [A].[AssetID] = [T].[AssetID]");
                _Query.Append("WHERE TagID = @TagID");

                var _ExcelTagID = ExcelRow["Tag_ID"].ToString() ?? "";
                var _ExcelAssetCode = ExcelRow["Code"].ToString() ?? "";
                var _ExcelEmailID = ExcelRow["Email"].ToString() ?? "";
                var _ExcelEmployeeName = ExcelRow["Display Name"].ToString() ?? "";
                val_Manufacturer = ExcelRow["Brand"].ToString() ?? "";
                val_Model = ExcelRow["Model"].ToString() ?? "";
                val_Private = string.Empty;


                if (_ExcelTagID.Length > 0)
                {
                    Command = new SqlCommand(_Query.ToString(), GetConnection());
                    Command.Parameters.AddWithValue("@TagID", _ExcelTagID);

                    var _Adapter = new SqlDataAdapter(Command);
                    var _DataSet = new DataSet();

                    var _AssetFound = false;
                    var _AssetID = 0;
                    var _EmailID = string.Empty;
                    var _GUID = Guid.NewGuid().ToString();
                    var _EmployeeName = string.Empty;
                    var _SerialNo = string.Empty;
                    var _AssetCode = 0;


                    _Adapter.Fill(_DataSet);
                    if (_DataSet.Tables.Count > 0)
                    {
                        var _DataTable = _DataSet.Tables[0];
                        if (_DataTable.Rows.Count > 0)
                        {
                            _AssetFound = true;
                            _AssetID = (int)_DataTable.Rows[0]["AssetID"];
                            _SerialNo = _DataTable.Rows[0]["ManufactureSN"] == DBNull.Value ? ""
                                      : _DataTable.Rows[0]["ManufactureSN"].ToString();
                            _AssetCode = (int)_DataTable.Rows[0]["AssetCode"];


                            // Insert Employee Record in SQL Database, if not exist (No update of Employee)
                            EmployeeGUID = Guid.NewGuid().ToString();
                            DepartmentGUID = Guid.NewGuid().ToString();

                            UpdateEmployee(_ExcelEmailID, _ExcelEmployeeName, EmployeeGUID);

                        }
                    }

                    // Asset Save to AssetInspector.DB

                    if (_AssetFound && _AssetID > 0)
                    {
                        // Asset Table Update


                        if (!string.IsNullOrEmpty(ExcelRow["Serial Number"].ToString())) { _SerialNo = ExcelRow["Serial Number"].ToString(); }
                        int.TryParse(ExcelRow["Serial Number"].ToString(), out _AssetCode);

                        _Query = new();
                        _Query.AppendLine("UPDATE [Assets] ");
                        _Query.AppendLine("SET [AssetCode]=@AssetCode,");
                        _Query.AppendLine("[ManufactureSN]=@ManufactureSN ");
                        _Query.AppendLine("WHERE [AssetID] = @AssetID");

                        Command = new SqlCommand(_Query.ToString(), GetConnection());
                        Command.Parameters.AddWithValue("@AssetID", _AssetID);
                        Command.Parameters.AddWithValue("@AssetCode", ExcelRow["Code"].ToString());
                        Command.Parameters.AddWithValue("@ManufactureSN", ExcelRow["Serial Number"].ToString() ?? "");

                        var No = Command.ExecuteNonQuery();

                        if (No > 0)
                        {
                            UpdateCustomFields(_AssetID);
                            MyProgressBar.Saved++;
                            TagSaved.Add(_ExcelTagID);
                        }


                    }
                    else
                    {
                        TagSkiped.Add(_ExcelTagID);
                        MyProgressBar.Skiped++;
                    }

                }
            }
            MyProgressBar.Counter++;
            await Task.Delay(100);
            return MyProgressBar;
        }

        private void UpdateCustomFields(int _AssetID)
        {
            var _ClassID = 0;
            var _FieldValueID = 0;

            #region Getting (A) Getting Custome Field Defination 
            var _Query = new StringBuilder();
            _Query.AppendLine($"SELECT * FROM [Assets] WHERE [AssetID] = {_AssetID}");
            Command = new SqlCommand(_Query.ToString(), GetConnection());

            var _Adapter = new SqlDataAdapter(Command);
            var _DataSet = new DataSet();
            _Adapter.Fill(_DataSet, "Assets");
            if (_DataSet.Tables.Count > 0)
            {
                if (_DataSet.Tables[0].Rows.Count > 0)
                {
                    _ClassID = (int)_DataSet.Tables[0].Rows[0]["AssetClassID"];
                    GetCustomeFieldIds(_ClassID);
                }
            }
            #endregion

            #region (b) Getting Asset Custom Field Values  [Manufacturer]

            var _SQLAction = string.Empty;
            _Query = new();
            var _Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Query.AppendLine($"SELECT * FROM [CustomFieldsValue] WHERE [AssetID]={_AssetID} AND [FieldID]={_Def_ManufactureID}");
            Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Adapter = new SqlDataAdapter(Command);
            _DataSet = new DataSet();

            _Adapter.Fill(_DataSet, "CustomFieldsValue");
            if (_DataSet.Tables.Count > 0)
            {
                if (_DataSet.Tables["CustomFieldsValue"]!.Rows.Count == 0)
                {
                    _SQLAction = "Insert";
                }
                else
                {
                    _FieldValueID = (int)_DataSet.Tables["CustomFieldsValue"]!.Rows[0]["FieldValueID"];
                    _SQLAction = "Update";
                }
            }



            if (_SQLAction == "Update")
            {
                _Query = new();
                _Query.AppendLine($"UPDATE [CustomFieldsValue] SET [FieldValue]='{val_Manufacturer}'");
                _Query.AppendLine($"WHERE [FieldValueId] = {_FieldValueID}");
                _Command = new SqlCommand(_Query.ToString(), GetConnection());
                _Command.ExecuteNonQuery();

            }
            else
            {
                _Query = new();
                _Query.AppendLine($"INSERT INTO [CustomFieldsValue] ( ");
                _Query.AppendLine("[FieldId],");
                _Query.AppendLine("[AssetId],");
                _Query.AppendLine("[TitleAR],");
                _Query.AppendLine("[TitleEN],");
                _Query.AppendLine("[FieldValue],");
                _Query.AppendLine("[LastUpdatedTime]");
                _Query.AppendLine(") VALUES (");
                _Query.AppendLine("@FieldId,");
                _Query.AppendLine("@AssetId,");
                _Query.AppendLine("@TitleAR,");
                _Query.AppendLine("@TitleEN,");
                _Query.AppendLine("@FieldValue,");
                _Query.AppendLine("@LastUpdatedTime)");

                _Command = new SqlCommand(_Query.ToString(), GetConnection());
                _Command.Parameters.AddWithValue("@FieldId", _Def_ManufactureID);
                _Command.Parameters.AddWithValue("@AssetId", _AssetID);
                _Command.Parameters.AddWithValue("@TitleAR", "الماركة");
                _Command.Parameters.AddWithValue("@TitleEN", "Manufacturer");
                _Command.Parameters.AddWithValue("@FieldValue", val_Manufacturer);
                _Command.Parameters.AddWithValue("@LastUpdatedTime", DBNull.Value);
                _Command.ExecuteNonQuery();
            }
            #endregion

            #region (b) Getting Asset Custom Field Values  [Model]

            _SQLAction = string.Empty;
            _Query = new();
            _Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Query.AppendLine($"SELECT * FROM [CustomFieldsValue] WHERE [AssetID]={_AssetID} AND [FieldID]={_Def_Model}");
            Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Adapter = new SqlDataAdapter(Command);
            _DataSet = new DataSet();

            _Adapter.Fill(_DataSet, "CustomFieldsValue");
            if (_DataSet.Tables.Count > 0)
            {
                if (_DataSet.Tables["CustomFieldsValue"]!.Rows.Count == 0)
                {
                    _SQLAction = "Insert";
                }
                else
                {
                    _FieldValueID = (int)_DataSet.Tables["CustomFieldsValue"]!.Rows[0]["FieldValueID"];
                    _SQLAction = "Update";
                }
            }

            if (_SQLAction == "Update")
            {
                _Query = new();
                _Query.AppendLine($"UPDATE [CustomFieldsValue] SET [FieldValue]='{val_Model}'");
                _Query.AppendLine($"WHERE [FieldValueId] = {_FieldValueID}");
                _Command = new SqlCommand(_Query.ToString(), GetConnection());
                _Command.ExecuteNonQuery();

            }
            else
            {
                _Query = new();
                _Query.AppendLine($"INSERT INTO [CustomFieldsValue] ( ");
                _Query.AppendLine("[FieldId],");
                _Query.AppendLine("[AssetId],");
                _Query.AppendLine("[TitleAR],");
                _Query.AppendLine("[TitleEN],");
                _Query.AppendLine("[FieldValue],");
                _Query.AppendLine("[LastUpdatedTime]");
                _Query.AppendLine(") VALUES (");
                _Query.AppendLine("@FieldId,");
                _Query.AppendLine("@AssetId,");
                _Query.AppendLine("@TitleAR,");
                _Query.AppendLine("@TitleEN,");
                _Query.AppendLine("@FieldValue,");
                _Query.AppendLine("@LastUpdatedTime)");

                _Command = new SqlCommand(_Query.ToString(), GetConnection());
                _Command.Parameters.AddWithValue("@FieldId", _Def_Model);
                _Command.Parameters.AddWithValue("@AssetId", _AssetID);
                _Command.Parameters.AddWithValue("@TitleAR", "الموديل");
                _Command.Parameters.AddWithValue("@TitleEN", "Model No");
                _Command.Parameters.AddWithValue("@FieldValue", val_Model);
                _Command.Parameters.AddWithValue("@LastUpdatedTime", DBNull.Value);
                _Command.ExecuteNonQuery();
            }
            #endregion

            #region (b) Getting Asset Custom Field Values  [Private]

            _SQLAction = string.Empty;
            _Query = new();
            _Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Query.AppendLine($"SELECT * FROM [CustomFieldsValue] WHERE [AssetID]={_AssetID} AND [FieldID]='{_Def_Private}'");
            Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Adapter = new SqlDataAdapter(Command);
            _DataSet = new DataSet();

            _Adapter.Fill(_DataSet, "CustomFieldsValue");
            if (_DataSet.Tables.Count > 0)
            {
                if (_DataSet.Tables["CustomFieldsValue"]!.Rows.Count == 0)
                {
                    _SQLAction = "Insert";
                }
                else
                {
                    _FieldValueID = (int)_DataSet.Tables["CustomFieldsValue"]!.Rows[0]["FieldValueID"];

                    _SQLAction = "Update";
                }
            }

            if (_SQLAction == "Update")
            {
                _Query = new();
                _Query.AppendLine($"UPDATE [CustomFieldsValue] SET [FieldValue]='{val_Private}'");
                _Query.AppendLine($"WHERE [FieldValueId] = {_FieldValueID}");
                _Command = new SqlCommand(_Query.ToString(), GetConnection());
                _Command.ExecuteNonQuery();

            }
            else
            {
                _Query = new();
                _Query.AppendLine($"INSERT INTO [CustomFieldsValue] ( ");
                _Query.AppendLine("[FieldId],");
                _Query.AppendLine("[AssetId],");
                _Query.AppendLine("[TitleAR],");
                _Query.AppendLine("[TitleEN],");
                _Query.AppendLine("[FieldValue],");
                _Query.AppendLine("[LastUpdatedTime]");
                _Query.AppendLine(") VALUES (");
                _Query.AppendLine("@FieldId,");
                _Query.AppendLine("@AssetId,");
                _Query.AppendLine("@TitleAR,");
                _Query.AppendLine("@TitleEN,");
                _Query.AppendLine("@FieldValue,");
                _Query.AppendLine("@LastUpdatedTime)");

                _Command = new SqlCommand(_Query.ToString(), GetConnection());
                _Command.Parameters.AddWithValue("@FieldId", _Def_Private);
                _Command.Parameters.AddWithValue("@AssetId", _AssetID);
                _Command.Parameters.AddWithValue("@TitleAR", "أصل خاص");
                _Command.Parameters.AddWithValue("@TitleEN", "Private");
                _Command.Parameters.AddWithValue("@FieldValue", val_Private);
                _Command.Parameters.AddWithValue("@LastUpdatedTime", DBNull.Value);
                _Command.ExecuteNonQuery();
            }
            #endregion

        }

        private void GetCustomeFieldIds(int _ClassID)
        {
            var _Query = new StringBuilder();
            _Query.Append($"SELECT * FROM [CustomFieldsDefinition] WHERE [ClassID] = {_ClassID}");

            Command = new SqlCommand(_Query.ToString(), GetConnection());
            var _Adapter = new SqlDataAdapter(Command);
            var _DataSet = new DataSet();
            _Adapter.Fill(_DataSet, "Defination");
            if (_DataSet.Tables.Count > 0)
            {
                if (_DataSet.Tables["Defination"]!.Rows.Count == 0)
                {
                    CreateCustomFieldDefination(_ClassID);
                }
                _Adapter.Fill(_DataSet, "Defination");
                foreach (DataRow _Row in _DataSet.Tables["Defination"]!.Rows)
                {
                    if (_Row["TitleEN"].ToString() == "Manufacturer") { _Def_ManufactureID = (int)_Row["FieldID"]; }
                    if (_Row["TitleEN"].ToString() == "Model No") { _Def_Model = (int)_Row["FieldID"]; }
                    if (_Row["TitleEN"].ToString() == "Private") { _Def_Private = (int)_Row["FieldID"]; }
                }
            }


        }


        private void UpdateEmployee(string _EmailID, string _EmployeeName, string _GUID)
        {
            if (!IsTransaction)
            {
                UpdateDepartment(DepartmentGUID);
                IsTransaction = true;
            }

            Command = new SqlCommand($"SELECT * FROM [Employee] WHERE [EmployeeSocialID] = '{_EmailID}'", GetConnection());
            var _Adapter = new SqlDataAdapter(Command);
            var _DataSet = new DataSet();
            _Adapter.Fill(_DataSet);
            if (_DataSet.Tables.Count > 0)
            {
                var _DataTable = _DataSet.Tables[0];
                if (_DataTable.Rows.Count == 0)
                {
                    EmployeeGUID = Guid.NewGuid().ToString();
                    UpdateTransaction(EmployeeGUID, $"Insert Employee {_EmailID} {_EmployeeName}");
                    var _Query = new StringBuilder();

                    _Query.AppendLine("INSERT INTO [Employee] ( ");
                    _Query.AppendLine("[EmployeePhoto],");
                    _Query.AppendLine("[EmployeeName],");
                    _Query.AppendLine("[EmployeeSocialID],");
                    _Query.AppendLine("[DepartmentID], ");
                    _Query.AppendLine("[TransactionID], ");
                    _Query.AppendLine("[IntgID], ");
                    _Query.AppendLine("[IsTrackable] ");
                    _Query.AppendLine(") Values ( ");
                    _Query.AppendLine("@EmployeePhoto,");
                    _Query.AppendLine("@EmployeeName,");
                    _Query.AppendLine("@EmployeeSocialID,");
                    _Query.AppendLine("@DepartmentID, ");
                    _Query.AppendLine("@TransactionID, ");
                    _Query.AppendLine("@IntgID, ");
                    _Query.AppendLine("@IsTrackable) ");


                    Command = new SqlCommand(_Query.ToString(), GetConnection());
                    Command.Parameters.AddWithValue("@EmployeePhoto", DBNull.Value);
                    Command.Parameters.AddWithValue("@EmployeeName", _EmployeeName);
                    Command.Parameters.AddWithValue("@EmployeeSocialID", _EmailID);
                    Command.Parameters.AddWithValue("@DepartmentID", DepartmentID);
                    Command.Parameters.AddWithValue("@TransactionID", EmployeeGUID);
                    Command.Parameters.AddWithValue("@IntgID", DBNull.Value);
                    Command.Parameters.AddWithValue("@IsTrackable", 0);

                    Command.ExecuteNonQuery();
                }
            }

        }
        private int UpdateDepartment(string _GUID)
        {
            int _result = 0;
            Command = new SqlCommand($"SELECT * FROM [EmployeeDeparment]", GetConnection());
            var _Adapter = new SqlDataAdapter(Command);
            var _DataSet = new DataSet();
            _Adapter.Fill(_DataSet);
            if (_DataSet.Tables.Count > 0)
            {
                var _DataTable = _DataSet.Tables[0];
                if (_DataTable.Rows.Count == 0)
                {
                    UpdateTransaction(_GUID, "Insert Department by Excel Data");
                    var _Query = new StringBuilder();
                    _Query.AppendLine("INSERT INTO [EmployeeDeparment] ( ");
                    _Query.AppendLine("[DepartmentCode],");
                    _Query.AppendLine("[DepartmentName],");
                    _Query.AppendLine("[LocationID], ");
                    _Query.AppendLine("[TransactionID], ");
                    _Query.AppendLine("[IntgID] ");
                    _Query.AppendLine(") Values ( ");
                    _Query.AppendLine("@DepartmentCode,");
                    _Query.AppendLine("@DepartmentName,");
                    _Query.AppendLine("@LocationID,");
                    _Query.AppendLine("@TransactionID,");
                    _Query.AppendLine("@IntgID)");

                    Command = new SqlCommand(_Query.ToString(), GetConnection());
                    Command.Parameters.AddWithValue("@DepartmentCode", "101");
                    Command.Parameters.AddWithValue("@DepartmentName", "Office");
                    Command.Parameters.AddWithValue("@LocationID", 1);
                    Command.Parameters.AddWithValue("@TransactionID", _GUID);
                    Command.Parameters.AddWithValue("@IntgID", DBNull.Value);

                    Command.ExecuteNonQuery();

                    var No = _Adapter.Fill(_DataSet);

                    if (No > 0)
                    {
                        DepartmentID = (int)_DataSet.Tables[0].Rows[0]["DepartmentID"];
                    }
                }
                else
                {
                    DepartmentID = (int)_DataTable.Rows[0]["DepartmentID"];
                }
            }
            return _result;
        }
        private void UpdateTransaction(string _GUID, string _Title)
        {

            var _Query = new StringBuilder();
            _Query.AppendLine("INSERT INTO [SystemTransaction] ( ");
            _Query.AppendLine("[TransactionID],");
            _Query.AppendLine("[TransactionName],");
            _Query.AppendLine("[TransactionDate],");
            _Query.AppendLine("[TransactionByUserID],");
            _Query.AppendLine("[TransactionType]");
            _Query.AppendLine(") VALUES (");
            _Query.AppendLine("@TransactionID,");
            _Query.AppendLine("@TransactionName,");
            _Query.AppendLine("@TransactionDate,");
            _Query.AppendLine("@TransactionByUserID,");
            _Query.AppendLine("@TransactionType)");

            Command = new SqlCommand(_Query.ToString(), GetConnection());
            Command.Parameters.AddWithValue("@TransactionID", _GUID);
            Command.Parameters.AddWithValue("@TransactionName", _Title);
            Command.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
            Command.Parameters.AddWithValue("@TransactionByUserID", 1);
            Command.Parameters.AddWithValue("@TransactionType", 1);

            var _No = Command.ExecuteNonQuery();
        }
        private void CreateCustomFieldDefination(int _ClassID)
        {
            var _Query = new StringBuilder();
            _Query.AppendLine("INSERT INTO [CustomFieldsDefinition] ( ");
            _Query.AppendLine("[ClassID],");
            _Query.AppendLine("[TypeId],");
            _Query.AppendLine("[TitleAR],");
            _Query.AppendLine("[TitleEN],");
            _Query.AppendLine("[IsRequired],");
            _Query.AppendLine("[IsActive],");
            _Query.AppendLine("[IsDeleted]");
            _Query.AppendLine(") VALUES (");
            _Query.AppendLine("@ClassID,");
            _Query.AppendLine("@TypeId,");
            _Query.AppendLine("@TitleAR,");
            _Query.AppendLine("@TitleEN,");
            _Query.AppendLine("@IsRequired,");
            _Query.AppendLine("@IsActive,");
            _Query.AppendLine("@IsDeleted)");

            var _Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Command.Parameters.AddWithValue("@ClassID", _ClassID);
            _Command.Parameters.AddWithValue("@TypeID", 2);
            _Command.Parameters.AddWithValue("@TitleAR", "الماركة");
            _Command.Parameters.AddWithValue("@TitleEN", "Manufacturer");
            _Command.Parameters.AddWithValue("@IsRequired", 0);
            _Command.Parameters.AddWithValue("@IsActive", 1);
            _Command.Parameters.AddWithValue("@IsDeleted", 0);
            _Command.ExecuteNonQuery();                   // Add Manafucter

            _Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Command.Parameters.AddWithValue("@ClassID", _ClassID);
            _Command.Parameters.AddWithValue("@TypeID", 2);
            _Command.Parameters.AddWithValue("@TitleAR", "الموديل");
            _Command.Parameters.AddWithValue("@TitleEN", "Model No");
            _Command.Parameters.AddWithValue("@IsRequired", 0);
            _Command.Parameters.AddWithValue("@IsActive", 1);
            _Command.Parameters.AddWithValue("@IsDeleted", 0);
            _Command.ExecuteNonQuery();                   // Model No

            _Command = new SqlCommand(_Query.ToString(), GetConnection());
            _Command.Parameters.AddWithValue("@ClassID", _ClassID);
            _Command.Parameters.AddWithValue("@TypeID", 3);
            _Command.Parameters.AddWithValue("@TitleAR", "أصل خاص");
            _Command.Parameters.AddWithValue("@TitleEN", "Private");
            _Command.Parameters.AddWithValue("@IsRequired", 0);
            _Command.Parameters.AddWithValue("@IsActive", 1);
            _Command.Parameters.AddWithValue("@IsDeleted", 0);
            _Command.ExecuteNonQuery();                   // Model No

        }
    }
}
