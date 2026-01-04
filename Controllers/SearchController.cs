using Microsoft.AspNetCore.Mvc;

namespace WarehouseMVC.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Go(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index", "Home");
            }

            var term = q.Trim().ToLower();

          
            if (term.Contains("sale"))
            {
                return RedirectToAction("Index", "Sales");
            }

            if (term.Contains("item"))
            {
                return RedirectToAction("Index", "Items");
            }

            if (term.Contains("location") || term.Contains("config"))
            {
                return RedirectToAction("Index", "Locations");
            }

            if (term.Contains("movement") || term.Contains("stock"))
            {
                return RedirectToAction("Index", "StockMovements");
            }

            if (term.Contains("report"))
            {
                return RedirectToAction("StockBalances", "Reports");
            }

            if (term.Contains("dashboard") || term.Contains("home"))
            {
                return RedirectToAction("Index", "Home");
            }

            // Fallback: dashboard
            return RedirectToAction("Index", "Home");
        }
    }
}
