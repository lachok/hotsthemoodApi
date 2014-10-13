using System;
using hotsthemoodApi.ModuleExtensions;
using hotsthemoodApi.Modules.Checkin;
using Nancy;

namespace hotsthemoodApi.Modules.DeviceCheckinHistory
{
    public class DeviceCheckinHistoryModule : NancyModule
    {
        private ICheckinRepository _checkinRepository;

        public DeviceCheckinHistoryModule(ICheckinRepository checkinRepository)
        {
            _checkinRepository = checkinRepository;

            Get["/checkinHistory/{deviceId}"] = _ => this.RunHandler<CheckinHistoryRequest, CheckinHistoryResponse>(GetCheckinsByDeviceId);
        }

        private CheckinHistoryResponse GetCheckinsByDeviceId(CheckinHistoryRequest request)
        {
            return new CheckinHistoryResponse()
            {
                Checkins = _checkinRepository.GetAllByDeviceId(request.DeviceId)
            };
        }
    }

    internal class CheckinHistoryRequest
    {
        public string DeviceId { get; set; }
    }

    internal class CheckinHistoryResponse : CheckinHistoryRequest
    {
        public Checkin.Checkin[] Checkins { get; set; }
    }
}
