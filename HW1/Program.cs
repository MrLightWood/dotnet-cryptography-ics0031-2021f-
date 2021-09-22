using System;
using System.Text;
using System.Text.RegularExpressions;

namespace HW1
{
    class Program
    {
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "abcdefghijklmnopqrstuvwxyz" + "0123456789" + "+/="; // and "=" as filler

        private static string _userFirstChoice = "";
        private static string _userSecondChoice = "";
        private static string _userFinalChoice = "";
        private static string _userContinue = "";
        private static int _failsCount;

        private enum MenuKey
        {
            Continue,
            Back,
            ToMainMenu,
            Exit
        }
        static void Main(string[] args)
        {
            MenuKey resultKey;
            do
            {
                resultKey = MenuStart(); //Menu starts and the loop goes until it gets Exit key
            } while (resultKey is not MenuKey.Exit);
        }

        private static MenuKey MenuStart()
        {
            do
            {
                Console.WriteLine();
                Console.WriteLine("<==== CAESAR AND VIGENERE ENCRYPTION AND DECRYPTION ====>");
                Console.WriteLine("C) Caesar");
                Console.WriteLine("V) Vigenere");
                Console.WriteLine("X) Exit program");
                Console.WriteLine("-----------------");
                Console.Write("Your choice:");
                
                _userFirstChoice = GetMenuInput();
                if (!IsMenuInputValid(_userFirstChoice, true)) continue;
                if (!CheckValidOptions(_userFirstChoice, "CVX"))
                {
                    Console.WriteLine($"The choice {_userFirstChoice} is not valid");
                }
            } while (!CheckValidOptions(_userFirstChoice, "CVX") || !IsMenuInputValid(_userFirstChoice, false));

            if(_userFirstChoice is "X" or "x") { return MenuKey.Exit; }
            var returnKey = MenuEncryptOrDecrypt();
            return returnKey;
        }
        
        private static MenuKey MenuEncryptOrDecrypt()
        {
            MenuKey shouldContinue;
            do
            {
                do
                {
                    Console.WriteLine("-----------------");
                    Console.WriteLine("Do you want to Encrypt or Decrypt: ");
                    Console.WriteLine("E) Encrypt");
                    Console.WriteLine("D) Decrypt");
                    Console.WriteLine("X) Back");
                    Console.WriteLine("-----------------");
                    Console.Write("Your choice:");

                    _userSecondChoice = GetMenuInput();
                    if (!IsMenuInputValid(_userSecondChoice, true)) continue;
                    
                    if (!CheckValidOptions(_userSecondChoice, "EDX"))
                    {
                        Console.WriteLine($"The choice {_userSecondChoice} is not valid");
                    }
                } while (!CheckValidOptions(_userSecondChoice, "EDX") || !IsMenuInputValid(_userSecondChoice, false));

                if (_userSecondChoice.Equals("X")) return MenuKey.Back;
                
                _userFinalChoice = _userFirstChoice + _userSecondChoice;
                var algorithmOutput = _userFinalChoice switch
                {
                    "CE" => CaesarEncrypt(),
                    "CD" => CaesarDecrypt(),
                    "VE" => VigenereEncrypt(),
                    "VD" => VigenereDecrypt(),
                    _ => ""
                };
                Console.WriteLine();
                Console.WriteLine($"Your encrypted text is: {algorithmOutput}");
                Console.WriteLine();
                shouldContinue = MenuContinueAlgorithm();
            } while (shouldContinue is not(MenuKey.ToMainMenu or MenuKey.Exit));
            
            return shouldContinue;
        }
        
        private static MenuKey MenuContinueAlgorithm()
        {
            do
            {
                var algorithm = _userFirstChoice.Equals("C") ? "Caesar" : "Vigenere";
                Console.WriteLine("-----------------");
                Console.WriteLine($"Do you want to Continue with {algorithm} algorithm?");
                Console.WriteLine("Y) Yes, let's continue");
                Console.WriteLine("N) No, go back to main menu");
                Console.WriteLine("X) Exit program");
                Console.WriteLine("-----------------");
                Console.Write("Your choice:");

                _userContinue = GetMenuInput();
                if (!IsMenuInputValid(_userContinue, true)) continue;

                if (!CheckValidOptions(_userContinue, "YNX"))
                {
                    Console.WriteLine($"The choice {_userContinue} is not valid");
                }

            } while (!CheckValidOptions(_userContinue, "YNX") || !IsMenuInputValid(_userContinue, false));

            return _userContinue switch
            {
                "Y" => MenuKey.Continue,
                "N" => MenuKey.ToMainMenu,
                "X" => MenuKey.Exit,
                _ => MenuKey.Exit
            };
        }

        private static bool CheckValidOptions(string input, string options) //This function checks if the key is valid for current menu options
        {
            return input.Length != 0 && options.Contains(input);
        }
        
        private static string GetMenuInput() //This function gets and returns the input or empty string in case of null
        {
            return Console.ReadLine()?.Trim().ToUpper() ?? "";
        }
        
        private static string GetTextInput(string targetField) //Check if input is a valid text. 
        {
            string input;
            do
            {
                Console.WriteLine();
                Console.Write("Please, enter " + targetField + ":");
                input = Console.ReadLine()?.Trim() ?? "";
                if (input.Length != 0) break;
                Console.WriteLine("Your input is empty, Please try again and provide some text \n");
                CheckFails();
            } while (input.Length == 0);

            _failsCount = 0;
            return input;
        }

