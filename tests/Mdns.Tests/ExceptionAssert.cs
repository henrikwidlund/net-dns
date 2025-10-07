using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;

namespace Makaretu.Mdns;

/// <summary>
///   Asserting an <see cref="Exception"/>.
/// </summary>
public static class ExceptionAssert
{
    public static async Task ThrowsAsync<T>(Func<Task> action, string expectedMessage = null) where T : Exception
    {
        Exception thrown = null;
        try
        {
            await action();
        }
        catch (AggregateException e)
        {
            thrown = e.InnerExceptions.OfType<T>().FirstOrDefault();
            if (thrown == null)
                throw;
        }
        catch (T e)
        {
            thrown = e;
        }
        thrown.ShouldNotBeNull();
        if (expectedMessage != null)
            thrown.Message.ShouldBe(expectedMessage);
    }
}