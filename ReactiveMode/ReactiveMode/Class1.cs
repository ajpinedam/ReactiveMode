using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace ReactiveMode
{
    public interface IAppEventObservable
    {
        IObservable<bool> IsInBackground();
    }

    public interface IAppEvent
    {
        void ChangeIsInBackground(bool isInBackground);
    }

    public class AppEvents : IAppEvent, IAppEventObservable
    {
        private BehaviorSubject<bool> _isInBackground = new BehaviorSubject<bool>(false);
        public IObservable<bool> IsInBackground() =>
            _isInBackground.AsObservable();
    
        public void ChangeIsInBackground(bool isInBackground)
            => _isInBackground.OnNext(isInBackground);
    }
}
