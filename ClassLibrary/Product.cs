
namespace ClassLibrary
{
    public class Product
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public long Quantity { get; set; }

        public string Units { get; set; }

        public string InfoPack { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Quantity}";
        }

    }
}
