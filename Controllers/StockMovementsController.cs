using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseMvc.Data;
using WarehouseMvc.Models;
using Microsoft.AspNetCore.Authorization;


namespace WarehouseMvc.Controllers
{
    [Authorize]
    public class StockMovementsController : Controller
    {
        private readonly WarehouseContext _context;

        public StockMovementsController(WarehouseContext context)
        {
            _context = context;
        }

        // GET: StockMovements
        public async Task<IActionResult> Index()
        {
            var movements = await _context.StockMovements
                .AsNoTracking()
                .Include(s => s.Item)
                .Include(s => s.FromLocation)
                .Include(s => s.ToLocation)
                .OrderByDescending(s => s.TimestampUtc)
                .ToListAsync();

            return View(movements);
        }

        // GET: StockMovements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _context.StockMovements
                .AsNoTracking()
                .Include(s => s.Item)
                .Include(s => s.FromLocation)
                .Include(s => s.ToLocation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movement == null) return NotFound();

            return View(movement);
        }

        // GET: StockMovements/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: StockMovements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,Type,Quantity,FromLocationId,ToLocationId,Note")] StockMovement stockMovement)
        {
            ApplyBusinessRules(stockMovement);

           
            if (stockMovement.Type == MovementType.Out && stockMovement.FromLocationId is int fromA)
            {
                var onHand = await GetOnHandAsync(stockMovement.ItemId, fromA);
                if (stockMovement.Quantity > onHand)
                    ModelState.AddModelError(nameof(StockMovement.Quantity),
                        $"Not enough stock. On hand at selected location: {onHand}.");
            }
            else if (stockMovement.Type == MovementType.Transfer && stockMovement.FromLocationId is int fromB)
            {
                var onHand = await GetOnHandAsync(stockMovement.ItemId, fromB);
                if (stockMovement.Quantity > onHand)
                    ModelState.AddModelError(nameof(StockMovement.Quantity),
                        $"Not enough stock to transfer. On hand at source: {onHand}.");
            }

            if (ModelState.IsValid)
            {
                stockMovement.TimestampUtc = DateTime.UtcNow;
                _context.Add(stockMovement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(stockMovement);
            return View(stockMovement);
        }

        // GET: StockMovements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _context.StockMovements.FindAsync(id);
            if (movement == null) return NotFound();

            PopulateDropdowns(movement);
            return View(movement);
        }

        // POST: StockMovements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemId,Type,Quantity,FromLocationId,ToLocationId,TimestampUtc,Note")] StockMovement stockMovement)
        {
            if (id != stockMovement.Id) return NotFound();

            ApplyBusinessRules(stockMovement);

            if (stockMovement.Type == MovementType.Out && stockMovement.FromLocationId is int fromA)
            {
                var onHand = await GetOnHandAsync(stockMovement.ItemId, fromA);
                if (stockMovement.Quantity > onHand)
                    ModelState.AddModelError(nameof(StockMovement.Quantity),
                        $"Not enough stock. On hand at selected location: {onHand}.");
            }
            else if (stockMovement.Type == MovementType.Transfer && stockMovement.FromLocationId is int fromB)
            {
                var onHand = await GetOnHandAsync(stockMovement.ItemId, fromB);
                if (stockMovement.Quantity > onHand)
                    ModelState.AddModelError(nameof(StockMovement.Quantity),
                        $"Not enough stock to transfer. On hand at source: {onHand}.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockMovement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockMovementExists(stockMovement.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(stockMovement);
            return View(stockMovement);
        }

        // GET: StockMovements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _context.StockMovements
                .AsNoTracking()
                .Include(s => s.Item)
                .Include(s => s.FromLocation)
                .Include(s => s.ToLocation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movement == null) return NotFound();

            return View(movement);
        }

        // POST: StockMovements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movement = await _context.StockMovements.FindAsync(id);
            if (movement != null)
            {
                _context.StockMovements.Remove(movement);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

      
        private void PopulateDropdowns(StockMovement? current = null)
        {
            ViewData["ItemId"] = new SelectList(
                _context.Items.AsNoTracking().OrderBy(i => i.Name),
                "Id", "Name",
                current?.ItemId
            );

            ViewData["FromLocationId"] = new SelectList(
                _context.Locations.AsNoTracking().OrderBy(l => l.Name),
                "Id", "Name",
                current?.FromLocationId
            );

            ViewData["ToLocationId"] = new SelectList(
                _context.Locations.AsNoTracking().OrderBy(l => l.Name),
                "Id", "Name",
                current?.ToLocationId
            );
        }

        private void ApplyBusinessRules(StockMovement m)
        {
           
            ModelState.Remove(nameof(StockMovement.FromLocationId));
            ModelState.Remove(nameof(StockMovement.ToLocationId));

            if (m.Type == MovementType.In)
            {
                if (m.ToLocationId is null)
                    ModelState.AddModelError(nameof(StockMovement.ToLocationId), "Choose the destination location for IN.");
                m.FromLocationId = null;
            }
            else if (m.Type == MovementType.Out)
            {
                if (m.FromLocationId is null)
                    ModelState.AddModelError(nameof(StockMovement.FromLocationId), "Choose the source location for OUT.");
                m.ToLocationId = null;
            }
            else if (m.Type == MovementType.Transfer)
            {
                if (m.FromLocationId is null || m.ToLocationId is null)
                    ModelState.AddModelError(string.Empty, "Select both From and To locations for TRANSFER.");
                else if (m.FromLocationId == m.ToLocationId)
                    ModelState.AddModelError(string.Empty, "From and To locations cannot be the same.");
            }

            if (m.Quantity <= 0)
                ModelState.AddModelError(nameof(StockMovement.Quantity), "Quantity must be greater than zero.");
        }

        private async Task<int> GetOnHandAsync(int itemId, int locationId)
        {
            var qty = await _context.StockMovements
                .Where(m => m.ItemId == itemId &&
                       (
                           (m.Type == MovementType.In && m.ToLocationId == locationId) ||
                           (m.Type == MovementType.Out && m.FromLocationId == locationId) ||
                           (m.Type == MovementType.Transfer && (m.ToLocationId == locationId || m.FromLocationId == locationId))
                       ))
                .SumAsync(m =>
                    m.Type == MovementType.In && m.ToLocationId == locationId ? m.Quantity :
                    m.Type == MovementType.Out && m.FromLocationId == locationId ? -m.Quantity :
                    m.Type == MovementType.Transfer && m.ToLocationId == locationId ? m.Quantity :
                    m.Type == MovementType.Transfer && m.FromLocationId == locationId ? -m.Quantity :
                    0
                );

            return qty;
        }

        private bool StockMovementExists(int id) =>
            _context.StockMovements.Any(e => e.Id == id);
    }
}
