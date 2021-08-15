using System;
using System.Security.Cryptography;

namespace infrastructure.Services
{
    public interface ITokenFactory
    {
        string GenerateRandomStringToken(int size = 32);
    }

    public sealed class TokenFactory : ITokenFactory
    {
        public string GenerateRandomStringToken(int size = 32)
        {
            var randomNumber = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}