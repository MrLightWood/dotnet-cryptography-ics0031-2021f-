using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HW2
{
    public class RSA
    {
        private Random randomObject = new Random();
        private ulong p; //first prime
        private ulong q; //second prime
        private ulong n; //p*q
        private ulong phi; //(p-1) * (q-1)
        private ulong e; //public key
        private ulong d; //private key
        public bool isConfigured;
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + 
                                              "abcdefghijklmnopqrstuvwxyz" + 
                                              "0123456789" + 
                                              "+/ ="; // base64 alphabet with space
        private static readonly List<int> PrimesList = Program.GeneratePrimesNaive(100000);
        
        static void Mainp()
        {
            Console.WriteLine(crackRSA(5, 10142789312725007));
            //var rsa = new RSA((ulong)PrimesList[5], (ulong)PrimesList[10]);
            //rsa.printRSA();

            //var encryptedValue = 96;
            //rsa.findPublicKey(rsa.getE(), rsa.enc(150));
            //Console.WriteLine("Enc: " + rsa.enc(500));
            //var enc = modpow(50, 3, 55);
            //Console.WriteLine(enc);
            //Console.WriteLine(modpow(enc, 27, 55));
        }
        
        public RSA() {}

        public void printRSA()
        {
            Console.WriteLine("p: " + p);
            Console.WriteLine("q: " + q);
            Console.WriteLine("n: " + n);
            Console.WriteLine("phi: " + phi);
            Console.WriteLine("e: " + e);
            Console.WriteLine("d: " + d);
        }
        public RSA(ulong p, ulong q)
        {
            try
            {
                this.p = p;
                this.q = q;
                this.n = checked(p * q);
                this.phi = checked((p - 1) * (q - 1));
                this.e = generatePublicKey(phi);
                this.d = generatePrivateKey(e, phi);
                this.isConfigured = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Your inputs are too big. Please provide another numbers");
                this.isConfigured = false;
            }
        }
        
        public RSA(ulong p, ulong q, ulong e)
        {
            try
            {
                this.p = p;
                this.q = q;
                this.n = checked(p * q);
                this.phi = checked((p - 1) * (q - 1));
                this.e = e;
                this.d = generatePrivateKey(e, phi);
                this.isConfigured = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Your inputs are too big. Please provide another numbers");
                this.isConfigured = false;
            }
        }
        
        public string GetPublicKey()
        {
            return $"\n e: {this.e}\n n: {this.n}\n";
        }

        private static ulong crackRSA(ulong e, ulong n)
        {
            var startingValue = Program.FloorSqrt(n);
            ulong p = 0, q = 0, d = 0;
            
            for (var i = startingValue; i != 0; i--)
            {
                if (!Program.IsPrime(i)) continue;
                if (n % i != 0) continue;
                p = i;
                break;
            }

            q = n / p;

            d = generatePrivateKey(e, (p - 1) * (q - 1));
            
            return d;
        }
        
        /*
        private ulong findPublicKey(ulong e, ulong encryptedValue)
        {
            ulong p = 0;
            ulong q = 0;
            ulong n = 0;
            ulong phi = 0;
            ulong d = 0;
            
            for (var i = 0; i < PrimesList.Count; i++)
            {
                p = (ulong)PrimesList[i];
                for (var j = 0; j < PrimesList.Count; j++)
                {
                    q = (ulong)PrimesList[j];
                    n = p * q;
                    phi = (p - 1) * (q - 1);
                    d = generatePrivateKey(e, phi);
                    if (d == 27 && p == 5 && q == 11)
                    {
                        Console.WriteLine();
                    }
                    for (ulong k = 1; k < n; k++)
                    {
                        var num = modpow(k, e, n);
                        
                        if (num == encryptedValue)
                        {
                            if(modpow(encryptedValue, d, n) == k)
                            {
                                Console.WriteLine($"I found the value. The private key is {d} p is {p} q is {q}");
                                return d;
                            }
                        }
                    }
                    
                }    
            }
            
            return 0;
        }
        */
        private ulong generatePublicKey(ulong phi)
        {
            ulong e;
            for (e = Program.FloorSqrt(Program.FloorSqrt(phi)); e < phi; e++)
            {
                if (Program.Gcd(e, phi) == 1 && randomObject.Next(0, 101) % 2 == 0) // random is used to always create different e values
                {
                    break;
                }

                if (e + 1 == phi)
                {
                    e = 2;
                }
            }

            return e;
        }
        
        private static ulong generatePrivateKey(ulong e, ulong phi) //https://www.di-mgt.com.au/euclidean.html#extendedeuclidean
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

        public string Encrypt(string input)
        {
            printRSA();
            ulong num = 0; //num
            ulong pNum = 0; //previous num
            
            List<ulong> listOfEncryptedNumbers = new List<ulong>();

            for (var i = 0; i < input.Length; i++)
            {
                try
                {
                    var b64Chr = input[i];
                    num += 65 * num + (ulong)(Base64Alphabet.IndexOf(b64Chr) + 1);
                }
                catch (OverflowException)
                {
                    listOfEncryptedNumbers.Add(Program.Modpow(pNum, e, n));
                    i--;
                    num = 0;
                    pNum = 0;
                    continue;
                }
                
                if (num > n)
                {
                    listOfEncryptedNumbers.Add(Program.Modpow(pNum, e, n));
                    i--;
                    num = 0;
                    pNum = 0;
                    continue;
                }

                pNum = num;
                
                if (i == input.Length - 1)
                {
                    listOfEncryptedNumbers.Add(Program.Modpow(pNum, e, n));
                }
            }
            
            var ciphertext = "";
            
            foreach (var item in listOfEncryptedNumbers)
            {
                ciphertext += Convert.ToBase64String(BitConverter.GetBytes(item));
            }

            return ciphertext;
        }
        
        public string Decrypt(string input)
        {
            List<string> listOfCipherTexts = new List<string>();
            string plaintext = "";
            
            try
            {
                for (var i = 0; i < input.Length; i += 12)
                {
                    listOfCipherTexts.Add(input.Substring(i, 12));
                }

                for (var i = 0; i < listOfCipherTexts.Count; i++)
                {
                    string currentString = listOfCipherTexts[i];

                    byte[] base64EncodedBytes;
                    base64EncodedBytes = Convert.FromBase64String(currentString);


                    var num = BitConverter.ToUInt64(base64EncodedBytes, 0);
                    num = Program.Modpow(num, d, n);
                    string tempText = "";

                    while (num > 0)
                    {
                        var chrIndex = (num % 66);
                        tempText = Base64Alphabet[(int)chrIndex - 1] + tempText;
                        num -= chrIndex;
                        num /= 66;
                    }

                    plaintext += tempText;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Looks like your ciphertext is wrong. I could not decrypt it. Please, try again");
                return "";
            }

            return plaintext;
        }

        /*
        public static string ConvertUlong(ulong number)
        {
            var validCharacters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890!@#$%^&()_-";
            char[] charArray = validCharacters.ToCharArray();
            var buffer = new StringBuilder();
            var quotient = number;
            ulong remainder;
            while (quotient != 0)
            {
                remainder = quotient % (ulong)charArray.LongLength;
                quotient = quotient / (ulong)charArray.LongLength;
                buffer.Insert(0, charArray[remainder].ToString());
            }
            return buffer.ToString();
        }
        */

    }
}