using System.ComponentModel.DataAnnotations;

namespace ChattingApplication.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string userName { get; set; }
        [Required]
        [StringLength(8)]
        public string password { get; set; }

    }
}
