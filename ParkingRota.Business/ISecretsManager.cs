namespace ParkingRota.Business
{
    using System.Threading.Tasks;

    public interface ISecretsManager
    {
        Task<string> Fetch(string key);
    }
}