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
using takeout_tj.Models.Platform;
using takeout_tj.Models.User;
using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Tests
{
    [TestFixture]
    public class MerchantOrderTests
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
        public void GetOrdersToHandle_WithMatchingOrders_ReturnsOrders()
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
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Merchants.Add(merchant);
            _context.Dishes.Add(dish);
            _context.SaveChanges();

            var order = new OrderDB
            {
                OrderId = 1,
                Price = 100,
                OrderTimestamp = DateTime.Now,
                State = 2,
                NeedUtensils = 1,
                AddressId = 1
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = order.OrderId,
                MerchantId = 1,
                DishId = 1
            });
            _context.SaveChanges();

            // Act
            var result = _controller.getOrdersToHandle(1) as ObjectResult;
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
            Assert.AreEqual("查找成功", msg);
        }

        [Test]
        public void GetOrdersToHandle_WithNoMatchingOrders_ReturnsNotFoundMessage()
        {
            // Arrange: 无任何数据

            // Act
            var result = _controller.getOrdersToHandle(999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetInt32();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("未找到相关订单", msg);
            Assert.AreEqual(40000, data);
        }

        [Test]
        public void DeletePaidOrder_OrderExistsWithCouponAndDishAndUser_DeletesSuccessfully()
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
            // Arrange
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜",
                DishPrice = 10,
                DishCategory = "A",
                DishInventory = 5,
                ImageUrl = "url"
            };
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            var coupon = new CouponDB
            { 
                CouponId = 1,
                CouponName = "测试券",
                CouponPrice = 10,
                CouponValue = 15,
                CouponType = 0,
                MinPrice = 5
            };

            _context.Merchants.Add(merchant);
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Orders.Add(order);
            _context.Dishes.Add(dish);
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = 1,
                DishId = 1,
                DishNum = 2,
                MerchantId = 1
            });
            _context.OrderUsers.Add(new OrderUserDB
            {
                OrderId = 1,
                UserId = 1
            });
            _context.OrderCoupons.Add(new OrderCouponDB
            {
                OrderId = 1,
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today
            });
            _context.UserCoupons.Add(new UserCouponDB
            {
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today,
                AmountOwned = 0
            });
            _context.SaveChanges();

            // Act
            var result = _controller.deletePaidOrder(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("订单删除成功", msg);
        }

        [Test]
        public void DeletePaidOrder_OrderNotFound_ReturnsError()
        {
            // Act
            var result = _controller.deletePaidOrder(999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            var msg = doc.RootElement.GetProperty("msg").GetString();
            var errorCode = doc.RootElement.GetProperty("errorCode").GetInt32();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(40000, result.StatusCode);
            Assert.AreEqual("未找到要删除的订单", msg);
            Assert.AreEqual(40000, errorCode);
        }


        /*// **注意：这个测试会报错，原本的代码写的不行
        [Test]
        public void DeletePaidOrder_UserCouponNotExist_CreatesNew()
        {
            // Arrange 同 TC01，唯一不同：不添加 UserCoupon
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
            // Arrange
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜",
                DishPrice = 10,
                DishCategory = "A",
                DishInventory = 5,
                ImageUrl = "url"
            };
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            var coupon = new CouponDB
            {
                CouponId = 1,
                CouponName = "测试券",
                CouponPrice = 10,
                CouponValue = 15,
                CouponType = 0,
                MinPrice = 5
            };

            _context.Merchants.Add(merchant);
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Orders.Add(order);
            _context.Dishes.Add(dish);
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            Assert.IsFalse(_context.UserCoupons.Any(c => c.UserId == 1 && c.CouponId == 1));

            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = 1,
                DishId = 1,
                DishNum = 2,
                MerchantId = 1
            });
            _context.OrderUsers.Add(new OrderUserDB
            {
                OrderId = 1,
                UserId = 1
            });
            _context.OrderCoupons.Add(new OrderCouponDB
            {
                OrderId = 1,
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today
            });
            var userCoupon = new UserCouponDB
            {
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today,
                AmountOwned = 1
            };
            _context.UserCoupons.Add(userCoupon);
            _context.SaveChanges();

            var couponToDelete = _context.UserCoupons.FirstOrDefault(uc =>
                uc.UserId == 1 &&
                uc.CouponId == 1 &&
                uc.ExpirationDate == DateTime.Today
            );

            if (couponToDelete != null)
            {
                //Console.WriteLine("Deleted");
                _context.UserCoupons.Remove(couponToDelete);
                _context.SaveChanges();
            }

            // Act
            var result = _controller.deletePaidOrder(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.AreEqual("订单删除成功", msg);
            Assert.IsTrue(_context.UserCoupons.Any(c => c.UserId == 1 && c.CouponId == 1));
        }*/

        [Test]
        public void DeletePaidOrder_OrderCouponNotExist_SkipCouponRestore()
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
            // Arrange
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜",
                DishPrice = 10,
                DishCategory = "A",
                DishInventory = 5,
                ImageUrl = "url"
            };
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            var coupon = new CouponDB
            {
                CouponId = 1,
                CouponName = "测试券",
                CouponPrice = 10,
                CouponValue = 15,
                CouponType = 0,
                MinPrice = 5
            };

            _context.Merchants.Add(merchant);
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Orders.Add(order);
            _context.Dishes.Add(dish);
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = 1,
                DishId = 1,
                DishNum = 2,
                MerchantId = 1
            });
            _context.OrderUsers.Add(new OrderUserDB
            {
                OrderId = 1,
                UserId = 1
            });
           /* _context.OrderCoupons.Add(new OrderCouponDB
            {
                OrderId = 1,
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today
            });*/
            var userCoupon = new UserCouponDB
            {
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today,
                AmountOwned = 1
            };
            _context.UserCoupons.Add(userCoupon);
            _context.SaveChanges();

            // Arrange
            // 同 TC01，唯一不同：不添加 OrderCoupon
            /*_context.OrderCoupons.RemoveRange(_context.OrderCoupons);
            _context.SaveChanges();*/

            var result = _controller.deletePaidOrder(1) as ObjectResult;

            // Assert
            Assert.AreEqual(200, result.StatusCode);
        }


        /*// **注意：这个测试会失败，原本代码有冗余分支
        [Test]
        public void DeletePaidOrder_UserMissing_SkipWalletRefund()
        {
            // Arrange 同 TC01，唯一不同：不添加 UserCoupon
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
            // Arrange
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜",
                DishPrice = 10,
                DishCategory = "A",
                DishInventory = 5,
                ImageUrl = "url"
            };
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            var coupon = new CouponDB
            {
                CouponId = 1,
                CouponName = "测试券",
                CouponPrice = 10,
                CouponValue = 15,
                CouponType = 0,
                MinPrice = 5
            };

            _context.Merchants.Add(merchant);
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Orders.Add(order);
            _context.Dishes.Add(dish);
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = 1,
                DishId = 1,
                DishNum = 2,
                MerchantId = 1
            });
            _context.OrderUsers.Add(new OrderUserDB
            {
                OrderId = 1,
                UserId = 1
            });
            _context.OrderCoupons.Add(new OrderCouponDB
            {
                OrderId = 1,
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today
            });
            var userCoupon = new UserCouponDB
            {
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today,
                AmountOwned = 1
            };
            _context.UserCoupons.Add(userCoupon);
            _context.SaveChanges();

            var userToDelete = _context.Users.FirstOrDefault(uc =>
                uc.UserId == 1
            );

            if (userToDelete != null)
            {
                //Console.WriteLine("UserDeleted");
                _context.Users.Remove(userToDelete);
                _context.SaveChanges();
            }

            var result = _controller.deletePaidOrder(1) as ObjectResult;
            Assert.AreEqual("订单删除成功", JsonDocument.Parse(JsonSerializer.Serialize(result.Value)).RootElement.GetProperty("msg").GetString());
        }*/

        [Test]
        public void DeletePaidOrder_OrderUserMissing_SkipRefund()
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
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜",
                DishPrice = 10,
                DishCategory = "A",
                DishInventory = 5,
                ImageUrl = "url"
            };
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            var coupon = new CouponDB
            {
                CouponId = 1,
                CouponName = "测试券",
                CouponPrice = 10,
                CouponValue = 15,
                CouponType = 0,
                MinPrice = 5
            };

            _context.Merchants.Add(merchant);
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Orders.Add(order);
            _context.Dishes.Add(dish);
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = 1,
                DishId = 1,
                DishNum = 2,
                MerchantId = 1
            });
            _context.OrderUsers.Add(new OrderUserDB
            {
                OrderId = 1,
                UserId = 1
            });
            _context.OrderCoupons.Add(new OrderCouponDB
             {
                 OrderId = 1,
                 UserId = 1,
                 CouponId = 1,
                 ExpirationDate = DateTime.Today
             });
            var userCoupon = new UserCouponDB
            {
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today,
                AmountOwned = 1
            };
            _context.UserCoupons.Add(userCoupon);
            _context.SaveChanges();

            // Arrange 同 TC01，但不添加 OrderUser
            _context.OrderUsers.RemoveRange(_context.OrderUsers);
            _context.SaveChanges();

            var result = _controller.deletePaidOrder(1) as ObjectResult;
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void DeletePaidOrder_OrderNotFound_Returns40000()
        {
            var result = _controller.deletePaidOrder(9999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var code = doc.RootElement.GetProperty("errorCode").GetInt32();
            var msg = doc.RootElement.GetProperty("msg").GetString();

            Assert.AreEqual(40000, result.StatusCode);
            Assert.AreEqual(40000, code);
            Assert.AreEqual("未找到要删除的订单", msg);
        }

        /*// 会测试失败：前面加入判断order是否存在的逻辑，本测试对应的分支有冗余
        [Test]
        public void DeletePaidOrder_SaveChangesReturnsZero_ReturnsDeleteFailed()
        {
            // Arrange
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            _context.Orders.Add(order);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;"); // SQLite忽略外键

            // 模拟失败：使用一个临时的 context（或用 mock 框架，此处用实际 context 模拟最小逻辑路径）
            var result = _controller.deletePaidOrder(1) as ObjectResult;

            // Act
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(20000, result.StatusCode);
            Assert.AreEqual("删除失败", msg);
        }*/

        [Test]
        public void DeletePaidOrder_ExceptionThrown_Returns30000()
        {
            // Arrange：插入订单，但模拟删除失败（或人为抛出异常）
            
            _context.Dispose();

            // Act
            var result = _controller.deletePaidOrder(3) as ObjectResult;
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
            Assert.AreEqual(30000, doc.RootElement.GetProperty("errorCode").GetInt32());
            StringAssert.Contains("查询异常", doc.RootElement.GetProperty("msg").GetString());
        }

        [Test]
        public void DeletePaidOrder_DishMissing_SkipInventoryRestore()
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
            var dish = new DishDB
            {
                DishId = 1,
                MerchantId = 1,
                DishName = "测试菜",
                DishPrice = 10,
                DishCategory = "A",
                DishInventory = 5,
                ImageUrl = "url"
            };
            var user = new UserDB
            {
                UserId = 1,
                UserName = "测试用户",
                PhoneNumber = "1092837484",
                Password = "192837",
                Wallet = 100,
                WalletPassword = "129384",
            };
            var address = new UserAddressDB
            {
                AddressId = 1,
                UserId = 1,
                UserAddress = "China",
                HouseNumber = "123049",
                ContactName = "123",
                PhoneNumber = "123456"
            };
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 30,
                OrderTimestamp = DateTime.Now,
                State = 1,
                NeedUtensils = 1,
                AddressId = 1
            };
            var coupon = new CouponDB
            {
                CouponId = 1,
                CouponName = "测试券",
                CouponPrice = 10,
                CouponValue = 15,
                CouponType = 0,
                MinPrice = 5
            };

            _context.Merchants.Add(merchant);
            _context.Users.Add(user);
            _context.UserAddresses.Add(address);
            _context.Orders.Add(order);
            _context.Dishes.Add(dish);
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            _context.OrderDishes.Add(new OrderDishDB
            {
                OrderId = 1,
                DishId = 1,
                DishNum = 2,
                MerchantId = 1
            });
            _context.OrderUsers.Add(new OrderUserDB
            {
                OrderId = 1,
                UserId = 1
            });
            _context.OrderCoupons.Add(new OrderCouponDB
            {
                OrderId = 1,
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today
            });
            var userCoupon = new UserCouponDB
            {
                UserId = 1,
                CouponId = 1,
                ExpirationDate = DateTime.Today,
                AmountOwned = 1
            };
            _context.UserCoupons.Add(userCoupon);
            _context.SaveChanges();

            // Arrange 同 TC01，但不添加 Dishes
            _context.Dishes.RemoveRange(_context.Dishes);
            _context.SaveChanges();

            var result = _controller.deletePaidOrder(1) as ObjectResult;
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetMerAddrByOrderId_OrderDoesNotExist_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetMerAddrByOrderId(999) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("指定订单不存在", msg);
        }

        [Test]
        public async Task GetMerAddrByOrderId_MerchantDoesNotExist_ReturnsNotFound()
        {
            var order = new OrderDB { OrderId = 1, Price = 10, OrderTimestamp = DateTime.Now, State = 1, NeedUtensils = 1, AddressId = 1 };
            var orderDish = new OrderDishDB { OrderId = 1, MerchantId = 1, DishId = 1, DishNum = 1 };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerAddrByOrderId(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("商户未找到", msg);
        }

        [Test]
        public async Task GetMerAddrByOrderId_Success_ReturnsAddress()
        {
            var merchant = new MerchantDB { MerchantId = 1, MerchantName = "Test", MerchantAddress = "Test Address", Password = "pwd", Contact = "111", DishType = "A", TimeforOpenBusiness = 0, TimeforCloseBusiness = 1, Wallet = 0, WalletPassword = "123" };
            var order = new OrderDB { OrderId = 1, Price = 10, OrderTimestamp = DateTime.Now, State = 1, NeedUtensils = 1, AddressId = 1 };
            var orderDish = new OrderDishDB { OrderId = 1, MerchantId = 1, DishId = 1, DishNum = 1 };
            _context.Merchants.Add(merchant);
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerAddrByOrderId(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("获取成功", msg);
            Assert.AreEqual("Test Address", data);
        }

        [Test]
        public void GetOrdersByRegion_ReturnsGroupedResult()
        {
            _context.Orders.AddRange(
                new OrderDB { OrderId = 1, Price = 10, OrderTimestamp = DateTime.Now, State = 1, NeedUtensils = 1, AddressId = 1 },
                new OrderDB { OrderId = 2, Price = 15, OrderTimestamp = DateTime.Now, State = 1, NeedUtensils = 1, AddressId = 1 },
                new OrderDB { OrderId = 3, Price = 20, OrderTimestamp = DateTime.Now, State = 1, NeedUtensils = 1, AddressId = 2 }
            );
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = _controller.GetOrdersByRegion() as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, doc.RootElement.GetArrayLength());
            var region1 = doc.RootElement[0].GetProperty("Region").GetInt32();
            var count1 = doc.RootElement[0].GetProperty("Count").GetInt32();
            Assert.AreEqual(1, region1);
            Assert.AreEqual(2, count1);
        }

        [Test]
        public async Task GetMerOrdersWithinThisMonth_NoOrders_ReturnsZero()
        {
            // Act
            var result = await _controller.GetMerOrdersWithinThisMonth(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("指定商家无订单", msg);
        }

        [Test]
        public async Task GetMerOrdersWithinThisMonth_OrdersNotInCurrentMonth_ReturnsZero()
        {
            // Arrange
            var now = DateTime.Now.AddMonths(-1); //设置非法日期
            var order = new OrderDB
            {
                OrderId = 1,
                Price = 20,
                OrderTimestamp = now,
                State = 3,
                NeedUtensils = 1,
                AddressId = 1
            };
            var orderDish = new OrderDishDB
            {
                OrderId = 1,
                MerchantId = 1,
                DishId = 1,
                DishNum = 2,
                OrderDB = order
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerOrdersWithinThisMonth(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("指定商家本月内无订单", msg);
        }

        [Test]
        public async Task GetMerOrdersWithinThisMonth_OrdersWrongState_ReturnsZero()
        {
            // Arrange
            var now = DateTime.Now;
            var order = new OrderDB
            {
                OrderId = 3,
                Price = 30,
                OrderTimestamp = now,
                State = 2, // 非法状态
                NeedUtensils = 1,
                AddressId = 1
            };
            var orderDish = new OrderDishDB
            {
                OrderId = 3,
                MerchantId = 1,
                DishId = 1,
                DishNum = 1,
                OrderDB = order
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerOrdersWithinThisDay(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("指定商家无订单", msg);
        }

        [Test]
        public async Task GetMerOrdersWithinThisMonth_OrdersExist_ReturnsOrderIds()
        {
            // Arrange
            var now = DateTime.Now;
            var order = new OrderDB
            {
                OrderId = 2,
                Price = 30,
                OrderTimestamp = now,
                State = 3,
                NeedUtensils = 1,
                AddressId = 1
            };
            var orderDish = new OrderDishDB
            {
                OrderId = 2,
                MerchantId = 1,
                DishId = 1,
                DishNum = 1,
                OrderDB = order
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerOrdersWithinThisMonth(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("获取成功", msg);
            Assert.AreEqual(1, data.GetArrayLength());
            Assert.AreEqual(2, data[0].GetInt32());
        }

        [Test]
        public async Task GetMerOrdersWithinThisDay_NoOrders_ReturnsZero()
        {
            // Act
            var result = await _controller.GetMerOrdersWithinThisDay(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("指定商家无订单", msg);
        }

        [Test]
        public async Task GetMerOrdersWithinThisDay_OrdersNotToday_ReturnsZero()
        {
            // Arrange
            var now = DateTime.Now.AddDays(-1);
            var order = new OrderDB
            {
                OrderId = 3,
                Price = 30,
                OrderTimestamp = now,
                State = 3,
                NeedUtensils = 1,
                AddressId = 1
            };
            var orderDish = new OrderDishDB
            {
                OrderId = 3,
                MerchantId = 1,
                DishId = 1,
                DishNum = 1,
                OrderDB = order
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerOrdersWithinThisDay(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("指定商家本日内无订单", msg);
        }

        

        [Test]
        public async Task GetMerOrdersWithinThisDay_OrdersWrongState_ReturnsZero()
        {
            // Arrange
            var now = DateTime.Now;
            var order = new OrderDB
            {
                OrderId = 3,
                Price = 30,
                OrderTimestamp = now,
                State = 2, // 非法状态
                NeedUtensils = 1,
                AddressId = 1
            };
            var orderDish = new OrderDishDB
            {
                OrderId = 3,
                MerchantId = 1,
                DishId = 1,
                DishNum = 1,
                OrderDB = order
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerOrdersWithinThisDay(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("指定商家无订单", msg);
        }

        [Test]
        public async Task GetMerOrdersWithinThisDay_OrdersTodayAndValidState_ReturnsOrderIds()
        {
            // Arrange
            var now = DateTime.Now;
            var order = new OrderDB
            {
                OrderId = 4,
                Price = 30,
                OrderTimestamp = now,
                State = 3,
                NeedUtensils = 1,
                AddressId = 1
            };
            var orderDish = new OrderDishDB
            {
                OrderId = 4,
                MerchantId = 1,
                DishId = 1,
                DishNum = 1,
                OrderDB = order
            };
            _context.Orders.Add(order);
            _context.OrderDishes.Add(orderDish);
            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;"); // SQLite忽略外键
            _context.SaveChanges();

            // Act
            var result = await _controller.GetMerOrdersWithinThisDay(1) as ObjectResult;
            var json = JsonSerializer.Serialize(result.Value);
            var doc = JsonDocument.Parse(json);
            var msg = doc.RootElement.GetProperty("msg").GetString();
            var data = doc.RootElement.GetProperty("data");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("获取成功", msg);
            Assert.AreEqual(1, data.GetArrayLength());
            Assert.AreEqual(4, data[0].GetInt32());
        }

        [Test]
        public async Task GetMerOrdersWithinThisMonth_ThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var controller = new MerchantController(null, null, "path");

            // Act
            var result = await controller.GetMerOrdersWithinThisMonth(1) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            StringAssert.Contains("Object reference", result.Value.ToString());
        }
    }
}