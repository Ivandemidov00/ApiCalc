using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ApiCalc;
using ApiCalc.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TestApiCalc
{
    public class MaxConcurrentRequestsTests
    {
        private struct HttpResponseInformation
        {
            public HttpStatusCode StatusCode { get; set; }

            public TimeSpan Timing { get; set; }

            public override string ToString()
            {
                return $"StatusCode: {StatusCode} | Timing {Timing}";
            }
        }

        private const string DEFAULT_RESPONSE = "-- Demo.AspNetCore.MaxConcurrentConnections --";

        private const int SOME_CONCURRENT_REQUESTS_COUNT = 30;
        private const int SOME_MAX_CONCURRENT_REQUESTS_LIMIT = 10;
        

        private TestServer PrepareTestServer(IEnumerable<KeyValuePair<string, string>> configuration = null)
        {
            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>();

            if (configuration != null)
            {
                ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddInMemoryCollection(configuration);
                IConfiguration buildedConfiguration = configurationBuilder.Build();

                webHostBuilder.UseConfiguration(buildedConfiguration);
                webHostBuilder.ConfigureServices((services) =>
                {
                    services.Configure<MaxConcurrentRequestsOptions>(options => buildedConfiguration.GetSection("MaxConcurrentRequestsOptions").Bind(options));
                });
            }

            return new TestServer(webHostBuilder);
        }

        [Fact]
        public async Task SingleRequest_ReturnsSuccessfulResponse()
        {
            using (TestServer server = PrepareTestServer())
            {
                using (HttpClient client = server.CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5000");

                    Assert.True(response.IsSuccessStatusCode);
                }
            }
        }

        [Fact]
        public async Task SingleRequest_ReturnsDefaultResponse()
        {
            using (TestServer server = PrepareTestServer())
            {
                using (HttpClient client = server.CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5000");
                    string responseText = await response.Content.ReadAsStringAsync();

                    Assert.Equal(DEFAULT_RESPONSE, responseText);
                }
            }
        }

        [Fact]
        public void SomeMaxConcurrentRequestsLimit_Drop_SomeConcurrentRequestsCount_CountMinusLimitRequestsReturnServiceUnavailable()
        {
            Dictionary<string, string> configuration = new Dictionary<string, string>
            {
                {"MaxConcurrentRequestsOptions:Limit", SOME_MAX_CONCURRENT_REQUESTS_LIMIT.ToString() },
                {"MaxConcurrentRequestsOptions:LimitExceededPolicy", MaxConcurrentRequestsLimitExceededPolicy.Drop.ToString() }
            };

            HttpResponseInformation[] responseInformation = GetResponseInformation(configuration, SOME_CONCURRENT_REQUESTS_COUNT);

            Assert.Equal(SOME_CONCURRENT_REQUESTS_COUNT - SOME_MAX_CONCURRENT_REQUESTS_LIMIT, responseInformation.Count(i => i.StatusCode == HttpStatusCode.ServiceUnavailable));
        }
        
        private HttpResponseInformation[] GetResponseInformation(Dictionary<string, string> configuration, int concurrentRequestsCount)
        {
            HttpResponseInformation[] responseInformation;

            using (TestServer server = PrepareTestServer(configuration))
            {
                List<HttpClient> clients = new List<HttpClient>();
                for (int i = 0; i < concurrentRequestsCount; i++)
                {
                    clients.Add(server.CreateClient());
                }

                List<Task<HttpResponseMessageWithTiming>> responsesWithTimingsTasks = new List<Task<HttpResponseMessageWithTiming>>();
                foreach (HttpClient client in clients)
                {
                    responsesWithTimingsTasks.Add(Task.Run(async () => { return await client.GetWithTimingAsync("https://localhost:5001"); }));
                }
                Task.WaitAll(responsesWithTimingsTasks.ToArray());

                clients.ForEach(client => client.Dispose());

                responseInformation = responsesWithTimingsTasks.Select(task => new HttpResponseInformation
                {
                    StatusCode = task.Result.Response.StatusCode,
                    Timing = task.Result.Timing
                }).ToArray();
            }
            return responseInformation;
        }
    }
}