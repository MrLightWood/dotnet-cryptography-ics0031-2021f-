using System;

namespace HW2
{
    public class DiffieHellman
    {
        private Random randomObject = new Random();
        private ulong p; //first prime
        private ulong q; //second prime
        private ulong n; //p*q
        private ulong phi; //(p-1) * (q-1)
        private ulong e; //public key
        private ulong d; //private key
        private ulong personAInput;
        private ulong personBInput;
        public bool isConfigured;
        
        public DiffieHellman() {}

        public void printDH()
        {
            Console.WriteLine("p: " + p);
            Console.WriteLine("q: " + q);
            Console.WriteLine("n: " + n);
            Console.WriteLine("phi: " + phi);
            Console.WriteLine("e: " + e);
            Console.WriteLine("d: " + d);
            Console.WriteLine("personAInput: " + personAInput);
            Console.WriteLine("personBInput: " + personBInput);
        }
        
        public DiffieHellman(ulong p, ulong q, ulong personA, ulong personB)
        {
            try
            {
                this.p = p;
                this.q = q;
                this.n = checked(p * q);
                this.phi = checked((p - 1) * (q - 1));
                this.e = generatePublicKey(phi);
                this.d = generatePrivateKey(e, phi);
                this.personAInput = personA;
                this.personBInput = personB;
                this.isConfigured = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Your inputs are too big. Please provide another numbers");
                this.isConfigured = false;
            }
        }

        private ulong generatePublicKey(ulong phi)
        {
            ulong e;
            for (e = Program.FloorSqrt(Program.FloorSqrt(phi)); e < phi; e++)
            {
                if (Program.Gcd(e, phi) == 1 && randomObject.Next(0, 101) % 2 == 0)
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
        
        private ulong generatePrivateKey(ulong e, ulong phi) //https://www.di-mgt.com.au/euclidean.html#extendedeuclidean
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
        
        private static ulong modpow(ulong a,ulong b,ulong mod)
        {
            ulong product,pseq;
            product=1;
            pseq=a%mod;
            try
            {
                while (b > 0)
                {
                    if ((b & 1) == 1)
                        product = modmult(product, pseq, mod);
                    pseq = modmult(pseq, pseq, mod);
                    b >>= 1;
                }
            } catch (OverflowException)
            {
                Console.WriteLine("Prime numbers are too big, please select smaller ones");
            }

            return product;
        }

        public ulong getSharedKey()
        {
            ulong personAOut = modpow(q, personAInput, p);
            ulong personBOut = modpow(q, personBInput, p);
            
            ulong personAKey = modpow(personBOut, personAInput, p);
            ulong personBKey = modpow(personAOut, personBInput, p);

            if (personAKey == personBKey)
            {
                return personAKey;
            }

            return 0;
        }
        
        private static ulong modmult(ulong a,ulong b,ulong mod)
        {
            if (a == 0 || b < mod / a)
                return (a*b)%mod;
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
    }
}