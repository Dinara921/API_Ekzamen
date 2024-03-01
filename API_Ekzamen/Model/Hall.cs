namespace API_Ekzamen.Model
{
    public class Hall
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<SessionWithFilm> session { get; set; } = new List<SessionWithFilm>();
    }

    public class Place
    {
        public int place_id { get; set; }
        public int place_number { get; set; }
        public bool is_occupied { get; set; }
    }
}
