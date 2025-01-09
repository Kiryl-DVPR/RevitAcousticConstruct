using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using ClassLibrary;

namespace AcoustiCUtils
{
    [Transaction(TransactionMode.Manual)]
    public class TotalCalc : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
 
            UIApplication uiapp = commandData.Application;//Обращаемся к приложению Revit
            UIDocument uidoc = uiapp.ActiveUIDocument; //Оращаемся к интерфейсу Revit
            Document doc = uidoc.Document;//Обращаемся к проекту Revit

            var allElementsInDocList = new FilteredElementCollector(doc) //Сортирует все элементы в документе
                                                                         //.WhereElementIsCurveDriven()
               .WhereElementIsNotElementType()
               .Cast<Element>()
               .ToList();

            const string BRAND_CODE = "AG";

            if (CodeFilter.FindElements(allElementsInDocList, BRAND_CODE))
            {

                ConstrInfoPerType.GetInfo(CodeFilter.FilteredElementList, doc);

                var list = new List<Product>();


                var window = new AG_Table(list, ConstrInfoPerType.constructionInfo); // Передаём через конструтор таблицы пустой список продуктов и заполненный список конструкций

                var dispatcher = window.Dispatcher;

                try
                {
                    var task = Task.Run(async () =>
                    {

                        var productList = await Rest.GetCalcProduct(ConstrInfoPerType.elementInfo); //Ответ от сервиса в виде листа с продуктами

                        var productId = 1;

                        foreach (var product in productList)
                        {
                            product.Id = productId;
                            productId++;
                            if (char.IsLetter(product.Code[0])) { product.Code = "-"; }
                        };

                        _ = dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            window.UpdateListOfItems(productList); // Вызываем метод таблицы для записи данных о продуктах полученных с сервиса
                        }));

                    });
                    task.Wait();

                }
                catch (Exception)
                {

                    TaskDialog.Show("Acoustic Group", $"Проблемы с сервисом. Продукты Acoustic Group не просчитанны");
                }

                finally
                {
                    window.ShowDialog();
                }


            }

            else TaskDialog.Show("Acoustic Group", "Продукты Acoustic Group не найдены");

            return Result.Succeeded;

        }

    }
  
}
