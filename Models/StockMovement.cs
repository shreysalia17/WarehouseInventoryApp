using System.ComponentModel.DataAnnotations;

namespace WarehouseMvc.Models;

public class StockMovement
{
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }
    public Item? Item { get; set; }

    [Required]
    public MovementType Type { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public int? FromLocationId { get; set; }
    public Location? FromLocation { get; set; }

    public int? ToLocationId { get; set; }
    public Location? ToLocation { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string? Note { get; set; }
}
