using System;
using System.Collections.Generic;
using static HW2.Utils;

namespace HW2
{
    class Program
    {
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                              "abcdefghijklmnopqrstuvwxyz" +
                                              "0123456789" +
                                              "+/="; // and "=" as filler

        private static string _userFirstChoice = "";
        private static string _userSecondChoice = "";
        private static string _userFinalChoice = "";
        private static string _userContinue = "";
        private static int _failsCount;
        private static readonly List<int> PrimesList = GeneratePrimesNaive(100000);
        private static readonly Random RandomObject = new Random();
        private static RSA _rsaObject = new RSA();
        private static DiffieHellman _diffieHellmanObject = new DiffieHellman();

        private enum MenuKey
        {
            Continue,
            Back,
            ToMainMenu,
            Exit
        }

        static void Main()
        {
            MenuKey resultKey;
            do
            {
                resultKey = MenuStart(); //Menu starts and the loop goes until it gets Exit key
            } while (resultKey is not MenuKey.Exit);
            
        }

        private static MenuKey MenuStart()
        {

            _userFirstChoice = MenuChooseOption
            (
                "<==== ENCRYPTION AND DECRYPTION PROGRAM ====>",
                "Choose the encryption/decryption algorithm",
                new Dictionary<string, string>()
                {
                    { "C", "Caesar" },
                    { "V", "Vigenere" },
                    { "D", "Diffie-Hellman" },
                    { "R", "RSA" },
                    { "X", "Exit program" }
                }
            );

            if (_userFirstChoice is "X" or "x")
            {
                return MenuKey.Exit;
            }

            var returnKey = MenuEncryptOrDecrypt();
            return returnKey;
        }

        private static string GetAlgorithmString(string userChoice)
        {
            return userChoice switch
            {
                "C" => "Caesar",
                "V" => "Vigenere",
                "D" => "Diffie-Hellman",
                "R" => "RSA",
                _ => ""
            };
        }

        private static MenuKey MenuEncryptOrDecrypt()
        {
            MenuKey shouldContinue;
            var algorithm = GetAlgorithmString(_userFirstChoice);
            var menuDescription = _userFirstChoice.Equals("D")
                ? "Do you want to get shared key or go back: "
                : "Do you want to Encrypt or Decrypt: ";
            var menuOptions = _userFirstChoice switch
            {
                "D" => new Dictionary<string, string>()
                    {
                        { "G", "Get shared key" },
                        { "B", "Back to main menu" },
                    },
                "R" => new Dictionary<string, string>()
                    {
                        { "E", "Encrypt" },
                        { "D", "Decrypt" },
                        { "C", "Crack RSA"},
                        { "P", "Get RSA Public key values"},
                        { "B", "Back to main menu" },
                    },
                _ => new Dictionary<string, string>()
                    {
                        { "E", "Encrypt" },
                        { "D", "Decrypt" },
                        { "B", "Back to main menu" },
                    }
            };

            do
            {
                _userSecondChoice = MenuChooseOption
                (
                    $"-------- {algorithm} algorithm --------",
                    menuDescription,
                    menuOptions
                );

                if (_userSecondChoice.Equals("B")) return MenuKey.Back;

                _userFinalChoice = _userFirstChoice + _userSecondChoice;
                var algorithmOutput = _userFinalChoice switch
                {
                    "CE" => CaesarEncrypt(),
                    "CD" => CaesarDecrypt(),
                    "VE" => VigenereEncrypt(),
                    "VD" => VigenereDecrypt(),
                    "DG" => Diffie_Hellman(),
                    "RE" => RsaCheckExistingObject(true)(),
                    "RD" => RsaCheckExistingObject(false)(),
                    "RC" => CrackRsa(),
                    "RP" => GetRsaPublicKey(),
                    _ => ""
                };
                if (algorithmOutput.Length != 0) Console.WriteLine($"Your output result is: {algorithmOutput}");
                Console.WriteLine();
                shouldContinue = MenuContinueAlgorithm();
            } while (shouldContinue is not (MenuKey.ToMainMenu or MenuKey.Exit));

            return shouldContinue;
        }

