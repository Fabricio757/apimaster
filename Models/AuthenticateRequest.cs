using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class AuthenticateRequest
    {

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class InputJsonRequest
    {
        [Required]
        public string Input { get; set; }
    }
}