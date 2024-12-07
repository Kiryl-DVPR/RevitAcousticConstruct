using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Color = Autodesk.Revit.DB.Color;
using Document = Autodesk.Revit.DB.Document;


namespace AcoustiCTab
{
    [Transaction(TransactionMode.Manual)]
    public class CreateConstrAg : IExternalCommand
    {
        public static string Code; 
        public static string Name;
        public static int Thicness;
        public static string ButtonName = null;
        public static string TypeAgConstr = null;

        private string Version = "2022";

        private string utilsFolderPath;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;//Обращаемся к приложению Revit
            UIDocument uidoc = uiapp.ActiveUIDocument; //Оращаемся к интерфейсу Revit
            Document doc = uidoc.Document;//Обращаемся к проекту Revit
 
            utilsFolderPath = $@"C:\ProgramData\Autodesk\Revit\Addins\{Version}\AcousticConstructor";

            var ButtonName = Tab.ExecutedItemName;

            Code = Tab.ExecutedItemCode;

            Thicness = Tab.ExecutedItemThicness;

            TypeAgConstr = Tab.ExecutedItemType;

            string NameConstr = "AG_" + $"{ButtonName}" + "_" + $"{Thicness}"; // Записываем имя конструкции

            if(TypeAgConstr == "Потолки")
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create New Ceiling Type");

                    CeilingType newCeilingType = null;

                    try
                    {

                        // Находим тип стены, который будет использоваться в качестве основы
                        var baseCeilingType = new FilteredElementCollector(doc)
                            .OfClass(typeof(CeilingType))
                            .Cast<CeilingType>()
                            .ToList();


                        foreach (var ceiling in baseCeilingType)
                        {
                            if(ceiling.FamilyName == "Многослойный потолок" || ceiling.FamilyName == "Compound Ceiling")
                            {
                                newCeilingType = ceiling.Duplicate(NameConstr) as CeilingType;
                                break;
                            }

                        }
                        if (baseCeilingType == null)
                        {
                            message = "Не найден тип ";
                            return Result.Failed;
                        }
                        if (newCeilingType == null)
                        {
                            message = "Не удалось создать ";
                            return Result.Failed;
                        };

                        tx.Commit();

                    }
                    catch
                    {
                        TaskDialog.Show("Загрузка семейства", "Семейство уже создано");

                        return Result.Failed;
                    }

                    CreateParam(uiapp, uidoc);

                    FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Ceilings);
                    List<Element> ceilings = collector.ToList();

                    using (Transaction tr = new Transaction(doc, "Заполнения параметра шифра конструкции"))
                    {
                        tr.Start();

                        foreach (var ceiling in ceilings)
                        {
                            Parameter CodeParam = ceiling.LookupParameter("Шифр конструкции");

                            if (CodeParam != null && ceiling.Name == NameConstr)
                            {
                                CodeParam.Set($"{Code}");

                                break;
                            }
                        }


                        tr.Commit();

                    }
                    using (Transaction tx5 = new Transaction(doc, "Create materials"))
                    {
                        tx5.Start();
                        
                        CreateMaterial(doc, Code, new Color((byte)0, (byte)204, (byte)153));

                        tx5.Commit();
                    }


