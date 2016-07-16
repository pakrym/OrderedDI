using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.OrderProvider
{
    public interface IOrdered<out T>: IEnumerable<T>
    {
    }
}