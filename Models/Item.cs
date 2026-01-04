using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace WarehouseMvc.Models;

public class Item
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [Required, StringLength(40)]
    public string Sku { get; set; } = "";

    [StringLength(60)]
    public string? Category { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; } = 0;

    public ICollection<StockMovement>? Movements { get; set; }
}
