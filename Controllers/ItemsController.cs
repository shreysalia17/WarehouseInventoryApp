using Microsoft.AspNetCore.Authorization;          
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WarehouseMvc.Data;
using WarehouseMvc.Models;


namespace WarehouseMVC.Controllers
{
    [Authorize]                                
    public class ItemsController : Controller
    {
        private readonly WarehouseContext _context;

        public ItemsController(WarehouseContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .AsNoTracking()// data is fetched only in read only manner
                .OrderBy(i => i.Name)
                .ToListAsync();//fetches data from the Item table 

            return View(items);
        }

        // GET: Items/Details
        public async Task<IActionResult> Details(int? id)// fetch the data by its ID from the item table 
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);//View to display the Item

            if (item == null) return NotFound();

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Sku,Category,UnitPrice,ReorderLevel")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();

                TempData["Toast"] = "Item created.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // GET: Items/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Items/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]//use to create cross site fogery attacks ie coming from a valid source.
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Sku,Category,UnitPrice,ReorderLevel")] Item item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    TempData["Toast"] = "Item updated.";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id)) return NotFound();
                    else throw;
                }
            }
            return View(item);
        }

        // GET: Items/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Items/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                TempData["Toast"] = "Item deleted.";
                TempData["ToastType"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id) =>
            _context.Items.Any(e => e.Id == id);
    }
}
