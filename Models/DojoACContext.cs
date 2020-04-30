using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dojo_Activity.Models
{

    public class DojoACContext : DbContext
    {

        public DojoACContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users {get;set;}
        public DbSet<Actvty> Activities {get;set;}
        public DbSet<Participation> Participations {get;set;}
    }

}