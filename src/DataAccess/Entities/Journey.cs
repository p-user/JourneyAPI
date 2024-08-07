using SharedLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Journey : BaseEntity
    {
        public string StartLocation { get; set; }
        public DateTime StartTime { get; set; }
        public string ArrivalLocation { get; set; }
        public DateTime ArrivalTime { get; set; }
        public TransportationType TransportationType { get; set; }
        [Range(0, double.MaxValue)]
        public double RouteDistance { get; set; } //assuming the distance unit is the same
        public bool DailyGoalAchieved { get; set; }
       
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string? SocialMediaLink { get; set; }
        public virtual ICollection<JourneyShare>? SharedWith { get; set; }
    }
}
