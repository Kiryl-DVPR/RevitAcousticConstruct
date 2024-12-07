using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcoustiCUtils
{
    [Transaction(TransactionMode.Manual)]
    public class OpenWebSite : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;//Обращаемся к приложению Revit
            UIDocument uidoc = uiapp.ActiveUIDocument; //Оращаемся к интерфейсу Revit
            Document doc = uidoc.Document;//Обращаемся к проекту Revit

            System.Diagnostics.Process.Start("https://www.acoustic.ru");

            return Result.Succeeded;
            
        }

        
    }
}
