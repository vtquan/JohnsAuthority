using System.ComponentModel.DataAnnotations;

namespace JohnsAuthority.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }
}
