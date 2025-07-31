using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
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
    public class ApiTests
    {
        private static IContainer _apiContainer;
        private static HttpClient _httpclient;
        private static MessageClient _messageClient;

        [ClassInitialize]
        public static async Task ClassInit(TestContext ctx)
        {
            var network = new NetworkBuilder().WithName(Guid.NewGuid().ToString("D")).Build();

            await network.CreateAsync();

            var sqlName = $"sql{Guid.NewGuid()}";
            var sql = new MsSqlBuilder()
                .WithNetwork(network)
                .WithName(sqlName)
                .Build();


            await sql.StartAsync();

            var connStr = $"Server={sqlName},1433;Database=master;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True";

            _apiContainer = new ContainerBuilder()
                .WithImagePullPolicy(PullPolicy.Never)
                .WithImage("testcontainersapi")
                .WithNetwork(network)
                .DependsOn(sql)
                .WithEnvironment("ConnectionStrings__Default", connStr)
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_HTTP_PORT", "8080")
                .WithName("IntegrationTestApi")
                .WithPortBinding(8080, true)
                .WithWaitStrategy(Wait
                    .ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health"), strat => strat.WithTimeout(TimeSpan.FromSeconds(300))))
                .Build();

            try
            {

                _apiContainer.StartAsync().Wait();

                _httpclient = new HttpClient
                {
                    BaseAddress = new Uri($"http://localhost:{_apiContainer.GetMappedPublicPort(8080)}")
                };

                _messageClient = new MessageClient($"http://localhost:{_apiContainer.GetMappedPublicPort(8080)}", _httpclient);
            }
            catch (Exception ex)
            {
                var logs = await _apiContainer.GetLogsAsync();

                throw ex;
            }

        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (_apiContainer != null)
            {
                await _apiContainer.StopAsync();
                await _apiContainer.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetMessages()
        {
            var messages = await _messageClient.MessagesAllAsync();

            Assert.AreEqual(2, messages.Count, "There should be 2 messages in the database");

            var reminderMessage = messages.FirstOrDefault(m => m.Id == 2);
            Assert.IsNotNull(reminderMessage, "Reminder message should exist");
            Assert.AreEqual("Reminder", reminderMessage.Subject, "The subject of the reminder message should be 'Reminder'");
        }

    }
}
