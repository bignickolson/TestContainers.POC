using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using TestContainers.Web;

namespace TestContainers.Tests
{
    [TestClass]
    public class WebTests : PageTest
    {
        private static IContainer _apiContainer;
        private static IContainer _webContainer;

        [ClassInitialize]
        public static async Task ClassInit(TestContext ctx)
        {
            var network = new NetworkBuilder().WithName(Guid.NewGuid().ToString("D")).Build();
            await network.CreateAsync();

            var sql = new MsSqlBuilder()
                .WithNetwork(network)
                .WithName("IntegrationTestSql")
                .Build();


            await sql.StartAsync();

            var connStr = "Server=IntegrationTestSql,1433;Database=master;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True";

            _apiContainer = new ContainerBuilder()
                .WithImage("testcontainersapi:dev")
                .WithImagePullPolicy(PullPolicy.Never)
                .WithName("api")
                .WithNetwork(network)
                .DependsOn(sql)
                .WithPortBinding(8080, true)
                .WithEnvironment("ConnectionStrings__Default", connStr)
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_HTTP_PORT", "8080")
                .WithName("IntegrationTestApi")
                .WithWaitStrategy(Wait
                    .ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health"), strat => strat.WithTimeout(TimeSpan.FromSeconds(30))))
                .Build();

            await _apiContainer.StartAsync();

            _webContainer = new ContainerBuilder()
                .WithImagePullPolicy(PullPolicy.Never)
                .WithImage("testcontainersweb:dev")
                .WithNetwork(network)
                .DependsOn(_apiContainer)
                .WithEnvironment("ApiUrl", "http://api:8080")
                .WithName("IntegrationTestWeb")
                .WithPortBinding(8081, true)
                .WithWaitStrategy(Wait
                    .ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8081).ForPath("/health"), strat => strat.WithTimeout(TimeSpan.FromSeconds(30))))
                .Build();

            await _webContainer.StartAsync();

         
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (_apiContainer != null)
            {
                await _apiContainer.StopAsync();
                await _apiContainer.DisposeAsync();
            }

            if (_webContainer != null)
            {
                await _webContainer.StopAsync();
                await _webContainer.DisposeAsync();
            }
        }

        public override BrowserNewContextOptions ContextOptions()
        {
            return new BrowserNewContextOptions
            {
                ViewportSize = new()
                {
                    Width = 1920,
                    Height = 1080
                },
                BaseURL = $"https://localhost:{_webContainer.GetMappedPublicPort(8081)}"
            };
        }

        [TestMethod]
        public async Task LoadHomepage()
        {
            var result = await Page.GotoAsync("/");
        }

    }
}
