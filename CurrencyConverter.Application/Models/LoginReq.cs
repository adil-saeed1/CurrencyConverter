using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Application.Models
{
    public class LoginReq
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
