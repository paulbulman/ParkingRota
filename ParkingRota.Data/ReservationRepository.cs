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

        public IReadOnlyList<Business.Model.Reservation> GetReservations(LocalDate firstDate, LocalDate lastDate) =>
            this.GetDataReservations(firstDate, lastDate)
                .Select(this.mapper.Map<Business.Model.Reservation>)
                .ToArray();

        public void AddReservations(IReadOnlyList<Business.Model.Reservation> reservations)
        {
            var existingReservations = this.GetOverlappingDataReservations(reservations);

            var newReservations = reservations.Where(r => !existingReservations.Any(e => ReservationsMatch(r, e)));

            this.context.Reservations.AddRange(newReservations.Select(r =>
                new Reservation { ApplicationUserId = r.ApplicationUser.Id, Date = r.Date, Order = r.Order }));

            this.context.SaveChanges();
        }

        public void RemoveReservations(IReadOnlyList<Business.Model.Reservation> reservations)
        {
            var existingReservations = this.GetOverlappingDataReservations(reservations);

            var deletedReservations = existingReservations.Where(e => reservations.Any(r => ReservationsMatch(r, e)));

            this.context.Reservations.RemoveRange(deletedReservations);

            this.context.SaveChanges();
        }

        private static bool ReservationsMatch(Business.Model.Reservation modelReservation, Reservation dataReservation) =>
            modelReservation.Date == dataReservation.Date &&
            modelReservation.ApplicationUser.Id == dataReservation.ApplicationUser.Id &&
            modelReservation.Order == dataReservation.Order;

        private IReadOnlyList<Reservation> GetOverlappingDataReservations(
            IReadOnlyList<Business.Model.Reservation> reservations)
        {
            if (!reservations.Any())
            {
                return new List<Reservation>();
            }

            var firstDate = reservations.OrderBy(r => r.Date).First().Date;
            var lastDate = reservations.OrderByDescending(r => r.Date).First().Date;

            return this.GetDataReservations(firstDate, lastDate);
        }

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