namespace PurrfectMatch.Models
{
    public class AdoptionRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CatId { get; set; }
        public bool? HasOtherAnimals { get; set; }  
        public bool? HasChildren { get; set; }  
        public bool? Housing { get; set; }
        public string Status { get; set; } = "Oczekujący";
        public string RejectionReason { get; set; } = "";
        public string AdoptionReason { get; set; }
    }
}
