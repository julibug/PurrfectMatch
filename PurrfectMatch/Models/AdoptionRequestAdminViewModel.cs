using Microsoft.AspNetCore.Mvc;

namespace PurrfectMatch.Models
{
    public class AdoptionRequestAdminViewModel
    {
        // Identyfikator wniosku adopcyjnego
        public int RequestId { get; set; }

        // Nazwa użytkownika, który złożył wniosek
        public string UserName { get; set; }

        // Identyfikator użytkownika, który złożył wniosek
        public string UserId { get; set; }  // Dodaj UserId

        // Nazwa kota, którego dotyczy wniosek
        public string CatName { get; set; }

        public int CatId { get; set; }

        // Czy użytkownik posiada inne zwierzęta
        public bool HasOtherAnimals { get; set; }

        // Czy użytkownik ma dzieci
        public bool HasChildren { get; set; }

        // Typ mieszkania (dom/mieszkanie)
        public bool Housing { get; set; }

        public string Status { get; set; }  // Dodajemy Status
        public string RejectionReason { get; set; }
        public string AdoptionReason { get; set; }
    }
}
