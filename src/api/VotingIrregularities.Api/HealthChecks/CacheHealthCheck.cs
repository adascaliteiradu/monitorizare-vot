using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VoteMonitor.Api.Core.Options;

namespace VotingIrregularities.Api.HealthChecks
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        public CacheHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var cacheOptions = new ApplicationCacheOptions();
            _configuration.GetSection(nameof(ApplicationCacheOptions)).Bind(cacheOptions);
            var result = HealthCheckResult.Unhealthy("No cache implementeation is specified!");

            switch (cacheOptions.Implementation)
            {
                case "NoCache":
                    {
                        result = CheckHealthNoCache();
                        break;
                    }
                case "RedisCache":
                    {
                        result = CheckHealthRedisCache();
                        break;
                    }
                case "MemoryDistributedCache":
                    {
                        result = CheckHealthMemoryDistributedCache();
                        break;
                    }
            }
            return Task.FromResult(result);
        }

        private HealthCheckResult CheckHealthNoCache()
        {
            return HealthCheckResult.Healthy("No cache... OK");
        }

        private HealthCheckResult CheckHealthRedisCache()
        {
            throw new NotImplementedException();
        }

        private HealthCheckResult CheckHealthMemoryDistributedCache()
        {
            throw new NotImplementedException();
        }
    }
}