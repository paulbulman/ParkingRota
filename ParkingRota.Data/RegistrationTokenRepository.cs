namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;

    public class RegistrationTokenRepository : IRegistrationTokenRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public RegistrationTokenRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.RegistrationToken> GetRegistrationTokens() =>
            this.context.RegistrationTokens
                .Select(this.mapper.Map<Business.Model.RegistrationToken>)
                .ToArray();

        public void DeleteRegistrationToken(string token)
        {
            var registrationToken = this.context.RegistrationTokens.Single(t => t.Token == token);

            this.context.RegistrationTokens.Remove(registrationToken);

            this.context.SaveChanges();
        }
    }
}