namespace BordspelAvond.Core.Domain
{
    public class GameNightParticipant
    {
        public int GameNightId { get; set; }
        public GameNight? GameNight { get; set; }

        public string? UserId { get; set; }
    }
}
