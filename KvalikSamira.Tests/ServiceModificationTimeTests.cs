using System;
using KvalikSamira.Models;
using KvalikSamira.Services;
using Xunit;

namespace KvalikSamira.Tests;

public class ServiceModificationTimeTests
{
    [Fact]
    public void NormalizeToUtc_UtcKind_PreservesInstant()
    {
        var input = new DateTime(2026, 4, 18, 12, 30, 0, DateTimeKind.Utc);
        var result = ServiceModificationTime.NormalizeToUtc(input);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(input.Ticks, result.Ticks);
    }

    [Fact]
    public void NormalizeToUtc_LocalKind_ConvertsToUtc()
    {
        var local = new DateTime(2026, 4, 18, 15, 0, 0, DateTimeKind.Local);
        var utc = ServiceModificationTime.NormalizeToUtc(local);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(local.ToUniversalTime(), utc);
    }

    [Fact]
    public void NormalizeToUtc_UnspecifiedKind_BecomesUtcKind()
    {
        var unspecified = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
        var utc = ServiceModificationTime.NormalizeToUtc(unspecified);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(unspecified.Ticks, utc.Ticks);
    }

    [Fact]
    public void GetTimestampUtc_ReturnsNormalizedUtc()
    {
        var stamp = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var got = ServiceModificationTime.GetTimestampUtc(stamp);
        Assert.Equal(DateTimeKind.Utc, got.Kind);
        Assert.Equal(stamp, got);
    }

    [Fact]
    public void ApplyLastModifiedUtc_SetsModifiedAtOnly()
    {
        var service = new Service
        {
            createdAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            modifiedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var newStamp = new DateTime(2026, 4, 18, 8, 0, 0, DateTimeKind.Utc);
        ServiceModificationTime.ApplyLastModifiedUtc(service, newStamp);
        Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), service.createdAt);
        Assert.Equal(newStamp, service.modifiedAt);
    }

    [Fact]
    public void ApplyNewEntityTimestampsUtc_SetsCreatedAndModifiedEqual()
    {
        var service = new Service();
        var t = new DateTime(2026, 3, 10, 14, 15, 16, DateTimeKind.Utc);
        ServiceModificationTime.ApplyNewEntityTimestampsUtc(service, t);
        Assert.Equal(t, service.createdAt);
        Assert.Equal(t, service.modifiedAt);
        Assert.True(ServiceModificationTime.IsModifiedNotBeforeCreated(service));
    }

    [Fact]
    public void IsModifiedNotBeforeCreated_WhenModifiedEarlier_ReturnsFalse()
    {
        var service = new Service
        {
            createdAt = new DateTime(2026, 4, 18, 12, 0, 0, DateTimeKind.Utc),
            modifiedAt = new DateTime(2026, 4, 17, 12, 0, 0, DateTimeKind.Utc)
        };
        Assert.False(ServiceModificationTime.IsModifiedNotBeforeCreated(service));
    }

    [Fact]
    public void IsModifiedNotBeforeCreated_WhenSame_ReturnsTrue()
    {
        var t = new DateTime(2026, 4, 18, 12, 0, 0, DateTimeKind.Utc);
        var service = new Service { createdAt = t, modifiedAt = t };
        Assert.True(ServiceModificationTime.IsModifiedNotBeforeCreated(service));
    }

    [Fact]
    public void ApplyLastModifiedUtc_SecondUpdate_MovesModifiedForward()
    {
        var service = new Service
        {
            createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            modifiedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var t1 = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2026, 4, 18, 10, 0, 0, DateTimeKind.Utc);
        ServiceModificationTime.ApplyLastModifiedUtc(service, t1);
        Assert.Equal(t1, service.modifiedAt);
        ServiceModificationTime.ApplyLastModifiedUtc(service, t2);
        Assert.Equal(t2, service.modifiedAt);
        Assert.True(service.modifiedAt > service.createdAt);
    }
}
