using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseMvc.Data;
using WarehouseMvc.Models;

namespace WarehouseMvc.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly WarehouseContext _context;

        public SalesController(WarehouseContext context)
        {
            _context = context;
        }

        // GET: /Sales  (Sales report)
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Item)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            // Treat Revenue and Cost as PER-UNIT values
            var totalRevenue = sales.Sum(s => s.Revenue * s.Quantity);
            var totalCost = sales.Sum(s => s.Cost * s.Quantity);
            var totalProfit = totalRevenue - totalCost;

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCost = totalCost;
            ViewBag.TotalProfit = totalProfit;

            return View(sales);
        }

        // GET: /Sales/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ItemId"] = new SelectList(
                await _context.Items.OrderBy(i => i.Name).ToListAsync(),
                "Id", "Name");

            return View(new Sale
            {
                Date = DateTime.UtcNow
            });
        }

        // POST: /Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ItemId"] = new SelectList(
                    await _context.Items.OrderBy(i => i.Name).ToListAsync(),
                    "Id", "Name", sale.ItemId);

                return View(sale);
            }

            _context.Add(sale);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Sale recorded successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
