using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ExportFont("FrankRuhlLibre-Black.ttf", Alias="HeaderFont-Black")]
[assembly: ExportFont("Mada-Light.ttf", Alias = "BodyFont-Light")]
[assembly: ExportFont("Mada-Regular.ttf", Alias = "BodyFont-Regular")]

namespace TravelApp
{
    public partial class App : Application
    {
        public App()
        {
            Device.SetFlags(new string[] { "MediaElement_Experimental" });
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
