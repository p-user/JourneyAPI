using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SharedLayer.Dtos
{
    public class JourneyDto
    {
        [Required]
        public string StartLocation { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public string ArrivalLocation { get; set; }
        [Required]
        public DateTime ArrivalTime { get; set; }
        [Required]
        public TransportationType TransportationType { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double RouteDistance { get; set; } //assuming the distance unit is the same
        public bool DailyGoalAchieved
        {
            get
            {
                return RouteDistance > GlobalConstants.DailyRewardBase;
            }
        }
       

     }

    public class ViewJourneyDto : JourneyDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public Status Status { get; set; }
        public string? SocialMediaLink { get; set; }
        public List<SharedJourneyDto>? SharedWith { get; set; }

    }

    public class EditJourneyDto : JourneyDto
    {
        public Guid Id { get; set; }
    }

    public class SharedJourneyDto
    {
        public Guid Id { get; set; }
        public Guid JourneyId { get; set; }
        public string SharedWithUserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public Status Status { get; set; }
        public bool IsFavorite { get; set; } = false;

    }

    public class ShareJourneyRequestDto
    {
        [Required]
        public Guid JourneyId { get; set; }
        [Required]
        public string userId { get; set; }
    }

    public class JourneyFilterDto
    {
       
        public string? StartLocation { get; set; }

        public DateTime StartTime { get; set; } = DateTime.MinValue;
       
        public string? ArrivalLocation { get; set; }
        
        public DateTime ArrivalTime { get; set; } = DateTime.MinValue;
        
        public TransportationType TransportationType { get; set; } 
       
        public double? RouteDistance { get; set; }

        public bool? DailyGoalAchieved { get; set; } 
        public string? UserId { get; set; }

    }
}
