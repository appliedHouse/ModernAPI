using ModernAPI.Data;

namespace ModernAPI.Services
{
    public class AuthTokenService
    {
        private readonly IConfiguration Config;
        private TokenClass TokenClass { get; set; }
        public string TokenText => TokenClass.TokenText;

        public AuthTokenService(IConfiguration configuration)
        {
            Config = configuration;
            TokenClass = new(Config);
        }
    }
}