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

        public IReadOnlyList<Business.Model.RegistrationToken> RegistrationTokens =>
            this.context.RegistrationTokens
                .Select(this.mapper.Map<Business.Model.RegistrationToken>)
                .ToArray();
    }
}