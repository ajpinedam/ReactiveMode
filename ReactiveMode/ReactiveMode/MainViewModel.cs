using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Disposables;

namespace ReactiveMode
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel(IAppEventObservable appEvents)
        {
            // Tomamos un C# event y lo convertimos a un Observable.
            // Este Observable retornara el estado de la conneccion a Internet
            var isConnected = Observable.FromEventPattern<ConnectivityChangedEventArgs>
                                                (h => Connectivity.ConnectivityChanged += h,
                                                h => Connectivity.ConnectivityChanged -= h)
                                                .Select(IsDeviceConnected);

            // Observamos cuando el valor de la propiedad `Name` cambie usando `WhenAnyValue`
            var nameChanged = this.WhenAnyValue(x => x.Name)
                                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler);

            // Observamos cuando el valor de la propiedad `LastName` cambie usando `WhenAnyValue`
            var lastNameChanged = this.WhenAnyValue(x => x.LastName)
                                      .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler);

            // Tomamos 3 Observables distintos y lo combinamos usando CombineLatest. 
            // El Observable resultante es un IObservable<bool> el cual decidira si un Commando se puede ejecutar.
            // Para el isConnected tenemos que usar `StartWith` ya que este Observable no nos va a notificar hasta cuando
            // ocurra un cambio en la conneccion pero necesitamos el estado actual.
            var canExecuteCreate = isConnected.StartWith(GetCurrentNetworkState())
                                              .CombineLatest(nameChanged, lastNameChanged,
                                                (connected, name, lastName) => !string.IsNullOrWhiteSpace(name)
                                            && !string.IsNullOrWhiteSpace(lastName) && connected);

            // Creamos un ReactiveCommand al cual usara como `canExecute` el Observable `canExecuteCreate`
            // Esto hara que cuando haya un cambio en cualquiera de los observables de ese pipeline este Commando reacionara a esto.
            CreateCommand = ReactiveCommand.CreateFromTask(ExecuteCreate, canExecuteCreate);

            // Creamos un segundo ReactiveCommand
            SubmitCommand = ReactiveCommand.Create(ExecuteSubmit);

            var isAppInBackground = appEvents.IsInBackground();

            // Combinamos dos observables y el observable resultante lo llevamos a un
            // OAPH utilizando `ToProperty`
            var changed = nameChanged.CombineLatest(lastNameChanged,
                (name, lastName) =>  $"{lastName}, {name}")
                .ToProperty(this, nameof(FullName), out _fullName)
                .DisposeWith(ViewModelSubscriptions);

            // Nos subscribimos al pipelione del ReactiveCommand y cuando este completa 
            // invocamos un segundo ReactiveCommand utilizando `InvokeCommand`
            CreateCommand
                .Select(a =>  Unit.Default)
                .InvokeCommand(SubmitCommand)
                .DisposeWith(ViewModelSubscriptions);

            // Nos subscribimos al pipelione del ReactiveCommand y si ocurre una Exception durante su ejecucion
            // nos subscribimos a ella
            CreateCommand
                .ThrownExceptions
                .Subscribe(error => 
                {
                    // Aqui podemos actual en base al error
                })
                .DisposeWith(ViewModelSubscriptions);

            //Tomando el resultado del ReactiveCommand y lo llevamos un OAPH utilizando `ToProperty`
            CreateCommand.ToProperty(this, nameof(LatestDocumentId), out _latestDocId);

            // Muestra de como podemos invocar un RactiveCommand desde cualquier Pipeline
            nameChanged
                .Where(x => x == "ReactiveUI")
                .Select(x => Unit.Default)
                .InvokeCommand(CreateCommand);

            // Podemos subscribirnos al Observable IsExecuting del Commando para verificar el estado de la ejecucion.
            SubmitCommand.IsExecuting
                .CombineLatest(CreateCommand.IsExecuting, (submitIsExecuting, createIsExecuting) => !submitIsExecuting && !createIsExecuting)
                .Skip(1)
                .Where(isExecuting => !isExecuting)
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    // Aqui tenemos acceso a ejecutar cualquier accion o cambiar el valor de alguna propiedad.
                    // Pero si ese es el caso mejor evaluas hacerlo con `InvokeCommand` o `ToProperty` dependiendo
                    // Cual sea tu necesidad. Aqui solo te mostramos que es posible.
                })
                .DisposeWith(ViewModelSubscriptions);

            var timedHasPassed = Observable.Interval(TimeSpan.FromMinutes(15));

            // Tomamos 3 Observables y le hacer Merge. Cuando alguno de estos 3 Observables de este Pipeline
            // notifique y las condiciones se den se ejecutra el metodo dentro del Subscribe.
            isConnected.Where(connected => !connected)
                       .Merge(isAppInBackground.Where(isInBackground => isInBackground))
                       .Merge(timedHasPassed.Select(a => true)) //Merge necesita que todos los Observables sean del mismo tipo
                       .Subscribe(StopLongRunningService)
                       .DisposeWith(ViewModelSubscriptions);
        }

        public ReactiveCommand<Unit, Unit> SubmitCommand { get; set; }

        public ReactiveCommand<Unit, int> CreateCommand { get; set; }

        [Reactive] public string UserName { get; set; }
        [Reactive] public string Password { get; set; }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public string LastName 
        {
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }

        public int LatestDocumentId => _latestDocId.Value;
        public string FullName => _fullName.Value;

        private async Task<int> ExecuteCreate()
        {
            // Mocking a Running task en el servidor or el cualquier lugar
            await Task.Delay(TimeSpan.FromSeconds(5));

            return new Random().Next(500);
        }

        private Unit ExecuteSubmit()
        {
            return Unit.Default;
        }

        private void StopLongRunningService(bool obj)
        {

        }

        private static bool GetCurrentNetworkState() =>
            Connectivity.NetworkAccess == NetworkAccess.Internet || Connectivity.NetworkAccess == NetworkAccess.Local;


        // Una buena forma de hacer nuestros Pipelines de observables mas simples de leer es moviendo
        // Predicates en funciones que por lo regular seran estaticas (Static)
        private static bool IsDeviceConnected(EventPattern<ConnectivityChangedEventArgs> a) =>
            a.EventArgs.NetworkAccess == NetworkAccess.Internet || a.EventArgs.NetworkAccess == NetworkAccess.Local;


        private string _name;
        private string _lastName;

        // Definiendo OAPH. Estos deben ser readonly
        // Documentation: https://www.reactiveui.net/docs/handbook/observable-as-property-helper/
        private readonly ObservableAsPropertyHelper<int> _latestDocId;
        private readonly ObservableAsPropertyHelper<string> _fullName;

        private readonly CompositeDisposable ViewModelSubscriptions = new CompositeDisposable();
    }
}
