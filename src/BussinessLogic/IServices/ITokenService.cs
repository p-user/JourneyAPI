using DataAccessLayer.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.IServices
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user);
        Task<string> CreateJwtToken(List<Claim> claims, SigningCredentials credentials, DateTime expiration, ApplicationUser user);
        Task<string> GenerateRefreshToken(ApplicationUser user, CancellationToken cancellationToken);
        List<Claim> GetPrincipalFromExpiredToken(string token);
        Task FindAndInValidateRefreshToken(string refreshToken, CancellationToken cancellationToken);
        Task<string> ReGenerateAccessTokenFromExisting(List<Claim> claims, ApplicationUser user);
    }
}
