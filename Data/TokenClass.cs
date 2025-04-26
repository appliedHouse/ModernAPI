using System.Text;
using System;
using ModernAPI.Pages;
using System.Text.Json;

namespace ModernAPI.Data
{
    public class TokenClass
    {
        string UserName;
        string Password;
        string Url;

        public string TokenText { get; set; } = string.Empty;
        
        public string TokenHeader { get; set; } = string.Empty;
        public string TokenPayLoad { get; set; } = string.Empty;    
        public string TokenSignature { get; set; } = string.Empty;


        public IConfiguration Config { get; set; }

        public TokenClass(IConfiguration _Config)
        {
            Config = _Config;
            UserName = Config.GetValue<string>("ApiUser") ?? "";
            Password = Config.GetValue<string>("ApiPassword") ?? "";
            Url = "https://localhost/API/api/Auth/GetToken";
        }


        public async Task<string> GetToken()
        {
            var _Text = string.Empty;
            using var client = new HttpClient();

            var json = $"{{\"username\":\"{UserName}\",\"password\":\"{Password}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Url, content);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                _Text = token;
            }
            else
            {
                _Text = string.Empty;
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}, {error}");
            }
            

            var jsonDoc = JsonDocument.Parse(_Text);
            TokenText = jsonDoc.RootElement.GetProperty("token").GetString() ?? "";

            return TokenText;
        }
    }
}

