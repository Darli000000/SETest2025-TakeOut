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
    public class MerchantDishTests
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
        public void GetDishesByMerchantId_ShouldReturnSuccess_WhenDishesExist()
        {
            // Arrange: 插入一个商家和菜品
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "登录测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            
            var dish = new DishDB 
            {
                MerchantId = 1,
                DishId = 1, 
                DishName = "测试菜品", 
                DishPrice = 10, 
                DishCategory = "主菜", 
                ImageUrl = "test.jpg", 
                DishInventory = 100 
            };
            _context.Merchants.Add(merchant);
            _context.Dishes.Add(dish);
            _context.SaveChanges();

            var result = _controller.GetDishesByMerchantId(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("获取成功", msg);
        }

        [Test]
        public void GetDishesByMerchantId_ShouldReturnNotFound_WhenNoDishes()
        {
            var result = _controller.GetDishesByMerchantId(99) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("未找到任何菜品", msg);
        }

        [Test]
        public void DeleteDish_ShouldReturnSuccess_WhenDishDeleted()
        {
            //Directory.CreateDirectory("test_uploads");
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "登录测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            var dish = new DishDB 
            { 
                MerchantId = 1,
                DishId = 1, 
                DishName = "测试菜品", 
                DishPrice = 10, 
                DishCategory = "主菜", 
                ImageUrl = "test_uploads/test.jpg", 
                DishInventory = 100 
            };
            _context.Merchants.Add(merchant);
            _context.Dishes.Add(dish);
            _context.SaveChanges();
            //File.WriteAllText("test_uploads/test.jpg", "dummy image");

            var result = _controller.DeleteDish(1, 1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("菜品删除成功", msg);
        }

        [Test]
        public void DeleteDish_ShouldReturnNotFound_WhenDishDoesNotExist()
        {
            var result = _controller.DeleteDish(1, 99) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("菜品未找到", msg);
        }

        [Test]
        public void DeleteDish_ShouldReturnException_WhenThrows()
        {
            
            // 模拟 context 抛异常：将 context.Dispose 后再访问（会出异常）
            _context.Dispose();

            var result = _controller.DeleteDish(1, 1) as ObjectResult;  
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("删除异常", msg);
        }

        [Test]
        public void EditDish_ShouldReturnDishNotFound_WhenDishDoesNotExist()
        {
            var dto = new DishDBDto { DishId = 1, MerchantId = 1 };
            var result = _controller.EditDish(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            StringAssert.Contains("菜品未找到", msg);
        }

        [Test]
        public void EditDish_ShouldUpdateSuccessfully()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "登录测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "面食",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            var dish = new DishDB
            {
                MerchantId = 1,
                DishId = 1,
                DishName = "Old Name",
                DishPrice = 10,
                DishCategory = "A",
                ImageUrl = "old.png",
                DishInventory = 5
            };
            _context.Merchants.Add(merchant);
            _context.Dishes.Add(dish);
            _context.SaveChanges();

            var dto = new DishDBDto
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "New Name",
                DishPrice = 20,
                DishCategory = "B",
                ImageUrl = "new.png",
                DishInventory = 10
            };

            var result = _controller.EditDish(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = false
            });
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("菜品更新成功", msg);
        }

        [Test]
        public void CreateDish_ShouldReturnBadRequest_WhenDishDtoIsNull()
        {
            var result = _controller.CreateDish(null) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            StringAssert.Contains("请求数据无效", msg);
        }

        [Test]
        public void CreateDish_ShouldSucceed()
        {
            var merchant = new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd123",
                MerchantName = "登录测试商家",
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

            var dto = new DishDBDto
            {
                MerchantId = 1,
                DishName = "Test Dish",
                DishPrice = 15,
                DishCategory = "Test",
                ImageUrl = "img.png",
                DishInventory = 100
            };

            var result = _controller.CreateDish(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("菜品创建成功", msg);
        }

        [Test]
        public async Task UploadImage_ShouldReturnBadRequest_WhenFileIsNull()
        {
            var result = await _controller.UploadImage(null) as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("No file uploaded.", result.Value);
        }

        [Test]
        public async Task UploadImage_ShouldSucceed()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            var result = await _controller.UploadImage(file) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var url = doc.RootElement.GetProperty("url").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("http://localhost:5079/uploads/", url);
        }

        [Test]
        public void GetDishInfo_DishExists_ReturnsOkWithDish()
        {
            // Arrange
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
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜品",
                DishPrice = 10.0m,
                DishCategory = "主食",
                DishInventory = 10,
                ImageUrl = "test-url"
            };
            _context.Dishes.Add(dish);
            _context.SaveChanges();

            // Act
            var result = _controller.getDishInfo(1, 1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = false
            });
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("查找成功", msg);
        }

        [Test]
        public void GetDishInfo_DishNotExists_ReturnsNotFound()
        {
            // 没有插入任何菜品

            // Act
            var result = _controller.getDishInfo(1, 999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            StringAssert.Contains("未找到相关菜品", msg);
        }

        [Test]
        public void GetDishInfo_DbThrowsException_ReturnsError()
        {
            // Arrange：使用一个已释放的 context 模拟异常（或可使用 mock 抛出异常）
            _context.Dispose(); // 手动销毁 context

            // Act
            var result = _controller.getDishInfo(1, 1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("查询异常", msg);
        }
    }
}