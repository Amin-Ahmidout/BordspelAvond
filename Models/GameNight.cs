using Microsoft.AspNetCore.Identity;

namespace BordspelAvond.Models
{
    public class GameNight
    {
        public int Id { get; set; }
       public string OrganizerId { get; set; }
        public IdentityUser Organizer { get; set; }

        public string Address { get; set; }
        public int MaxPlayers { get; set; }
        public DateTime DateAndTime { get; set; }

        public List<IdentityUser> Players { get; set; } = new List<IdentityUser>(); // Spelers die meedoen
        public List<Game> PlayedGames { get; set; } = new List<Game>(); // Gespeelde spellen
    }

}
