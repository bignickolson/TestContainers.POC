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

            var sqlName = $"sql{Guid.NewGuid()}";

            var sql = new MsSqlBuilder()
                .WithNetwork(network)
                .WithName(sqlName)
                .Build();


            await sql.StartAsync();

            var connStr = $"Server={sqlName},1433;Database=master;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True";

            _apiContainer = new ContainerBuilder()
                .WithImage("testcontainersapi")
                .WithImagePullPolicy(PullPolicy.Never)
                .WithName("api")
                .WithNetwork(network)
                .DependsOn(sql)
                .WithPortBinding(8080, true)
                .WithEnvironment("ConnectionStrings__Default", connStr)
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_HTTP_PORT", "8080")
                .WithWaitStrategy(Wait
                    .ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health"), strat => strat.WithTimeout(TimeSpan.FromSeconds(30))))
                .Build();

            await _apiContainer.StartAsync();

            _webContainer = new ContainerBuilder()
                .WithImagePullPolicy(PullPolicy.Never)
                .WithImage("testcontainersweb")
                .WithNetwork(network)
                .DependsOn(_apiContainer)
                .WithEnvironment("ApiUrl", "http://api:8080")
                .WithName("IntegrationTestWeb")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_HTTP_PORT", "8080")
                .WithPortBinding(8080, true)
                .WithWaitStrategy(Wait
                    .ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health"), strat => strat.WithTimeout(TimeSpan.FromSeconds(30))))
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
                BaseURL = $"http://localhost:{_webContainer.GetMappedPublicPort(8080)}"
            };
        }

        [TestMethod]
        public async Task LoadHomepage()
        {
            var result = await Page.GotoAsync("/");
            Assert.IsTrue(result.Ok);

            var messageCount = await Page.Locator(".message-subject").CountAsync();
            Assert.AreEqual(2, messageCount, "There should be 2 messages displayed on the homepage");
        }

        [TestMethod]
        public async Task LoadMessageFromHomePage()
        {
            var result = await Page.GotoAsync("/");
            Assert.IsTrue(result.Ok);

            // click the second link, should be the reminder
            await Page.Locator(".message-subject").Last.ClickAsync();

            await Page.WaitForURLAsync("/message/2");

            var subject = await Page.Locator("#message-subject").TextContentAsync();

            Assert.AreEqual("Reminder", subject);
        }

        [TestMethod]
        public async Task MessageAdd()
        {
            var result = await Page.GotoAsync("/addmessage");
            Assert.IsTrue(result.Ok);


            // Fill out the form fields
            await Page.FillAsync("#Message_Subject", "Test Subject");
            await Page.FillAsync("#Message_Content", "This is a test message.");

            // Submit the form (assuming a button[type=submit] exists)
            await Page.ClickAsync("button[type=submit]");

            // Optionally, wait for navigation or confirmation
            await Page.WaitForURLAsync("/");
            var messageCount = await Page.Locator(".message-subject").CountAsync();
            Assert.AreEqual(3, messageCount, "There should be 3 messages displayed after adding a new message");

            // Verify the new message is displayed
            var newMessage = await Page.Locator(".message-subject").Last.TextContentAsync();
            Assert.AreEqual("Test Subject", newMessage, "The new message should be displayed on the homepage");
        }

        [TestMethod]
        public async Task MessageDelete()
        {
            var result = await Page.GotoAsync("/");
            Assert.IsTrue(result.Ok);

            var messageCount = await Page.Locator(".message-subject").CountAsync();
            Assert.AreEqual(3, messageCount, "There should be 3 messages displayed ");

            await Page.Locator(".deleteLink").Last.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.Load);
            await Page.WaitForFunctionAsync("document.querySelectorAll('.message-subject').length === 2");

            messageCount = await Page.Locator(".message-subject").CountAsync();
            Assert.AreEqual(2, messageCount, "There should be 2 messages displayed after deleting a message");

        }
    }
}
