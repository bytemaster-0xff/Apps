using KitchenKiosk.Services;
using KitchenKiosk.Views;
using LagoVista.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.UWP.Services;
using LagoVista.Core.UWP.ViewModels.Common;
using LagoVista.Core.ViewModels;
using LagoVista.ISY.UI.UWP.Services;
using LagoVista.ISY994i.Core.Models;
using LagoVista.ISY994i.Core.Services;
using LagoVista.ISY994i.Core.ViewModels;
using LagoVista.UWP.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KitchenKiosk
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        DateTime _startTime;

        RESTClient _restClient;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            SLWIOC.Register<IViewModelNavigation>(LagoVista.UWP.UI.Navigation.Instance);
        }

        private void RegisterUWPServices(CoreDispatcher dispatcher)
        {
            SLWIOC.RegisterSingleton<IDispatcherServices>(new DispatcherServices(dispatcher));
            SLWIOC.RegisterSingleton<IStorageService>(new StorageService());
            SLWIOC.RegisterSingleton<IPopupServices>(new PopupsService());

            SLWIOC.RegisterSingleton<IDeviceManager>(new DeviceManager());

            SLWIOC.RegisterSingleton<INetworkService>(new NetworkService());
            SLWIOC.Register<IImaging>(new Imaging());
            SLWIOC.Register<IBindingHelper>(new BindingHelper());

            SLWIOC.RegisterSingleton<ISSDPClient>(new SSDPClient());
            SLWIOC.RegisterSingleton<IWebServer>(new WebServer());

            SLWIOC.Register<ISSDPClient>(typeof(SSDPClient));
            SLWIOC.Register<IWebServer>(typeof(WebServer));
            SLWIOC.Register<ISSDPServer>(new SSDPServer());

            SLWIOC.Register<ITimerFactory>(new TimerFactory());
        }


        //040f4f8a7a514d6d9f72b139cc277fedb7fd18fa

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            _startTime = DateTime.Now;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Navigation.Instance.Initialize(rootFrame);
                Window.Current.Content = rootFrame;

                RegisterUWPServices(Window.Current.Dispatcher);

                var mobileCenterAnalytics = new LagoVista.Core.UWP.Loggers.MobileCenterLogger("9b075936-0855-40ff-b332-86c57fffa6ae");
                SLWIOC.RegisterSingleton<ILogger>(mobileCenterAnalytics);

//                SLWIOC.RegisterSingleton<ILogger>(new LagoVista.IoT.Logging.DebugLogger());

                await SmartThingsHubs.Instance.InitAsync();

                UnhandledException += App_UnhandledException;

                Navigation.Instance.Add<FolderViewModel, FolderView>();
                Navigation.Instance.Add<DeviceDiscoveryViewModel, DeviceDiscoveryView>();

                _restClient = new RESTClient();
                _restClient.ShowDiagnostics = true;


                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT")
                {
                    var config = new LagoVista.Core.Networking.Models.UPNPConfiguration()
                    {
                        DefaultPageHtml = "<html>HelloWorld</html>",
                        DeviceType = "X_LagoVista_ISY_Kiosk_Device",
                        FriendlyName = "ISY Remote Kiosk",
                        Manufacture = "Software Logistics, LLC",
                        ManufactureUrl = "www.TheWolfBytes.com",
                        ModelDescription = "ISY Remote UI and SmartThings Bridge",
                        ModelName = "ISYRemoteKiosk",
                        ModelNumber = "SL001",
                        ModelUrl = "www.TheWolfBytes.com",
                        SerialNumber = "KSK001"
                    };

                    try
                    {

                        await _restClient.MakeDiscoverableAsync(9301, config);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                ISYService.Instance.Connected += Instance_Connected;
                ISYService.Instance.Disconnected += Instance_Disconnected;
                ISYEventListener.Instance.ISYEventReceived += Instance_ISYEventReceived;

                await Managers.LedController.Instance.InitAsync(0x40, mobileCenterAnalytics);
                await Managers.ScreenManager.Instance.InitAsync(mobileCenterAnalytics);

                await ISYService.Instance.InitAsync();
                await ISYService.Instance.RefreshAsync();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window

            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }


        private async void Instance_ISYEventReceived(object sender, ISYEvent evt)
        {
            if ((DateTime.Now - _startTime).TotalSeconds > 30)
            {
                var json = JsonConvert.SerializeObject(evt);
                await SmartThingsHubs.Instance.SendToHubsAsync(json);
            }
            else
                Debug.WriteLine("START UP MESSAGE" + evt.Device.Address);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            LagoVista.Core.PlatformSupport.Services.Logger.AddException("Unhandled Exception", ex.Exception);
        }

        private void Instance_Disconnected(object sender, EventArgs e)
        {
            ISYEventListener.Instance.Disconnect();
        }

        private async void Instance_Connected(object sender, EventArgs e)
        {
            await ISYEventListener.Instance.StartListening(ISYService.Instance, ISYService.Instance.ConnectionSettings);
        }


        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