        private static void CheckFails()
        {
            if(_failsCount <= 10) _failsCount++;
            switch (_failsCount)
            {
                case 1:
                    Console.WriteLine("Please, try again");
                    break;
                case 2:
                    Console.WriteLine("Read instructions carefully");
                    break;
                case 3:
                    Console.WriteLine("Oh, you provided invalid input again. Try harder please ");
                    break;
                case 5:
                    Console.WriteLine("Seriously, just provide me a valid input and I will continue my work");
                    break;
                case 8:
                    Console.WriteLine("...");
                    break;
                case 9:
                    Console.WriteLine("... ...");
                    break;
                case 10:
                    Console.WriteLine("Okay, do whatever you want. I won't be commenting anything");
                    break;
            }
        }
        
        private static bool IsMenuInputValid(string input, bool logError) //Check if input is a valid menu choice. 
        {
            Console.WriteLine();
            switch (input.Length)
            {
                case 0:
                    if(logError) Console.WriteLine("Your input is empty, please try again \n");
                    CheckFails();
                    return false;
                case > 1:
                    if(logError) Console.WriteLine("Please, provide only one character");
                    CheckFails();
                    return false;
            }
            if (int.TryParse(input, out _))
            {
                if(logError) Console.WriteLine("Numbers are not accepted, please try again");
                CheckFails();
                return false;
            }

            Console.WriteLine();
            _failsCount = 0;
            return true;
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
                    Console.WriteLine($"The shift of '{shiftString}' is not a valid input! Provide only numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            _failsCount = 0;
            shiftAmount %= Base64Alphabet.Length;

            Console.WriteLine($"Cesar shift amount: {shiftAmount}");

            var plaintext = GetTextInput("plaintext");
            
            var utf8 = new UTF8Encoding();
            var utf8Bytes = utf8.GetBytes(plaintext);
            var base64Str = Convert.ToBase64String(utf8Bytes);

            var ciphertext = "";

            for (int i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                chrIndex += shiftAmount;
                if (chrIndex < 0)
                {
                    chrIndex = Base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex = chrIndex - (Base64Alphabet.Length );
                }

                ciphertext += Base64Alphabet[chrIndex];
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

                inputIsValid = int.TryParse(shiftString, out shiftAmount);
                if (!inputIsValid)
                {
                    Console.WriteLine($"The shift of '{shiftString}' is not a valid input! Provide only numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            _failsCount = 0;
            shiftAmount = shiftAmount % Base64Alphabet.Length;

            Console.WriteLine($"Cesar shift amount: {shiftAmount}");

            var ciphertext = GetTextInput("ciphertext");

            var plaintext = "";

            for (int i = 0; i < ciphertext.Length; i++)
            {
                var b64Chr = ciphertext[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                chrIndex -= shiftAmount;
                if (chrIndex < 0)
                {
                    chrIndex = Base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex -= (Base64Alphabet.Length );
                }

                plaintext += Base64Alphabet[chrIndex];
            }
            Console.WriteLine();
            bool isCipherTextOkay;
            
            (isCipherTextOkay, plaintext) = Base64Decode(plaintext);
            return !isCipherTextOkay ? "" : plaintext;
        }
        private static string VigenereEncrypt()
        {
            Console.WriteLine("========== Vigenere Encryption ===========");

            bool inputIsValid;
            string secretKey;

            do
            {
                Console.Write("Please input the key:");
                var shiftString = Console.ReadLine()?.Trim();
                
                inputIsValid = IsBase64Chars(shiftString);
                secretKey = shiftString;
                if (!inputIsValid)
                {
                    Console.WriteLine($"The key of '{shiftString}' is not a valid input! Provide only English letters and numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            _failsCount = 0;
            Console.WriteLine($"Vigenere secret key: {secretKey}");

            var plaintext = GetTextInput("plaintext");

            var utf8 = new UTF8Encoding();
            var utf8Bytes = utf8.GetBytes(plaintext);
            var base64Str = Convert.ToBase64String(utf8Bytes);
            
            var extendedSecretKey = ExtendKey(secretKey, base64Str);
            var ciphertext = "";

            for (var i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = Base64Alphabet.IndexOf(keyChr);
                chrIndex += keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex = Base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex = chrIndex - (Base64Alphabet.Length );
                }

                ciphertext += Base64Alphabet[chrIndex];
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
                    Console.WriteLine($"The key of '{shiftString}' is not a valid input! Provide only English letters and numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            _failsCount = 0;
            Console.WriteLine($"Vigenere secret key: {secretKey}");

            Console.Write("Please enter ciphertext:");
            var ciphertext = GetTextInput("ciphertext");
            var extendedSecretKey = ExtendKey(secretKey, ciphertext);

            var plaintext = "";
            
            for (int i = 0; i < ciphertext.Length; i++)
            {
                var b64Chr = ciphertext[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = Base64Alphabet.IndexOf(keyChr);
                chrIndex -= keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex = Base64Alphabet.Length + chrIndex;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex -= (Base64Alphabet.Length );
                }

                plaintext += Base64Alphabet[chrIndex];
            }
            Console.WriteLine();
            bool isCipherTextOkay;
            (isCipherTextOkay, plaintext) = Base64Decode(plaintext);
            return !isCipherTextOkay ? "" : plaintext;
        }

        private static (bool, string) Base64Decode(string base64EncodedData) //Decodes Base64 to UTF-8 String
        {
            string result;
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                result = Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception)
            {
                Console.WriteLine("Looks like your ciphertext is wrong. I could not decrypt it. Please, try again");
                return (false, "");    
            }

            return (true, result);
        }
        private static bool IsBase64Chars(string base64) //Checks if a string contains only base64 characters
        {
            base64 = base64.Trim();
            if (base64.Length == 0)
            {
                Console.WriteLine("Input is empty");
                return false;
            }
            return Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        private static string ExtendKey(string key, string plainText) //Extends key for vigenere algorithm.
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