using SharedLayer.Dtos;
using SharedLayer;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Entities;

namespace DataAccessLayer.QueryFilters
{
    public static class QueryExtension
    {
        public static IQueryable<T> ApplyFilterJourney<T>(this IQueryable<T> query, JourneyFilterDto filter) where T : class
        {
            var predicate = PredicateBuilder.New<T>(x => true); 

            if (!string.IsNullOrEmpty(filter.StartLocation))
            {
                predicate = predicate.And(x => EF.Property<string>(x, nameof(Journey.StartLocation)) == filter.StartLocation);
            }

            if (filter.StartTime != DateTime.MinValue)
            {
                predicate = predicate.And(x => EF.Property<DateTime>(x, nameof(Journey.StartTime)) == filter.StartTime);
            }

            if (!string.IsNullOrEmpty(filter.ArrivalLocation))
            {
                predicate = predicate.And(x => EF.Property<string>(x, nameof(Journey.ArrivalLocation)) == filter.ArrivalLocation);
            }

            if (filter.ArrivalTime != DateTime.MinValue)
            {
                predicate = predicate.And(x => EF.Property<DateTime>(x, nameof(Journey.ArrivalTime)) == filter.ArrivalTime);
            }

            if (filter.TransportationType != TransportationType.Bus)
            {
                predicate = predicate.And(x => EF.Property<TransportationType>(x, nameof(Journey.TransportationType)) == filter.TransportationType);
            }

            if (filter.RouteDistance.HasValue)
            {
                predicate = predicate.And(x => EF.Property<double?>(x, nameof(Journey.RouteDistance)) == filter.RouteDistance);
            }

            if (filter.DailyGoalAchieved.HasValue)
            {
                predicate = predicate.And(x => EF.Property<bool?>(x, nameof(Journey.DailyGoalAchieved)) == filter.DailyGoalAchieved);
            }

            if (!string.IsNullOrEmpty(filter.UserId))
            {
                predicate = predicate.And(x => EF.Property<string>(x, nameof(Journey.UserId)) == filter.UserId);
            }

            return query.Where(predicate);
        }
    }

}
