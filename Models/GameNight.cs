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
        public bool Is18Plus { get; set; }
        public bool ProvidesVegetarianOptions { get; set; }
        public bool ProvidesLactoseFreeOptions { get; set; }
        public bool ProvidesNutsFreeOptions { get; set; }
        public bool ProvidesAlcoholFreeOptions { get; set; }
        public bool IsSnackRequired { get; set; }

        public List<GameNightGame> GameNightGames { get; set; } = new List<GameNightGame>();
        public ICollection<GameNightParticipant> Participants { get; set; } = new List<GameNightParticipant>();
        public ICollection<GameNightSnack> Snacks { get; set; } = new List<GameNightSnack>();
    }
}


