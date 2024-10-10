namespace BordspelAvond.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GameGenre Genre { get; set; }
        public bool is18Plus { get; set; }
        public GameType Type { get; set; }  
        public string PhotoUrl { get; set; }

        public List<GameNight> GameNights { get; set; } = new List<GameNight>();

        public enum GameType
        {
            BoardGame,
            CardGame,
            DiceGame,
            PartyGame
        }
        public enum GameGenre
        {
            Strategy,
            Luck,
            Party,
            Family
        }
    }
}
