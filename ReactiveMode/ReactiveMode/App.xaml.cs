using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReactiveMode
{
    public partial class App : Application
    {
        private AppEvents appEvents;

        public App()
        {
            InitializeComponent();

            appEvents = new AppEvents();

            MainPage = new MainPage(appEvents);
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            appEvents.ChangeIsInBackground(true);
        }

        protected override void OnResume()
        {
            appEvents.ChangeIsInBackground(false);
        }
    }
}
