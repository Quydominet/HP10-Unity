using System.Text;

public static class Encryptor
{
    private static int GetSeed(string key)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in key)
                hash = hash * 31 + c;
            return hash;
        }
    }

    public static string Encrypt(string key, string input)
    {
        var rand = new System.Random(GetSeed(key));
        var sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            int b = (byte)c;
            int offset = rand.Next(0, 128);
            sb.Append((char)(b ^ offset));
        }

        return sb.ToString();
    }

    // XOR is its own inverse, decryption is encryption
    public static string Decrypt(string key, string input) => Encrypt(key, input);
}