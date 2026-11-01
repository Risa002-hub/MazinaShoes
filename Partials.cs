using System;
using System.Linq;

namespace MazinaShoes
{
    // Вычисляемые свойства лежат здесь, а не в Product.cs и др.:
    // файлы сущностей генерируются заново при обновлении модели, этот файл остаётся.
    public partial class Product
    {
        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(Picture))
                    return "/img/picture.png";
                return $"/img/{Picture}";
            }
        }

        // количество пар суммарно по всем размерам
        public int TotalCount
        {
            get { return StockItem.Sum(s => s.Count); }
        }

        public string StockText
        {
            get { return TotalCount > 5 ? "много" : "мало"; }
        }

        // три пары и меньше: строку в списке подсвечиваем
        public bool IsLowStock
        {
            get { return TotalCount <= 3; }
        }

        // скидка 25%, если в прошлом календарном месяце заказов этой модели не было
        public bool HasDiscount
        {
            get
            {
                DateTime thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime prevMonth = thisMonth.AddMonths(-1);
                return !OrderItem.Any(oi => oi.Order.OrderDate >= prevMonth && oi.Order.OrderDate < thisMonth);
            }
        }

        public decimal PriceWithDiscount
        {
            get { return HasDiscount ? Math.Round(Price * 0.75m, 2) : Price; }
        }
    }

    public partial class User
    {
        public string FullName
        {
            get { return $"{LastName} {FirstName} {Patronymic}".Trim(); }
        }
    }

    public partial class StockItem
    {
        // подпись размера в выпадающем списке
        public string Caption
        {
            get { return $"{Size.SizeValue:0.#} (в наличии: {Count})"; }
        }
    }

    public partial class OrderItem
    {
        public string ProductText
        {
            get { return $"{Product.ProductName} ({Product.Manufacturer.ManufacturerName})"; }
        }

        public decimal LineSum
        {
            get { return Count * PriceOneItem; }
        }
    }
}
