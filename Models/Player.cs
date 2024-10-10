using System.ComponentModel.DataAnnotations;

namespace BordspelAvond.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
