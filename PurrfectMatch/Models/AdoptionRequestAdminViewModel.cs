using Microsoft.AspNetCore.Mvc;

namespace PurrfectMatch.Models
{
    public class AdoptionRequestAdminViewModel
    {
        // Identyfikator wniosku adopcyjnego
        public int RequestId { get; set; }

        // Nazwa użytkownika, który złożył wniosek
        public string UserName { get; set; }

        // Nazwa kota, którego dotyczy wniosek
        public string CatName { get; set; }

        // Czy użytkownik posiada inne zwierzęta
        public bool HasOtherAnimals { get; set; }

        // Czy użytkownik ma dzieci
        public bool HasChildren { get; set; }

        // Typ mieszkania (dom/mieszkanie)
        public bool Housing { get; set; }
    }
}
