using Microsoft.AspNetCore.Identity;
using SharedLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public Status Status { get; set; }
        public virtual ICollection<Journey>? Journeys { get; set; }
        public virtual ICollection<JourneyShare>? SharedJourneys { get; set; }
    }
}
