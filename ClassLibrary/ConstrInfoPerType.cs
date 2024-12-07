using AcoustiCUtils.Library;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace AcoustiCUtils
{
    public static class ConstrInfoPerType
    {
        public static List<Constr> elementInfo = new List<Constr>(); // Создание списка для помещения информации об элементах (Стены, потолки, полы...)

        public static List<Construction> constructionInfo = new List<Construction>();

        public static void GetInfo(List<Element> _CodeListFilter, Document doc)
        {
            int index = 1; //Счётчик для id элемента
            int indexConstr = 1;

            elementInfo.Clear();
            constructionInfo.Clear();

            foreach (Element element in _CodeListFilter)
            {
                if (element is Wall wallAg)
                {
                    var wallCode = wallAg.WallType.LookupParameter("Шифр конструкции").AsString().ToString();

                    var lenX_ = wallAg.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                    var lenZ_ = wallAg.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                    var area_ = wallAg.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);

                    var lenX = ((int)UnitUtils.ConvertFromInternalUnits(lenX_.AsDouble(), UnitTypeId.Millimeters));
                    var lenZ = ((int)UnitUtils.ConvertFromInternalUnits(lenZ_.AsDouble(), UnitTypeId.Millimeters));
                    var area = ((int)UnitUtils.ConvertFromInternalUnits(area_.AsDouble(), UnitTypeId.SquareMillimeters));
                    var areaConstruction = Math.Round((float)UnitUtils.ConvertFromInternalUnits(area_.AsDouble(), UnitTypeId.SquareMeters), 2); //Для таблицы конструкций

                    var openings = GetOpenings(wallAg, doc); // Получаем лист с проёмами

                    elementInfo.Add(new Constr() { Code = wallCode, LenX = lenX, LenZ = lenZ, Openings = openings });

                    var ConstrRef = constructionInfo.Find((el) => el.Code == wallCode);

                    if (ConstrRef != null)
                    {
                        ConstrRef.Quantity += areaConstruction;
                    }
                    else
                    {
                        constructionInfo.Add(new Construction()
                        {
                            Id = indexConstr++,
                            Name = element.Name,
                            Code = wallCode,
                            Units = "м.кв.",
                            Quantity = areaConstruction,
                            Weight = "- кг/ед.",
                        });
                    }

                    index++;

                }
                else if (element is Ceiling ceilingElement)
                {
                    var idCeiling = ceilingElement.GetTypeId();
                    var ceilingType = doc.GetElement(idCeiling);

                    var ceilingCode = ceilingType.LookupParameter("Шифр конструкции").AsString().ToString();

                    var area_ = ceilingElement.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);//Достаём свойства объекта (Площадь)
                    var area = ((int)UnitUtils.ConvertFromInternalUnits(area_.AsDouble(), UnitTypeId.SquareMillimeters));//Конвертация в "мм2"

                    var openings = GetOpenings(ceilingElement, doc); // Получаем лист с проёмами

                    var perimeter_ = ceilingElement.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED);//Достаём свойства объекта (Периметр)
                    var perimeter = ((int)UnitUtils.ConvertFromInternalUnits(perimeter_.AsDouble(), UnitTypeId.Millimeters));//Конвертация в "мм"

                    var areaConstruction = Math.Round((double)UnitUtils.ConvertFromInternalUnits(area_.AsDouble(), UnitTypeId.SquareMeters), 2);//Для таблицы конструкций

                    elementInfo.Add(new Constr() { Code = ceilingCode, Area = area, Perimeter = perimeter, Openings = openings });//Добавляем свойства в список свойств


                    var ConstrRef = constructionInfo.Find((el) => el.Code == ceilingCode);

                    if (ConstrRef != null)
                    {
                        ConstrRef.Quantity += areaConstruction;
                    }
                    else
                    {
                        constructionInfo.Add(new Construction()
                        {
                            Id = indexConstr++,
                            Name = element.Name,
                            Code = ceilingCode,
                            Units = "м.кв.",
                            Quantity = areaConstruction,
                            Weight = "- кг/ед.",
                        });
                    }

                    index++;

                }
                else if (element is Floor floorAg)
                {
                    var floorCode = floorAg.FloorType.LookupParameter("Шифр конструкции").AsString().ToString();

                    var area_ = floorAg.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                    var perimeter_ = floorAg.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED);

                    var area = ((int)UnitUtils.ConvertFromInternalUnits(area_.AsDouble(), UnitTypeId.SquareMillimeters));
                    var perimeter = ((int)UnitUtils.ConvertFromInternalUnits(perimeter_.AsDouble(), UnitTypeId.Millimeters));

                    var areaConstruction = Math.Round((float)UnitUtils.ConvertFromInternalUnits(area_.AsDouble(), UnitTypeId.SquareMeters), 2); //Для таблицы конструкций

                    var openings = GetOpenings(floorAg, doc); // Получаем лист с проёмами

                    elementInfo.Add(new Constr() { Code = floorCode, Area = area, Perimeter = perimeter, Openings = openings });

                    var ConstrRef = constructionInfo.Find((el) => el.Code == floorCode);

                    if (ConstrRef != null)
                    {
                        ConstrRef.Quantity += areaConstruction;
                    }
                    else
                    {
                        constructionInfo.Add(new Construction()
                        {
                            Id = indexConstr++,
                            Name = element.Name,
                            Code = floorCode,
                            Units = "м.кв.",
                            Quantity = areaConstruction,
                            Weight = "- кг/ед.",
                        });
                    }

                    index++;
                }
            }

        }

        private static List<Opening> GetOpenings(Element element, Document doc)
        {
            IList<ElementId> IdOpeningListId = new List<ElementId>();

            var openings = new List<Opening>();

            if (element is Wall)
            {
                IdOpeningListId = ((Wall)element).FindInserts(true, false, false, false);

            }
            if (element is Ceiling)
            {

                openings.Add(new Opening()
                {
                    Length = 0,
                    Width = 0,
                    Area = 0,
                });
            }
            if (element is Floor)
            {
                openings.Add(new Opening()
                {
                    Length = 0,
                    Width = 0,
                    Area = 0,
                });

                return openings;
            }

            foreach (var id in IdOpeningListId)
            {
                var opening = doc.GetElement(id);

                int? LengthHead;
                int? LengthBotom;
                int? area;
                int width;
                int length;

                var TypeName = ((BuiltInCategory)opening.Category.Id.IntegerValue).ToString();


                if (opening.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM) != null || opening.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED) != null)
                {
                    LengthHead = ((int)UnitUtils.ConvertFromInternalUnits(opening.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsDouble(), UnitTypeId.Millimeters));
                    LengthBotom = ((int)UnitUtils.ConvertFromInternalUnits(opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble(), UnitTypeId.Millimeters));
                    area = ((int)UnitUtils.ConvertFromInternalUnits(opening.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble(), UnitTypeId.SquareMillimeters));
                    length = (int)(LengthHead - LengthBotom);
                    width = (int)(area / length);

                }
                else
                {
                    var a = ((int)UnitUtils.ConvertFromInternalUnits(opening.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble(), UnitTypeId.Millimeters));
                    area = a * a; width = a; length = (int)(a);
                }

                openings.Add
                (
                    new Opening()
                    {
                        Length = length,
                        Width = width,
                        Area = (int)area,
                        Type = TypeName
                    }
                );
            }
            return openings;
        }

    }
}