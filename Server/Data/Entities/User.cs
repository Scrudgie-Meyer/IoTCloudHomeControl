using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsAdmin { get; set; } = false;
        public string EmailConfirmationToken { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; } = false;
        public bool Hidden { get; set; } = false;
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public User(string username, string email, string passwordHash)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsAdmin = false;
            IsEmailConfirmed = false;
            Hidden = false;
            EmailConfirmationToken = string.Empty;
        }
    }
}
