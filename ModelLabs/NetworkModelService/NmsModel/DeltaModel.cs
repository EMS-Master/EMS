using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.NmsModel
{
    public class DeltaModel
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public byte[] Delta { get; set; }
    }
}
