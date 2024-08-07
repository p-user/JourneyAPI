using Microsoft.AspNetCore.Http;
using SharedLayer;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Config
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return GlobalConstants.GuestUser;

            }
            else
            {
               
                var userId = user.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier)?.Value;
                return userId;
                
            }
        }
    }
}
