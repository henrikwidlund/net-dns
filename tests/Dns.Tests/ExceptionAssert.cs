using System;
using System.Linq;
using System.Threading.Tasks;

namespace DnsTests;

/// <summary>
///   Asserting an <see cref="Exception"/>.
/// </summary>
public static class ExceptionAssert
{
    public static async Task Throws<T>(Action action, string? expectedMessage = null) where T : Exception
    {
        try
        {
            action();
        }
        catch (AggregateException e)
        {
            var match = e.InnerExceptions.OfType<T>().FirstOrDefault();
            if (match == null)
                throw;

            if (expectedMessage != null)
                await Assert.That(match.Message).IsEqualTo(expectedMessage).Because("Wrong exception message.");
            return;

        }
        catch (T e)
        {
            if (expectedMessage != null)
                await Assert.That(e.Message).IsEqualTo(expectedMessage);
            return;
        }
        catch (Exception e)
        {
            Assert.Fail($"Exception of type {typeof(T)} should be thrown not {e.GetType()}.");
        }

        Assert.Fail($"Expected Exception of type {typeof(T)} but nothing was thrown.");
    }
}