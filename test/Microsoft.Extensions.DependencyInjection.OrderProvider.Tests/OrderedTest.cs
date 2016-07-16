using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.OrderProvider.Tests
{
    internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }

    public class Program
    {
        [Fact]
        public void ReturnsImplementationTypeBasedServicesInRegistrationOrder()
        {
            var serviceCollection = new TestServiceCollection();
            serviceCollection.AddOrdered<IService, Foo>();
            serviceCollection.AddOrdered<IService, Bar>();
            serviceCollection.Reverse();

            var provider = serviceCollection.BuildServiceProvider();
            var services = provider.GetService<IOrdered<IService>>().ToArray();

            Assert.IsType<Foo>(services[0]);
            Assert.IsType<Bar>(services[1]);
        }

        [Fact]
        public void ReturnsFactoryBasedServicesInRegistrationOrder()
        {
            var serviceCollection = new TestServiceCollection();
            serviceCollection.AddOrdered<IService>((p) => new Foo());
            serviceCollection.AddOrdered<IService>((p) => new Bar());
            serviceCollection.Reverse();

            var provider = serviceCollection.BuildServiceProvider();
            var services = provider.GetService<IOrdered<IService>>().ToArray();

            Assert.IsType<Foo>(services[0]);
            Assert.IsType<Bar>(services[1]);
        }

        [Fact]
        public void ReturnsInstanceBasedServicesInRegistrationOrder()
        {
            var serviceCollection = new TestServiceCollection();
            serviceCollection.AddOrdered<IService>(new Foo());
            serviceCollection.AddOrdered<IService>(new Bar());
            serviceCollection.Reverse();

            var provider = serviceCollection.BuildServiceProvider();
            var services = provider.GetService<IOrdered<IService>>().ToArray();

            Assert.IsType<Foo>(services[0]);
            Assert.IsType<Bar>(services[1]);
        }

        [Fact]
        public void ReturnsMixedBasedServicesInRegistrationOrder()
        {
            var serviceCollection = new TestServiceCollection();
            serviceCollection.AddOrdered<IService>((p) => new Foo());
            serviceCollection.AddOrdered<IService>(new Bar());
            serviceCollection.AddOrdered<IService, Baz>();

            serviceCollection.Reverse();

            var provider = serviceCollection.BuildServiceProvider();
            var services = provider.GetService<IOrdered<IService>>().ToArray();

            Assert.IsType<Foo>(services[0]);
            Assert.IsType<Bar>(services[1]);
            Assert.IsType<Baz>(services[2]);
        }


        private interface IService
        {

        }

        private class Foo: IService
        {
        }

        private class Bar : IService
        {
        }
        private class Baz : IService
        {
        }
    }
}
