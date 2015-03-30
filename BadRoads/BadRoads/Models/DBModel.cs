using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BadRoads.Models
{
    public class DBModel:DbContext
    {
        public DBModel():base("TempModelDB")
        {       }
        public DbSet<User> User { get; set; }
    }

    [Table("Users")]
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
    }
}