using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Data.Entities
{
    public class UserEntity : BaseEntity
    {
        // ReSharper disable once InconsistentNaming
        private readonly List<RefreshTokenEntity> refreshTokens = new List<RefreshTokenEntity>();

        // ReSharper disable once InconsistentNaming
        [ForeignKey(nameof(IdentityId))] public IdentityUser IdentityAppUser { get; set; }
        public string IdentityId { get; set; }
        public string DisplayName => IdentityAppUser.UserName;
        public string Email => IdentityAppUser.Email;
        public string PasswordHash => IdentityAppUser.PasswordHash;
        public IReadOnlyCollection<RefreshTokenEntity> RefreshTokens => refreshTokens.AsReadOnly();

        public bool HasValidRefreshToken(string refreshToken)
        {
            return refreshTokens.Any(rt => rt.Token == refreshToken && rt.Active);
        }

        public void AddRefreshToken(string token, string remoteIpAddress, double daysToExpire = 5)
        {
            refreshTokens.Add(new RefreshTokenEntity(token, DateTime.UtcNow.AddDays(daysToExpire), Id,
                remoteIpAddress));
        }
    }
}