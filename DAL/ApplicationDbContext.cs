using System;
using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Caesar> Caesars { get; set; }
        public DbSet<Vigenere> Vigeneres { get; set; }
        public DbSet<DiffieHellman> DiffieHellmans { get; set; }
        public DbSet<RSA> Rsas { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}