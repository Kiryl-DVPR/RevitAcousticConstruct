using Autodesk.Revit;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace AcoustiCUtils
{
    public static class CodeFilter
    {
        public static List<Element> FilteredElementList = new List<Element>();

        public static bool FindElements(List<Element> listConstr, string code)
        {
            var isElementsFound = false;

            FilteredElementList.Clear();

            foreach (Element element in listConstr) //Фильтрация элементов с префиксом "AG"
            {
                var elementName = element.Name.ToString();

                if (elementName.Contains(code))
                {
                    FilteredElementList.Add(element);

                    isElementsFound = true;

                }

            }

            return isElementsFound;

        }

        public static bool FindElements(IList<Reference> _selectedElementRefList, string code, Document _doc)
        {
            var isElementsFound = false;

            FilteredElementList.Clear();

            foreach (var element in _selectedElementRefList) //Фильтрация элементов с префиксом "AG"
            {
                Element oElement = _doc.GetElement(element) as Element;

                var elementName = oElement.Name.ToString();

                if (elementName.Contains(code))
                {
                    FilteredElementList.Add(oElement);
                    isElementsFound = true;
                }
            }

            return isElementsFound;

        }


    }

}
