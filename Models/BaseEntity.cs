namespace Online_Restaurant.Models
{
    public class BaseEntity
    {   // Status flag
        public bool IsActive { get; set; } = true;

        // Auditing and Reporting properties
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; } //Can't be null except in some special cases [Guests Actions]
                                               //Could be used for reviews
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

   

   
