using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoTec.Models
{
    public class ChangePassword
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage="Password Mismatch")]
        public string ComparePassword { get; set; }
    }
}
