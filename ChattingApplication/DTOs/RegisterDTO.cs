using System.ComponentModel.DataAnnotations;

namespace ChattingApplication.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string userName { get; set; }
        [Required]
        public string knownAs { get; set; }
        [Required]
        public DateOnly? DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        [StringLength(8)]
        public string password { get; set; }

    }
}
