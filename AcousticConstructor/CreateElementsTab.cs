using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AcousticConstructor
{
    public class CreateElementsTab
    {
        public PulldownButton CreatePulldownButton(string name, string nameButton, RibbonPanel ribbonPanel, Uri uriImagepulldownButton)
        {
            var dataPulldownButton = new PulldownButtonData(name, nameButton);

            var pulldownButtonFloor = ribbonPanel.AddItem(dataPulldownButton) as PulldownButton;

            BitmapImage Image = new BitmapImage(uriImagepulldownButton);

            pulldownButtonFloor.LargeImage = Image;

            return pulldownButtonFloor;
        }

    }
}
