using Microsoft.AspNetCore.Identity;

namespace BordspelAvond.Models
{
    public class GameNight
    {
        public int Id { get; set; }
        public int OrganizerId { get; set; }
        public Player Organizer { get; set; } = null!; 
        public string Address { get; set; } = string.Empty; 
        public int MaxPlayers { get; set; }
        public DateTime DateAndTime { get; set; }
        public List<Player> Players { get; set; } = new List<Player>(); 
        public List<Game> PlayedGames { get; set; } = new List<Game>(); 
    }

}
