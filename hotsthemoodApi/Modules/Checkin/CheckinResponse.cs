using Nancy;

namespace hotsthemoodApi.Modules.Checkin
{
    public class CheckinResponse
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        public CheckinResponse()
        {
            HttpStatusCode = HttpStatusCode.OK;
        }
    }
}