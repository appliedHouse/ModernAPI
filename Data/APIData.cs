using Microsoft.Extensions.Logging;
using ModernAPI.Models;
using ModernAPI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace ModernAPI.Data
{
    public class APIData
    {
        //private static readonly HttpClient client = new HttpClient();
        public IHttpClientFactory HttpClientFactory { get; set; }
        public string TokenText { get; set; } = string.Empty;

        public APIData(IHttpClientFactory httpClientFactory, AuthTokenService tokenService)
        {
            HttpClientFactory = httpClientFactory;
            TokenText = tokenService.TokenText;
        }

        public async Task<T?> GetData<T>(EndPoints _EndPoint, string _Parameters)
        {
            try
            {
                var _HttpClient = HttpClientFactory.CreateClient();

                var model = APIFunctions.GetEndPoint(_EndPoint, _Parameters);
                using var Request = new HttpRequestMessage(HttpMethod.Get, model.APIUrl);

                Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenText);
                Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var Response = await _HttpClient.SendAsync(Request);

                if (!Response.IsSuccessStatusCode)
                {
                    //_logger.LogError(Response.ReasonPhrase, "Failed to fetch data from API");
                    var error = await Response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"HTTP Error: {Response.StatusCode}. Details: {error}");
                }

                var _response = await Response.Content.ReadFromJsonAsync<T>(DefaultJsonOptions);

                return _response;
            }
            catch (System.Text.Json.JsonException)
            {
                return default;
            }
        }

        #region Response API Model

        public class ApiResponse<T>
        {
            [JsonPropertyName("isSuccess")]
            public bool IsSuccess { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("returnedValue")]
            public List<T>? Items { get; set; }

            public List<T>? GetResults()
            {
                return Items ?? [];
            }
        }

        #endregion


        #region Convertor for Single or Array Data


        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new SingleOrArrayConverterFactory() }
        };

        public class SingleOrArrayConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert.IsGenericType &&
                       typeToConvert.GetGenericTypeDefinition() == typeof(List<>);
            }

            public override System.Text.Json.Serialization.JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {

                var elementType = typeToConvert.GetGenericArguments()[0];
                var converterType = typeof(SingleOrArrayConverter<>).MakeGenericType(elementType);
                return (System.Text.Json.Serialization.JsonConverter?)Activator.CreateInstance(converterType);
            }
        }

        public class SingleOrArrayConverter<T> : System.Text.Json.Serialization.JsonConverter<List<T>>
        {
            public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var list = new List<T>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        var item = System.Text.Json.JsonSerializer.Deserialize<T>(ref reader, options);
                        if (item != null)
                            list.Add(item);
                    }
                    return list;
                }
                else
                {
                    var item = System.Text.Json.JsonSerializer.Deserialize<T>(ref reader, options);
                    return item != null ? new List<T> { item } : new List<T>();
                }
            }

            public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
            {
                if (value.Count == 1)
                {
                    System.Text.Json.JsonSerializer.Serialize(writer, value[0], options);
                }
                else
                {
                    System.Text.Json.JsonSerializer.Serialize(writer, value, options);
                }
            }
        }
        #endregion


        #region Edit Asset

        public async Task<string> CreateOrEditAssetAsync(EditAssetModel asset)
        {
            string apiUrl = "https://localhost/API/api/AssetOperation/CreateOrEditAsset";

            var _HttpClient = HttpClientFactory.CreateClient();
            var API_Model = APIFunctions.GetEndPoint(EndPoints.assetEdit, string.Empty);

            _HttpClient.DefaultRequestHeaders.Accept.Clear();
            _HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenText);

            var JsonText = System.Text.Json.JsonSerializer.Serialize(asset, new JsonSerializerOptions 
            {
                WriteIndented=true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var content = new StringContent(JsonText, Encoding.UTF8, "application/json");
            var response = await _HttpClient.PostAsync(API_Model.APIUrl, content);


            //HttpResponseMessage response = await _HttpClient.PostAsJsonAsync(API_Model.APIUrl, asset);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {error}");
            }
        }


        #endregion


        #region convertion Json Data into Data Table
        public class ApiDataTableResult
        {
            public bool IsSuccess { get; set; }
            public string? Message { get; set; }
            public DataTable? Data { get; set; }
        }

        public async Task<ApiDataTableResult> GetDataTableAsync(EndPoints _EndPoint, string _Parameters)
        {
            var result = new ApiDataTableResult();

            try
            {
                var _HttpClient = HttpClientFactory.CreateClient();
                var model = APIFunctions.GetEndPoint(_EndPoint, _Parameters);

                using var Request = new HttpRequestMessage(HttpMethod.Get, model.APIUrl);
                Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenText);
                Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var Response = await _HttpClient.SendAsync(Request);
                var json = await Response.Content.ReadAsStringAsync();

                if (!Response.IsSuccessStatusCode)
                {
                    result.IsSuccess = false;
                    result.Message = $"HTTP Error: {Response.StatusCode}. Details: {json}";
                    return result;
                }

                var root = JObject.Parse(json);
                result.IsSuccess = root["isSuccess"]?.Value<bool>() ?? true;
                result.Message = root["message"]?.ToString();

                var dataToken = root["returnedValue"];
                if (dataToken == null || dataToken.Type == JTokenType.Null)
                {
                    result.Data = new DataTable();
                    return result;
                }

                // 🔹 If it's a single object, wrap it in an array
                if (dataToken.Type == JTokenType.Object)
                    dataToken = new JArray(dataToken);

                // 🔹 Create DataTable manually to flatten simple properties
                var array = (JArray)dataToken;
                var table = new DataTable("Result");

                // Build columns from first record’s simple properties only
                var first = (JObject)array.First;
                foreach (var prop in first.Properties())
                {
                    if (prop.Value.Type != JTokenType.Object && prop.Value.Type != JTokenType.Array)
                        table.Columns.Add(prop.Name);
                }

                // Fill rows
                foreach (var obj in array)
                {
                    var row = table.NewRow();
                    foreach (var prop in ((JObject)obj).Properties())
                    {
                        if (table.Columns.Contains(prop.Name) &&
                            prop.Value.Type != JTokenType.Object &&
                            prop.Value.Type != JTokenType.Array)
                        {
                            row[prop.Name] = prop.Value.ToString();
                        }
                    }
                    table.Rows.Add(row);
                }

                result.Data = table;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error: {ex.Message}";
                return result;
            }
        }
        #endregion

    }
}
