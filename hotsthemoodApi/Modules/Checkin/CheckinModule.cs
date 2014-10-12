using AutoMapper;
using hotsthemoodApi.ModuleExtensions;
using Nancy;
using Raven.Client;

namespace hotsthemoodApi.Modules.Checkin
{
    public class CheckinModule : NancyModule
    {
        private readonly ICheckinRepository _checkinRepository;

        public CheckinModule(ICheckinRepository checkinRepository)
        {
            _checkinRepository = checkinRepository;

            Put["/checkin/"] = _ => this.RunHandler<CheckinRequest, CheckinResponse>(CheckIn);
        }

        private CheckinResponse CheckIn(CheckinRequest checkinRequest)
        {
            var checkinDto = Mapper.Map<CheckinDto>(checkinRequest);
            _checkinRepository.Insert(checkinDto);

            return new CheckinResponse();
        }
    }

    public interface ICheckinRepository
    {
        void Insert(CheckinDto checkinDto);
    }

    public class CheckinRepository : ICheckinRepository
    {
        private readonly IDocumentSession _session;

        public CheckinRepository(IDocumentSession session)
        {
            _session = session;
        }

        public void Insert(CheckinDto checkinDto)
        {
            _session.Store(checkinDto);
        }
    }
}