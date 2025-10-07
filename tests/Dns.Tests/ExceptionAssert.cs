using System;
using System.Linq;
using Shouldly;

namespace DnsTests;

/// <summary>
///   Asserting an <see cref="Exception"/>.
/// </summary>
public static class ExceptionAssert
{
    public static void Throws<T>(Action action, string expectedMessage = null) where T : Exception
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
                match.Message.ShouldBe(expectedMessage, "Wrong exception message.");
            return;

        }
        catch (T e)
        {
            if (expectedMessage != null)
                e.Message.ShouldBe(expectedMessage);
            return;
        }
        catch (Exception e)
        {
            throw new Xunit.Sdk.XunitException($"Exception of type {typeof(T)} should be thrown not {e.GetType()}.");
        }

        throw new Xunit.Sdk.XunitException($"Expected Exception of type {typeof(T)} but nothing was thrown.");
    }
}