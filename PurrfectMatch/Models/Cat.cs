namespace PurrfectMatch.Models
{
    public class Cat
    {
        public int Id { get; set; } // Identyfikator kota
        public string Name { get; set; } // Imię kota
        public int Age { get; set; } // Wiek kota
        public string Gender { get; set; } // Płeć kota
        public string Description { get; set; } // Opis kota 
        public string ImageUrl { get; set; } // Ścieżka do zdjęcia kota
        public bool IsReserved { get; set; } // Czy kot jest zarezerwowany do adopcji
        public string Diseases { get; set; } // Choroby kota 
    }
}
