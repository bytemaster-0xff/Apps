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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LagoVista.ManCaveController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //private DispatcherTimer _refreshTimer;
        public MainPage()
        {
            this.InitializeComponent();
//            this.Loaded += MainPage_Loaded;
            
            /*_refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += _refreshTimer_Tick;*/
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
  /*          DataContext = LagoVista.Common.Yahama.YahamaAVR.Initailize("MANCAVE", "10.1.1.233");
            _refreshTimer.Start();
            Refresh();*/
        }

        private void Refresh()
        {
            //await LagoVista.Common.Yahama.YahamaAVR.Devices["MANCAVE"].RefreshStatus(Common.Yahama.YahamaAVR.Zones.MainZone);
            //await LagoVista.Common.Yahama.YahamaAVR.Devices["MANCAVE"].GetSiriusXMStatus();
        }

        private void _refreshTimer_Tick(object sender, object e)
        {
            //Refresh();
        }
    }
}
