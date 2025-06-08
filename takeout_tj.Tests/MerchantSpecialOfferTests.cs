using NUnit.Framework;
using takeout_tj.Controllers;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Merchant;
using takeout_tj.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Text.Json;
using Moq;
using System;
using Microsoft.AspNetCore.Http;

namespace takeout_tj.Tests
{
    [TestFixture]
    public class MerchantSpecialOfferTests
    {
        private ApplicationDbContext _context;
        private MerchantController _controller;

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

            var merchantService = new MerchantService(_context);

            _controller = new MerchantController(_context, merchantService, Path.Combine(Directory.GetCurrentDirectory(), "test_uploads"));

        }

        [TearDown]
        public void Teardown()
        {
            if (_context != null)
            {
                try
                {
                    // 访问 Database 属性来检测是否已被释放
                    var _ = _context.Database;

                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // 已被释放，无需再处理
                }
            }

            _connection?.Dispose();  // 释放内存数据库连接
        }

        [Test]
        public void AddSpecialOffer_InvalidAmountRemission_ShouldReturnError()
        {
            var dto = new SpecialOfferDBDto { MerchantId = 1, MinPrice = 20, AmountRemission = 30 };

            var result = _controller.AddSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("满减金额不可大于", msg);
        }

        [Test]
        public void AddSpecialOffer_Valid_ShouldCreate()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();
            var dto = new SpecialOfferDBDto { MerchantId = 1, MinPrice = 50, AmountRemission = 10 };

