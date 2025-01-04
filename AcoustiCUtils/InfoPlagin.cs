using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcoustiCUtils
{
    [Transaction(TransactionMode.Manual)]
    public class InfoPlagin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;//Обращаемся к приложению Revit
            UIDocument uidoc = uiapp.ActiveUIDocument; //Оращаемся к интерфейсу Revit
            Document doc = uidoc.Document;//Обращаемся к проекту Revit

            string pathVercsion = $@"C:\ProgramData\Autodesk\Revit\Addins\{ClassLibrary.GlobalData.VersionRevit}\AcousticConstructor\update\version.txt";
            string textVersion = null;

            var task = Task.Run(async () =>
            {
                using (StreamReader reader = new StreamReader(pathVercsion))
                {
                    textVersion = await reader.ReadToEndAsync();
                }

            });
            task.Wait();

            TaskDialog.Show("Информация о плагине ACOUSTIC®", $"Текущая версия плагина: {textVersion}");

            return Result.Succeeded;
            
        } 
    }
}
