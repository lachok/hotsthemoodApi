using System.Collections.Generic;
using AutoMapper;
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
            var checkinDto = Mapper.Map<CheckinDto>(checkinRequest);
            _checkinRepository.Insert(checkinDto);

            return new CheckinResponse();
        }
    }

    public interface ICheckinRepository
    {
        void Insert(CheckinDto checkinDto);
    }

    public class FakeCheckinRepository : ICheckinRepository
    {
        private readonly List<CheckinDto> _checkins;

        public FakeCheckinRepository()
        {
            _checkins = new List<CheckinDto>();
        }

        public void Insert(CheckinDto checkinDto)
        {
            _checkins.Add(checkinDto);
        }
    }
}