namespace BordspelAvond.Core.Domain
{
    public class GameNightGame
    {
        public int GameNightId { get; set; }
        public GameNight GameNight { get; set; } = null!;

        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
    }
}
