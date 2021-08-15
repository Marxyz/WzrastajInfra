namespace infrastructure.Models
{
    public class RefreshTokenProcessingModel
    {
        public readonly string Location;

        public RefreshTokenProcessingModel(string accessToken, string refreshToken, string signingKey, string location)
        {
            Location = location;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            SigningKey = signingKey;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string SigningKey { get; set; }
    }
}