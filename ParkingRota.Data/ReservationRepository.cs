namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;

    public class ReservationRepository : IReservationRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public ReservationRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.Reservation> GetReservations(LocalDate firstDate, LocalDate lastDate)
        {
            var firstDbDate = DbConvert.LocalDate.ToDb(firstDate);
            var lastDbDate = DbConvert.LocalDate.ToDb(lastDate);

            return this.context.Reservations
                .Include(r => r.ApplicationUser)
                .Where(r => r.DbDate >= firstDbDate && r.DbDate <= lastDbDate)
                .ToArray()
                .Select(this.mapper.Map<Business.Model.Reservation>)
                .ToArray();
        }
    }
}