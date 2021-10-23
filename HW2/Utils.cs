using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HW2
{
    public static class Utils
    {
        public static List<int> PrimesList = GeneratePrimesNaive(100000);
        public static Random RandomObject = new Random();
        public static string Base64Encode(string input)
        {
            var utf8 = new UTF8Encoding();
            var utf8Bytes = utf8.GetBytes(input);
            return Convert.ToBase64String(utf8Bytes);
        }
        
        public static (bool, string) Base64Decode(string base64EncodedData) //Decodes Base64 to UTF-8 String
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

        public static bool IsBase64Chars(string base64) //Checks if a string contains only base64 characters
        {
            base64 = base64.Trim();
            if (base64.Length == 0)
            {
                Console.WriteLine("Input is empty");
                return false;
            }

            return Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*$", RegexOptions.None);
        }
        
        public static bool IsEnglishLetters(string base64) //Checks if a string contains only base64 characters
        {
            base64 = base64.Trim();
            if (base64.Length == 0)
            {
                Console.WriteLine("Input is empty");
                return false;
            }

            return Regex.IsMatch(base64, @"^[a-zA-Z0-9' '=\+/]*$", RegexOptions.None);
        }

        public static string ExtendKey(string key, string plainText) //Extends key for vigenere algorithm.
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
        
        public static ulong FloorSqrt(ulong num)
        {
            if (0 == num)
            {
                return 0;
            } // Avoid zero divide  

            ulong n = (num / 2) + 1; // Initial estimate, never low  
            ulong n1 = (n + (num / n)) / 2;
            while (n1 < n)
            {
                n = n1;
                n1 = (n + (num / n)) / 2;
            } // end while  

            return n;
        }
        
        public static List<int> GeneratePrimesNaive(int n)
        {
            List<int> primes = new List<int>();
            primes.Add(2);
            int nextPrime = 3;
            while (primes.Count < n)
            {
                int sqrt = (int)FloorSqrt((ulong)nextPrime);
                bool isPrime = true;
                for (int i = 0; primes[i] <= sqrt; i++)
                {
                    if (nextPrime % primes[i] == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }

                if (isPrime)
                {
                    primes.Add(nextPrime);
                }

                nextPrime += 2;
            }

            return primes;
        }

        public static ulong Modpow(ulong a, ulong b, ulong mod)
        {
            ulong product, pseq;
            product = 1;
            pseq = a % mod;
            try
            {
                while (b > 0)
                {
                    if ((b & 1) == 1)
                        product = Modmult(product, pseq, mod);
                    pseq = Modmult(pseq, pseq, mod);
                    b >>= 1;
                }
            }
            catch (OverflowException)
            {
                Console.WriteLine("Prime numbers are too big, please select smaller ones");
            }

            return product;
        }

        public static ulong Modmult(ulong a, ulong b, ulong mod)
        {
            if (a == 0 || b < mod / a)
                return (a * b) % mod;
            ulong sum;
            sum = 0;

            try
            {
                while (b > 0)
                {
                    if ((b & 1) == 1)
                        sum = checked((sum + a) % mod);
                    a = (2 * a) % mod;
                    b >>= 1;
                }
            }
            catch (OverflowException)
            {
                Console.WriteLine("Prime numbers are too big, please select smaller ones");
            }

            return sum;
        }

        public static ulong Gcd(ulong a, ulong b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }
        
        public static ulong Modinv(ulong e, ulong phi) //https://www.di-mgt.com.au/euclidean.html#extendedeuclidean
        {
            ulong inv, u1, u3, v1, v3, t1, t3, q;
            int iter;
            u1 = 1;
            u3 = e;
            v1 = 0;
            v3 = phi;
            iter = 1;
            while (v3 != 0)
            {
                q = u3 / v3;
                t3 = u3 % v3;
                t1 = u1 + q * v1;
                u1 = v1; v1 = t1; u3 = v3; v3 = t3;
                iter = -iter;
            }
            if (u3 != 1)
                return 0;
            if (iter < 0)
                inv = phi - u1;
            else
                inv = u1;
            return inv;
        }
        
        public static bool IsPrime(ulong number)
        {
            if (number < 2) return false;
            if (number % 2 == 0) return (number == 2);
            int root = (int)FloorSqrt(number);
            for (int i = 3; i <= root; i += 2)
            {
                if (number % (ulong)i == 0) return false;
            }

            return true;
        }
    }
}