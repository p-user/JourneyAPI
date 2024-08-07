
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SharedLayer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Config
{
    public sealed  class UpdateAuditableInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdateAuditableInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await UpdateAuditableEntities(eventData.Context);
            }

            try
            {
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;

            }

        }

        private async Task UpdateAuditableEntities(DbContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {

                DateTime utcNow = DateTime.UtcNow;
                var scopedServiceProvider = scope.ServiceProvider;

                var userContext = scopedServiceProvider.GetRequiredService<IUserContext>();

                string _userId = userContext.GetUserId();

                var users = context.ChangeTracker.Entries<ApplicationUser>().ToList();
                foreach (EntityEntry<ApplicationUser> entry in users)
                {
                    if (entry.State == EntityState.Added)
                    {
                        SetCurrentPropertyValue(entry, nameof(BaseEntity.CreatedDate), utcNow);
                        SetCurrentPropertyValue(entry, nameof(ApplicationUser.Status), Status.Enabled);
                        //SetCurrentPropertyValue(entry, nameof(BaseEntity.CreatedBy), _userId);
                    }


                    SetCurrentPropertyValue(entry, nameof(ApplicationUser.LastUpdatedDate), utcNow);
                }


                var entities = context.ChangeTracker.Entries<BaseEntity>().ToList();

                foreach (EntityEntry<BaseEntity> entry in entities)
                {
                    if (entry.State == EntityState.Added)
                    {
                        SetCurrentPropertyValue(entry, nameof(BaseEntity.CreatedDate), utcNow);
                        SetCurrentPropertyValue(entry, nameof(BaseEntity.CreatedBy), _userId);
                        SetCurrentPropertyValue(entry, nameof(BaseEntity.Status), Status.Enabled);
                        
                    }

                    SetCurrentPropertyValue(entry, nameof(BaseEntity.LastUpdatedDate), utcNow);
                    SetCurrentPropertyValue(entry, nameof(BaseEntity.LastUpdatedBy), _userId);

                    
                }
            }


        }
        private void SetCurrentPropertyValue<TEntity>(EntityEntry<TEntity> entry, string propertyName, object value) where TEntity : class
        {
            entry.Property(propertyName).CurrentValue = value;
        }

    }
}
