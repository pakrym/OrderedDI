using System;
using System.Linq;

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

        private static OrderContainer<T> GetOrderContainer<T>(this IServiceCollection collection) 
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
}