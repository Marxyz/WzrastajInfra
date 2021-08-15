using System;
using Domain.Enums;

namespace Domain.Models
{
    public class Token
    {
        private Token()
        {
        }

        public string Value { get; private set; }
        public DateTime PassedInMemoryTime { get; private set; }
        public TokenType Type { get; private set; }

        public static Token Create(string value, TokenType type)
        {
            return new Token { Type = type, PassedInMemoryTime = DateTime.Now, Value = value };
        }
    }
}