namespace ParkingRota.Data
{
    using System.Linq;
    using AutoMapper;
    using Business.Model;

    public class SystemParameterListRepository : ISystemParameterListRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public SystemParameterListRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public Business.Model.SystemParameterList GetSystemParameterList() => 
            this.mapper.Map<Business.Model.SystemParameterList>(this.context.SystemParameterLists.Single());

        public void UpdateSystemParameterList(Business.Model.SystemParameterList updated)
        {
            var existing = this.context.SystemParameterLists.Single();

            existing.TotalSpaces = updated.TotalSpaces;
            existing.ReservableSpaces = updated.ReservableSpaces;
            existing.NearbyDistance = updated.NearbyDistance;
            existing.FromEmailAddress = updated.FromEmailAddress;
            existing.LastServiceRunTime = updated.LastServiceRunTime;

            context.SaveChanges();
        }
    }
}