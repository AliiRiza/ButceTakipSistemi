using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // [Column] için

namespace ButceTakipSistemi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur")]
        [StringLength(100)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tutar girmelisiniz")]
        [Column(TypeName = "decimal(18, 2)")] // Veritabanında ondalık sayı tipi
        [Display(Name = "Tutar (TL)")]
        public decimal Amount { get; set; }

        [Display(Name = "Tarih")]
        [DataType(DataType.Date)] // Sadece tarih olarak tut
        public DateTime Date { get; set; } = DateTime.Now;

        // İlişki: Category Tablosu
        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}