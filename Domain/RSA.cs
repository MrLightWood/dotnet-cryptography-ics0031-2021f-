using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class RSA
    {
        public int Id { get; set; }
        
        public ulong P { get; set; }
        public ulong Q { get; set; }
        public ulong N { get; set; }
        public ulong Phi { get; set; }
        public ulong E { get; set; }
        public ulong D { get; set; }
        
        [MaxLength(256)]
        public string CipherText { get; set; }
        
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }
    }
}