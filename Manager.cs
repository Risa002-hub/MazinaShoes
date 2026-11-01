using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MazinaShoes
{
    internal class Manager
    {
        public static Frame MainFrame {get; set;}

        // null, пока вошёл гость или никто не входил
        public static User CurrentUser { get; set; }

        // заказ, который собирается в данный момент
        public static List<CartLine> Cart = new List<CartLine>();

        public static bool IsAuthorized
        {
            get { return CurrentUser != null; }
        }

        // 1 - администратор, 2 - менеджер (RoleID из таблицы Role)
        public static bool IsStaff
        {
            get { return CurrentUser != null && (CurrentUser.RoleID == 1 || CurrentUser.RoleID == 2); }
        }

        public static bool IsAdmin
        {
            get { return CurrentUser != null && CurrentUser.RoleID == 1; }
        }

        // Вернуть пары позиции заказа на склад (при удалении позиции или заказа)
        public static void ReturnToStock(OrderItem item)
        {
            var ctx = MazinaShoesEntities.GetContext();
            var stock = ctx.StockItem.FirstOrDefault(s => s.ProductID == item.ProductID && s.SizeID == item.SizeID);
            if (stock == null)
                ctx.StockItem.Add(new StockItem { ProductID = item.ProductID, SizeID = item.SizeID, Count = item.Count });
            else
                stock.Count += item.Count;
        }
    }
}
