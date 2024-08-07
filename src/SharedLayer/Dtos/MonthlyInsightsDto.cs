using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.Dtos
{
    
    public class MonthlyInsightsDto 
    {
        public string UserId { get; set; }
        public int Monthly { get; set; } 

    }
    public class MonthlyInsightsViewDto : MonthlyInsightsDto
    {

        public double TotalDistance { get; set; }
        public int TotalNoOfRoutes { get; set; }
        public int TotalDailyRewards { get; set; }
    }
}
