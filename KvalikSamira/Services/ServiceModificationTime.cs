using System;
using KvalikSamira.Models;

namespace KvalikSamira.Services
{
    public static class ServiceModificationTime
    {
        public static DateTime NormalizeToUtc(DateTime value) =>
            value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value.ToUniversalTime()
            };

        public static DateTime GetTimestampUtc(DateTime utcNow) => NormalizeToUtc(utcNow);

        public static void ApplyLastModifiedUtc(Service service, DateTime utcNow)
        {
            service.modifiedAt = GetTimestampUtc(utcNow);
        }

        public static void ApplyNewEntityTimestampsUtc(Service service, DateTime utcNow)
        {
            var t = GetTimestampUtc(utcNow);
            service.createdAt = t;
            service.modifiedAt = t;
        }

        public static bool IsModifiedNotBeforeCreated(Service service) =>
            service.modifiedAt >= service.createdAt;
    }
}
