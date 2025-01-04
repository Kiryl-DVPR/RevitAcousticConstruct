using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using ClassLibrary;
using Serilog;
using Path = System.IO.Path;

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

        public static ILogger Logger;

        public Result OnShutdown(UIControlledApplication application)
        {   
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
 
            ComponentManager.PreviewExecute += ComponentManager_PreviewExecute;

            Logger = ClassLibrary.Logger.CreateLogger();

            Logger.Information("Start");

            var taskGetListAg = Task.Run(async () => await Requests.GetInfoConstr());
            var taskGetVersion = Task.Run(async () => await Requests.GetVersion());

            Task.WaitAll(taskGetListAg, taskGetVersion);
            
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

            //CREATE PANEL FOR ADD PRODUCT AG  

            ClassLibrary.GlobalData.VersionService = taskGetVersion.Result.data.Version;
            ListConstrAg = taskGetListAg.Result;

            var panelConstraction =
                application.CreateRibbonPanel(tabName,
                    "Звукоизоляционные конструкции");

            var pulldownButtonFloor = new CreateElementsTab().CreatePulldownButton("Полы Acoustic Group", "Полы",
                panelConstraction, 
                new Uri($@"{utilsFolderPath}\images\Floor.png", UriKind.Absolute)); //Create pullButton

            var pulldownButtonCeiling = new CreateElementsTab().CreatePulldownButton("Потолки Acoustic Group",
                "Потолки", panelConstraction, 
                new Uri($@"{utilsFolderPath}\images\Ceil.png", UriKind.Absolute)); 

            var pulldownButtonCladding = new CreateElementsTab().CreatePulldownButton("Облицовки Acoustic Group",
                "Облицовки", panelConstraction,
                new Uri($@"{utilsFolderPath}\images\Cladding.png", UriKind.Absolute)); 

            var pulldownButtonPartiti = new CreateElementsTab().CreatePulldownButton("Перегородки Acoustic Group",
                "Перегородки", panelConstraction,
                new Uri($@"{utilsFolderPath}\images\Partiti.png", UriKind.Absolute)); 

            PushButtonData buttonPull = null;

            string[] type = { "partition", "cladding", "floor", "ceiling", "zips" };

            foreach (var item in ListConstrAg)
            {

                if (type.Any(s => s.Contains(item.Type)))
                {
                    buttonPull = new PushButtonData($"{item.Code}", $"{item.Code} {item.Name}",
                        Path.Combine(utilsFolderPath, "AcousticConstructor.dll"), "AcoustiCTab.CreateConstrAg");//Create ButtonPull
                }

                ImageSource imageSourceConstrAg;

                try
                { 
                    imageSourceConstrAg = new BitmapImage(new Uri($@"{utilsFolderPath}\images\Img_constr\{item.Img}", UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    imageSourceConstrAg = new BitmapImage(new Uri($@"{utilsFolderPath}\images\no_image.jpg", UriKind.RelativeOrAbsolute));
                }

                buttonPull.ToolTipImage = imageSourceConstrAg;

                buttonPull.ToolTip = $"{item.Description}\n- Толщина = {item.Thickness} мм \n- Rw = {item.SoundIndex} дБ; \n- Lnw = {item.ImpactNoseIndex} дБ; \n {item.Specification}";

                switch (item.Type)
                {
                    case "ceiling":
                        pulldownButtonCeiling.AddPushButton(buttonPull); //Add pushButton in pullButton;
                        break;
                    case "zips":
                        pulldownButtonCladding.AddPushButton(buttonPull);
                        pulldownButtonCeiling.AddPushButton(buttonPull);
                        break;
                    case "partition":
                        pulldownButtonPartiti.AddPushButton(buttonPull);
                        break;
                    case "cladding":
                        pulldownButtonCladding.AddPushButton(buttonPull);
                        break;
                    case "floor":
                        pulldownButtonFloor.AddPushButton(buttonPull);
                        break;

                }
            }

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

            Log.Information("Finish Panel");

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