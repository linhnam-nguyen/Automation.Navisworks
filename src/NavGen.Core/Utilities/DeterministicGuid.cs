using System.Security.Cryptography;
using System.Text;

namespace NavGen.Core.Utilities;

public static class DeterministicGuid
{
    public static Guid Create(string namespaceName, string value)
    {
        using var algorithm = MD5.Create();
        var namespaceBytes = Encoding.UTF8.GetBytes(namespaceName);
        var valueBytes = Encoding.UTF8.GetBytes(value);

        var buffer = new byte[namespaceBytes.Length + valueBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, buffer, 0, namespaceBytes.Length);
        Buffer.BlockCopy(valueBytes, 0, buffer, namespaceBytes.Length, valueBytes.Length);

        var hash = algorithm.ComputeHash(buffer);
        hash[6] = (byte)((hash[6] & 0x0F) | (3 << 4));
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash);
    }
}
