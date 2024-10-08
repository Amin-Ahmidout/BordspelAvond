namespace BordspelAvond.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public bool is18Plus { get; set; }
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
