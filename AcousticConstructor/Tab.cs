using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AcousticConstructor;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using ClassLibrary;
using Serilog;
using Path = System.IO.Path;
using Revit.Async;

namespace AcoustiCTab
{
    [Transaction(TransactionMode.Manual)]
 
    public class Tab : IExternalApplication
    {
        public static string ExecutedItemName;

        public static string ExecutedItemCode;

        public static string ExecutedItemType;

        public static int ExecutedItemThicness;

        public static List<ListAg> ListConstrAg;

        public static string Version = ClassLibrary.GlobalData.VersionRevit;


        public Result OnShutdown(UIControlledApplication application)
        {   
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
 
            ComponentManager.PreviewExecute += ComponentManager_PreviewExecute;

            ClassLibrary.Logger.CreateLogger();

            ClassLibrary.Logger.Logger1.Information("Start");

            RevitTask.Initialize(application);

            //CREATE TAB ACOUSTIC®

            const string tabName = "ACOUSTIC®";

            application.CreateRibbonTab(tabName); //Создание вкладки в Revit
            
            var utilsFolderPathUtils = $@"C:\ProgramData\Autodesk\Revit\Addins\{Version}\AcousticConstructor\utils";

            var utilsFolderPath = $@"C:\ProgramData\Autodesk\Revit\Addins\{Version}\AcousticConstructor";

    //CREATE PANEL CALCULATE PRODUCT AG

            var panel = application.CreateRibbonPanel(tabName, "Расчёт продуктов AG"); // Create panel 'Расчёт продуктов AG'

            //Create button calc all:
            var buttonCalcAll = new PushButtonData("Вывод спецификации \nво всём существующем проекте", " Весь объём \nпроекта",
                System.IO.Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.TotalCalc"); 
            var uriImageCalcAll = new Uri($@"{utilsFolderPath}\images\LogoRevit.png", UriKind.Absolute);
            var LargeImageCalcAll = new BitmapImage(uriImageCalcAll);
            buttonCalcAll.LargeImage = LargeImageCalcAll;

            //Create button calc select:
            var buttonCalcSelect = new PushButtonData("Вывод спецификации \nдля выделенной области вручную", " Выделенная \nобласть",
                System.IO.Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.SelectCalc");
            var uriImageCalcSelect = new Uri($@"{utilsFolderPath}\images\LogoRevitSelect.png", UriKind.Absolute);
            var LargeImageCalcSelect = new BitmapImage(uriImageCalcSelect);
            buttonCalcSelect.LargeImage = LargeImageCalcSelect;

            panel.AddItem(buttonCalcAll);
            panel.AddItem(buttonCalcSelect);
    //CREATE PANEL PRODUCT AG ASYNC/AWAIT
            new RevitAsyncWindow();
            
            //CREATE PANEL INFO

            var panelWeb = application.CreateRibbonPanel(tabName, "Сайт"); // Create panel 

            var buttonWeb = new PushButtonData("Сайт Acoustic Group", "Перейти на сайт",
                Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.OpenWebSite");
            ImageSource ImageSourceWebAG = new BitmapImage(new Uri($@"{utilsFolderPath}\images\web16.png", UriKind.RelativeOrAbsolute));
            buttonWeb.Image = ImageSourceWebAG;
            buttonWeb.ToolTip = "Перейти на сайт Acoustic Group";

            var buttonInfoPlag = new PushButtonData("Информация о плагине", "Информация",
                Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.InfoPlagin");
            ImageSource ImageSourceInfoPlag = new BitmapImage(new Uri($@"{utilsFolderPath}\images\info16.png", UriKind.RelativeOrAbsolute));
            buttonInfoPlag.Image = ImageSourceInfoPlag;

            var projectButtons = new List<Autodesk.Revit.UI.RibbonItem>();

            projectButtons.AddRange(panelWeb.AddStackedItems(buttonWeb, buttonInfoPlag));

            Logger.Logger1.Information("Finish Tab");

            return Result.Succeeded;
        }

        private static void ComponentManager_PreviewExecute(object sender, EventArgs e)
        {
            var a = sender as Autodesk.Windows.RibbonItem;

            if (!a.Text.Contains("AG")) return;
            ExecutedItemName = a.Text;

            var index = ExecutedItemName.IndexOf(" ");

            ExecutedItemCode = ExecutedItemName.Substring(0, index);

            if (a.Id.Contains("Облицовки Acoustic Group"))
            {
                ExecutedItemType = "Облицовки";
            }
            if (a.Id.Contains("Потолки Acoustic Group"))
            {
                ExecutedItemType = "Потолки";

            }
            if (a.Id.Contains("Полы Acoustic Group"))
            {
                ExecutedItemType = "Полы";

            }
            if (a.Id.Contains("Перегородки Acoustic Group"))
            {
                ExecutedItemType = "Перегородки";

            }

            foreach (var item in ListConstrAg.Where(item => item.Code == ExecutedItemCode))
            {
                ExecutedItemThicness = item.Thickness;
            }
        }


    }
}