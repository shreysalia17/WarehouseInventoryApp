using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseMvc.Data;      
using WarehouseMvc.Models;
using WarehouseMVC.Models;    

namespace WarehouseMvc.Controllers
{
    public class ReportsController : Controller
    {
        private readonly WarehouseContext _context;

        public ReportsController(WarehouseContext context)
        {
            _context = context;
        }

        // STOCK BALANCES REPORT
        public async Task<IActionResult> StockBalances()
        {
            var rows = await CalculateStockBalancesAsync();//fetches the stock balance
            return View(rows);
        }

        
        // LOW STOCK REPORT
      
        public async Task<IActionResult> LowStock()
        {
            
            const int reorderLevel = 5;

            var balances = await CalculateStockBalancesAsync();

            var lowStock = balances
                .Where(b => b.Quantity <= reorderLevel)
                .Select(b => new LowStockRow
                {
                    ItemName = b.ItemName,
                    LocationName = b.LocationName,
                    Quantity = b.Quantity,
                    ReorderLevel = reorderLevel
                })
                .OrderBy(r => r.ItemName)
                .ThenBy(r => r.LocationName)
                .ToList();

            return View(lowStock);
        }

        
        // INTERNAL HELPER: CALCULATE CURRENT STOCK BY ITEM+LOCATION
      
        private async Task<List<StockBalanceRow>> CalculateStockBalancesAsync()
        {
            var movements = await _context.StockMovements
                .Include(m => m.Item)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .ToListAsync();// fetches data from db such as location, Items and StockMovements 

            // Key: (ItemName, LocationName) -> Quantity
            var totals = new Dictionary<(string Item, string Location), int>();

            foreach (var m in movements)
            {
                var itemName = m.Item?.Name ?? "Unknown item";
                var fromName = m.FromLocation?.Name;
                var toName = m.ToLocation?.Name;

                void AddQty(string locationName, int delta)
                {
                    if (string.IsNullOrWhiteSpace(locationName))
                        return;

                    var key = (itemName, locationName);

                    if (!totals.ContainsKey(key))
                    {
                        totals[key] = 0;
                    }

                    totals[key] += delta;
                }

                switch (m.Type)
                {
                    case MovementType.In:
                        // Stock coming into a location
                        AddQty(toName, m.Quantity);
                        break;

                    case MovementType.Out:
                        // Stock leaving a location
                        AddQty(fromName, -m.Quantity);
                        break;

                    case MovementType.Transfer:
                        // Move from one location to another
                        AddQty(fromName, -m.Quantity);
                        AddQty(toName, m.Quantity);
                        break;

                    default:
                        break;
                }
            }

            var rows = totals
                .Where(kvp => kvp.Value != 0)
                .Select(kvp => new StockBalanceRow
                {
                    ItemName = kvp.Key.Item,
                    LocationName = kvp.Key.Location,
                    Quantity = kvp.Value
                })
                .OrderBy(r => r.ItemName)
                .ThenBy(r => r.LocationName)
                .ToList();

            return rows;
        }
    }
}
