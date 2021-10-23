using System;
using System.Text;
using static HW2.Utils;

namespace HW2
{
    public static class Caesar
    {
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                              "abcdefghijklmnopqrstuvwxyz" +
                                              "0123456789" +
                                              "+/="; // and "=" as filler
        
        public static (bool, string) Encrypt(string plaintext, int shiftAmount)
        {
            var base64Str = Base64Encode(plaintext);
            var cipherText = "";
            for (var i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                chrIndex = chrIndex + shiftAmount;
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
        
        public static (bool, string) Decrypt(string ciphertext, int shiftAmount)
        {
            var plainText = "";
            for (var i = 0; i < ciphertext.Length; i++)
            {
                var b64Chr = ciphertext[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                chrIndex = chrIndex - shiftAmount;
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

    }
}