using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DJI.WindowsSDK;
using System.Threading.Tasks;
using Grpc.Core;
using mavic;


//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace Droniada_telemetry_UWP
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    /// 

    public class TelemetryServiceClass : TelemetryService.TelemetryServiceBase
    {
        public override Task<Telemetry> GetTelemetry(Empty request, ServerCallContext context)
        {
            // Fetch the telemetry data using DJI Windows SDK.
            // The actual implementation will depend on how the DJI SDK works.
            var telemetry = new Telemetry();

            Task<ResultValue<LocationCoordinate2D?>> task1 = DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync();
            Task<ResultValue<DoubleMsg?>> task2 = DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync();

            task1.Wait();
            task2.Wait();

            var location = task1.Result;
            var altitude = task2.Result;

            if (location.error == SDKError.NO_ERROR && altitude.error == SDKError.NO_ERROR)
            {
                telemetry.Lat = (float)location.value.Value.latitude;
                telemetry.Long = (float)location.value.Value.longitude;
                telemetry.Altitude = (float)altitude.value.Value.value;
                telemetry.Heading = float.NaN;
            }
            else
            {
                telemetry.Lat = telemetry.Long = telemetry.Lat = telemetry.Altitude = float.NaN;
            }

            return Task.FromResult(telemetry);
        }
    }


    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            DJISDKManager.Instance.RegisterApp("0a5c06c83135687009ecde5e");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

        }

        private async void Timer_Tick(object sender, object e)
        {
            var locationResult = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync();
            var alt = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync();
            if (locationResult.error == SDKError.NO_ERROR && alt.error == SDKError.NO_ERROR)
            {
                var location = locationResult.value.Value;
                LocationLabel.Text += $"Latitude: {location.latitude}, Longitude: {location.longitude}, ALt: {alt.value.Value.value}\n";
            }
            else
            {
                LocationLabel.Text += "Error retrieving location: " + locationResult.error+"\n\n";
            }
            MyScrollViewer.ChangeView(null, MyScrollViewer.ScrollableHeight, null);
        }


        public void setStatus(String s)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                statusText.Text = s + '\n';
            });
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
               setStatus("Register app successfully.");
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    timer.Start();
                });
               

                //The product connection state will be updated when it changes here.
                DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (value != null && value?.value != ProductType.UNRECOGNIZED)
                        {
                           setStatus("The Aircraft is connected now.");
                            //You can load/display your pages according to the aircraft connection state here.
                        }
                        else
                        {
                           setStatus("The Aircraft is disconnected now.");
                            //You can hide your pages according to the aircraft connection state here, or show the connection tips to the users.
                        }
                    });
                };

                /*//If you want to get the latest product connection state manually, you can use the following code
                var productType = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value;
                if (productType != null && productType?.value != ProductType.UNRECOGNIZED)
                {
                   setStatus("The Aircraft is connected now.");
                    //You can load/display your pages according to the aircraft connection state here.
                }*/
            }
            else
            {
               setStatus("Register SDK failed, the error is: " + resultCode.ToString());      
            }
        }

    }
}
