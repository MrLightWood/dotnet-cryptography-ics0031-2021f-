using System;
using System.Collections.Generic;
using static HW2.Utils;

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
        private static readonly List<int> PrimesList = GeneratePrimesNaive(100000);
        
        public ulong N => n;
        public ulong Phi => phi;
        public ulong E => e;
        public ulong D => d;

        public void setEvalue(ulong E)
        {
            this.e = E;
        }
        
        public void setDvalue(ulong D)
        {
            this.d = D;
        }
        public RSA() {}

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
        
        private ulong generatePublicKey(ulong phi)
        {
            ulong e;
            for (e = FloorSqrt(FloorSqrt(phi)); e < phi; e++)
            {
                if (Gcd(e, phi) == 1 && randomObject.Next(0, 101) % 2 == 0) // random is used to always create different e values
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

        public (bool, string) Encrypt(string input)
        {
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
                    listOfEncryptedNumbers.Add(Modpow(pNum, e, n));
                    i--;
                    num = 0;
                    pNum = 0;
                    continue;
                }
                
                if (num > n)
                {
                    listOfEncryptedNumbers.Add(Modpow(pNum, e, n));
                    i--;
                    num = 0;
                    pNum = 0;
                    continue;
                }

                pNum = num;
                
                if (i == input.Length - 1)
                {
                    listOfEncryptedNumbers.Add(Modpow(pNum, e, n));
                }
            }
            
            var ciphertext = "";
            
            foreach (var item in listOfEncryptedNumbers)
            {
                ciphertext += Convert.ToBase64String(BitConverter.GetBytes(item));
            }

            return (true, ciphertext);
        }
        
        public (bool, string) Decrypt(string input)
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
                    num = Modpow(num, d, n);
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
                return (false, "");
            }

            return (true, plaintext);
        }
        
    }
}