                    using (Transaction tx4 = new Transaction(doc, "Изменение слоёв конструкции"))
                    {

                        tx4.Start("Удаление слоёв");
                        CompoundStructure structure2 = newCeilingType.GetCompoundStructure();

                        var ListLayers = new List<CompoundStructureLayer>();

                        AddMaterialList(doc, Code, Thicness, ListLayers);
                        
                        structure2.SetLayers(ListLayers);

                        newCeilingType.SetCompoundStructure(structure2);

                        tx4.Commit();

                    }

                }

            }
            if (TypeAgConstr == "Облицовки" || TypeAgConstr == "Перегородки")
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create New Wall Type");

                    WallType newWallType = null;

                    try
                    {

                        // Находим тип стены, который будет использоваться в качестве основы
                        var baseWallType = new FilteredElementCollector(doc)
                            .OfClass(typeof(WallType))
                            .Cast<WallType>()
                            .ToList();

                        foreach (var wallType in baseWallType)
                        {
                            if (wallType.Kind.ToString() == "Basic")
                            {
                                newWallType = wallType.Duplicate(NameConstr) as WallType;

                                break;
                            }
                        }
                        if (baseWallType == null)
                        {
                            message = "Не найден тип базовой ";
                            return Result.Failed;
                        }
                        if (newWallType == null)
                        {
                            message = "Не удалось создать новый тип ";
                            return Result.Failed;
                        };

                        tx.Commit();

                    }
                    catch
                    {
                        TaskDialog.Show("Загрузка семейства", "Семейство уже создано");

                        return Result.Failed;
                    }

                    CreateParam(uiapp, uidoc);

                    FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls);
                    List<Element> walls = collector.ToList();

                    using (Transaction tr = new Transaction(doc, "Заполнения параметра шифра конструкции"))
                    {
                        tr.Start();

                        foreach (var wall in walls)
                        {
                            Parameter CodeParam = wall.LookupParameter("Шифр конструкции");

                            if (CodeParam != null && wall.Name == NameConstr)
                            {
                                CodeParam.Set($"{Code}");

                                break;
                            }
                        }


                        tr.Commit();

                    }
                    using (Transaction tx5 = new Transaction(doc, "Create materials"))
                    {
                        tx5.Start();
                        CreateMaterial(doc, Code, new Color((byte)102, (byte)102, (byte)204));

                        tx5.Commit();
                    }


                    using (Transaction tx4 = new Transaction(doc, "Изменение слоёв конструкции"))
                    {

                        tx4.Start("Удаление слоёв");
                        CompoundStructure structure2 = newWallType.GetCompoundStructure();

                        var ListLayers = new List<CompoundStructureLayer>();

                        AddMaterialList(doc, Code, Thicness, ListLayers);

                        structure2.SetLayers(ListLayers);

                        newWallType.SetCompoundStructure(structure2);

                        tx4.Commit();

                    }

                }
            }
            if (TypeAgConstr == "Полы")
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create New Floor Type");

                    FloorType newFloorType = null;

                    try
                    {

                        // Находим тип стены, который будет использоваться в качестве основы
                        var baseFloorType = new FilteredElementCollector(doc)
                            .OfClass(typeof(FloorType))
                            .Cast<FloorType>()
                            .ToList();

                        foreach (var floorType in baseFloorType)
                        {

                            newFloorType = floorType.Duplicate(NameConstr) as FloorType;

                            break;

                        }
                        if (baseFloorType == null)
                        {
                            message = "Не найден тип пола";
                            return Result.Failed;
                        }
                        if (newFloorType == null)
                        {
                            message = "Не удалось создать новый тип пола";
                            return Result.Failed;
                        };

                        tx.Commit();

                    }
                    catch
                    {
                        TaskDialog.Show("Загрузка семейства", "Семейство уже создано");

                        return Result.Failed;
                    }

                    CreateParam(uiapp, uidoc);

                    FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors);
                    List<Element> floors = collector.ToList();

                    using (Transaction tr = new Transaction(doc, "Заполнения параметра шифра конструкции"))
                    {
                        tr.Start();

                        foreach (var floor in floors)
                        {
                            Parameter CodeParam = floor.LookupParameter("Шифр конструкции");

                            if (CodeParam != null && floor.Name == NameConstr)
                            {
                                CodeParam.Set($"{Code}");

                                break;
                            }
                        }


                        tr.Commit();

                    }

                    using (Transaction tx5 = new Transaction(doc, "Create materials"))
                    {
                        tx5.Start();
                        CreateMaterial(doc, Code, new Color((byte)153, (byte)102, (byte)0));

                        tx5.Commit();
                    }


                    using (Transaction tx4 = new Transaction(doc, "Изменение слоёв конструкции"))
                    {

                        tx4.Start("Удаление слоёв");
                        CompoundStructure structure2 = newFloorType.GetCompoundStructure();

                        var ListLayers = new List<CompoundStructureLayer>();

                        AddMaterialList(doc, Code, Thicness, ListLayers);

                        structure2.SetLayers(ListLayers);

                        newFloorType.SetCompoundStructure(structure2);

                        tx4.Commit();

                    }

                }

                }

            TaskDialog.Show("Создание семейств AG", $"Семейство {ButtonName} создано");
            
            return Result.Succeeded;

        }
        public  void  CreateParam(UIApplication uiapp, UIDocument uidoc)
        {

            Document doc = uidoc.Document;//Обращаемся к проекту Revit

            uiapp.Application.SharedParametersFilename = $@"{utilsFolderPath}\images\AGSharedParamFile.txt"; //Путь к файлу общих параметров

            var parametersFile = uiapp.Application.OpenSharedParameterFile(); // Открываем файл общих пвараметров

            ExternalDefinition def = null; // Объявляем переменную 


            using (Transaction t23 = new Transaction(doc, "Создание файла общих параметров и создание параметров проекта перекрытия"))
            {
                t23.Start();

                foreach (var defGroup2 in parametersFile.Groups) // Перебираем группы файла общих параметров
                {
                    foreach (ExternalDefinition item2 in defGroup2.Definitions)
                    {
                        if (item2.Name == "Шифр конструкции")
                        {
                            def = item2;

                            break;
                        }
                    }
                }


                var w = Category.GetCategory(doc, BuiltInCategory.OST_Walls);
                var f = Category.GetCategory(doc, BuiltInCategory.OST_Floors);
                var c = Category.GetCategory(doc, BuiltInCategory.OST_Ceilings);


                CategorySet categorySet = new CategorySet();

                categorySet.Insert(w);
                categorySet.Insert(f);
                categorySet.Insert(c);


                var typeBinding = new TypeBinding(categorySet);

                var instanceBinding = new InstanceBinding(categorySet);

                doc.ParameterBindings.Insert(def, typeBinding);
                doc.ParameterBindings.Insert(def, instanceBinding);

                t23.Commit();

            }

        }

        public List<CompoundStructureLayer> AddMaterialList(Document doc,string Name, double width, List<CompoundStructureLayer> ListStruct)
        {
            
                ElementClassFilter filter = new ElementClassFilter(typeof(Material));

                var ListMaterials = new FilteredElementCollector(doc).WherePasses(filter).ToList();

                 ElementId idMaterial = null;

                 foreach (var element in ListMaterials)
                {
                    if (element.Name == Name)
                    {
                        idMaterial = element.Id;
                        break;
                    }

                 }


                double Width = UnitUtils.ConvertToInternalUnits(width, UnitTypeId.Millimeters);

                ListStruct.Add(new CompoundStructureLayer(Width, MaterialFunctionAssignment.Structure, idMaterial));

                return ListStruct;

        }

        public ElementId CreateMaterial(Document doc, string Name, Color color)
        {

            ElementClassFilter filter = new ElementClassFilter(typeof(Material));

            var ListMaterials = new FilteredElementCollector(doc).WherePasses(filter).ToList();

            ElementId idMaterial = null;


            foreach (var element in ListMaterials)
            {
                if (element.Name == Name)
                {
                    idMaterial = element.GetTypeId();
                    break;
                }
                {
                    idMaterial = null;
                };
            }

            if(idMaterial == null)
            {
                idMaterial = Material.Create(doc, Name);
                ((Material)doc.GetElement(idMaterial)).Color = color;
            }
            
            

            return idMaterial;

        }

    }


}

            

              
    
  
    


