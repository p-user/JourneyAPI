using AutoMapper;
using BusinessLayer.IServices;
using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLayer.Nlog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class TokenService : ITokenService
    {
        private const int ExpirationMinutes = 30;
        private readonly ILoggerService _logger;
        private readonly JwtConfig _jwtConfig;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(ILoggerService loggerService, IOptions<JwtConfig> jwtConfig, IRefreshTokenRepository refreshTokenRepository, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _logger = loggerService;
            _jwtConfig = jwtConfig.Value;
            _refreshTokenRepo = refreshTokenRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<string> CreateToken(ApplicationUser user) //change it TODO
        {
            var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
            var token = await CreateJwtToken(await CreateClaims(user), CreateSigningCredentials(), expiration, user);
            return token;
        }

        public async Task<string> CreateJwtToken(List<Claim> claims, SigningCredentials credentials, DateTime expiration, ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return accessToken;
        }

        private async Task<List<Claim>> CreateClaims(ApplicationUser user)
        {
            
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud, _jwtConfig.ValidAudience),
                    new Claim(JwtRegisteredClaimNames.Iss, _jwtConfig.ValidIssuer),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                };
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                return claims;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var symmetricSecurityKey = _jwtConfig.Secret;

            return new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKey)), SecurityAlgorithms.HmacSha256);
        }

        public async Task<string> GenerateRefreshToken(ApplicationUser user, CancellationToken cancellationToken)
        {
            var refreshToken = Guid.NewGuid().ToString();
            await SaveRefreshToken(user, refreshToken, cancellationToken);
            return refreshToken;
        }

        public List<Claim> GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = MapTokenValidationParameters();

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal.Claims.ToList();
        }


        private async Task SaveRefreshToken(ApplicationUser user, string token, CancellationToken cancellationToken)
        {
            var refreshToken = new RefreshToken()
            {
                Token = token,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddMinutes(2),

            };
            await _refreshTokenRepo.AddAsync(refreshToken, cancellationToken);
            await _refreshTokenRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task FindAndInValidateRefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            var tokenEntity = await _refreshTokenRepo.GetRefreshToken(refreshToken);
            if (tokenEntity is null)
            {
                throw new InvalidOperationException("Refresh token does not exist!");
            }
            if (tokenEntity.ExpiryDate < DateTime.UtcNow)
            {
                _refreshTokenRepo.Delete(tokenEntity);
                await _refreshTokenRepo.SaveChangesAsync(cancellationToken);


            }
            if (tokenEntity.ExpiryDate >= DateTime.UtcNow)
            {
                throw new Exception("Token is still valid");
            }
        }

        public async Task<string> ReGenerateAccessTokenFromExisting(List<Claim> claims, ApplicationUser user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
            var token = await CreateJwtToken(claims, CreateSigningCredentials(), expiration, user);
            return token;
        }

        private TokenValidationParameters MapTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtConfig.ValidIssuer,
                ValidAudience = _jwtConfig.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret))

            };
        }
    }
}
