using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConnection.Model
{
    public class PalmImage
    {
        [Key]
        [ForeignKey("Palm")]
        public int PalmId { get; set; }
        public byte[] Image { get; set; }
        public Palm Palm { get; set; }
    }
}
