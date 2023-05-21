using DroniadaTelemetryUWP;
using Grpc.Core;
using System.Threading.Tasks;

namespace Droniada_telemetry_UWP
{

    public class TelemetryServiceClass : TelemetryService.TelemetryServiceBase
    {
        public override Task<Telemetry> GetTelemetry(DroniadaTelemetryUWP.Empty request, ServerCallContext context)
        {
            // Fetch the telemetry data using DJI Windows SDK.
            // The actual implementation will depend on how the DJI SDK works.
            var telemetry = new Telemetry();

            // TODO: fetch data using DJI SDK and assign to telemetry
            // For example:
            telemetry.Lat = 23f;
            telemetry.Long = 56f;
            telemetry.Altitude = 2f;
            telemetry.Heading = 359f;

            return Task.FromResult(telemetry);
        }
    }
}