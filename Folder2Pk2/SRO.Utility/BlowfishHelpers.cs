using SRO.Security;

using System;
using System.Text;

namespace SRO.Utility
{
    public static class BlowfishHelpers
    {
        public static void Initialize(this Blowfish Blowfish, string key)
        {
            // Using the default base key from Silkroad Online
            Blowfish.Initialize(key, new byte[] { 0x03, 0xF8, 0xE4, 0x44, 0x88, 0x99, 0x3F, 0x64, 0xFE, 0x35 });
        }
        public static void Initialize(this Blowfish Blowfish, string key, byte[] base_key)
        {
            // Get bytes from text
            byte[] a_key = Encoding.Default.GetBytes(key);

            // Max count of 56 key bytes
            var keyLength = a_key.Length;
            if (keyLength > 56)
                keyLength = 56;

            // This is the Silkroad base key used in all versions
            byte[] b_key = new byte[56];

            // Copy key to array to keep the b_key at 56 bytes. b_key has to be bigger than a_key
            // to be able to xor every index of a_key.
            Array.ConstrainedCopy(base_key, 0, b_key, 0, base_key.Length);

            // Their key modification algorithm for the final blowfish key
            byte[] bf_key = new byte[keyLength];
            for (byte x = 0; x < keyLength; ++x)
                bf_key[x] = (byte)(a_key[x] ^ b_key[x]);

            Blowfish.Initialize(bf_key);
        }
    }
}
