using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using RateLimiter;
using Xunit;

namespace UnitTests;

public class RateLimiterTests
{
    
    [Fact]
    public void WhenNoOtherRequestsFromCustomer_ThenRequestShouldSucceed()
    {
        var sut = new SlidingWindowRateLimiter(100, TimeSpan.FromMinutes(1));

        var result = sut.RateLimit(1);

        result.Should().BeTrue();
    }

    [Fact]
    public void When101RequestsInLessThanOneMinute_ThenRequestShouldFail()
    {
        var sut = new SlidingWindowRateLimiter(100, TimeSpan.FromMinutes(1));

        for (var i = 1; i <= 100; i++)
        {
            sut.RateLimit(1);
        }

        var result = sut.RateLimit(1);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void WhenOneCustomerIsRateLimited_ThenDifferentCustomerCanStillMakeRequest()
    {
        var sut = new SlidingWindowRateLimiter(100, TimeSpan.FromMinutes(1));

        for (var i = 1; i <= 100; i++)
        {
            sut.RateLimit(1);
        }

        using (new AssertionScope())
        {
            sut.RateLimit(1).Should().BeFalse();
            sut.RateLimit(2).Should().BeTrue();
        }
    }

    [Fact]
    public void WhenTimespanPassed_ThenRequestCanGoThrough()
    {
        var sut = new SlidingWindowRateLimiter(1, TimeSpan.FromSeconds(1));

        sut.RateLimit(1).Should().BeTrue();

        sut.RateLimit(1).Should().BeFalse();
        
        Thread.Sleep(TimeSpan.FromSeconds(1));

        sut.RateLimit(1).Should().BeTrue();
    }

    [Fact]
    public void RateLimiter_ShouldOnlyAllowPositiveRates()
    {
        using(new AssertionScope()){
            Assert.Throws<ArgumentException>(() => new SlidingWindowRateLimiter(-1, TimeSpan.FromSeconds(1)));
            Assert.Throws<ArgumentException>(() => new SlidingWindowRateLimiter(0, TimeSpan.FromSeconds(1)));
        }
        
    }
    
    [Fact]
    public void RateLimiter_ShouldOnlyAllowPositiveTimespans()
    {
        using (new AssertionScope())
        {
            Assert.Throws<ArgumentException>(() => new SlidingWindowRateLimiter(1, TimeSpan.MinValue));
            Assert.Throws<ArgumentException>(() => new SlidingWindowRateLimiter(1, TimeSpan.Zero));
        }
    }
}