        private static string MenuChooseOption(string title, string description, Dictionary<string, string> options)
        {
            string userChoice;
            string optionsAsString = "";
            foreach (KeyValuePair<string, string> entry in options)
            {
                optionsAsString += entry.Key;
            }

            do
            {
                Console.WriteLine(title);
                Console.WriteLine(description);
                foreach (KeyValuePair<string, string> entry in options)
                {
                    Console.WriteLine($"{entry.Key}) {entry.Value}");
                }

                Console.WriteLine("-----------------");
                Console.Write("Your choice:");

                userChoice = GetMenuInput();
                if (!IsMenuInputValid(userChoice, true)) continue;

                if (!CheckValidOptions(userChoice, optionsAsString))
                {
                    Console.WriteLine($"The choice {userChoice} is not valid");
                }
            } while (!CheckValidOptions(userChoice, optionsAsString) || !IsMenuInputValid(userChoice, false));

            return userChoice;
        }

        private static string Diffie_Hellman()
        {
            string userChoice = MenuChooseOption
            (
                $"-------- Diffie-Hellman configuration --------",
                "Do you want to configure inputs manually or use random generator?",
                new Dictionary<string, string>()
                {
                    { "Y", "Yes, I will provide inputs on my own" },
                    { "N", "No, generate it randomly. I am super lazy" },
                    { "X", "Cancel" },
                }
            );

            ulong p = 0;
            ulong q = 0;
            ulong personA = 0;
            ulong personB = 0;

            switch (userChoice)
            {
                case "Y":
                    p = GetNumberInput("First prime number", true);
                    do
                    {
                        q = GetNumberInput("Second prime number", true);
                        if (q == p) Console.WriteLine("Second prime must be different from the first");
                    } while (p == q);

                    personA = GetNumberInput("First user random number", false);
                    do
                    {
                        personB = GetNumberInput("Second user random number", false);
                        if (personA == personB) Console.WriteLine("Second user number must be different from the first");
                    } while (personA == personB);

                    break;
                case "N":
                    p = (ulong)PrimesList[RandomObject.Next(0, PrimesList.Count + 1)];
                    q = (ulong)PrimesList[RandomObject.Next(0, PrimesList.Count + 1)];
                    personA = (ulong)RandomObject.Next(0, 100001);
                    personB = (ulong)RandomObject.Next(0, 100001);
                    break;
                case "X":
                    return "";
            }

            _diffieHellmanObject = new DiffieHellman(p, q, personA, personB);
            if (!_diffieHellmanObject.isConfigured)
            {
                return "";
            }

            var sharedKey = _diffieHellmanObject.GetSharedKey();

            return $"Your shared key is: {sharedKey}";
        }

        private static string GetRsaPublicKey()
        {
            return _rsaObject.isConfigured ? _rsaObject.GetPublicKey() : "RSA is not configured yet";
        }
        
        private static Func<string> RsaCheckExistingObject(bool encrypt)
        {
            string userChoice;
            var method = encrypt ? "Encryption" : "Decryption";
            if (_rsaObject.isConfigured)
            {
                userChoice = MenuChooseOption
                (
                    $"-------- RSA {method} --------",
                    $"You have already configured RSA. Do you want to continue with the existing configuration for {method}?",
                    new Dictionary<string, string>()
                    {
                        { "Y", "Yes, continue with the existing configuration" },
                        { "N", "No, reset the configuration" },
                        { "C", "Cancel" },
                    }
                );
            }
            else
            {
                userChoice = MenuChooseOption
                (
                    $"-------- RSA {method} --------",
                    $"Looks like that you haven't configured RSA configuration yet. Before {method} you have to setup initial configuration. Proceed now?",
                    new Dictionary<string, string>()
                    {
                        { "Y", "Yes" },
                        { "C", "Cancel" },
                    }
                );
            }
            
            if(_rsaObject.isConfigured){
                if(userChoice.Equals("Y"))
                {
                    return encrypt ? RsaEncrypt : RsaDecrypt;
                }
            }
            
            if (userChoice.Equals("C"))
            {
                return () => "";
            }
            
            if (ConfigureRsa())
            {
                return encrypt ? RsaEncrypt : RsaDecrypt;
            }

            return () => "";
        }

