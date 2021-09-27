using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ApiCalc;
using ApiCalc.Middleware;
using ApiCalc.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

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
        public static Int32 InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var res = Convert.ToInt32(config["Limit"]);
            
            return res;
        }
        
        ITestOutputHelper output;
        private readonly ITestOutputHelper _testOutputHelper;
        
        private readonly String DefaultResponce = "test";
        
        private readonly Int32 ConcurrentCount = 24;
        private readonly Int32 ConcurrentLimit = InitConfiguration();
        
        public MaxConcurrentRequestsTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        

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
        public async Task SingleRequest()
        {
            using (TestServer server = PrepareTestServer())
            {
                using (HttpClient client = server.CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5000");
                    string responseText = await response.Content.ReadAsStringAsync();

                    Assert.Equal(DefaultResponce, responseText);
                }
            }
        }

        [Fact]
        public void MaxConcurrentRequestsLimit()
        {
            Dictionary<string, string> configuration = new Dictionary<string, string>
            {
                {"MaxConcurrentRequestsOptions:Limit", ConcurrentLimit.ToString() },
            };

            HttpResponseInformation[] responseInformation = GetResponseInformation(configuration, ConcurrentCount);
           
            Assert.Equal(ConcurrentCount - ConcurrentLimit, responseInformation.Count(i => i.StatusCode == HttpStatusCode.ServiceUnavailable));
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
            foreach (var rs in responseInformation)
            {
                _testOutputHelper.WriteLine(rs.ToString());
            }
            return responseInformation;
        }
    }
}