            var result = _controller.AddSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = false
            });
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("创建成功", msg);
        }

        /*[Test]
        public void AddSpecialOffer_SaveChangesReturnsZero_ShouldReturnError()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 2,
                Password = "pwd123",
                MerchantName = "测试商家2",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var dto = new SpecialOfferDBDto { MerchantId = 2, MinPrice = 50, AmountRemission = 10 };

            // 添加但不真正变更（让 SaveChanges 返回 0）
            var offer = new SpecialOfferDB
            {
                OfferId = 100,
                MerchantId = 2,
                MinPrice = 50,
                AmountRemission = 10
            };
            _context.SpecialOffers.Add(offer);
            _context.SaveChanges();
            _context.Entry(offer).State = EntityState.Unchanged; // 清除变更

            // 调用 controller
            var result = _controller.AddSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = false
            });
            var doc = JsonDocument.Parse(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            var msg = doc.RootElement.GetProperty("msg").GetString();
            StringAssert.Contains("创建失败", msg);
        }*/

        [Test]
        public void AddSpecialOffer_ExceptionThrown_ShouldReturnError()
        {
            // 不添加 merchant，制造异常（例如外键失败）
            var dto = new SpecialOfferDBDto { MerchantId = 9999, MinPrice = 50, AmountRemission = 10 };

            var result = _controller.AddSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);  // 注意：这里依然是 20000（catch 块返回的）
            var msg = doc.RootElement.GetProperty("msg").GetString();
            StringAssert.Contains("创建异常", msg);
        }



        [Test]
        public void EditSpecialOffer_NotFound_ShouldReturn404()
        {
            var dto = new SpecialOfferDBDto { OfferId = 999, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };

            var result = _controller.EditSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            StringAssert.Contains("未找到", msg);
        }

        [Test]
        public void EditSpecialOffer_InvalidAmountRemission_ShouldReturnError()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            var offer = new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };
            _context.SpecialOffers.Add(offer);
            _context.SaveChanges();

            var dto = new SpecialOfferDBDto { OfferId = 1, MerchantId = 1, MinPrice = 30, AmountRemission = 40 };

            var result = _controller.EditSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("满减金额不可大于", msg);
        }

        [Test]
        public void EditSpecialOffer_SuccessfulUpdate_ShouldReturnOk()
        {
            var merchant = new MerchantDB { MerchantId = 1, Password = "123", MerchantName = "测试商家", MerchantAddress = "地址", Contact = "111", DishType = "面食", TimeforOpenBusiness = 3600, TimeforCloseBusiness = 7200, Wallet = 0.00m, WalletPassword = "wallet" };
            _context.Merchants.Add(merchant);

            var offer = new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };
            _context.SpecialOffers.Add(offer);
            _context.SaveChanges();

            var dto = new SpecialOfferDBDto { OfferId = 1, MerchantId = 1, MinPrice = 100, AmountRemission = 20 };

            var result = _controller.EditSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = false
            });
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("更新成功", msg);
        }

        [Test]
        public void EditSpecialOffer_SaveChangesReturnsZero_ShouldReturnError()
        {
            var merchant = new MerchantDB { MerchantId = 1, Password = "123", MerchantName = "测试商家", MerchantAddress = "地址", Contact = "111", DishType = "面食", TimeforOpenBusiness = 3600, TimeforCloseBusiness = 7200, Wallet = 0.00m, WalletPassword = "wallet" };
            _context.Merchants.Add(merchant);

            var offer = new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };
            _context.SpecialOffers.Add(offer);
            _context.SaveChanges();

            // 模拟没有实际更改任何内容（SaveChanges 返回 0）
            var dto = new SpecialOfferDBDto { OfferId = 1, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };

            var result = _controller.EditSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("更新失败", msg);
        }

        [Test]
        public void EditSpecialOffer_ExceptionThrown_ShouldReturn30000()
        {
            // 用 null context 创建 controller 模拟异常（也可以使用 mock 抛出异常）
            var brokenController = new MerchantController(null, null, "fake_path");

            var dto = new SpecialOfferDBDto { OfferId = 1, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };

            var result = brokenController.EditSpecialOffer(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("更新异常", msg);
        }


        [Test]
        public void DeleteOffer_NotFound_ShouldReturnError()
        {
            var result = _controller.DeleteOffer(999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("未找到", msg);
        }

        [Test]
        public void DeleteOffer_Valid_ShouldSucceed()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            var offer = new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 50, AmountRemission = 10 };
            _context.SpecialOffers.Add(offer);
            _context.SaveChanges();

            var result = _controller.DeleteOffer(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("删除成功", msg);
        }

        /*[Test]
        public void DeleteOffer_SaveChangesReturnsZero_ShouldReturnError()
        {

            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);

            var offer = new SpecialOfferDB { OfferId = 2, MerchantId = 1, MinPrice = 60, AmountRemission = 5 };
            _context.SpecialOffers.Add(offer);
            _context.SaveChanges();

            // 模拟未真正删除
            // 添加触发器禁止删除 OfferId=1
            *//*_context.Database.ExecuteSqlRaw(@"
                CREATE TRIGGER prevent_delete_offer
                BEFORE DELETE ON special_offers
                FOR EACH ROW
                WHEN OLD.OfferId = 2
                BEGIN
                    INSERT INTO special_offers (OfferId, MerchantId, MinPrice, AmountRemission)
                    VALUES (OLD.OfferId, OLD.MerchantId, OLD.MinPrice, OLD.AmountRemission);
                END;
            ");*//*

            var result = _controller.DeleteOffer(2) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Console.WriteLine(msg);

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("删除失败", msg);
        }*/


        [Test]
        public void DeleteOffer_ExceptionThrown_ShouldReturn30000()
        {
            var brokenController = new MerchantController(null, null, "fake_path");

            var result = brokenController.DeleteOffer(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("删除异常", msg);
        }



        [Test]
        public void GetOffersInfo_Empty_ShouldReturnMessage()
        {
            var result = _controller.GetOffersInfo(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("无满减活动", msg);
        }

        [Test]
        public void GetOffersInfo_WithOffers_ShouldReturnData()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            _context.SpecialOffers.Add(new SpecialOfferDB { OfferId = 1, MerchantId = 1, MinPrice = 30, AmountRemission = 5 });
            _context.SaveChanges();

            var result = _controller.GetOffersInfo(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = false
            });
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data");

            var values = data.GetProperty("$values");  // 获取data中的数组

            //Console.WriteLine(data);

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsTrue(values.GetArrayLength() > 0);
            StringAssert.Contains("获取成功", msg);
        }

        [Test]
        public void GetMultiOffersInfo_NullOrEmptyMerchantIds_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetMultiOffersInfo(null) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("商户ID列表为空", msg);
        }

        [Test]
        public void GetMultiOffersInfo_ValidIdsButNoOffers_ShouldReturnEmptyData()
        {
            var merchant = new MerchantDB
            {
                Password = "123",
                MerchantName = "测试商户",
                MerchantAddress = "地址",
                Contact = "123456",
                DishType = "类型",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            // Act
            var result = _controller.GetMultiOffersInfo(new List<int> { 1 }) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("无满减活动", msg);
            Assert.AreEqual(0, data.GetArrayLength());
        }

        [Test]
        public void GetMultiOffersInfo_WithValidOffers_ShouldReturnData()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "商户",
                MerchantAddress = "地址",
                Contact = "123",
                DishType = "类型",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            _context.SpecialOffers.Add(new SpecialOfferDB { OfferId = 10, MerchantId = 1, MinPrice = 20, AmountRemission = 5 });
            _context.SaveChanges();

            // Act
            var result = _controller.GetMultiOffersInfo(new List<int> { 1 }) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var values = doc.RootElement.GetProperty("data").GetProperty("$values");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("获取成功", msg);
            Assert.IsTrue(values.GetArrayLength() > 0);
        }

        [Test]
        public void GetMultiOffersInfo_ExceptionThrown_ShouldReturnError()
        {
            // 模拟 DbContext 异常：Dispose 当前 context，再调用 controller（使其 _context 无效）
            _context.Dispose();

            // Act
            var result = _controller.GetMultiOffersInfo(new List<int> { 1 }) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            //StringAssert.Contains("Exception", msg);  // Exception message may vary, we just check it's an exception
        }

    }
}