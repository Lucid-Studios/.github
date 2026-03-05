using System.Security.Cryptography;
using System.Text;

namespace EngramGovernance.Services;

public sealed class EncryptionService
{
    public SelfGelPayload PrepareSelfGelPayload(
        string cmeId,
        Guid soulFrameId,
        Guid contextId,
        string cognitionBody,
        bool encryptForCrypticLayer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(cognitionBody);

        var bodyBytes = Encoding.UTF8.GetBytes(cognitionBody);
        var bodyHash = HashHex(bodyBytes);

        if (!encryptForCrypticLayer)
        {
            return new SelfGelPayload(
                bodyHash,
                $"selfgel://{contextId:D}/{bodyHash[..16]}",
                false);
        }

        var keyMaterial = Encoding.UTF8.GetBytes($"{cmeId}|{soulFrameId:D}|steward");
        var key = SHA256.HashData(keyMaterial);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(bodyBytes, 0, bodyBytes.Length);

        var ivAndCipher = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, ivAndCipher, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, ivAndCipher, aes.IV.Length, cipherBytes.Length);

        var cipherHash = HashHex(ivAndCipher);
        var storagePointer = $"cselfgel://{contextId:D}/{bodyHash[..16]}/{cipherHash[..16]}";
        return new SelfGelPayload(bodyHash, storagePointer, true);
    }

    public static string ComputeBodyHash(string cognitionBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cognitionBody);
        return HashHex(Encoding.UTF8.GetBytes(cognitionBody));
    }

    private static string HashHex(byte[] value) =>
        Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
}

public sealed record SelfGelPayload(
    string BodyHash,
    string StoragePointer,
    bool EncryptForCrypticLayer);
