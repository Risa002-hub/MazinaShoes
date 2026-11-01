namespace MazinaShoes
{
    // Одна строка будущего заказа: размер конкретной модели и сколько пар берём
    public class CartLine
    {
        public StockItem Stock { get; set; }
        public int Count { get; set; }

        public Product Product
        {
            get { return Stock.Product; }
        }

        public string ProductText
        {
            get { return $"{Product.ProductName} ({Product.Manufacturer.ManufacturerName})"; }
        }

        public decimal Price
        {
            get { return Product.PriceWithDiscount; }
        }

        public decimal Sum
        {
            get { return Price * Count; }
        }
    }
}
