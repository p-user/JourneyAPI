using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class JourneyShare : BaseEntity
    {
        public Guid JourneyId { get; set; }
        [ForeignKey("JourneyId")]
        public virtual Journey Journey { get; set; }


        public string SharedWithUserId { get; set; }
        [ForeignKey("SharedWithUserId")]
        public virtual ApplicationUser SharedWithUser { get; set; }

        public bool IsFavorite { get; set; }
    }
}
