using System.ComponentModel.DataAnnotations;

namespace ButceTakipSistemi.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; } = string.Empty;

        // 0: Gider, 1: Gelir
        [Required(ErrorMessage = "Kategori Tipi zorunludur")]
        [Display(Name = "Tip (0: Gider, 1: Gelir)")]
        public int Type { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}
