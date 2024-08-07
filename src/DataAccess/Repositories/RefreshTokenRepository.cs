using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApiDbContext apiDbContext) : base(apiDbContext)
        {


        }

        public async Task<RefreshToken> GetRefreshToken(string refreshToken)
        {
            return await _entities.Where(s => s.Token == refreshToken).FirstOrDefaultAsync();
        }
    }
}
