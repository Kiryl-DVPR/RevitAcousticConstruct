using System.Collections.Generic;

namespace ClassLibrary
{
    public class Constr
    {
        public string Code { get; set; }
        public int LenX { get; set; }
        public int LenY { get; set; }
        public int LenZ { get; set; }
        public bool Dframe { get; set; }
        public int Area { get; set; }
        public int Perimeter { get; set; }
        public int Step { get; set; } = 600;
        public List<Opening> Openings { get; set; }
    }

    public class Opening
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public int Area { get; set; }
        public string Type { get; set; }

    }
}