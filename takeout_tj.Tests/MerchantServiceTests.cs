using NUnit.Framework;
using takeout_tj.Data;
using takeout_tj.Models.Merchant;
using takeout_tj.Models.Platform;
using takeout_tj.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace takeout_tj.Tests
{
    [TestFixture]
    public class MerchantServiceTests
    {
        private ApplicationDbContext _context;
        private MerchantService _merchantService;
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _merchantService = new MerchantService(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _connection?.Dispose();
        }

        // ---------- Tests for AssignId ----------

        [Test]
        public void AssignId_NoMerchants_Returns1()
        {
            var id = _merchantService.AssignId();
            Assert.AreEqual(1, id);
        }

        [Test]
        public void AssignId_SequentialIds_ReturnsNext()
        {
            _context.Merchants.AddRange(
                new MerchantDB { 
                    MerchantId = 1, 
                    MerchantName = "A",
                    Password = "123456",
                    MerchantAddress = "测试地址",
                    Contact = "123456789",
                    DishType = "中餐",
                    TimeforOpenBusiness = 3600,
                    TimeforCloseBusiness = 7200,
                    WalletPassword = "wallet123"
                },
                new MerchantDB
                {
                    MerchantId = 2,
                    MerchantName = "B",
                    Password = "123456",
                    MerchantAddress = "测试地址",
                    Contact = "123456789",
                    DishType = "中餐",
                    TimeforOpenBusiness = 3600,
                    TimeforCloseBusiness = 7200,
                    WalletPassword = "wallet123"
                }
            );
            _context.SaveChanges();

            var id = _merchantService.AssignId();
            Assert.AreEqual(3, id);
        }

        [Test]
        public void AssignId_GapExists_ReturnsGapId()
        {
            _context.Merchants.AddRange(
                new MerchantDB
                {
                    MerchantId = 1,
                    MerchantName = "A",
                    Password = "123456",
                    MerchantAddress = "测试地址",
                    Contact = "123456789",
                    DishType = "中餐",
                    TimeforOpenBusiness = 3600,
                    TimeforCloseBusiness = 7200,
                    WalletPassword = "wallet123"
                },
                new MerchantDB
                {
                    MerchantId = 3,
                    MerchantName = "B",
                    Password = "123456",
                    MerchantAddress = "测试地址",
                    Contact = "123456789",
                    DishType = "中餐",
                    TimeforOpenBusiness = 3600,
                    TimeforCloseBusiness = 7200,
                    WalletPassword = "wallet123"
                }
            );
            _context.SaveChanges();

            var id = _merchantService.AssignId();
            Assert.AreEqual(2, id);
        }

        // ---------- Tests for AssignDishId ----------

        [Test]
        public void AssignDishId_NoDishes_Returns1()
        {
            var id = _merchantService.AssignDishId();
            Assert.AreEqual(1, id);
        }

        [Test]
        public void AssignDishId_SequentialIds_ReturnsNext()
        {
            _context.Dishes.AddRange(
                new DishDB { DishId = 1, MerchantId = 1, DishName = "A", DishPrice = 10, DishInventory = 10, ImageUrl = "img", DishCategory = "A" },
                new DishDB { DishId = 2, MerchantId = 1, DishName = "B", DishPrice = 12, DishInventory = 5, ImageUrl = "img", DishCategory = "A" }
            );
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            var id = _merchantService.AssignDishId();
            Assert.AreEqual(3, id);
        }

        [Test]
        public void AssignDishId_GapExists_ReturnsGapId()
        {
            _context.Dishes.AddRange(
                new DishDB { DishId = 1, MerchantId = 1, DishName = "A", DishPrice = 10, DishInventory = 10, ImageUrl = "img", DishCategory = "A" },
                new DishDB { DishId = 3, MerchantId = 1, DishName = "B", DishPrice = 12, DishInventory = 5, ImageUrl = "img", DishCategory = "A" }
            );
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            var id = _merchantService.AssignDishId();
            Assert.AreEqual(2, id);
        }

        // ---------- Tests for AssignSpecialOfferId ----------

        [Test]
        public void AssignSpecialOfferId_NoOffers_Returns1()
        {
            var id = _merchantService.AssignSpecialOfferId();
            Assert.AreEqual(1, id);
        }

        [Test]
        public void AssignSpecialOfferId_SequentialIds_ReturnsNext()
        {
            _context.SpecialOffers.AddRange(
                new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 20, AmountRemission = 30 },
                new SpecialOfferDB { OfferId = 2, MerchantId = 1, MinPrice = 10, AmountRemission = 20 }
            );
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            var id = _merchantService.AssignSpecialOfferId();
            Assert.AreEqual(3, id);
        }

        [Test]
        public void AssignSpecialOfferId_GapExists_ReturnsGapId()
        {
            _context.SpecialOffers.AddRange(
                new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 20, AmountRemission = 30 },
                new SpecialOfferDB { OfferId = 3, MerchantId = 1, MinPrice = 10, AmountRemission = 20 }
            );
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            var id = _merchantService.AssignSpecialOfferId();
            Assert.AreEqual(2, id);
        }
    }
}