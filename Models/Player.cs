using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BordspelAvond.Models
{
    public class Player : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string Gender { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }

    }
}
