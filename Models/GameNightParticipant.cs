namespace BordspelAvond.Models
{
    public class GameNightParticipant
    {
        public int GameNightId { get; set; }
        public GameNight? GameNight { get; set; }

        public string? UserId { get; set; } 
    }
}
