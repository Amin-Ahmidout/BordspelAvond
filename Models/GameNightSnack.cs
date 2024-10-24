namespace BordspelAvond.Models
{
    public class GameNightSnack
    {
        public int Id { get; set; }
        public int GameNightId { get; set; }
        public string? SnackName { get; set; }
        public bool IsVegetarian { get; set; }
        public bool IsLactoseFree { get; set; }
        public bool IsNutFree { get; set; }
        public bool IsAlcoholFree { get; set; }

        //public string? UserId { get; set; } // Relatie met User  
        //public Player? User { get; set; } // Relatie met User

        public GameNight? GameNight { get; set; } // Relatie met GameNight
    }
}
