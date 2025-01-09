using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using AcoustiCTab;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Autodesk.Revit.UI;
using ClassLibrary;
using Revit.Async;
using Tab = AcoustiCTab.Tab;

namespace AcousticConstructor
{

    public partial class RevitAsyncWindow : UserControl
    {
        public RevitAsyncWindow()
        {
            CreatePanel();
            GetVersion();
            InitializeComponent();
        }

        public async void GetVersion()
        {

            var version = await Rest.GetVersion();

            ClassLibrary.GlobalData.VersionService = version.data.Version;
        }


        public async void CreatePanel()
        {
            var listAg = await RevitTask.RunAsync(async (UIApplication) =>
                {
                    var listAg = await Rest.GetInfoConstr();

                    Tab.ListConstrAg = listAg;

                    return listAg;
                }
            );


            await RevitTask.RunAsync(async (UIApplication) =>
            {

                var ListConstrAg = listAg;

                const string tabName = "ACOUSTIC®";

                var utilsFolderPathUtils =
                    $@"C:\ProgramData\Autodesk\Revit\Addins\{ClassLibrary.GlobalData.VersionRevit}\AcousticConstructor\utils";

                var utilsFolderPath =
                    $@"C:\ProgramData\Autodesk\Revit\Addins\{ClassLibrary.GlobalData.VersionRevit}\AcousticConstructor";

                var panelConstraction =
                    UIApplication.CreateRibbonPanel(tabName,
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
                            Path.Combine(utilsFolderPath, "AcousticConstructor.dll"),
                            "AcoustiCTab.CreateConstrAg"); //Create ButtonPull
                    }

                    ImageSource imageSourceConstrAg;

                    try
                    {
                        imageSourceConstrAg =
                            new BitmapImage(new Uri($@"{utilsFolderPath}\images\Img_constr\{item.Img}",
                                UriKind.RelativeOrAbsolute));
                    }
                    catch
                    {
                        imageSourceConstrAg = new BitmapImage(new Uri($@"{utilsFolderPath}\images\no_image.jpg",
                            UriKind.RelativeOrAbsolute));
                    }

                    buttonPull.ToolTipImage = imageSourceConstrAg;

                    buttonPull.ToolTip =
                        $"{item.Description}\n- Толщина = {item.Thickness} мм \n- Rw = {item.SoundIndex} дБ; \n- Lnw = {item.ImpactNoseIndex} дБ; \n {item.Specification}";

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

                return 0;
            }

            );
        }


    }
}



