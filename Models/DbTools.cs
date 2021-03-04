using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CryptoTec.Models
{
    public class DbTools : DbContext
    {
        // Database table for `Login`
        public DbSet<Login> Login { get; set; }
        // Database table for `TodoItem`
        public DbSet<TodoItem> TodoItem { get; set; }

        /* Constructor */
        public DbTools(DbContextOptions<DbTools> options) : base(options) { }

        // This has been made obsolete because of the connstring inside `startup.cs`
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //    optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-2CPTVPL\JFKK;Initial Catalog=jfkkdb;Integrated Security=True");
        //}
    }
}
