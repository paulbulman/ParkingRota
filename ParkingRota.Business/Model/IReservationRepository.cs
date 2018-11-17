namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;
    using NodaTime;

    public interface IReservationRepository
    {
        IReadOnlyList<Reservation> GetReservations(LocalDate firstDate, LocalDate lastDate);

        void AddReservations(IReadOnlyList<Reservation> reservations);

        void RemoveReservations(IReadOnlyList<Reservation> reservations);
    }
}