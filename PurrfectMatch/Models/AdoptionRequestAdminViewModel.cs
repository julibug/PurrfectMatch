using Microsoft.AspNetCore.Mvc;

namespace PurrfectMatch.Models
{
    public class AdoptionRequestAdminViewModel
    {

        public int RequestId { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }  
        public string CatName { get; set; }
        public int CatId { get; set; }
        public bool? HasOtherAnimals { get; set; }
        public bool? HasChildren { get; set; }
        public bool? Housing { get; set; }
        public string Status { get; set; }  
        public string RejectionReason { get; set; }
        public string AdoptionReason { get; set; }
    }
}
