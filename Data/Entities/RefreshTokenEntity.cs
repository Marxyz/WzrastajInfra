using System;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// Explanation: EntityFrameworkCore needs it anyway

namespace Data.Entities
{
    public class RefreshTokenEntity : BaseEntity
    {
        public RefreshTokenEntity(string token, DateTime expires, Guid userId, string remoteIpAddress)
        {
            Token = token;
            Expires = expires;
            UserId = userId;
            RemoteIpAddress = remoteIpAddress;
        }

        public string Token { get; private set; }
        public DateTime Expires { get; private set; }
        public Guid UserId { get; private set; }
        public bool Active => DateTime.UtcNow <= Expires;
        public string RemoteIpAddress { get; private set; }
    }
}