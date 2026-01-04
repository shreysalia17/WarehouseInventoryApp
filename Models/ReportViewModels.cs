namespace WarehouseMVC.Models
{
    // Used by StockBalances report
    public class StockBalanceRow
    {
        public string ItemName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    // Used by LowStock report
    public class LowStockRow
    {
        public string ItemName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;

        // Current quantity available
        public int Quantity { get; set; }

        // Minimum quantity before we consider it “low”
        public int ReorderLevel { get; set; }
    }
}
