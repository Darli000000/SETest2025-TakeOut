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

namespace takeout_tj.Tests
{
    [TestFixture]
    public class MerchantBasicInfoTests
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

            _context.Database.OpenConnection();  // **这是第33行 
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
        public void InitMerchant_ShouldReturnOk_WhenValidDto()
        {
            // Arrange
            var dto = new MerchantDBDto
            {
                Password = "123456",
                MerchantName = "测试商家",
                MerchantAddress = "测试地址",
                Contact = "123456789",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                WalletPassword = "wallet123"
            };



            // Act
            var result = _controller.InitMerchant(dto) as OkObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var data = doc.RootElement.GetProperty("data").GetInt32();
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual("注册成功", msg);
            Assert.IsTrue(data > 0);
        }

        [Test]
        public void InitMerchant_ShouldReturnError_WhenSaveFails()
        {
            // Arrange
            _context.Dispose(); // 强制制造异常
            var dto = new MerchantDBDto
            {
                Password = "123456",
                MerchantName = "错误商家",
                MerchantAddress = "错误地址",
                Contact = "000",
                DishType = "类型",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                WalletPassword = "123"
            };

            // Act
            var result = _controller.InitMerchant(dto) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("创建异常", msg);
        }

        [Test]
        public void Login_ShouldReturnOk_WhenCredentialsCorrect()
        {
            // Arrange: 插入一个商家
            var merchant = new MerchantDB
            {
                MerchantId = 123,
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

            var dto = new MerchantDBDto
            {
                MerchantId = 123,
                Password = "pwd123"
            };

            // Act
            var result = _controller.Login(dto) as ObjectResult;

            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("ok", msg);
        }

        [Test]
        public void Login_ShouldReturnError_WhenCredentialsWrong()
        {
            // Arrange: 插入一个商家
            var merchant = new MerchantDB
            {
                MerchantId = 456,
                Password = "correct_pwd",
                MerchantName = "错误密码测试商家",
                MerchantAddress = "地址",
                Contact = "111",
                DishType = "烧烤",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 36000,
                Wallet = 0.00m,
                WalletPassword = "wallet"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var dto = new MerchantDBDto
            {
                MerchantId = 456,
                Password = "wrong_pwd"
            };

            // Act
            var result = _controller.Login(dto) as ObjectResult;

            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            Assert.AreEqual("密码错误", msg);
            //Assert.That(msg, Is.EqualTo("密码错误"), "登录失败信息应该为 '密码错误'");
        }

        [Test]
        public void GetMerchantInfo_ShouldReturnSuccess_WhenMerchantExists()
        {
            // Arrange
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家",
                MerchantAddress = "测试地址",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "walletpwd"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            // Act
            var result = _controller.GetMerchantInfo(merchant.MerchantId) as OkObjectResult;
            var json = JsonSerializer.Serialize(result?.Value);
            var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            var name = data.GetProperty("MerchantName").GetString();            
            var msg = doc.RootElement.GetProperty("msg").GetString();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("测试商家", name);
            Assert.AreEqual("获取成功", msg);
        }

        [Test]
        public void GetMerchantInfo_ShouldReturnNotFound_WhenMerchantMissing()
        {
            // Act
            var result = _controller.GetMerchantInfo(9999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            Assert.AreEqual("商户未找到", msg);
        }

        [Test]
        public void GetMerchantAddress_ShouldReturnAddress_WhenMerchantExists()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家",
                MerchantAddress = "地址1",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "walletpwd",
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var address = _controller.GetType()
                .GetMethod("GetMerchantAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_controller, new object[] { merchant.MerchantId }) as string;


            Assert.AreEqual("地址1", address);
        }

        [Test]
        public void GetMerchantAddress_ShouldReturnNull_WhenMerchantMissing()
        {
            var address = _controller.GetType()
                .GetMethod("GetMerchantAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_controller, new object[] { 9999 });

            Assert.IsNull(address);
        }

        [Test]
        public void EditMerchant_ShouldReturnOk_WhenUpdateSuccess()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家",
                MerchantAddress = "测试地址",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "wp"
            };
            //var merchant = new MerchantDB { MerchantName = "原名", WalletPassword = "wp" };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var dto = new MerchantDBDto
            {
                MerchantId = 1, // 已知分配的Id是1
                Password = "pwd",
                MerchantName = "新测试商家",
                MerchantAddress = "新测试地址",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "newwp"
            };

            var result = _controller.EditMerchant(dto) as OkObjectResult;

            Console.WriteLine("result:" + result);

            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("用户信息更新成功", msg);
        }

        [Test]
        public void EditMerchant_ShouldReturnNotFound_WhenMerchantMissing()
        {
            var dto = new MerchantDBDto { MerchantId = 9999 };

            var result = _controller.EditMerchant(dto) as NotFoundObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("用户未找到", msg);
        }

        [Test]
        public void EditMerchant_ShouldReturnServerError_WhenExceptionThrown()
        {
            // 使用 SQLite in-memory context
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            // 添加测试数据
            context.Merchants.Add(new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd",
                MerchantName = "商家",
                MerchantAddress = "地址",
                Contact = "123456",
                CouponType = 0,
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                WalletPassword = "wallet"
            });
            context.SaveChanges();

            var controller = new MerchantController(context, new MerchantService(context), "uploads");

            // 模拟 context 抛异常：将 context.Dispose 后再访问（会出异常）
            context.Dispose();

            var dto = new MerchantDBDto
            {
                MerchantId = 1,
                Password = "newpwd",
                MerchantName = "新商家"
            };

            var result = controller.EditMerchant(dto) as ObjectResult;

            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("更新异常", msg);
        }


        /* 和wallet相关的测试(用例范围外，可忽略) */
        [Test]
        public void WalletRecharge_ShouldReturnNotFound_WhenMerchantMissing()
        {
            var result = _controller.WalletRecharge(999, 100) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            StringAssert.Contains("未找到", msg);
        }

        [Test]
        public void WalletRecharge_ShouldReturnOk_WhenAddMoneyIsZero()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家",
                MerchantAddress = "测试地址",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 50,
                WalletPassword = "wp"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var result = _controller.WalletRecharge(1, 0) as ObjectResult;  // 已知商家Id为1
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("更新成功", msg);
            Assert.AreEqual(50, data);
        }

