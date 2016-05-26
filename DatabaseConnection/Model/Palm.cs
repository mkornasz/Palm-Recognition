using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace DatabaseConnection.Model
{
    public class Palm
    {
        [Key]
        public int PalmId { get; set; }
        public string Description { get; set; }
        public double PalmRadius { get; set; }
        public double IndexFingerLength { get; set; }
        public double IndexFingerTop { get; set; }
        public double IndexFingerMid { get; set; }
        public double IndexFingerBot { get; set; }
        public double MiddleFingerLength { get; set; }
        public double MiddleFingerTop { get; set; }
        public double MiddleFingerMid { get; set; }
        public double MiddleFingerBot { get; set; }
        public double RingFingerLength { get; set; }
        public double RingFingerTop { get; set; }
        public double RingFingerMid { get; set; }
        public double RingFingerBot { get; set; }
        public double PinkyFingerLength { get; set; }
        public double PinkyFingerTop { get; set; }
        public double PinkyFingerMid { get; set; }
        public double PinkyFingerBot { get; set; }
        public PalmImage Image { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class PalmContext : DbContext
    {
        public DbSet<Palm> Palms { get; set; }
        public DbSet<PalmImage> PalmImages { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
