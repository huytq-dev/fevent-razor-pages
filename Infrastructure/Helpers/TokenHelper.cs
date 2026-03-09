using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace Infrastructure;

internal static class TokenHelper
{
    public static string GenerateToken(int size = 64)
    {
        byte[] bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }
}