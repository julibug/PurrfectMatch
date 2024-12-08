namespace PurrfectMatch.Models
{
    public class AdoptionRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CatId { get; set; }
        public bool HasOtherAnimals { get; set; }  
        public bool HasChildren { get; set; }  
        public bool Housing { get; set; }
    }
}
