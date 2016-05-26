using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseConnection.Model
{
    public class PalmImage
    {
        [Key]
        [ForeignKey("Palm")]
        public int PalmId { get; set; }
        public byte[] Image { get; set; }
        public Palm Palm { get; set; }
        public byte[] DefectsImage { get; set; }
    }
}
