using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace DatabaseConnection
{
    public class User
    {
        [Key]
        public string Login { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }

    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
