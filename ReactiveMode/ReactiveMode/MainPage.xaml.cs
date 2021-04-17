using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ReactiveMode
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel ViewModel { get; private set; }
        public MainPage(IAppEventObservable appEvents)
        {
            ViewModel = new MainViewModel(appEvents);
            BindingContext = ViewModel;
            InitializeComponent();
        }
    }
}
