using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.Dtos
{
    public class ApplicationUserDto : BaseEntityDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? Role { get; set; }
    }

    public class ApplicationRoleDto : BaseEntityDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


}
