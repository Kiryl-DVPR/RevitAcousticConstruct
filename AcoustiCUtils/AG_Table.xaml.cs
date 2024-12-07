using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using SD = System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows;
using Window = System.Windows.Window;

using System.Globalization;
using ClassLibrary;
using System.Windows.Media;


namespace AcoustiCUtils
{
    public partial class AG_Table : Window
    {
        public static AG_Table mainWindow;
        private List<Product> _productsList = new List<Product>();
        private List<Construction> _constructionsList = new List<Construction>();

        public AG_Table(List<Product> productsList, List<Construction> construction)
        {
            InitializeComponent();
            mainWindow = this;
            ProductsListTable.ItemsSource = productsList; // Записываем данные переданные через конструктор в DataGRID продуктов
            ConstrListTable.ItemsSource = construction; // Записываем данные переданные через конструктор в DataGRID Конструкций

            _productsList = productsList; // Записываем в лист продуктов
            _constructionsList = construction; // Записываем в лист конструкций

        }

        private void Drag(object sender, SD.RoutedEventArgs e) //Метод для перетаскивания экрана
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                AG_Table.mainWindow.DragMove();
            }
        }

        public void UpdateListOfItems(List<Product> products)
        {
            ProductsListTable.ItemsSource = products; // Записываем продукты в DataGrid
            _productsList = products;
        }

        private void Button_Click(object sender, SD.RoutedEventArgs e)
        {

            Excel.Application excel = new Excel.Application();

            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];
            Worksheet sheet2 = (Worksheet)workbook.Sheets.Add();

            sheet1.Name = "Материалы";

            sheet2.Name = "Конструкции";

            //Ширина столбцов спецификации Материалов при выводе в Excel:
            sheet1.Columns[1].ColumnWidth = 4;
            sheet1.Columns[2].ColumnWidth = 65;
            sheet1.Columns[3].ColumnWidth = 7;
            sheet1.Columns[4].ColumnWidth = 7;
            sheet1.Columns[5].ColumnWidth = 10;
            sheet1.Columns[6].ColumnWidth = 40;

            //Ширина столбцов спецификации Конструкций при выводе в Excel:
            sheet2.Columns[1].ColumnWidth = 4;
            sheet2.Columns[2].ColumnWidth = 65;
            sheet2.Columns[3].ColumnWidth = 10;
            sheet2.Columns[4].ColumnWidth = 10;
            sheet2.Columns[5].ColumnWidth = 10;
            sheet2.Columns[6].ColumnWidth = 30;


            Range myRange1;
            Range myRange2;

            //Данные из списка конструкций
            for (int i = 0; i < ConstrListTable.Columns.Count; i++)
            {
                myRange1 = sheet2.Cells[1, i + 1]; // Создаём ячейки в Excel
                sheet2.Cells[1, i + 1].Font.Bold = true; // Стиль ячейки в Excel
                myRange1.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                myRange1.Value2 = ConstrListTable.Columns[i].Header; //Записываем в Excel содержимое таблицы AG_Table "Конструкции"

                for (int j = 0; j < _constructionsList.Count; j++)
                {
                    myRange2 = sheet2.Cells[j + 2, i + 1]; //Создаём ячейку в Excel файле
                    myRange2.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    switch (i)
                    {
                        case 0: myRange2.Value2 = _constructionsList[j].Id; break;
                        case 1: myRange2.Value2 = _constructionsList[j].Name; break;
                        case 2: myRange2.Value2 = _constructionsList[j].Units; break;
                        case 3: myRange2.Value2 = _constructionsList[j].Quantity; break;
                        case 4: myRange2.Value2 = _constructionsList[j].Weight; break;
                        case 5: myRange2.Value2 = _constructionsList[j].Description; break;
                        default: break;
                    };

                }
            }

            myRange1 = null;
            myRange2 = null;
            //Данные из списка материалов
            for (int i = 0; i < ProductsListTable.Columns.Count; i++)
            {
                myRange1 = sheet1.Cells[1, i + 1]; // Создаём ячейки в Excel
                myRange1.Font.Bold = true; // Стиль ячейки в Excel
                myRange1.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                myRange1.Value2 = ProductsListTable.Columns[i].Header; //Записываем в Excel содержимое таблицы MaterialTableAG

                for (int j = 0; j < _productsList.Count; j++)
                {
                    myRange2 = sheet1.Cells[j + 2, i + 1]; //Создаём ячейку в Excel файле
                    myRange2.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    switch (i)
                    {
                        case 0: myRange2.Value2 = j + 1; break;
                        case 1: myRange2.Value2 = _productsList[j].Name; break;
                        case 2: myRange2.Value2 = _productsList[j].Units; break;
                        case 3: myRange2.Value2 = _productsList[j].Quantity; break;
                        case 4:
                            if (_productsList[j].Code.Contains("."))
                            {
                                myRange2.Value2 = $"'{_productsList[j].Code}";
                            }
                            else
                            {
                                myRange2.Value2 = $"{_productsList[j].Code}";
                            };
                            break;
                        case 5: myRange2.Value2 = _productsList[j].InfoPack; break;
                        default: break;
                    };
                }
            }

            excel.Visible = true;

        }

        private void Button_ClickCancel(object sender, RoutedEventArgs e) //Кнопка для закрытия окна
        {
            this.Close();
        }

        private void Button_Clean(object sender, RoutedEventArgs e) //Кнопка для сворачивания окна
        {

            this.WindowState = WindowState.Minimized;
        }

    }

}
