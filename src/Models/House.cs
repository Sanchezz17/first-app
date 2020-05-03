namespace covidSim.Models
{
    public class House
    {
        public House(int id, Vec cornerCoordinates, bool isShop)
        {
            Id = id;
            Coordinates = new HouseCoordinates(cornerCoordinates);
            IsShop = IsShop;
        }

        public int Id;
        public HouseCoordinates Coordinates;
        public int ResidentCount = 0;
        public bool IsShop { get; set; }
    }
}