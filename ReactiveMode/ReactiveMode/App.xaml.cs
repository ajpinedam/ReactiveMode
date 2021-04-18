using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ReactiveUI;
using Splat;

namespace ReactiveMode
{
    public partial class App : Application
    {
        private AppEvents appEvents;

        public App()
        {
            InitializeComponent();

            RegisterDependencies();

            appEvents = Locator.Current.GetService<IAppEventObservable>() as AppEvents;

            MainPage = new MainPage();
        }

        private void RegisterDependencies()
        {
            Locator.CurrentMutable.RegisterConstant(new AppEvents(), typeof(IAppEventObservable));

            Locator.CurrentMutable.Register(() => new MainViewModel(Locator.Current.GetService<IAppEventObservable>()), typeof(MainViewModel));
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
