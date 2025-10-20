using Microsoft.SqlServer.Server;
using ModernAPI.Pages;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace ModernAPI.Data
{
    public class AddCustodian
    {
        public SqlConnection MyConnection { get; set; }
        public DataTable ExcelTable { get; set; }
        //public DataTable EmployeeTable { get; set; }
        public SqlCommand MyCommand { get; set; }
        public string _AssetTag = string.Empty; // Asset Tag ID
        public string MyGUID = string.Empty;
        public int MaxFormID = 0;
        public int UserID = 1;              // System User..
        public int ProcessID = 1;
        public int ProcessTypeID = 3;
        public int ProcessStateID = 1;
        public string FormReportURL = "Reports/AssignGuadrenReportFile";
        public int FormProcessReasonID = 27;
        public int NewEmployeeDepartmentID = 1;
        public DataRow? AssetRow = null;
        public int AssetID = 0;
        public List<string> MyMessage = [];
        public int NewAssetTransactionLogID = 0;
        public bool IsCompleted { get; set; } = false;



        public AddCustodian(SqlConnection connection, DataTable dataTable)
        {
            MyConnection = connection;
            ExcelTable = dataTable;
            MyCommand = new SqlCommand("", MyConnection);

        }


        // Main Function of Update Custodian / Guardian
        public void UpdateCustodian()
        {
            if (MyConnection.State != ConnectionState.Open) { MyConnection.Open(); }
            if (ExcelTable == null || ExcelTable.Rows.Count == 0) { return; }

            var excluded = new[] { "No", "Code", "Brand", "Model", "Serial Number", "IsPrivate", "Display Name", "Email", "Tag_ID" };

            bool NoHeaderFound = !ExcelTable.Columns.Cast<DataColumn>()
                .Any(c => !excluded.Any(e => c.ColumnName.Contains(e)));

            if(NoHeaderFound) { MyMessage.Add("Excel Sheet Header are not Match with [No,Code,Brand,Model,Serial Number,IsPrivate,Display Name,Email,Tag_ID]"); return; }

            foreach (DataRow Row in ExcelTable.Rows)
            {
                var _EmailID = Row.Field<string>("Email") ?? ""; if (string.IsNullOrEmpty(_EmailID)) { continue; }
                var _EmployeeName = Row.Field<string>("Display Name") ?? "";
                var _AssetTagID = Row.Field<string>("Tag_ID") ?? "";
                var _EmployeeID = GetEmployeeID(_EmailID);
                MaxFormID = MaxAsetFormID();

                if (_EmployeeID == 0)
                { continue; }

                AssetRow = GetAssetRow(_AssetTagID);
                if (AssetRow == null) { MyMessage.Add("Asset Record not found.."); return; }

                #region Asset Form & Asset Form Log

                AssetID = AssetRow.Field<int>("AssetID");
                MyMessage.Add($"Asset Id {AssetID}");

                MyGUID = Guid.NewGuid().ToString();
                UpdateTransaction(MyGUID, $"Asset {AssetID} Form Add Custodian {_EmailID} {_EmployeeName}");



                #region SQL Query

                var _Query = new StringBuilder();
                //_Query.Append(" [FormID]");  auto increment ID
                _Query.Append("INSERT INTO [AssetForm] ");
                _Query.Append("([FormTitle]");
                _Query.Append(",[FormCode]");
                _Query.Append(",[FormDate]");
                _Query.Append(",[FormProcessTypeID]");
                _Query.Append(",[FormProcessStateID]");
                _Query.Append(",[FormAssetCount]");
                _Query.Append(",[FormReportURL]");
                _Query.Append(",[FormRefrenceNo]");
                _Query.Append(",[FormSigns]");
                _Query.Append(",[FormProcessReasonID]");
                _Query.Append(",[FormProcessedByUserID]");
                _Query.Append(",[RequestProcessLocationID]");
                _Query.Append(",[OldEmployeeID]");
                _Query.Append(",[OldEmployeeDepartmentID]");
                _Query.Append(",[NewEmployeeID]");
                _Query.Append(",[NewEmployeeDepartmentID]");
                _Query.Append(",[Notes]");
                _Query.Append(",[TempProcessID]");
                _Query.Append(",[AddTransactionID]");
                _Query.Append(",[UpdateTransactionID]");
                _Query.Append(",[IsDeleted]");
                _Query.Append(") VALUES ");
                _Query.Append("(@FormTitle");
                _Query.Append(",@FormCode");
                _Query.Append(",@FormDate");
                _Query.Append(",@FormProcessTypeID");
                _Query.Append(",@FormProcessStateID");
                _Query.Append(",@FormAssetCount");
                _Query.Append(",@FormReportURL");
                _Query.Append(",@FormRefrenceNo");
                _Query.Append(",@FormSigns");
                _Query.Append(",@FormProcessReasonID");
                _Query.Append(",@FormProcessedByUserID");
                _Query.Append(",@RequestProcessLocationID");
                _Query.Append(",@OldEmployeeID");
                _Query.Append(",@OldEmployeeDepartmentID");
                _Query.Append(",@NewEmployeeID");
                _Query.Append(",@NewEmployeeDepartmentID");
                _Query.Append(",@Notes");
                _Query.Append(",@TempProcessID");
                _Query.Append(",@AddTransactionID");
                _Query.Append(",@UpdateTransactionID");
                _Query.Append(",@IsDeleted);");
                #endregion

                #region SQL Command & Parameters
                MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
                MyCommand.Parameters.AddWithValue("@FormTitle", $"Excel: Asset Custodian {_EmployeeName} {_EmailID}");
                MyCommand.Parameters.AddWithValue("@FormCode", MaxFormID.ToString("00000000"));
                MyCommand.Parameters.AddWithValue("@FormDate", DateTime.Now);
                MyCommand.Parameters.AddWithValue("@FormProcessTypeID", ProcessID);
                MyCommand.Parameters.AddWithValue("@FormProcessStateID", ProcessStateID);
                MyCommand.Parameters.AddWithValue("@FormAssetCount", 1);
                MyCommand.Parameters.AddWithValue("@FormReportURL", FormReportURL);
                MyCommand.Parameters.AddWithValue("@FormRefrenceNo", DateTime.Now.ToString("dd mmyy hhMMss"));
                MyCommand.Parameters.AddWithValue("@FormSigns", string.Empty);
                MyCommand.Parameters.AddWithValue("@FormProcessReasonID", FormProcessReasonID);
                MyCommand.Parameters.AddWithValue("@FormProcessedByUserID", UserID);
                MyCommand.Parameters.AddWithValue("@RequestProcessLocationID", DBNull.Value);
                MyCommand.Parameters.AddWithValue("@OldEmployeeID", DBNull.Value);
                MyCommand.Parameters.AddWithValue("@OldEmployeeDepartmentID", DBNull.Value);
                MyCommand.Parameters.AddWithValue("@NewEmployeeID", _EmployeeID);
                MyCommand.Parameters.AddWithValue("@NewEmployeeDepartmentID", NewEmployeeDepartmentID);
                MyCommand.Parameters.AddWithValue("@Notes", DBNull.Value);
                MyCommand.Parameters.AddWithValue("@TempProcessID", DBNull.Value);
                MyCommand.Parameters.AddWithValue("@AddTransactionID", MyGUID);
                MyCommand.Parameters.AddWithValue("@UpdateTransactionID", DBNull.Value);
                MyCommand.Parameters.AddWithValue("@IsDeleted", 0); // 0 = Not Deleted, 1 = Deleted
                #endregion

                var Records = MyCommand.ExecuteNonQuery();
                if (Records > 0)
                {
                    MyMessage.Add($"INSERT INTO [AssetForm] Sucessfully... FormID {MaxFormID}");

                    AssetFormLog(_EmailID, _EmployeeName);          // Add Asset form Log after asset form created.

                    #region Add Asset Transaction Log  [AssetTransactionLog]
                    AssetTransactionLog(AssetRow);
                    #endregion

                    #region Asset form Guardianship

                    _Query.Clear();
                    _Query.Append("INSERT INTO [AssetFormGurdianshipItem] ");
                    //_Query.Append("([GurdianshipFormItemID]");  Auto generate ID
                    _Query.Append("([GurdianshipFormID]");
                    _Query.Append(",[AssetID]");
                    _Query.Append(",[ProcessID]");
                    _Query.Append(",[AssetTransactionID]");
                    _Query.Append(",[OldAssetStateID]");
                    _Query.Append(",[NewStateStateID]");
                    _Query.Append(") VALUES ");
                    _Query.Append("(@GurdianshipFormID");
                    _Query.Append(",@AssetID");
                    _Query.Append(",@ProcessID");
                    _Query.Append(",@AssetTransactionID");
                    _Query.Append(",@OldAssetStateID");
                    _Query.Append(",@NewStateStateID");
                    _Query.Append(")");

                    MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
                    MyCommand.Parameters.AddWithValue("@GurdianshipFormID", MaxFormID);
                    MyCommand.Parameters.AddWithValue("@AssetID", AssetRow.Field<int>("AssetID"));
                    MyCommand.Parameters.AddWithValue("@ProcessID", DBNull.Value);
                    MyCommand.Parameters.AddWithValue("@AssetTransactionID", NewAssetTransactionLogID);
                    MyCommand.Parameters.AddWithValue("@OldAssetStateID", AssetRow.Field<int>("AssetStateID"));
                    MyCommand.Parameters.AddWithValue("@NewStateStateID", AssetRow.Field<int>("AssetStateID"));
                    Records = MyCommand.ExecuteNonQuery();

                    if (Records <= 0)
                    {
                        MyMessage.Add($"INSERT INTO [AssetFormGurdianshipItem] NOT Sucessful... FormID {MaxFormID}");
                    }
                    else
                    {
                        MyMessage.Add($"INSERT INTO [AssetFormGurdianshipItem] Sucessful... FormID {MaxFormID}");
                    }

                    #endregion

                    UpdateAsset(); // Update Asset Record
                }
                #endregion


            }

            IsCompleted = true;

        }


        #region Update Asset Record
        private void UpdateAsset()
        {


            var _AssetID = AssetRow!.Field<int>("AssetID");
            if (_AssetID != 0)
            {
                // Add Transaction ID 
                var _Guid = Guid.NewGuid().ToString();
                var _Title = $"Excel: Update asset {_AssetID} GuardianShip Form Id {MaxFormID}";
                UpdateTransaction(_Guid, _Title);

                //Query for Update Asset Record
                var _Query = new StringBuilder();

                _Query.Append("UPDATE [Assets] ");
                _Query.Append($"SET [UpdateTransactionID] = '{_Guid}',");
                _Query.Append($"[GurdianShipFormID] = '{MaxFormID}' ");
                _Query.Append($"WHERE [AssetID]={_AssetID}");
                MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
                var Records = MyCommand.ExecuteNonQuery();
                if (Records <= 0)
                {
                    MyMessage.Add($"UPDATE [Assets] NOT Sucessful... FormID {MaxFormID}");
                }
                else
                {
                    MyMessage.Add($"UPDATE [Assets] Sucessful... FormID {MaxFormID}");
                }
            }


        }
        #endregion



        #region Asset Transaction Log
        private void AssetTransactionLog(DataRow assetRow)
        {
            #region Query Text
            var _Query = new StringBuilder();
            _Query.AppendLine("INSERT INTO [AssetTransactionLog] ");
            _Query.AppendLine("([AssetID]");
            _Query.AppendLine(",[AssetClassID]");
            _Query.AppendLine(",[AssetCode]");
            _Query.AppendLine(",[AssetName]");
            _Query.AppendLine(",[AssetPhoto]");
            _Query.AppendLine(",[StartMaintancePlanDate]");
            _Query.AppendLine(",[MaintancePlanID]");
            _Query.AppendLine(",[ManufactureSN]");
            _Query.AppendLine(",[AssetUnit]");
            _Query.AppendLine(",[AssetWeightUnit]");
            _Query.AppendLine(",[AssetStateID]");
            _Query.AppendLine(",[MaintenanceStatusID]");
            _Query.AppendLine(",[LocationID]");
            _Query.AppendLine(",[GurdianShipFormID]");
            _Query.AppendLine(",[Quantity]");
            _Query.AppendLine(",[IsContainer]");
            _Query.AppendLine(",[IsInMaintenance]");
            _Query.AppendLine(",[IsTrackable]");
            _Query.AppendLine(",[IsRemoved]");
            _Query.AppendLine(",[IsDepreciable]");
            _Query.AppendLine(",[IsPendingState]");
            _Query.AppendLine(",[PendingProcessedFormID]");
            _Query.AppendLine(",[AssetPrice]");
            _Query.AppendLine(",[ColorID]");
            _Query.AppendLine(",[Weight]");
            _Query.AppendLine(",[RealtedAssetID]");
            _Query.AppendLine(",[Notes]");
            _Query.AppendLine(",[UserID]");
            _Query.AppendLine(",[AddTransactionID]");
            _Query.AppendLine(",[UpdateTransactionID]");
            _Query.AppendLine(",[TransactionDate]");
            _Query.AppendLine(",[TransactionReasoneID]");
            _Query.AppendLine(",[ProcessTypeID]");
            _Query.AppendLine(",[NewAssetFormID]");
            _Query.AppendLine(",[AssetGroupCode]");
            _Query.AppendLine(",[ProfitUnitAmount]");
            _Query.AppendLine(",[ExtraUnitCost]");
            _Query.AppendLine(",[CostPrice]");
            _Query.AppendLine(",[SalePrice]");

            _Query.AppendLine(") VALUES ");

            _Query.AppendLine("(@AssetID");
            _Query.AppendLine(",@AssetClassID");
            _Query.AppendLine(",@AssetCode");
            _Query.AppendLine(",@AssetName");
            _Query.AppendLine(",@AssetPhoto");
            _Query.AppendLine(",@StartMaintancePlanDate");
            _Query.AppendLine(",@MaintancePlanID");
            _Query.AppendLine(",@ManufactureSN");
            _Query.AppendLine(",@AssetUnit");
            _Query.AppendLine(",@AssetWeightUnit");
            _Query.AppendLine(",@AssetStateID");
            _Query.AppendLine(",@MaintenanceStatusID");
            _Query.AppendLine(",@LocationID");
            _Query.AppendLine(",@GurdianShipFormID");
            _Query.AppendLine(",@Quantity");
            _Query.AppendLine(",@IsContainer");
            _Query.AppendLine(",@IsInMaintenance");
            _Query.AppendLine(",@IsTrackable");
            _Query.AppendLine(",@IsRemoved");
            _Query.AppendLine(",@IsDepreciable");
            _Query.AppendLine(",@IsPendingState");
            _Query.AppendLine(",@PendingProcessedFormID");
            _Query.AppendLine(",@AssetPrice");
            _Query.AppendLine(",@ColorID");
            _Query.AppendLine(",@Weight");
            _Query.AppendLine(",@RealtedAssetID");
            _Query.AppendLine(",@Notes");
            _Query.AppendLine(",@UserID");
            _Query.AppendLine(",@AddTransactionID");
            _Query.AppendLine(",@UpdateTransactionID");
            _Query.AppendLine(",@TransactionDate");
            _Query.AppendLine(",@TransactionReasoneID");
            _Query.AppendLine(",@ProcessTypeID");
            _Query.AppendLine(",@NewAssetFormID");
            _Query.AppendLine(",@AssetGroupCode");
            _Query.AppendLine(",@ProfitUnitAmount");
            _Query.AppendLine(",@ExtraUnitCost");
            _Query.AppendLine(",@CostPrice");
            _Query.AppendLine(",@SalePrice");
            _Query.AppendLine(")");

            MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
            MyCommand.Parameters.AddWithValue("@AssetID", AssetRow!["AssetID"]);
            MyCommand.Parameters.AddWithValue("@AssetClassID", AssetRow!["AssetClassID"]);
            MyCommand.Parameters.AddWithValue("@AssetCode", AssetRow!["AssetCode"]);
            MyCommand.Parameters.AddWithValue("@AssetName", AssetRow!["AssetName"]);
            MyCommand.Parameters.AddWithValue("@AssetPhoto", AssetRow!["AssetPhoto"]);
            MyCommand.Parameters.AddWithValue("@StartMaintancePlanDate", AssetRow!["StartMaintancePlanDate"]);
            MyCommand.Parameters.AddWithValue("@MaintancePlanID", AssetRow!["MaintancePlanID"]);
            MyCommand.Parameters.AddWithValue("@ManufactureSN", AssetRow!["ManufactureSN"]);
            MyCommand.Parameters.AddWithValue("@AssetUnit", AssetRow!["AssetUnit"]);
            MyCommand.Parameters.AddWithValue("@AssetWeightUnit", AssetRow!["AssetWeightUnit"]);
            MyCommand.Parameters.AddWithValue("@AssetStateID", AssetRow!["AssetStateID"]);
            MyCommand.Parameters.AddWithValue("@MaintenanceStatusID", AssetRow!["MaintenanceStatusID"]);
            MyCommand.Parameters.AddWithValue("@LocationID", AssetRow!["LocationID"]);
            MyCommand.Parameters.AddWithValue("@GurdianShipFormID", AssetRow!["GurdianShipFormID"]);
            MyCommand.Parameters.AddWithValue("@Quantity", AssetRow!["Quantity"]);
            MyCommand.Parameters.AddWithValue("@IsContainer", AssetRow!["IsContainer"]);
            MyCommand.Parameters.AddWithValue("@IsInMaintenance", AssetRow!["IsInMaintenance"]);
            MyCommand.Parameters.AddWithValue("@IsTrackable", AssetRow!["IsTrackable"]);
            MyCommand.Parameters.AddWithValue("@IsRemoved", AssetRow!["IsRemoved"]);
            MyCommand.Parameters.AddWithValue("@IsDepreciable", AssetRow!["IsDepreciable"]);
            MyCommand.Parameters.AddWithValue("@IsPendingState", AssetRow!["IsPendingState"]);
            MyCommand.Parameters.AddWithValue("@PendingProcessedFormID", AssetRow!["PendingProcessedFormID"]);
            MyCommand.Parameters.AddWithValue("@AssetPrice", 0.00);
            MyCommand.Parameters.AddWithValue("@ColorID", AssetRow!["ColorID"]);
            MyCommand.Parameters.AddWithValue("@Weight", AssetRow!["Weight"]);
            MyCommand.Parameters.AddWithValue("@RealtedAssetID", AssetRow!["RealtedAssetID"]);
            MyCommand.Parameters.AddWithValue("@Notes", AssetRow!["Notes"]);
            MyCommand.Parameters.AddWithValue("@UserID", UserID);                            // SYSTEM USER ID
            MyCommand.Parameters.AddWithValue("@AddTransactionID", AssetRow!["AddTransactionID"]);
            MyCommand.Parameters.AddWithValue("@UpdateTransactionID", AssetRow!["UpdateTransactionID"]);
            MyCommand.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
            MyCommand.Parameters.AddWithValue("@TransactionReasoneID", FormProcessReasonID);             // Transaction Reason ID
            MyCommand.Parameters.AddWithValue("@ProcessTypeID", ProcessTypeID);                   // Process Type ID
            MyCommand.Parameters.AddWithValue("@NewAssetFormID", MaxFormID);                       // Asset Form ID from Asset form.
            MyCommand.Parameters.AddWithValue("@AssetGroupCode", AssetRow!["AssetGroupCode"]);
            MyCommand.Parameters.AddWithValue("@ProfitUnitAmount", 0.00);
            MyCommand.Parameters.AddWithValue("@ExtraUnitCost", 0.00);
            MyCommand.Parameters.AddWithValue("@CostPrice", 0.00);
            MyCommand.Parameters.AddWithValue("@SalePrice", 0.00);
            #endregion


            var result = MyCommand.ExecuteNonQuery();
            if (result > 0)
            {
                var _Text = "SELECT MAX([AssetTransactionID]) FROM [AssetTransactionLog]";
                MyCommand.CommandText = _Text;
                var _Result = MyCommand.ExecuteScalar();
                if (_Result != null && int.TryParse(_Result.ToString(), out int _MaxID))
                {
                    NewAssetTransactionLogID = _MaxID;
                }
            }
        }
        #endregion

        #region Get Asset DataRow
        private DataRow? GetAssetRow(string AssetTagID)
        {
            var _Query = new StringBuilder();
            _Query.AppendLine("SELECT [A].* FROM [Assets] [A]");
            _Query.AppendLine("LEFT JOIN [AssetTag] [T] ON [T].[AssetID] = [A].[AssetID]");
            _Query.AppendLine($"WHERE [T].[TagID] = '{AssetTagID}'");

            MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
            SqlDataAdapter _Adapter = new SqlDataAdapter(MyCommand);
            DataTable _DataTable = new DataTable();
            _Adapter.Fill(_DataTable);
            if (_DataTable.Rows.Count > 0)
            {
                return _DataTable.Rows[0];
            }
            else
            {
                return null;
            }



        }
        #endregion

        #region Get Employee ID
        private int GetEmployeeID(string EmailID)
        {
            var _Query = new StringBuilder();
            _Query.AppendLine("SELECT [EmployeeID] FROM [Employee] WHERE [EmployeeSocialID] = @EmailID");
            MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
            MyCommand.Parameters.AddWithValue("@EmailID", EmailID);
            var _Result = MyCommand.ExecuteScalar();
            if (_Result != null && int.TryParse(_Result.ToString(), out int _EmployeeID))
            {
                return _EmployeeID;
            }
            return 0;
        }
        #endregion

        #region Asset Form and Asset Form Log
        public int MaxAsetFormID()
        {
            var _Text = "SELECT MAX([FormID]) FROM [AssetForm]";
            MyCommand.CommandText = _Text;
            var _Result = MyCommand.ExecuteScalar();
            if (_Result != null && int.TryParse(_Result.ToString(), out int _MaxID))
            {
                return _MaxID + 1;
            }
            return 0;
        }

        private void AssetFormLog(string _EmailID, string _EmployeeName)
        {
            #region Asset Form Log

            //MyGUID = Guid.NewGuid().ToString();
            //UpdateTransaction(MyGUID, $"Update Guardian {_EmailID} {_EmployeeName}");
            var _Query = new StringBuilder();

            _Query.AppendLine("INSERT INTO [AssetFormLog] ( ");
            //_Query.AppendLine("[FormLogID],"); Auto generate ID
            _Query.AppendLine("[FormID],");
            _Query.AppendLine("[FormTitle],");
            _Query.AppendLine("[FormProcessStateID],");
            _Query.AppendLine("[FormProcessedByUserID],");
            _Query.AppendLine("[Notes],");
            _Query.AppendLine("[TransactionID] ");
            _Query.AppendLine(") VALUES ( ");
            _Query.AppendLine("@FormID,");
            _Query.AppendLine("@FormTitle,");
            _Query.AppendLine("@FormProcessStateID,");
            _Query.AppendLine("@FormProcessedByUserID,");
            _Query.AppendLine("@Notes,");
            _Query.AppendLine("@TransactionID) ");

            MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
            MyCommand.Parameters.AddWithValue("@FormID", MaxFormID);
            MyCommand.Parameters.AddWithValue("@FormTitle", $"Excel: Asset Cusdodian Add {_EmployeeName} {_EmailID}");
            MyCommand.Parameters.AddWithValue("@FormProcessStateID", ProcessStateID);
            MyCommand.Parameters.AddWithValue("@FormProcessedByUserID", UserID);
            MyCommand.Parameters.AddWithValue("@Notes", DBNull.Value);
            MyCommand.Parameters.AddWithValue("@TransactionID", MyGUID);
            int Records = MyCommand.ExecuteNonQuery();

            if (Records <= 0)
            {
                MyMessage.Add($"INSERT INTO [AssetFormLog] NOT Sucessful... FormID {MaxFormID}");
            }
            else
            {
                MyMessage.Add($"INSERT INTO [AssetFormLog] Sucessful... FormID {MaxFormID}");
            }


            #endregion
        }
        #endregion

        #region Transaction ID
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

            MyCommand = new SqlCommand(_Query.ToString(), MyConnection);
            MyCommand.Parameters.AddWithValue("@TransactionID", _GUID);
            MyCommand.Parameters.AddWithValue("@TransactionName", _Title);
            MyCommand.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
            MyCommand.Parameters.AddWithValue("@TransactionByUserID", UserID);
            MyCommand.Parameters.AddWithValue("@TransactionType", 1);

            var Records = MyCommand.ExecuteNonQuery();
            if (Records > 0)
            {
                MyMessage.Add($"{Records}: Add {_GUID}, _Title {_Title}");
            }
            else
            {
                MyMessage.Add($"ERROR: NOT Add {_GUID}, _Title {_Title}");
            }

        }
        #endregion
    }
}
