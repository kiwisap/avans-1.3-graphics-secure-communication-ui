using System;

namespace Assets.Code.Models
{
    [Serializable]
    public class AccessTokenResponseDto
    {
        public string TokenType { get; set; }

        public string AccessToken { get; set; }

        public long ExpiresIn { get; set; }

        public string RefreshToken { get; set; }
    }
}
