using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WarehouseMvc.Data;
using WarehouseMvc.Models;
using WarehouseMVC.Models;

namespace WarehouseMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WarehouseContext _context;

        public HomeController(ILogger<HomeController> logger, WarehouseContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);   // last 7 days (today included)

            // Treat Revenue and Cost as PER-UNIT values
            var salesToday = await _context.Sales
                .Where(s => s.Date.Date == today)
                .ToListAsync();

            var allSales = await _context.Sales.ToListAsync();

            decimal todaySalesTotal = salesToday.Sum(s => s.Revenue * s.Quantity);
            decimal totalProfit = allSales.Sum(s => (s.Revenue - s.Cost) * s.Quantity);

            int movementsToday = await _context.StockMovements
                .CountAsync(m => m.TimestampUtc.Date == today);

            int movementsWeek = await _context.StockMovements
                .CountAsync(m => m.TimestampUtc.Date >= weekStart && m.TimestampUtc.Date <= today);

            //ViewBag Pass the Data from the Controller to the View In ASP.NET
            ViewBag.TodaySales = todaySalesTotal;//fetches data from the sales table 
            ViewBag.TotalProfit = totalProfit;//fetches the profit data by revenue - cost
            ViewBag.MovementsToday = movementsToday;
            ViewBag.MovementsWeek = movementsWeek;//overview of the warehouse inventory activity

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()//redirects to the error page
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
