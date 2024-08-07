using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DataAccessLayer.Entities;
using SharedLayer;
using Microsoft.AspNetCore.Identity;

namespace DataAccessLayer.Config
{
    public class ApiDbContext : IdentityDbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies(false);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

           

            Expression<Func<BaseEntity, bool>> filterExpr = bm => (bm.Status == Status.Enabled);

            foreach (var mutableEntityType in builder.Model.GetEntityTypes())
            {
                // check if current entity type is child of BaseModel
                if (mutableEntityType.ClrType.IsAssignableTo(typeof(BaseEntity)) 
                    
                    //||
                  //  mutableEntityType.ClrType.IsAssignableTo(typeof(ApplicationUser))
                    )
                {
                    var statusProperty = mutableEntityType.ClrType.GetProperty("Status");
                    if (statusProperty != null)
                    {
                        // modify expression to handle correct child type
                        var parameter = Expression.Parameter(mutableEntityType.ClrType);
                        var body = ReplacingExpressionVisitor.Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
                        var lambdaExpression = Expression.Lambda(body, parameter);
                   
                        // set filter
                    mutableEntityType.SetQueryFilter(lambdaExpression);
                    }

                }
            }
            base.OnModelCreating(builder);
        }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public virtual DbSet<Journey> Journeys { get; set; }
        public virtual DbSet<JourneyShare> JourneyShares { get; set; }
        public virtual DbSet<DataAccessLayer.Entities.Nlog> NLogs { get; set; }
    }
}
