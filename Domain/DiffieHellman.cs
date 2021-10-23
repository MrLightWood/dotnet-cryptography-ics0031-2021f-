using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class DiffieHellman
    {
        public int Id { get; set; }
        
        public ulong P { get; set; }
        public ulong Q { get; set; }
        public ulong UserAInput { get; set; }
        public ulong UserBInput { get; set; }
        
        public ulong SharedKey { get; set; }
        
        [MaxLength(256)]
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }
    }
}