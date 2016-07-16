using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.OrderProvider
{
    internal class Ordered<T> : IOrdered<T>, IDisposable
    {
        private readonly List<IDisposable> _disposable = new List<IDisposable>();
        private readonly OrderContainer<T> _container;
        private readonly IEnumerable<T> _items;
        private readonly IServiceProvider _provider;
        
        public Ordered(IServiceProvider provider, OrderContainer<T> container, IEnumerable<T> items)
        {
            _provider = provider;
            _container = container;
            _items = items;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposable)
            {
                disposable.Dispose();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _container)
            {
                T result = default(T);
                if (item.ImplementationFactory != null)
                {
                    result = (T)item.ImplementationFactory(_provider);
                }
                else if (item.ImplementationInstance != null)
                {
                    result =  (T)item.ImplementationInstance;
                }
                else if (item.ImplementationType != null)
                {
                    // we can create instances manually bu better to give 
                    // container ability to optimize
                    result =  _items.SingleOrDefault(i => i.GetType() == item.ImplementationType);
                }
                var disposable = result as IDisposable;
                if (disposable != null)
                {
                    _disposable.Add(disposable);
                }
                yield return result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}