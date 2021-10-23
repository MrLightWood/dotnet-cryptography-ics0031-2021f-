using System;
using static HW2.Utils;

namespace HW2
{
    public class DiffieHellman
    {
        private Random randomObject = new Random();
        private ulong p; //first prime
        private ulong q; //second prime
        private ulong personAInput;
        private ulong personBInput;
        public bool isConfigured;

        public DiffieHellman() {}

        public DiffieHellman(ulong p, ulong q, ulong personA, ulong personB)
        {
            try
            {
                this.p = p;
                this.q = q;
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

        public (bool, ulong) GetSharedKey()
        {
            ulong personAOut = Modpow(q, personAInput, p);
            ulong personBOut = Modpow(q, personBInput, p);
            
            ulong personAKey = Modpow(personBOut, personAInput, p);
            ulong personBKey = Modpow(personAOut, personBInput, p);

            if (personAKey == personBKey)
            {
                return (true, personAKey);
            }

            return (false, 0);
        }

    }
}