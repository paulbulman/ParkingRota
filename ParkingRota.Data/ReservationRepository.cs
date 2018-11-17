namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business;
    using Business.Model;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;

    public class ReservationRepository : IReservationRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IDateCalculator dateCalculator;
        private readonly IMapper mapper;

        public ReservationRepository(IApplicationDbContext context, IDateCalculator dateCalculator, IMapper mapper)
        {
            this.context = context;
            this.dateCalculator = dateCalculator;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.Reservation> GetReservations(LocalDate firstDate, LocalDate lastDate) =>
            this.GetDataReservations(firstDate, lastDate)
                .Select(this.mapper.Map<Business.Model.Reservation>)
                .ToArray();

        public void UpdateReservations(IReadOnlyList<Business.Model.Reservation> reservations)
        {
            var activeDates = this.dateCalculator.GetActiveDates();

            var existingActiveReservations = this.GetDataReservations(activeDates.First(), activeDates.Last());

            var reservationsToRemove = existingActiveReservations
                .Where(e => !reservations.Any(r => ReservationsMatch(r, e)));

            var reservationsToAdd = reservations
                .Where(r => !existingActiveReservations.Any(e => ReservationsMatch(r, e)))
                .Select(r => new Reservation { ApplicationUserId = r.ApplicationUser.Id, Date = r.Date, Order = r.Order });

            this.context.Reservations.RemoveRange(reservationsToRemove);
            this.context.Reservations.AddRange(reservationsToAdd);

            this.context.SaveChanges();
        }

        private static bool ReservationsMatch(Business.Model.Reservation modelReservation, Reservation dataReservation) =>
            modelReservation.Date == dataReservation.Date &&
            modelReservation.ApplicationUser.Id == dataReservation.ApplicationUser.Id &&
            modelReservation.Order == dataReservation.Order;

        private IReadOnlyList<Reservation> GetDataReservations(LocalDate firstDate, LocalDate lastDate)
        {
            var firstDbDate = DbConvert.LocalDate.ToDb(firstDate);
            var lastDbDate = DbConvert.LocalDate.ToDb(lastDate);

            return this.context.Reservations
                .Include(r => r.ApplicationUser)
                .Where(r => r.DbDate >= firstDbDate && r.DbDate <= lastDbDate)
                .ToArray();
        }
    }
}