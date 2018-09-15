namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Business.Model;

    public class RegistrationTokenRepository : IRegistrationTokenRepository
    {
        private readonly ApplicationDbContext context;

        public RegistrationTokenRepository(ApplicationDbContext context) => this.context = context;

        public IReadOnlyList<Business.Model.RegistrationToken> RegistrationTokens =>
            this.context.RegistrationTokens
                .Select(t => t.ToModel())
                .ToArray();
    }
}