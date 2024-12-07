using AcousticConstructor;
using AcoustiCUtils;
using AcoustiCUtils.Library;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Threading;

using ClassLibrary;

namespace AcoustiCTab
{
    [Transaction(TransactionMode.Manual)]
 
    public class Tab : IExternalApplication
    {

        public static string _executedItemName;

        public static string _executedItemCode;

        public static string _executedItemType;

        public static int _executedItemThicness;

        public static List<ListAG> ListConstrAG;

        public static string Version = ClassLibrary.globalData.VersionRevit;

        public static string versionServise;

        public string pathVercsion = $@"C:\ProgramData\Autodesk\Revit\Addins\{Version}\AcousticConstructor\update\version.txt";
        public Result OnShutdown(UIControlledApplication application)
        {   
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
 
            ComponentManager.PreviewExecute += ComponentManager_PreviewExecute; 

            string tabName = "ACOUSTIC®";
           

            ClassLibrary.globalData.VersionService = REST.Requests.GetVersion().Result.data.Version;
            versionServise = ClassLibrary.globalData.VersionService;

            application.CreateRibbonTab(tabName); //Создание вкладки в Revit
          

            string utilsFolderPathUtils = $@"C:\ProgramData\Autodesk\Revit\Addins\{Version}\AcousticConstructor\utils";

            string utilsFolderPath = $@"C:\ProgramData\Autodesk\Revit\Addins\{Version}\AcousticConstructor";


            var panel = application.CreateRibbonPanel(tabName, "Расчёт продуктов AG"); // Создание панели во вкладке Acoustic


            //Создание кнопки для подсчёта материалов во всём проекте:
            var button1 = new PushButtonData("Вывод спецификации \nво всём существующем проекте", " Весь объём \nпроекта",
                System.IO.Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.TotalCalc"); 
            Uri uriImage = new Uri($@"{utilsFolderPath}\images\LogoRevit.png", UriKind.Absolute);
            BitmapImage LargeImage = new BitmapImage(uriImage);
            button1.LargeImage = LargeImage;

            panel.AddItem(button1);
            
            //Создание кнопки для подсчёта материалов выделенной облости:
            var button2 = new PushButtonData("Вывод спецификации \nдля выделенной области вручную", " Выделенная \nобласть",
                System.IO.Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.SelectCalc");
            Uri uriImage2 = new Uri($@"{utilsFolderPath}\images\LogoRevitSelect.png", UriKind.Absolute);
            BitmapImage LargeImage2 = new BitmapImage(uriImage2);
            button2.LargeImage = LargeImage2;
            panel.AddItem(button2);

            var panelConstraction = application.CreateRibbonPanel(tabName, "Звукоизоляционные конструкции"); // Создаём panel для Construction AG

            ListConstrAG = REST.Requests.GetInfoConstr().Result; // Получаем весь список конструкций 

            var pulldownButtonFloor = new CreateElementsTab().CreatePulldownButton("Полы Acoustic Group", "Полы", panelConstraction, 
                new Uri($@"{utilsFolderPath}\images\Floor.png", UriKind.Absolute)); //Создаём кнопку-список для полов

            var pulldownButtonCeiling = new CreateElementsTab().CreatePulldownButton("Потолки Acoustic Group", "Потолки", panelConstraction, 
                new Uri($@"{utilsFolderPath}\images\Ceil.png", UriKind.Absolute)); //Создаём кнопку-список для потолков

            var pulldownButtonCladding = new CreateElementsTab().CreatePulldownButton("Облицовки Acoustic Group", "Облицовки", panelConstraction,
                new Uri($@"{utilsFolderPath}\images\Cladding.png", UriKind.Absolute)); //Создаём кнопку-список для потолков

            var pulldownButtonPartiti = new CreateElementsTab().CreatePulldownButton("Перегородки Acoustic Group", "Перегородки", panelConstraction,
                new Uri($@"{utilsFolderPath}\images\Partiti.png", UriKind.Absolute)); //Создаём кнопку-список для потолков

            var panelWebSait = application.CreateRibbonPanel(tabName, "Сайт"); // Создаём panel для Construction AG

            var buttonWebSait = new PushButtonData("Сайт Acoustic Group", "Перейти на сайт",
                    Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.OpenWebSite");
            ImageSource ImageSourceWebAG = new BitmapImage(new Uri($@"{utilsFolderPath}\images\web16.png", UriKind.RelativeOrAbsolute));
            buttonWebSait.Image = ImageSourceWebAG;
            buttonWebSait.ToolTip = "Перейти на сайт Acoustic Group";

            var buttonInfoPlag = new PushButtonData("Информация о плагине", "Информация",
                    Path.Combine(utilsFolderPathUtils, "AcoustiCUtils.dll"), "AcoustiCUtils.InfoPlagin");
            ImageSource ImageSourceInfoPlag = new BitmapImage(new Uri($@"{utilsFolderPath}\images\info16.png", UriKind.RelativeOrAbsolute));
            buttonInfoPlag.Image = ImageSourceInfoPlag;

            List<Autodesk.Revit.UI.RibbonItem> projectButtons = new List<Autodesk.Revit.UI.RibbonItem>();

            projectButtons.AddRange(panelWebSait.AddStackedItems(buttonWebSait, buttonInfoPlag));

            PushButtonData buttonPull = null;

            foreach (var item in ListConstrAG)
            {    
                
                    switch(item.Type)
                    {
                        case "partition":
                            buttonPull = new PushButtonData($"{item.Code}", $"{item.Code} {item.Name}",
                    Path.Combine(utilsFolderPath, "AcousticConstructor.dll"), "AcoustiCTab.CreateConstrAg"); //Создание кнопки списка конструкций
                            break;
                        case "cladding":
                            buttonPull = new PushButtonData($"{item.Code}", $"{item.Code} {item.Name}",
                    Path.Combine(utilsFolderPath, "AcousticConstructor.dll"), "AcoustiCTab.CreateConstrAg"); //Создание кнопки списка конструкций
                            break;
                        case "floor":
                            buttonPull = new PushButtonData($"{item.Code}", $"{item.Code} {item.Name}",
                    Path.Combine(utilsFolderPath, "AcousticConstructor.dll"), "AcoustiCTab.CreateConstrAg"); //Создание кнопки списка конструкций
                            break;
                        case "ceiling":
                            buttonPull = new PushButtonData($"{item.Code}", $"{item.Code} {item.Name}",
                    Path.Combine(utilsFolderPath, "AcousticConstructor.dll"), "AcoustiCTab.CreateConstrAg"); //Создание кнопки списка конструкций
                            break;
                        case "zips":
                            buttonPull = new PushButtonData($"{item.Code}", $"{item.Code} {item.Name}",
                    Path.Combine(utilsFolderPath, "AcousticConstructor.dll"), "AcoustiCTab.CreateConstrAg"); //Создание кнопки списка конструкций
                            break;

                    }

                    ImageSource imageSourceConstrAG;

                    try
                    {
                        imageSourceConstrAG = new BitmapImage(new Uri($@"{utilsFolderPath}\images\Img_constr\{item.Img}", UriKind.RelativeOrAbsolute));
                    }
                    catch
                    {
                        imageSourceConstrAG = new BitmapImage(new Uri($@"{utilsFolderPath}\images\no_image.jpg", UriKind.RelativeOrAbsolute));
                    }

                    buttonPull.ToolTipImage = imageSourceConstrAG;

                    buttonPull.ToolTip = $"{item.Description}\n- Толщина = {item.Thickness} мм \n- Rw = {item.SoundIndex} дБ; \n- Lnw = {item.ImpactNoseIndex} дБ; \n {item.Specification}";

                    if (item.Type == "ceiling") pulldownButtonCeiling.AddPushButton(buttonPull); //Add кнопки для кнопки-списка полов;
                    if (item.Type == "zips")
                    {
                        pulldownButtonCladding.AddPushButton(buttonPull);
                        pulldownButtonCeiling.AddPushButton(buttonPull);
                    }
                    if (item.Type == "partition") pulldownButtonPartiti.AddPushButton(buttonPull);
                    if (item.Type == "cladding") pulldownButtonCladding.AddPushButton(buttonPull);
                    if (item.Type == "floor") pulldownButtonFloor.AddPushButton(buttonPull);

            }

            return Result.Succeeded;
        }

        private void ComponentManager_PreviewExecute(object sender, EventArgs e)
        {
            Autodesk.Windows.RibbonItem a = sender as Autodesk.Windows.RibbonItem;

            if (a.Text.Contains("AG"))
            {
                _executedItemName = a.Text;

                int index = _executedItemName.IndexOf(" ");

                _executedItemCode = _executedItemName.Substring(0, index);

                if (a.Id.Contains("Облицовки Acoustic Group"))
                {
                    _executedItemType = "Облицовки";
                }
                if (a.Id.Contains("Потолки Acoustic Group"))
                {
                    _executedItemType = "Потолки";

                }
                if (a.Id.Contains("Полы Acoustic Group"))
                {
                    _executedItemType = "Полы";

                }
                if (a.Id.Contains("Перегородки Acoustic Group"))
                {
                    _executedItemType = "Перегородки";

                }

                foreach (var item in ListConstrAG)
                {
                    if(item.Code == _executedItemCode)
                    {
                        _executedItemThicness = item.Thickness;
                    }
                }

            }
                
        }

    }
}