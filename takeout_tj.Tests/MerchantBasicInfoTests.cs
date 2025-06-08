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

            _context.Database.OpenConnection();  // **���ǵ�33�� 
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
                    // ���� Database ����������Ƿ��ѱ��ͷ�
                    var _ = _context.Database;

                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // �ѱ��ͷţ������ٴ���
                }
            }

            _connection?.Dispose();  // �ͷ��ڴ����ݿ�����
        }

        [Test]
        public void InitMerchant_ShouldReturnOk_WhenValidDto()
        {
            // Arrange
            var dto = new MerchantDBDto
            {
                Password = "123456",
                MerchantName = "�����̼�",
                MerchantAddress = "���Ե�ַ",
                Contact = "123456789",
                DishType = "�в�",
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
            Assert.AreEqual("ע��ɹ�", msg);
            Assert.IsTrue(data > 0);
        }

        [Test]
        public void InitMerchant_ShouldReturnError_WhenSaveFails()
        {
            // Arrange
            _context.Dispose(); // ǿ�������쳣
            var dto = new MerchantDBDto
            {
                Password = "123456",
                MerchantName = "�����̼�",
                MerchantAddress = "�����ַ",
                Contact = "000",
                DishType = "����",
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
            StringAssert.Contains("�����쳣", msg);
        }

        [Test]
        public void Login_ShouldReturnOk_WhenCredentialsCorrect()
        {
            // Arrange: ����һ���̼�
            var merchant = new MerchantDB
            {
                MerchantId = 123,
                Password = "pwd123",
                MerchantName = "��¼�����̼�",
                MerchantAddress = "��ַ",
                Contact = "111",
                DishType = "��ʳ",
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
            // Arrange: ����һ���̼�
            var merchant = new MerchantDB
            {
                MerchantId = 456,
                Password = "correct_pwd",
                MerchantName = "������������̼�",
                MerchantAddress = "��ַ",
                Contact = "111",
                DishType = "�տ�",
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
            Assert.AreEqual("�������", msg);
            //Assert.That(msg, Is.EqualTo("�������"), "��¼ʧ����ϢӦ��Ϊ '�������'");
        }

        [Test]
        public void GetMerchantInfo_ShouldReturnSuccess_WhenMerchantExists()
        {
            // Arrange
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�",
                MerchantAddress = "���Ե�ַ",
                Contact = "123456",
                DishType = "�в�",
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
            Assert.AreEqual("�����̼�", name);
            Assert.AreEqual("��ȡ�ɹ�", msg);
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
            Assert.AreEqual("�̻�δ�ҵ�", msg);
        }

        [Test]
        public void GetMerchantAddress_ShouldReturnAddress_WhenMerchantExists()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�",
                MerchantAddress = "��ַ1",
                Contact = "123456",
                DishType = "�в�",
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


            Assert.AreEqual("��ַ1", address);
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
                MerchantName = "�����̼�",
                MerchantAddress = "���Ե�ַ",
                Contact = "123456",
                DishType = "�в�",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "wp"
            };
            //var merchant = new MerchantDB { MerchantName = "ԭ��", WalletPassword = "wp" };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var dto = new MerchantDBDto
            {
                MerchantId = 1, // ��֪�����Id��1
                Password = "pwd",
                MerchantName = "�²����̼�",
                MerchantAddress = "�²��Ե�ַ",
                Contact = "123456",
                DishType = "�в�",
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
            Assert.AreEqual("�û���Ϣ���³ɹ�", msg);
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
            Assert.AreEqual("�û�δ�ҵ�", msg);
        }

        [Test]
        public void EditMerchant_ShouldReturnServerError_WhenExceptionThrown()
        {
            // ʹ�� SQLite in-memory context
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            // ��Ӳ�������
            context.Merchants.Add(new MerchantDB
            {
                MerchantId = 1,
                Password = "pwd",
                MerchantName = "�̼�",
                MerchantAddress = "��ַ",
                Contact = "123456",
                CouponType = 0,
                DishType = "�в�",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                WalletPassword = "wallet"
            });
            context.SaveChanges();

            var controller = new MerchantController(context, new MerchantService(context), "uploads");

            // ģ�� context ���쳣���� context.Dispose ���ٷ��ʣ�����쳣��
            context.Dispose();

            var dto = new MerchantDBDto
            {
                MerchantId = 1,
                Password = "newpwd",
                MerchantName = "���̼�"
            };

            var result = controller.EditMerchant(dto) as ObjectResult;

            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("�����쳣", msg);
        }


        /* ��wallet��صĲ���(������Χ�⣬�ɺ���) */
        [Test]
        public void WalletRecharge_ShouldReturnNotFound_WhenMerchantMissing()
        {
            var result = _controller.WalletRecharge(999, 100) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            StringAssert.Contains("δ�ҵ�", msg);
        }

        [Test]
        public void WalletRecharge_ShouldReturnOk_WhenAddMoneyIsZero()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�",
                MerchantAddress = "���Ե�ַ",
                Contact = "123456",
                DishType = "�в�",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 50,
                WalletPassword = "wp"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            var result = _controller.WalletRecharge(1, 0) as ObjectResult;  // ��֪�̼�IdΪ1
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("���³ɹ�", msg);
            Assert.AreEqual(50, data);
        }

        [Test]
        public void WalletRecharge_ShouldReturnOk_WhenRechargeSuccess()
        {
            var merchant1 = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�1",
                MerchantAddress = "���Ե�ַ1",
                Contact = "123456",
                DishType = "�в�",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 50,
                WalletPassword = "wp"
            };
            var merchant2 = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�2",
                MerchantAddress = "���Ե�ַ2",
                Contact = "123456",
                DishType = "�в�",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "wp"
            };
            _context.Merchants.Add(merchant1);
            _context.Merchants.Add(merchant2);
            _context.SaveChanges();


            // Ϊ�̼�2��Ǯ50Ԫ��Ӧ����150Ԫ
            var result = _controller.WalletRecharge(2, 50) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            StringAssert.Contains("���³ɹ�", msg);
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
            StringAssert.Contains("δ�ҵ�", msg);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnOk_WhenWithdrawZero()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�1",
                MerchantAddress = "���Ե�ַ1",
                Contact = "123456",
                DishType = "�в�",
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
            StringAssert.Contains("���³ɹ�", msg);
            Assert.AreEqual(200, data);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnOk_WhenWithdrawSuccess()
        {
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�1",
                MerchantAddress = "���Ե�ַ1",
                Contact = "123456",
                DishType = "�в�",
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
            StringAssert.Contains("���³ɹ�", msg);
            Assert.AreEqual(200, data);
        }

        [Test]
        public void WalletWithdraw_ShouldReturnServerError_WhenExceptionOccurs()
        {
            // Arrange
            var merchant = new MerchantDB
            {
                Password = "pwd",
                MerchantName = "�����̼�",
                MerchantAddress = "���Ե�ַ",
                Contact = "123456",
                CouponType = 0,
                DishType = "�в�",
                TimeforOpenBusiness = 3600,
                TimeforCloseBusiness = 7200,
                Wallet = 100,
                WalletPassword = "walletpwd"
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            // ģ�� context ���쳣���� context.Dispose ���ٷ��ʣ�����쳣��
            _context.Dispose();

            // Act
            var result = _controller.WalletWithdraw(1, 50) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(30000, result.StatusCode);
            StringAssert.Contains("�����쳣", msg);
        }

    }
}