        [Test]
        public void WalletRecharge_ShouldReturnOk_WhenRechargeSuccess()
        {
            var merchant1 = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家1",
                MerchantAddress = "测试地址1",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 50,
                WalletPassword = "wp"
            };
            var merchant2 = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家2",
                MerchantAddress = "测试地址2",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "wp"
            };
            _context.Merchants.Add(merchant1);
            _context.Merchants.Add(merchant2);
            _context.SaveChanges();


            // 为商家2充钱50元，应返回150元
            var result = _controller.WalletRecharge(2, 50) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("更新成功", msg);
            Assert.AreEqual(150, data);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnNotFound_WhenMerchantMissing()
        {
            var result = _controller.WalletWithdraw(999, 100) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            StringAssert.Contains("未找到", msg);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnOk_WhenWithdrawZero()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家1",
                MerchantAddress = "测试地址1",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 200,
                WalletPassword = "wp"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var result = _controller.WalletWithdraw(1, 0) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("更新成功", msg);
            Assert.AreEqual(200, data);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnOk_WhenWithdrawSuccess()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家1",
                MerchantAddress = "测试地址1",
                Contact = "123456",
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 300,
                WalletPassword = "wp"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var result = _controller.WalletWithdraw(1, 100) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("更新成功", msg);
            Assert.AreEqual(200, data);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnServerError_WhenExceptionOccurs()
        {
            // Arrange
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "测试商家",
                MerchantAddress = "测试地址",
                Contact = "123456",
                CouponType = 0,
                DishType = "中餐",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "walletpwd"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            // 模拟 context 抛异常：将 context.Dispose 后再访问（会出异常）
            _context.Dispose();

            // Act
            var result = _controller.WalletWithdraw(1, 50) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("提现异常", msg);
        }

    }
}