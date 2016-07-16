using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Microsoft.Extensions.DependencyInjection.OrderProvider
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrdered<T, I>(this IServiceCollection collection) 
            where I : class, T
            where T: class
        {
            var orderContainer = collection.GetOrderContainer<T>();

            // TODO: Add check so two services could not be added with same implementation type
            orderContainer.Add(ServiceDescriptor.Transient(typeof(T), typeof(I)));
            collection.AddTransient<T, I>();
            return collection;
        }

        public static IServiceCollection AddOrdered<T>(this IServiceCollection collection, T instance)
            where T : class
        {
            var orderContainer = collection.GetOrderContainer<T>();
            orderContainer.Add(ServiceDescriptor.Singleton(typeof(T), instance));
            return collection;
        }

        public static IServiceCollection AddOrdered<T>(this IServiceCollection collection, Func<IServiceProvider, T> factory)
            where T : class
        {
            var orderContainer = collection.GetOrderContainer<T>();
            orderContainer.Add(ServiceDescriptor.Transient(typeof(T), factory));
            return collection;
        }

        public static OrderContainer<T> GetOrderContainer<T>(this IServiceCollection collection) 
        {
            var containers = collection.Where(d => d.ServiceType == typeof(OrderContainer<T>));
            if (containers.Count() > 1)
            {
                throw new InvalidOperationException("Found multiple OrderContainers for the same type");
            }
            var containerDescriptor = containers.FirstOrDefault();
            if (containerDescriptor == null)
            {
                
                var container = new OrderContainer<T>();
                collection.AddSingleton(container);
                collection.AddTransient<IOrdered<T>, Ordered<T>>();
                return container;
            }
            else
            {
                if (containerDescriptor.ImplementationInstance == null ||
                    containerDescriptor.Lifetime != ServiceLifetime.Singleton ||
                    !(containerDescriptor.ImplementationInstance is OrderContainer<T>))
                {
                    throw new InvalidOperationException("Container service descriptor is invalid");
                }

                return (OrderContainer<T>)containerDescriptor.ImplementationInstance;
            }
        }
    }

    public interface IOrderProvider<T>
    {
        IEnumerable<T> Apply(IEnumerable<T> services);
    }

    public class Ordered<T> : IOrdered<T>, IDisposable
    {
        private static readonly List<IDisposable> _disposable = new List<IDisposable>();

        private OrderContainer<T> _container;
        private IEnumerable<T> _items;
        private IServiceProvider _provider;
        
        public Ordered(IServiceProvider provider, OrderContainer<T> container, IEnumerable<T> items)
        {
            _provider = provider;
            _container = container;
            _items = items;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
                if (result is IDisposable)
                {
                    _disposable.Add((IDisposable)result);
                    yield return result;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IOrdered<T>: IEnumerable<T>
    {
    }

    public class OrderContainer <T>: List<ServiceDescriptor>
    {
    }
}
