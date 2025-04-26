using ModernAPI.Data;
using System.Text;
using System.Text.Json;

namespace ModernAPI.Pages
{
    public partial class Assets
    {
        public AssetsModal MyModal { get; set; } = new AssetsModal();
        public string TokenText { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty; 
        public string Endpoint { get; set; } = string.Empty;
        public string APIUrl { get; set; } = string.Empty;
        
        public HttpClient _HttpClient { get; set; } = new HttpClient();

        public async void GetAssets()
        {
            MyModal.AssetList = await GetAssetList();
            await InvokeAsync(StateHasChanged);
        }

        public async Task<List<Asset>> GetAssetList()
        {
            BaseUrl = "https://localhost/api";
            Endpoint = "/api/Asset/GetPagedAssets";
            APIUrl = $"{BaseUrl}{Endpoint}?{GetParameters()}";

            TokenClass TokenService = new TokenClass(Config);
            TokenText = await TokenService.GetToken();

            var Request = new HttpRequestMessage(HttpMethod.Get, APIUrl);

            Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenText);
            Request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            var Response = await _HttpClient.SendAsync(Request);

            if(Response.IsSuccessStatusCode)
            {
                var resultText = await Response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AssetResponse>(resultText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var assets = result?.ReturnedValue;
                if(assets is not null)
                {
                    return assets;
                }
                else
                {
                    return [];
                }
                
            }
            else
            {
                // Handle error response
                var error = await Response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {Response.StatusCode}, {error}");
            }

        }

        public string GetParameters()
        {

            //https://localhost/api/api/Asset/GetPagedAssets?ClassificationId=1471PageNum=0&PageSize=100

            // ClassificationId=1471&PageNum=0&PageSize=100

            var _Text = new StringBuilder();

            if(MyModal.locationID > 0)
            {
                _Text.Append($"LocationId={MyModal.locationID}");
            }

            if (MyModal.ClassID > 0)
            {
                if(_Text.Length>0) { _Text.Append('&'); }

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


        public class AssetsModal
        {
            public int locationID { get; set; }
            public int ClassID { get; set; }
            public int EmployeeID { get; set; }
            public DateTime LastSyncDate { get; set; }
            public int PageNum { get; set; } = 0;
            public int PageSize { get; set; } = 100;
            public List<Asset> AssetList {get; set;}
        }

        public class AssetResponse
        {
            public bool IsSuccess { get; set; }
            public string? Message { get; set; }
            public List<Asset> ReturnedValue { get; set; }
        }


    }
}