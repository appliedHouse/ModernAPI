using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ModernAPI.Data;
using ModernAPI.Models;
using static ModernAPI.Data.APIData;
using static ModernAPI.Models.AssetModel;

namespace ModernAPI.Pages.Assets
{
    public partial class AssetInfo
    {
        [Parameter] public int? AssetID { get; set; }
        public Asset MyModel { get; set; }

        internal List<Asset> assets = [];
        internal List<Location> locations = [];
        internal List<Classification> classifications = [];
        internal Asset CurrentAsset = new();
        internal EditAssetModel EditAsset = new();
        internal Asset? selectedAsset;
        internal bool showModal = false;
        internal bool showDetailsModal = false;
        internal string searchTerm = string.Empty;
        internal APIData APIService;

        protected override async Task OnInitializedAsync()
        {
            //await LoadAssets();
            APIService = new APIData(HttpClientFactory, TokenService);

            if (AssetID != null)
            {
                await GetAsset((int)AssetID);
                //await LoadLookupData();
            }
        }

        public async Task GetAsset(int _AssetID)
        {
            //var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //var logger = loggerFactory.CreateLogger<APIData>();

            //APIService = new APIData(HttpClientFactory, TokenService);
            var result = await APIService.GetData<ApiResponse<Asset>>(EndPoints.assetDetail, _AssetID.ToString());
            var resultDB = await APIService.GetDataTableAsync(EndPoints.assetDetail, _AssetID.ToString());
            MyModel = result?.Items?.FirstOrDefault() ?? new();
            CurrentAsset = MyModel;
            await InvokeAsync(StateHasChanged);
        }


        private void GetEditAssetData(Asset asset)
        {
            EditAsset = new EditAssetModel();
            {
                EditAsset.AssetID = asset.AssetID;
                EditAsset.AssetName = asset.AssetName;
                EditAsset.ManufactureSN = asset.ManufactureSN ?? "";
                EditAsset.Weight = asset.Weight ?? 0.00;
                EditAsset.Price = asset.Price ?? 0.00;
                EditAsset.ClassRefID = asset.ClassID ?? 0;
                EditAsset.ClassRefName = ""; // If available
                EditAsset.ParentClassRefID = 0; // If available
                EditAsset.ParentClassRefName = ""; // If available
                EditAsset.LocationRefID = asset.LocationID ?? 0;
                EditAsset.LocationRefName = ""; // If available
                EditAsset.ParentLocationRefID = 0; // If available
                EditAsset.ParentLocationRefName = ""; // If available
                EditAsset.AssetStateID = asset.AssetStateID ?? 0;
                EditAsset.WeightUnitID = asset.WeightUnitID ?? 0;
                EditAsset.AssetUnitID = asset.AssetUnitID ?? 0;
                EditAsset.GuardianShipID = asset.GurdianShipID; // Property name fixed
                EditAsset.Quantity = asset.Quantity ?? 0;
                EditAsset.IsContainer = asset.IsContainer ?? false;
                EditAsset.AssetCode = asset.AssetCode ?? 0;
                EditAsset.AssetGroupCode = asset.AssetGroupCode;
                EditAsset.OriginalPrice = asset.OriginalPrice;
                EditAsset.CostPrice = asset.CostPrice;
                EditAsset.SalePrice = asset.SalePrice;
                EditAsset.ExtraCost = asset.ExtraUnitCost; // Property renamed
                EditAsset.Profit = asset.ProfitUnitAmount; // Property renamed
                EditAsset.AssetPhoto = asset.AssetPhoto;
                EditAsset.TagList = asset.TagList?.Select(t => new AssetTag
                {
                    TagID = t.TagID,
                    TagTypeKey = t.TagTypeKey
                }).ToList();
                EditAsset.IsReceived = asset.IsRecieved; // Fixed naming
                EditAsset.IsPending = asset.IsPennding ?? false; // Fixed naming
            }
            ;
        }




        private async Task SaveAsset()
        {
            try
            {
                if (CurrentAsset == null)
                {
                    await JS.InvokeVoidAsync("alert", "Current Asset not selected.");
                    return;
                }

                if (CurrentAsset.AssetID <= 0)
                {
                    await JS.InvokeVoidAsync("alert", "Invalid Asset ID.");
                    return;
                }

                GetEditAssetData(CurrentAsset);

                var success = await Save();

                if (success)
                    await JS.InvokeVoidAsync("alert", "Record saved successfully.");
                else
                    await JS.InvokeVoidAsync("alert", "Record not saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving asset: {ex.Message}");
                await JS.InvokeVoidAsync("alert", "An error occurred while saving.");
            }
        }


        public class AssetOperationResult
        {
            public int AssetID { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }


        private async Task<bool> Save()
        {
            if (EditAsset == null) { await JS.InvokeVoidAsync("No Asset Selected."); return false; }

            string message = string.Empty;

            try
            {
                message = await APIService.CreateOrEditAssetAsync(EditAsset);
                if(string.IsNullOrEmpty(message)) { return false; }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return true;
        }

        private void CloseModal(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            NavManager.NavigateTo("/");
        }
    }
}

