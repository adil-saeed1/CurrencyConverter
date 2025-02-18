using Newtonsoft.Json;

namespace CurrencyExchange.Application.Models
{
    public class Users
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public List<Users> GetMockUsers()
        {
            var json = System.IO.File.ReadAllText("users.json"); // Path to your JSON file
            return JsonConvert.DeserializeObject<List<Users>>(json);
        }
    }
}
