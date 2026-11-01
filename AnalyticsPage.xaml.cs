using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MazinaShoes
{
    // Одна строка топ-5: показывается в таблице и попадает в отчёт
    public class TopProduct
    {
        public string ProductName { get; set; }
        public string Manufacturer { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для AnalyticsPage.xaml
    /// </summary>
    public partial class AnalyticsPage : Page
    {
        private int _ordersCount;
        private decimal _salesSum;
        private List<TopProduct> _top = new List<TopProduct>();

        public AnalyticsPage()
        {
            InitializeComponent();

            // по умолчанию текущий календарный месяц
            DateTime firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DPStart.SelectedDate = firstDay;
            DPEnd.SelectedDate = firstDay.AddMonths(1).AddDays(-1);
        }

        // пересчёт сразу при смене любой даты
        private void Period_Changed(object sender, SelectionChangedEventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            ClearResults();

            // вторая дата ещё не выбрана (так бывает и при открытии страницы)
            if (DPStart.SelectedDate == null || DPEnd.SelectedDate == null)
                return;

            DateTime start = DPStart.SelectedDate.Value.Date;
            DateTime end = DPEnd.SelectedDate.Value.Date;
            if (start > end)
            {
                // окно показываем после смены даты, а не во время неё
                Dispatcher.BeginInvoke(new Action(() =>
                    MessageBox.Show("Дата начала не может быть позже даты окончания. Измените одну из дат.",
                        "Аналитика", MessageBoxButton.OK, MessageBoxImage.Warning)));
                return;
            }

            try
            {
                // конец периода включительно: берём всё до начала следующего дня
                DateTime endNext = end.AddDays(1);
                var orders = MazinaShoesEntities.GetContext().Order
                    .Where(o => o.OrderDate >= start && o.OrderDate < endNext)
                    .ToList();
                var items = orders.SelectMany(o => o.OrderItem).ToList();

                _ordersCount = orders.Count;
                _salesSum = items.Sum(i => i.LineSum);
                _top = items
                    .GroupBy(i => i.Product)
                    .Select(g => new TopProduct
                    {
                        ProductName = g.Key.ProductName,
                        Manufacturer = g.Key.Manufacturer.ManufacturerName,
                        Count = g.Sum(i => i.Count)
                    })
                    .OrderByDescending(t => t.Count)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось посчитать данные: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TBOrders.Text = "Заказов за период: " + _ordersCount;
            TBSum.Text = "Сумма продаж: " + _salesSum.ToString("N2") + " руб.";
            GridTop.ItemsSource = _top;
        }

        private void ClearResults()
        {
            _ordersCount = 0;
            _salesSum = 0;
            _top = new List<TopProduct>();
            TBOrders.Text = "Заказов за период: —";
            TBSum.Text = "Сумма продаж: —";
            GridTop.ItemsSource = null;
        }

        // PDF делает встроенный в Windows принтер «Microsoft Print to PDF»: пакеты не нужны
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (DPStart.SelectedDate == null || DPEnd.SelectedDate == null || DPStart.SelectedDate > DPEnd.SelectedDate)
            {
                MessageBox.Show("Сначала выберите правильный период.", "Экспорт в PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Calibri");
            doc.FontSize = 14;
            doc.PagePadding = new Thickness(50);

            var title = new Paragraph(new Run("Чудо Обувь. Отчёт по продажам"));
            title.FontSize = 22;
            title.FontWeight = FontWeights.Bold;
            doc.Blocks.Add(title);
            doc.Blocks.Add(new Paragraph(new Run("Период: " + DPStart.SelectedDate.Value.ToString("dd.MM.yyyy")
                + " – " + DPEnd.SelectedDate.Value.ToString("dd.MM.yyyy"))));
            doc.Blocks.Add(new Paragraph(new Run(TBOrders.Text)));
            doc.Blocks.Add(new Paragraph(new Run(TBSum.Text)));

            var topTitle = new Paragraph(new Run("Топ-5 моделей по количеству проданных пар:"));
            topTitle.FontWeight = FontWeights.Bold;
            doc.Blocks.Add(topTitle);
            if (_top.Count == 0)
                doc.Blocks.Add(new Paragraph(new Run("За период продаж нет.")));
            for (int i = 0; i < _top.Count; i++)
            {
                doc.Blocks.Add(new Paragraph(new Run((i + 1) + ". " + _top[i].ProductName
                    + " (" + _top[i].Manufacturer + ") — " + _top[i].Count + " пар.")));
            }

            var dialog = new PrintDialog();
            try
            {
                // сразу выбираем PDF-принтер, чтобы отчёт не ушёл на обычный принтер
                dialog.PrintQueue = new LocalPrintServer().GetPrintQueue("Microsoft Print to PDF");
            }
            catch (PrintQueueException)
            {
                MessageBox.Show("Принтер «Microsoft Print to PDF» не найден. Выберите в окне печати принтер, который сохраняет в PDF.",
                    "Экспорт в PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (dialog.ShowDialog() != true)
                return;

            // документ подгоняем под ширину страницы выбранного принтера
            doc.PageWidth = dialog.PrintableAreaWidth;
            doc.PageHeight = dialog.PrintableAreaHeight;
            doc.ColumnWidth = dialog.PrintableAreaWidth;
            try
            {
                dialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Отчёт по продажам");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить отчёт: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
