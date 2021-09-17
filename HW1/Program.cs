using System;
using System.Text;
using System.Text.RegularExpressions;

namespace HW1
{
    class Program
    {
        private static string base64Alphabet =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789" +
            "+/="; // and "=" as filler

        
        static void Main(string[] args)
        {
            string userChoice;
            do
            {
                Console.WriteLine("<==== CAESAR AND VIGENERE ENCRYPTION AND DECRYPTION ====>");
                Console.WriteLine("C) Caesar");
                Console.WriteLine("V) Vigenere");
                Console.WriteLine("X) Exit");
                Console.WriteLine("-----------------");
                Console.Write("Your choice:");
                
                userChoice = Console.ReadLine()?.Trim().ToUpper();

                if (userChoice is not ("C" or "V" or "X"))
                {
                    Console.WriteLine($"The choice {userChoice} is not valid");
                }
                if(userChoice is "X" or "x") { break; }
                
                Console.Clear();
                Console.WriteLine("-----------------");
                Console.Write("Do you want to Encrypt or Decrypt: \n");
                Console.WriteLine("E) Encrypt");
                Console.WriteLine("D) Decrypt");
                Console.WriteLine("X) Back");
                userChoice += Console.ReadLine()?.Trim().ToUpper();
                if (userChoice == "X") { continue; }
                Console.Clear();
                string output;
                switch (userChoice)
                {
                    case "CE":
                        output = CaesarEncrypt();
                        break;
                    case "CD":
                        output = CaesarDecrypt();
                        break;
                    case "VE":
                        output = VigenereEncrypt();
                        break;
                    case "VD":
                        output = VigenereDecrypt();
                        break;
                    default: 
                        Console.WriteLine($"The choice {userChoice[1]} is not valid");
                        continue;
                }
                Console.WriteLine("Your output is: " + output);
            } while (userChoice != "X");
        }

        private static string CaesarEncrypt()
        {
            Console.WriteLine("========== Cesar Encryption ===========");

            bool inputIsValid;
            int shiftAmount;

            do
            {
                Console.Write("Please input shift amount (enter C to cancel):");
                var shiftString = Console.ReadLine()?.Trim();

                // bail out
                if (shiftString is "C" or "c") return "";
                
                inputIsValid = int.TryParse(shiftString, out shiftAmount);
                if (!inputIsValid)
                {
                    Console.WriteLine($"The shift of '{shiftString}' is not a valid input!");
                }
            } while (!inputIsValid);

            shiftAmount %= base64Alphabet.Length;

            Console.WriteLine($"Cesar shift amount: {shiftAmount}");

            Console.Write("Please enter plaintext:");
            var plaintext = Console.ReadLine() ?? "";
            
            var utf8 = new UTF8Encoding();
            var utf8Bytes = utf8.GetBytes(plaintext);
            var base64Str = Convert.ToBase64String(utf8Bytes);

            var ciphertext = "";

            for (int i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var chrIndex = base64Alphabet.IndexOf(b64Chr);
                chrIndex += shiftAmount;
                if (chrIndex < 0)
                {
                    chrIndex = base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (base64Alphabet.Length - 1))
                {
                    chrIndex = chrIndex - (base64Alphabet.Length );
                }

                ciphertext += base64Alphabet[chrIndex];
            }
            Console.WriteLine();
            
            return ciphertext;
        }
        
