using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class Vigenere
    {
        public int Id { get; set; }
        
        [MaxLength(128)]
        public string Key { get; set; }
        
        [MaxLength(256)]
        public string CipherText { get; set; }
        
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }
    }
}