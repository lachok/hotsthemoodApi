using AutoMapper;
using hotsthemoodApi.Contracts;
using hotsthemoodApi.ModuleExtensions;
using Nancy;

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
            _checkinRepository.Add(checkinRequest.Checkin);

            return new CheckinResponse();
        }
    }
}