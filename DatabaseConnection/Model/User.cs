using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace DatabaseConnection
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
