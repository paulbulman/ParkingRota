namespace ParkingRota.Business.Model
{
    public interface ISystemParameterListRepository
    {
        SystemParameterList GetSystemParameterList();

        void UpdateSystemParameterList(SystemParameterList updated);
    }
}