using static ModernAPI.Pages.Assets;
using System.Buffers.Text;
using System.Net.Http;
using System.Text.Json;

namespace ModernAPI.Data
{
    public class GetAsset
    {
        public AssetsModal MyModal { get; set; } = new AssetsModal();
        public string TokenText { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string APIUrl { get; set; } = string.Empty;
        public HttpClient _HttpClient { get; set; } = new HttpClient();
        public IConfiguration Config { get; set; }


        public async Task<Asset> GetAssetByTag(string _TagID)
        {
            // https://localhost/API/api/Asset/GetAssetDetails?tagId=A0130000000B022430018843


            BaseUrl = "https://localhost/API";
            Endpoint = "/api/Asset/GetAssetDetails";
            APIUrl = $"{BaseUrl}{Endpoint}?tagId={_TagID}";

            TokenClass TokenService = new TokenClass(Config);
            TokenText = await TokenService.GetToken();

            var Request = new HttpRequestMessage(HttpMethod.Get, APIUrl);

            Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenText);
            Request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            var Response = await _HttpClient.SendAsync(Request);

            if (Response.IsSuccessStatusCode)
            {
                var resultText = await Response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AssetResponse>(resultText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var assets = result?.ReturnedValue;
                if (assets is not null)
                {
                    return assets;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                // Handle error response
                var error = await Response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {Response.StatusCode}, {error}");
            }

        }

    }

    public class AssetResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public Asset ReturnedValue { get; set; }            
    }

}
