using IndividuellUppgiftKlient.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient.Data
{
    public class UserDbContext : DbContext
    {


        public DbSet<User> users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
         => options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=UserAPI;Integrated Security=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

    }
}
