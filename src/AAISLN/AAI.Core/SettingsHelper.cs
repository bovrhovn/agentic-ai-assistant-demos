using System.Security.Cryptography;
using System.Text;

namespace AAI.Core;

public static class SettingsHelper
{
    public static string GetUniqueValue(this string input)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sBuilder = new StringBuilder();
        foreach (var currentByte in data) sBuilder.Append(currentByte.ToString("x2"));

        return sBuilder.ToString();
    }

}