using System.ComponentModel.DataAnnotations;

namespace asp_hub_kt7.Models
{
    public class User
    {
        public int Id { get; set; }
        public int Age { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Email { get; set; }
    }
}
