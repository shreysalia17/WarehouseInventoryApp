using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseMvc.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Optional link to an item
        public int? ItemId { get; set; }
        public Item? Item { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }   // money coming in

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }      // cost for this sale (purchase / manufacturing, etc.)

        [StringLength(200)]
        public string? Note { get; set; }

        [NotMapped]
        public decimal Profit => Revenue - Cost;
    }
}
