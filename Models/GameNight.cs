using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BordspelAvond.Models
{
    public class GameNight
    {
        public int Id { get; set; }
        public string? OrganizerId { get; set; }
        [NotMapped]
        public Player Organizer { get; set; } = null!; 
        public string Address { get; set; } = string.Empty; 
        public int MaxPlayers { get; set; }
        public DateTime DateAndTime { get; set; }
      
        public List<Game> PlayedGames { get; set; } = new List<Game>(); 
    }

}
