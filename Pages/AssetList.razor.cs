using ModernAPI.Data;
using System.Text;
using static ModernAPI.Data.APIData;
using static ModernAPI.Models.AssetModel;



namespace ModernAPI.Pages
{
    public partial class AssetList
    {
        public AssetsPageModal MyModal { get; set; } = new();
        public string MyMessage { get; set; } = string.Empty;

        public HttpClient _HttpClient { get; set; } = new HttpClient();

        

        public async void GetAssets()
        {
            // Enable if need logger fuction.
            //var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //var logger = loggerFactory.CreateLogger<APIData>();

            APIData APIService = new APIData(HttpClientFactory, TokenService);
            var result = await APIService.GetData<ApiResponse<Asset>>(EndPoints.assets, GetParameters());
            var resultTB = await APIService.GetDataTableAsync(EndPoints.assets, GetParameters());
            MyModal.AssetList = result?.GetResults() ?? [];

            await InvokeAsync(StateHasChanged);
        }

        public string GetParameters()
        {

            //https://localhost/api/api/Asset/GetPagedAssets?ClassificationId=1471PageNum=0&PageSize=100
                        

            var _Text = new StringBuilder();

            if (MyModal.locationID > 0)
            {
                _Text.Append($"LocationId={MyModal.locationID}");
            }

            if (MyModal.ClassID > 0)
            {
                if (_Text.Length > 0) { _Text.Append('&'); }

                _Text.Append($"ClassificationId={MyModal.ClassID}");
            }

            if (MyModal.LastSyncDate.ToString("yyyy-mm-dd") != "0001-00-01")
            {
                if (_Text.Length > 1) { _Text.Append('&'); }

                string DateFormat = "yyyy-mm-dd";
                _Text.Append($"LastSyncDate={MyModal.LastSyncDate.ToString(DateFormat)}");
            }

            if (_Text.Length > 0) { _Text.Append('&'); }
            _Text.Append($"PageNum={MyModal.PageNum}&PageSize={MyModal.PageSize}");


            return _Text.ToString();

        }


        public class AssetsPageModal
        {
            public int locationID { get; set; }
            public int ClassID { get; set; }
            public int EmployeeID { get; set; }
            public DateTime LastSyncDate { get; set; }
            public int PageNum { get; set; } = 0;
            public int PageSize { get; set; } = 100;
            public List<Asset> AssetList { get; set; }
            
        }

        private void ShowAssetInfo(int AssetID)
        {
            NavManager.NavigateTo($"/AssetsInfo/{AssetID}");
        }
        


    }
}