        private static string CaesarDecrypt()
        {
            Console.WriteLine("========== Cesar Decryption ===========");

            bool inputIsValid;
            int shiftAmount;

            do
            {
                Console.Write("Please input shift amount (enter C to cancel):");
                var shiftString = Console.ReadLine()?.Trim().ToUpper();

                // bail out
                if (shiftString == "C") return "";
                
                inputIsValid = int.TryParse(shiftString, out shiftAmount);
                if (!inputIsValid)
                {
                    Console.WriteLine($"The shift of '{shiftString}' is not a valid input!");
                }
            } while (!inputIsValid);

            shiftAmount = shiftAmount % base64Alphabet.Length;

            Console.WriteLine($"Cesar shift amount: {shiftAmount}");

            Console.Write("Please enter ciphertext:");
            var ciphertext = Console.ReadLine() ?? "";

            var plaintext = "";

            for (int i = 0; i < ciphertext.Length; i++)
            {
                var b64Chr = ciphertext[i];
                var chrIndex = base64Alphabet.IndexOf(b64Chr);
                chrIndex -= shiftAmount;
                if (chrIndex < 0)
                {
                    chrIndex = base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (base64Alphabet.Length - 1))
                {
                    chrIndex -= (base64Alphabet.Length );
                }

                plaintext += base64Alphabet[chrIndex];
            }
            Console.WriteLine();
            plaintext = Base64Decode(plaintext);
            return plaintext;
        }
        private static string VigenereEncrypt()
        {
            Console.WriteLine("========== Vigenere Encryption ===========");

            bool inputIsValid;
            string secretKey;

            do
            {
                Console.Write("Please input the key (enter C to cancel):");
                var shiftString = Console.ReadLine()?.Trim();

                // bail out
                if (shiftString == "C") return "";

                inputIsValid = IsBase64Chars(shiftString);
                secretKey = shiftString;
                if (!inputIsValid)
                {
                    Console.WriteLine($"The key of '{shiftString}' is not a valid input!");
                }
            } while (!inputIsValid);
            
            Console.WriteLine($"Vigenere secret key: {secretKey}");

            Console.Write("Please enter plaintext:");
            var plaintext = Console.ReadLine() ?? "";

            var utf8 = new UTF8Encoding();
            var utf8Bytes = utf8.GetBytes(plaintext);
            var base64Str = Convert.ToBase64String(utf8Bytes);
            
            var extendedSecretKey = ExtendKey(secretKey, base64Str);
            var ciphertext = "";

            for (var i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = base64Alphabet.IndexOf(keyChr);
                chrIndex += keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex = base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (base64Alphabet.Length - 1))
                {
                    chrIndex = chrIndex - (base64Alphabet.Length );
                }

                ciphertext += base64Alphabet[chrIndex];
            }
            Console.WriteLine();
            
            return ciphertext;
        }
        
        private static string VigenereDecrypt()
        {
            Console.WriteLine("========== Vigenere Decryption ===========");

            bool inputIsValid;
            string secretKey;

            do
            {
                Console.Write("Please input the key (enter C to cancel):");
                var shiftString = Console.ReadLine()?.Trim();

                // bail out
                if (shiftString == "C") return "";

                inputIsValid = IsBase64Chars(shiftString);
                secretKey = shiftString;
                if (!inputIsValid)
                {
                    Console.WriteLine($"The key of '{shiftString}' is not a valid input!");
                }
            } while (!inputIsValid);
            
            Console.WriteLine($"Vigenere secret key: {secretKey}");

            Console.Write("Please enter ciphertext:");
            var ciphertext = Console.ReadLine() ?? "";
            var extendedSecretKey = ExtendKey(secretKey, ciphertext);
            Console.WriteLine(ciphertext.Length);
            Console.WriteLine(extendedSecretKey.Length);
            var plaintext = "";
            
            for (int i = 0; i < ciphertext.Length; i++)
            {
                var b64Chr = ciphertext[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = base64Alphabet.IndexOf(keyChr);
                chrIndex -= keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex = base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (base64Alphabet.Length - 1))
                {
                    chrIndex -= (base64Alphabet.Length );
                }

                plaintext += base64Alphabet[chrIndex];
            }
            /*
            var utf8 = new UTF8Encoding();
            var utf8Bytes = utf8.GetBytes(plaintext);
            var base64Str = System.Convert.ToBase64String(utf8Bytes);

            var plaintext = "";
*/
            plaintext = Base64Decode(plaintext);
            Console.WriteLine();
            
            return plaintext;
        }
        
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        private static bool IsBase64Chars(string base64)
        {
            base64 = base64.Trim();
            return Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        private static string ExtendKey(string key, string plainText)
        {
            if (key.Length == plainText.Length) { return key; }
            StringBuilder builder;
            if (key.Length > plainText.Length)
            {
                builder = new StringBuilder(key.Length);
                builder.Append(key);
                builder.Length = plainText.Length;
                return builder.ToString();
            }
            builder = new StringBuilder(plainText.Length + key.Length - 1);
            while (builder.Length < plainText.Length) {
                builder.Append(key);
            }

            builder.Length = plainText.Length;
            return builder.ToString();
        }
    }
}