        private static ulong GetNumberInput(string inputString, bool isPrime) //Check if input is a valid text. 
        {
            bool inputIsValid;
            ulong primeNumber;
            do
            {
                Console.Write($"Please input {inputString} :");
                var primeString = Console.ReadLine()?.Trim();

                inputIsValid = ulong.TryParse(primeString, out primeNumber);
                if (!inputIsValid)
                {
                    Console.WriteLine($"The input of '{primeNumber}' is not a valid input! Provide only positive numbers");
                    CheckFails();
                    continue;
                }

                if (!isPrime) break;
                inputIsValid = IsPrime(primeNumber);

                if (!inputIsValid)
                {
                    Console.WriteLine(
                        $"The input of '{primeNumber}' is not a prime number! Provide only positive prime numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            return primeNumber;
        }
        
        private static bool ConfigureRsa()
        {
            string userChoice = MenuChooseOption
            (
                $"-------- RSA configuration --------",
                "Do you want to configure inputs manually or use random generator?",
                new Dictionary<string, string>()
                {
                    { "Y", "Yes, I will provide inputs on my own" },
                    { "N", "No, generate it randomly. I am super lazy" },
                    { "C", "Cancel" },
                }
            );

            switch (userChoice)
            {
                case "Y":
                {
                    ulong p;
                    ulong q;
                    bool isValidInput;
                    do
                    {
                        p = GetNumberInput("first prime", true);
                        do
                        {
                            q = GetNumberInput("second prime", true);
                            if(p == q) Console.WriteLine("Second prime must be different from first one");
                        } while (p == q);
                        isValidInput = (int)(p * q) > Base64Alphabet.Length;
                        if (!isValidInput)
                        {
                            Console.WriteLine(
                                "Your prime numbers' multiplication sum is too low. Please provide other prime numbers");
                        }
                    } while (!isValidInput);

                    _rsaObject = new RSA(p, q);
                    if (!_rsaObject.isConfigured)
                    {
                        return false;
                    }

                    break;
                }
                case "N":
                {
                    ulong p = (ulong)PrimesList[RandomObject.Next(0, PrimesList.Count - 1)];
                    ulong q = (ulong)PrimesList[RandomObject.Next(0, PrimesList.Count - 1)];
                    _rsaObject = new RSA(p, q);
                    if (!_rsaObject.isConfigured)
                    {
                        return false;
                    }

                    break;
                }
                case "C":
                    return false;
            }

            return true;
        }

        private static string RsaEncrypt()
        {
            return _rsaObject.Encrypt(GetRsaText("plaintext")).Item2;
        }

        private static string RsaDecrypt()
        {
            return _rsaObject.Decrypt(GetRsaText("ciphertext")).Item2;
        }

        private static MenuKey MenuContinueAlgorithm()
        {
            var algorithm = GetAlgorithmString(_userFirstChoice);
            _userContinue = MenuChooseOption
            (
                $"-------- Continue? --------",
                $"Do you want to Continue with {algorithm} algorithm?",
                new Dictionary<string, string>()
                {
                    { "Y", "Yes, let's continue" },
                    { "N", "No, go back to main menu" },
                    { "X", "Exit program" },
                }
            );

            return _userContinue switch
            {
                "Y" => MenuKey.Continue,
                "N" => MenuKey.ToMainMenu,
                "X" => MenuKey.Exit,
                _ => MenuKey.Exit
            };
        }

        private static bool
            CheckValidOptions(string input,
                string options) //This function checks if the key is valid for current menu options
        {
            if (input.Length != 1) return false;

            foreach (var chr in options)
            {
                if (input.Length != 1) continue;
                if (input[0].Equals(chr)) return true;
            }

            return false;
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
                Console.Write("Please, enter " + targetField + ":");
                input = Console.ReadLine()?.Trim() ?? "";
                if (input.Length != 0) break;
                Console.WriteLine("Your input is empty, Please try again and provide some text");
                CheckFails();
            } while (input.Length == 0);

            _failsCount = 0;
            return input;
        }

        private static void CheckFails()
        {
            if (_failsCount <= 10) _failsCount++;
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

            Console.WriteLine();
        }

        private static bool IsMenuInputValid(string input, bool logError) //Check if input is a valid menu choice. 
        {
            switch (input.Length)
            {
                case 0:
                    if (logError) Console.WriteLine("Your input is empty, please try again");
                    CheckFails();
                    return false;
                case > 1:
                    if (logError) Console.WriteLine("Please, provide only one character");
                    CheckFails();
                    return false;
            }

            if (int.TryParse(input, out _))
            {
                if (logError) Console.WriteLine("Numbers are not accepted, please try again");
                CheckFails();
                return false;
            }

            _failsCount = 0;
            return true;
        }

        private static (int, bool) GetCaesarShiftAmount()
        {
            bool inputIsValid;
            int shiftAmount;
            do
            {
                Console.Write("Please input shift amount (enter C to cancel):");
                var shiftString = Console.ReadLine()?.Trim();

                if (shiftString is "C" or "c") return (0, false);

                inputIsValid = int.TryParse(shiftString, out shiftAmount);
                if (!inputIsValid)
                {
                    Console.WriteLine($"The shift of '{shiftString}' is not a valid input! Provide only numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            return (shiftAmount, true);
        }

        private static string CaesarShiftingAlgorithm(string base64Str, int shiftAmount, bool encrypt)
        {
            var result = "";
            for (var i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                chrIndex = encrypt ? chrIndex + shiftAmount : chrIndex - shiftAmount;
                if (chrIndex < 0)
                {
                    chrIndex += Base64Alphabet.Length;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex -= (Base64Alphabet.Length);
                }

                result += Base64Alphabet[chrIndex];
            }

            return result;
        }

        private static string VigenereShiftingAlgorithm(string base64Str, string extendedSecretKey, bool encrypt)
        {
            var result = "";
            for (var i = 0; i < base64Str.Length; i++)
            {
                var b64Chr = base64Str[i];
                var keyChr = extendedSecretKey[i];
                var chrIndex = Base64Alphabet.IndexOf(b64Chr);
                var keyChrIndex = Base64Alphabet.IndexOf(keyChr);
                chrIndex = encrypt ? chrIndex + keyChrIndex : chrIndex - keyChrIndex;
                if (chrIndex < 0)
                {
                    chrIndex += Base64Alphabet.Length;
                }
                else if (chrIndex > (Base64Alphabet.Length - 1))
                {
                    chrIndex -= (Base64Alphabet.Length);
                }

                result += Base64Alphabet[chrIndex];
            }

            return result;
        }

        private static string CaesarEncrypt()
        {
            Console.WriteLine("========== Cesar Encryption ===========");

            bool inputIsValid;
            int shiftAmount;

            (shiftAmount, inputIsValid) = GetCaesarShiftAmount();

            if (!inputIsValid) return "";

            _failsCount = 0;
            shiftAmount %= Base64Alphabet.Length;
            
            Console.WriteLine($"Cesar shift amount: {shiftAmount}");

            var plaintext = GetTextInput("plaintext");
            var base64Str = Base64Encode(plaintext);

            var ciphertext = CaesarShiftingAlgorithm(base64Str, shiftAmount, true);

            Console.WriteLine();

            return ciphertext;
        }

        private static string CaesarDecrypt()
        {
            Console.WriteLine("========== Cesar Decryption ===========");

            bool inputIsValid;
            int shiftAmount;

            (shiftAmount, inputIsValid) = GetCaesarShiftAmount();

            if (!inputIsValid) return "";

            _failsCount = 0;
            shiftAmount = shiftAmount % Base64Alphabet.Length;

            Console.WriteLine($"Cesar shift amount: {shiftAmount}");

            var ciphertext = GetTextInput("ciphertext");

            var plaintext = CaesarShiftingAlgorithm(ciphertext, shiftAmount, false);

            Console.WriteLine();
            bool isCipherTextOkay;

            (isCipherTextOkay, plaintext) = Base64Decode(plaintext);
            return !isCipherTextOkay ? "" : plaintext;
        }


        private static string GetRsaText(string text)
        {
            bool inputIsValid;
            string plaintext;
            do
            {
                Console.Write($"Please input the {text}:");
                var inputString = Console.ReadLine()?.Trim();

                inputIsValid = IsEnglishLetters(inputString);
                plaintext = inputString;
                if (!inputIsValid)
                {
                    Console.WriteLine(
                        $"The plaintext of '{inputString}' is not a valid input! Provide only English letters and numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            return plaintext;
        }

        private static string GetVigenereKey()
        {
            bool inputIsValid;
            string secretKey;
            do
            {
                Console.Write("Please input the key:");
                var keyString = Console.ReadLine()?.Trim();

                inputIsValid = IsBase64Chars(keyString);
                secretKey = keyString;
                if (!inputIsValid)
                {
                    Console.WriteLine(
                        $"The key of '{keyString}' is not a valid input! Provide only English letters and numbers");
                    CheckFails();
                }
            } while (!inputIsValid);

            return secretKey;
        }

        private static string VigenereEncrypt()
        {
            Console.WriteLine("========== Vigenere Encryption ===========");
            var secretKey = GetVigenereKey();

            _failsCount = 0;
            Console.WriteLine($"Vigenere secret key: {secretKey}");

            var plaintext = GetTextInput("plaintext");
            var base64Str = Base64Encode(plaintext);

            var extendedSecretKey = ExtendKey(secretKey, base64Str);
            var ciphertext = VigenereShiftingAlgorithm(base64Str, extendedSecretKey, true);
            Console.WriteLine();

            return ciphertext;
        }

        private static string VigenereDecrypt()
        {
            Console.WriteLine("========== Vigenere Decryption ===========");
            var secretKey = GetVigenereKey();

            _failsCount = 0;
            Console.WriteLine($"Vigenere secret key: {secretKey}");

            var ciphertext = GetTextInput("ciphertext");
            var extendedSecretKey = ExtendKey(secretKey, ciphertext);

            var plaintext = VigenereShiftingAlgorithm(ciphertext, extendedSecretKey, false);

            Console.WriteLine();
            bool isCipherTextOkay;
            (isCipherTextOkay, plaintext) = Base64Decode(plaintext);
            return !isCipherTextOkay ? "" : plaintext;
        }
        private static string CrackRsa()
        {
            var e = GetNumberInput("e value", false);
            var n = GetNumberInput("n value", false);
            var ciphertext = GetRsaText("ciphertext");
            
            if (e >= n)
            {
                Console.WriteLine("E value cannot be higher than n value");
                return "";
            }
            var startingValue = FloorSqrt(n);
            ulong p = 0, d;
            
            for (var i = startingValue; i != 0; i--)
            {
                if (!IsPrime(i)) continue;
                if (n % i != 0) continue;
                p = i;
                break;
            }

            var q = n / p;

            d = Modinv(e, (p - 1) * (q - 1));

            var rsaObject = new RSA(p, q, e);
            var decryptedText = rsaObject.Decrypt(ciphertext);
            
            return $"d value is {d}. p value is {p}. q value is {q} \n Decrypted text is {decryptedText}";
        }
    }
}