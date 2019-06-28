namespace ParkingRota.Business
{
    using Model;
    using NodaTime;

    public class LastServiceRunTimeUpdater
    {
        private readonly IClock clock;
        private readonly ISystemParameterListRepository systemParameterListRepository;

        public LastServiceRunTimeUpdater(IClock clock, ISystemParameterListRepository systemParameterListRepository)
        {
            this.clock = clock;
            this.systemParameterListRepository = systemParameterListRepository;
        }

        public void Update()
        {
            var systemParameterList = this.systemParameterListRepository.GetSystemParameterList();

            systemParameterList.LastServiceRunTime = this.clock.GetCurrentInstant();

            this.systemParameterListRepository.UpdateSystemParameterList(systemParameterList);
        }
    }
}