namespace ParkingRota.Data
{
    using AutoMapper;

    public static class MapperBuilder
    {
        public static Mapper Build()
        {
            var mapperConfiguration = new MapperConfiguration(
                c =>
                {
                    c.CreateMap<Allocation, Business.Model.Allocation>();
                    c.CreateMap<BankHoliday, Business.Model.BankHoliday>();
                    c.CreateMap<EmailQueueItem, Business.Model.EmailQueueItem>();
                    c.CreateMap<RegistrationToken, Business.Model.RegistrationToken>();
                    c.CreateMap<Request, Business.Model.Request>();
                    c.CreateMap<Reservation, Business.Model.Reservation>();
                    c.CreateMap<ScheduledTask, Business.Model.ScheduledTask>();
                    c.CreateMap<SystemParameterList, Business.Model.SystemParameterList>();
                });

            return new Mapper(mapperConfiguration);
        }
    }
}