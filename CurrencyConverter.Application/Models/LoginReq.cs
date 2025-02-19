using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Application.Models
{
    public class LoginReq
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
