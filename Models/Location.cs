using System.ComponentModel.DataAnnotations;

namespace WarehouseMvc.Models;

public class Location
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = "";
}
