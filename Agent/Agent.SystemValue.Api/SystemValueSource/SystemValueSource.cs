using System;
using System.Collections.Generic;
using Agent.SystemValue.Api.Types.Base;

namespace Agent.SystemValue.Api.SystemValueSource
{
    public abstract class SystemValueSource<T> : ISystemValueSource<T> where T : ISystemValue, new()
    {
        private readonly IList<IObserver<T>> _observers = new List<IObserver<T>>();

        protected void Notify(T data)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(data);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Unsubscribe(_observers, observer);
        }

        private class Unsubscribe : IDisposable
        {
            private readonly IList<IObserver<T>> _observers;
            private readonly IObserver<T> _metric;

            public Unsubscribe(IList<IObserver<T>> observers, IObserver<T> metric)
            {
                _observers = observers;
                _metric = metric;
            }

            public void Dispose() => _observers.Remove(_metric);
        }
    }
}