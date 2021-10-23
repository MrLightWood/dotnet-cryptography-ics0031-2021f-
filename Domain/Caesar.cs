using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class Caesar
    {
        public int Id { get; set; }
        
        public int ShiftAmount { get; set; }
        
        [MaxLength(256)]
        public string CipherText { get; set; }
        
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }
    }
}