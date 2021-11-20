using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace ApiCalc.Middleware
{
    public class MaxConcurrentRequestsMiddleware
    {
        private int _concurrentRequestsCount;
        private readonly RequestDelegate _next;
        private readonly MaxConcurrentRequestsOptions _options;
      
        public MaxConcurrentRequestsMiddleware(RequestDelegate next, IOptions<MaxConcurrentRequestsOptions> options)
        {
            _concurrentRequestsCount = 0;
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options.Value ;
        }
        public async Task Invoke(HttpContext context)
        {
            if (CheckLimitExceeded())
            {
                IHttpResponseFeature responseFeature = context.Features.Get<IHttpResponseFeature>();
                responseFeature.StatusCode = StatusCodes.Status503ServiceUnavailable;
                responseFeature.ReasonPhrase = "Concurrent request limit exceeded.";
            }
            else
            {
                await _next(context);
                Interlocked.Decrement(ref _concurrentRequestsCount);
            }
        }
        private bool CheckLimitExceeded()
        {
             bool LimitExceeded;

            if (_options.Limit == MaxConcurrentRequestsOptions.ConcurrentRequestsUnlimited)
            {
                LimitExceeded = false;
            }
            else
            {
                int initialConcurrentRequestsCount, incrementedConcurrentRequestsCount;
                do
                {
                    LimitExceeded = true;

                    initialConcurrentRequestsCount = _concurrentRequestsCount;
                    if (initialConcurrentRequestsCount >= _options.Limit)
                    {
                        break;
                    }

                    LimitExceeded = false;
                    incrementedConcurrentRequestsCount = initialConcurrentRequestsCount + 1;
                }
                while (initialConcurrentRequestsCount != Interlocked.CompareExchange(ref _concurrentRequestsCount, incrementedConcurrentRequestsCount, initialConcurrentRequestsCount));
            }
            return LimitExceeded;
        }

        
    }
}