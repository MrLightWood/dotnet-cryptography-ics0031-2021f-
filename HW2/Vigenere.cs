using System;
using static HW2.Utils;
using System.Text;

namespace HW2
{
    public static class Vigenere
    {
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                              "abcdefghijklmnopqrstuvwxyz" +
                                              "0123456789" +
                                              "+/="; // and "=" as filler
        
        public static (bool, string) Encrypt(string plaintext, string key)
        {
            var base64Str = Base64Encode(plaintext);
            var extendedSecretKey = ExtendKey(key, base64Str);
            var cipherText = "";
            for (var i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = Base64Alphabet.IndexOf(keyChr);
                chrIndex = chrIndex + keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex += Base64Alphabet.Length;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex -= (Base64Alphabet.Length);
                }

                cipherText += Base64Alphabet[chrIndex];
            }

            return (true, cipherText);
        }
        
        public static (bool, string) Decrypt(string ciphertext, string key)
        {
            var extendedSecretKey = ExtendKey(key, ciphertext);
            var plainText = "";
            for (var i = 0; i < ciphertext.Length; i++)
            {
                var b64Chr = ciphertext[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = Base64Alphabet.IndexOf(keyChr);
                chrIndex = chrIndex - keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex += Base64Alphabet.Length;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex -= (Base64Alphabet.Length);
                }

                plainText += Base64Alphabet[chrIndex];
            }

            var (isOkay, result) = Base64Decode(plainText);
            if (isOkay) return (true, result);

            return (false, "");
        }
        
        private static string ExtendKey(string key, string plainText) //Extends key for vigenere algorithm.
        {
            if (key.Length == plainText.Length)
            {
                return key;
            }

            StringBuilder builder;
            if (key.Length > plainText.Length)
            {
                builder = new StringBuilder(key.Length);
                builder.Append(key);
                builder.Length = plainText.Length;
                return builder.ToString();
            }

            builder = new StringBuilder(plainText.Length + key.Length - 1);
            while (builder.Length < plainText.Length)
            {
                builder.Append(key);
            }

            builder.Length = plainText.Length;
            return builder.ToString();
        }
        
    }
}