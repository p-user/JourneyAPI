using Microsoft.AspNetCore.Identity;
using SharedLayer;


namespace DataAccessLayer.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public Status Status { get; set; }
    }
}
