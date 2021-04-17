using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;

namespace ReactiveMode
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(IAppEventObservable appEvents)
        {
            SubmitCommand = new Command(ExecuteSubmit, ItCanExecute);

            var isAppInBackground = appEvents.IsInBackground();

            var connectionChanged =
            Observable.FromEventPattern<ConnectivityChangedEventArgs>
                (h => Connectivity.ConnectivityChanged += h,
                h => Connectivity.ConnectivityChanged -= h)
                .Select(a => a.EventArgs.NetworkAccess == NetworkAccess.Internet || a.EventArgs.NetworkAccess == NetworkAccess.Local);

            connectionChanged
                .Subscribe(canExecute => SubmitCommand.CanExecute(canExecute));

            var timedHasPassed = Observable.Interval(TimeSpan.FromMinutes(15));

            connectionChanged.Where(isConnected => !isConnected)
                .Merge(isAppInBackground.Where(isInBackground => isInBackground))
                .Merge(timedHasPassed.Select(a => true))
                .Subscribe(StopLongRunningService);
        }

        private void StopLongRunningService(bool obj)
        {
            
        }

        private void ExecuteSubmit(object obj)
        {
         
        }

        private bool ItCanExecute(object arg)
        {
            return arg == null ? false : (bool)arg;
        }

        public ICommand SubmitCommand { get; private set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _lastName;
        public string LastName 
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _username;
        public string UserName
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string _password;
        public string Password 
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool itCanExecuteSubmitCommand = false;
    }
}
