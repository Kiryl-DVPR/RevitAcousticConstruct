using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using ClassLibrary;

namespace AcoustiCUtils
{
    [Transaction(TransactionMode.Manual)]
    public class SelectCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;//Обращаемся к приложению Revit
            UIDocument uidoc = uiapp.ActiveUIDocument; //Оращаемся к интерфейсу Revit
            Document doc = uidoc.Document;//Обращаемся к документу

            IList<Reference> selectedElementRefList = new List<Reference>();

            try
            {
                selectedElementRefList = uidoc.Selection.PickObjects(ObjectType.Element, "Выберете элементы");
            }
            catch
            {
                TaskDialog.Show("AcousticGroup продукты", "Продукты AcousticGroup не выделенны");
                return Result.Succeeded;
            }


            const string BRAND_CODE = "AG";

            if (CodeFilter.FindElements(selectedElementRefList, BRAND_CODE, doc) && selectedElementRefList != null)
            {
                ConstrInfoPerType.GetInfo(CodeFilter.FilteredElementList, doc);

                var list = new List<Product>();

                var window = new AG_Table(list, ConstrInfoPerType.constructionInfo);

                var dispatcher = window.Dispatcher;

                try
                {
                    var task = Task.Run(async () =>
                    {

                        var productList = await Rest.GetCalcProduct(ConstrInfoPerType.elementInfo);

                        var productId = 1;

                        foreach (var product in productList)
                        {
                            product.Id = productId;
                            productId++;
                            if (char.IsLetter(product.Code[0])) { product.Code = "-"; }
                        };
                        _ = dispatcher.BeginInvoke(new Action(() =>
                        {
                            window.UpdateListOfItems(productList);
                        }));

                    });
                    task.Wait();

                }
                catch (Exception)
                {
                    TaskDialog.Show("AcousticGroup", $"Проблемы с сервисом");
                }
                finally
                {
                    window.ShowDialog();
                }

            }

            return Result.Succeeded;

        }
    }
}