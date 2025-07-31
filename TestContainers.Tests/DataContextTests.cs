using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using TestContainers.Api.Data;

namespace TestContainers.Tests
{
    [TestClass]
    public sealed class DataContextTests
    {
        private DataContext _context;
        private static MsSqlContainer _sqlContainer;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _sqlContainer = new MsSqlBuilder().Build();
            await _sqlContainer.StartAsync();

            var ctx = new DataContext(
                new DbContextOptionsBuilder<DataContext>()
                    .UseSqlServer(_sqlContainer.GetConnectionString())
                    .Options
            );

            await ctx.Database.MigrateAsync();

        }

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new DataContext(
                new DbContextOptionsBuilder<DataContext>()
                    .UseSqlServer(_sqlContainer.GetConnectionString()) // Replace with actual connection string
                    .Options
            );
        }


        [TestMethod]
        public async Task MigrationsRunSuccessfully()
        {
            var pending = await _context.Database.GetPendingMigrationsAsync();

            Assert.IsFalse(pending.Any(), "There should be no pending migrations");
        }

        [TestMethod]
        public async Task SeedDataExists()
        {
            var count = await _context.Messages.CountAsync();
            Assert.AreEqual(2, count, "There should be seeded data in the Messages table");
        }

        [TestMethod]
        public async Task ReminderMessageExists()
        {
            var m = await _context.Messages.FirstOrDefaultAsync(i => i.Id == 2);
            Assert.IsNotNull(m, "There should be a message with Id 2");
            Assert.AreEqual("Reminder", m.Subject, "The subject of the message with Id 2 should be 'Reminder'");

        }
    }
}
