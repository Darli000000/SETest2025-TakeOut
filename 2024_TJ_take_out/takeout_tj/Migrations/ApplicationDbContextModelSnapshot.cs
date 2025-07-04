﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;
using takeout_tj.Data;

#nullable disable

namespace takeout_tj.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.25")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            OracleModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("takeout_tj.Models.Merchant.DishDB", b =>
                {
                    b.Property<int>("MerchantId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("DishId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("DishCategory")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("DishInventory")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("DishName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("NVARCHAR2(255)");

                    b.Property<decimal>("DishPrice")
                        .HasColumnType("numeric(10,2)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("MerchantId", "DishId");

                    b.ToTable("dishes", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.MerchantDB", b =>
                {
                    b.Property<int>("MerchantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MerchantId"), 1L, 1);

                    b.Property<string>("Contact")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("NVARCHAR2(255)");

                    b.Property<int>("CouponType")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("DishType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR2(20)");

                    b.Property<string>("MerchantAddress")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("NVARCHAR2(255)");

                    b.Property<string>("MerchantName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("NVARCHAR2(255)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR2(20)");

                    b.Property<int>("TimeforCloseBusiness")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("TimeforOpenBusiness")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("Wallet")
                        .HasColumnType("numeric(10,2)");

                    b.Property<string>("WalletPassword")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("MerchantId");

                    b.ToTable("merchants", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.MerchantStationDB", b =>
                {
                    b.Property<int>("MerchantId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("StationId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("MerchantId");

                    b.HasIndex("StationId");

                    b.ToTable("merchant_stations", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.SpecialOfferDB", b =>
                {
                    b.Property<int>("MerchantId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("OfferId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("AmountRemission")
                        .HasColumnType("numeric(10,2)");

                    b.Property<decimal>("MinPrice")
                        .HasColumnType("numeric(10,2)");

                    b.HasKey("MerchantId", "OfferId");

                    b.ToTable("special_offers", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.AdminDB", b =>
                {
                    b.Property<int>("AdminId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AdminId"), 1L, 1);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR2(20)");

                    b.HasKey("AdminId");

                    b.ToTable("admins", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.CouponDB", b =>
                {
                    b.Property<int>("CouponId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CouponId"), 1L, 1);

                    b.Property<string>("CouponName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("NVARCHAR2(100)");

                    b.Property<decimal>("CouponPrice")
                        .HasColumnType("numeric(10,2)");

                    b.Property<int>("CouponType")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("CouponValue")
                        .HasColumnType("numeric(10,2)");

                    b.Property<int>("IsOnShelves")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("MinPrice")
                        .HasColumnType("numeric(10,2)");

                    b.Property<int>("PeriodOfValidity")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("QuantitySold")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("CouponId");

                    b.ToTable("coupons", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.CouponPurchaseDB", b =>
                {
                    b.Property<int>("CouponPurchaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CouponPurchaseId"), 1L, 1);

                    b.Property<int>("CouponId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("PurchasingAmount")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTime>("PurchasingTimestamp")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("CouponPurchaseId");

                    b.HasIndex("CouponId");

                    b.HasIndex("UserId");

                    b.ToTable("coupon_purchases", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderCouponDB", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("CouponId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("OrderId");

                    b.HasIndex("UserId", "CouponId", "ExpirationDate");

                    b.ToTable("order_coupons", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderDB", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("OrderId"), 1L, 1);

                    b.Property<int>("AddressId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Comment")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<DateTime?>("ExpectedTimeOfArrival")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int?>("MerchantRating")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("NeedUtensils")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTime>("OrderTimestamp")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric(10,2)");

                    b.Property<DateTime?>("RealTimeOfArrival")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int?>("RiderRating")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("State")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("OrderId");

                    b.HasIndex("AddressId");

                    b.ToTable("orders", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderDishDB", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("MerchantId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("DishId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("DishNum")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("OrderId", "MerchantId", "DishId");

                    b.HasIndex("MerchantId", "DishId");

                    b.ToTable("order_dishes", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderRiderDB", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int?>("RiderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("RiderPrice")
                        .HasColumnType("numeric(10,2)");

                    b.HasKey("OrderId");

                    b.HasIndex("RiderId");

                    b.ToTable("order_riders", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderUserDB", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("OrderId");

                    b.HasIndex("UserId");

                    b.ToTable("order_users", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.RiderDB", b =>
                {
                    b.Property<int>("RiderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RiderId"), 1L, 1);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR2(20)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("NVARCHAR2(11)");

                    b.Property<string>("RiderName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("NVARCHAR2(50)");

                    b.Property<decimal>("Wallet")
                        .HasColumnType("numeric(10,2)");

                    b.Property<string>("WalletPassword")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("RiderId");

                    b.ToTable("riders", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.RiderStationDB", b =>
                {
                    b.Property<int>("RiderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("StationId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("RiderId");

                    b.HasIndex("StationId");

                    b.ToTable("rider_stations", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.RiderWageDB", b =>
                {
                    b.Property<int>("WageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WageId"), 1L, 1);

                    b.Property<int>("RiderId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<decimal>("Wage")
                        .HasColumnType("numeric(10,2)");

                    b.Property<DateTime>("WageTimestamp")
                        .HasColumnType("TIMESTAMP(7)");

                    b.HasKey("WageId");

                    b.HasIndex("RiderId");

                    b.ToTable("rider_wages", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.StationDB", b =>
                {
                    b.Property<int>("StationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StationId"), 1L, 1);

                    b.Property<string>("StationAddress")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("NVARCHAR2(255)");

                    b.Property<string>("StationName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("NVARCHAR2(100)");

                    b.HasKey("StationId");

                    b.ToTable("stations", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.FavoriteMerchantDB", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("MerchantId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("UserId", "MerchantId");

                    b.HasIndex("MerchantId");

                    b.ToTable("favorite_merchants", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.MembershipDB", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("Level")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("Points")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("UserId");

                    b.ToTable("memberships", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.ShoppingCartDB", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("MerchantId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("DishId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("ShoppingCartId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("DishNum")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("UserId", "MerchantId", "DishId", "ShoppingCartId");

                    b.HasIndex("MerchantId", "DishId");

                    b.ToTable("shoppingcarts", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserAddressDB", b =>
                {
                    b.Property<int>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AddressId"), 1L, 1);

                    b.Property<string>("ContactName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("NVARCHAR2(100)");

                    b.Property<string>("HouseNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("NVARCHAR2(50)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("NVARCHAR2(11)");

                    b.Property<string>("UserAddress")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("NVARCHAR2(255)");

                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("AddressId");

                    b.HasIndex("UserId");

                    b.ToTable("user_address", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserCouponDB", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("CouponId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int>("AmountOwned")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("UserId", "CouponId", "ExpirationDate");

                    b.HasIndex("CouponId");

                    b.ToTable("user_coupons", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserDB", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"), 1L, 1);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR2(20)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("NVARCHAR2(11)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("NVARCHAR2(50)");

                    b.Property<decimal>("Wallet")
                        .HasColumnType("numeric(10,2)");

                    b.Property<string>("WalletPassword")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("UserId");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserDefaultAddressDB", b =>
                {
                    b.Property<int>("AddressId")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("UserId")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("AddressId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_default_addresses", (string)null);
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.DishDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Merchant.MerchantDB", "MerchantDB")
                        .WithMany("DishDBs")
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MerchantDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.MerchantStationDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Merchant.MerchantDB", "MerchantDB")
                        .WithOne("MerchantStationDB")
                        .HasForeignKey("takeout_tj.Models.Merchant.MerchantStationDB", "MerchantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.Rider.StationDB", "StationDB")
                        .WithMany("MerchantStationDBs")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MerchantDB");

                    b.Navigation("StationDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.SpecialOfferDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Merchant.MerchantDB", "MerchantDB")
                        .WithMany("SpecialOfferDBs")
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MerchantDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.CouponPurchaseDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Platform.CouponDB", "CouponDB")
                        .WithMany("CouponPurchaseDBs")
                        .HasForeignKey("CouponId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithMany("CouponPurchaseDBs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CouponDB");

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderCouponDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Platform.OrderDB", "OrderDB")
                        .WithOne("OrderCouponDB")
                        .HasForeignKey("takeout_tj.Models.Platform.OrderCouponDB", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.User.UserCouponDB", "UserCouponDB")
                        .WithMany("OrderCouponDB")
                        .HasForeignKey("UserId", "CouponId", "ExpirationDate")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrderDB");

                    b.Navigation("UserCouponDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderDB", b =>
                {
                    b.HasOne("takeout_tj.Models.User.UserAddressDB", "UserAddressDB")
                        .WithMany("OrderDBs")
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAddressDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderDishDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Platform.OrderDB", "OrderDB")
                        .WithMany("OrderDishDBs")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.Merchant.DishDB", "DishDB")
                        .WithMany("OrderDishDBs")
                        .HasForeignKey("MerchantId", "DishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DishDB");

                    b.Navigation("OrderDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderRiderDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Platform.OrderDB", "OrderDB")
                        .WithOne("OrderRiderDB")
                        .HasForeignKey("takeout_tj.Models.Platform.OrderRiderDB", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.Rider.RiderDB", "RiderDB")
                        .WithMany("OrderRiderDBs")
                        .HasForeignKey("RiderId");

                    b.Navigation("OrderDB");

                    b.Navigation("RiderDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderUserDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Platform.OrderDB", "OrderDB")
                        .WithOne("OrderUserDB")
                        .HasForeignKey("takeout_tj.Models.Platform.OrderUserDB", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithMany("OrderUserDBs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrderDB");

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.RiderStationDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Rider.RiderDB", "RiderDB")
                        .WithOne("RiderStationDB")
                        .HasForeignKey("takeout_tj.Models.Rider.RiderStationDB", "RiderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.Rider.StationDB", "StationDB")
                        .WithMany("RiderStationDBs")
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RiderDB");

                    b.Navigation("StationDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.RiderWageDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Rider.RiderDB", "RiderDB")
                        .WithMany("RiderWageDBs")
                        .HasForeignKey("RiderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RiderDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.FavoriteMerchantDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Merchant.MerchantDB", "MerchantDB")
                        .WithMany("FavoriteMerchantDBs")
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithMany("FavoriteMerchantDBs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MerchantDB");

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.MembershipDB", b =>
                {
                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithOne()
                        .HasForeignKey("takeout_tj.Models.User.MembershipDB", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.ShoppingCartDB", b =>
                {
                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithMany("shoppingCartDBs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.Merchant.DishDB", "DishDB")
                        .WithMany("ShoppingCartDBs")
                        .HasForeignKey("MerchantId", "DishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DishDB");

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserAddressDB", b =>
                {
                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithMany("UserAddressDBs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserCouponDB", b =>
                {
                    b.HasOne("takeout_tj.Models.Platform.CouponDB", "CouponDB")
                        .WithMany("UserCouponDBs")
                        .HasForeignKey("CouponId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithMany("UserCouponDBs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CouponDB");

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserDefaultAddressDB", b =>
                {
                    b.HasOne("takeout_tj.Models.User.UserAddressDB", "UserAddressDB")
                        .WithOne("UserDefaultAddressDB")
                        .HasForeignKey("takeout_tj.Models.User.UserDefaultAddressDB", "AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("takeout_tj.Models.User.UserDB", "UserDB")
                        .WithOne("UserDefaultAddressDB")
                        .HasForeignKey("takeout_tj.Models.User.UserDefaultAddressDB", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAddressDB");

                    b.Navigation("UserDB");
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.DishDB", b =>
                {
                    b.Navigation("OrderDishDBs");

                    b.Navigation("ShoppingCartDBs");
                });

            modelBuilder.Entity("takeout_tj.Models.Merchant.MerchantDB", b =>
                {
                    b.Navigation("DishDBs");

                    b.Navigation("FavoriteMerchantDBs");

                    b.Navigation("MerchantStationDB")
                        .IsRequired();

                    b.Navigation("SpecialOfferDBs");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.CouponDB", b =>
                {
                    b.Navigation("CouponPurchaseDBs");

                    b.Navigation("UserCouponDBs");
                });

            modelBuilder.Entity("takeout_tj.Models.Platform.OrderDB", b =>
                {
                    b.Navigation("OrderCouponDB")
                        .IsRequired();

                    b.Navigation("OrderDishDBs");

                    b.Navigation("OrderRiderDB")
                        .IsRequired();

                    b.Navigation("OrderUserDB")
                        .IsRequired();
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.RiderDB", b =>
                {
                    b.Navigation("OrderRiderDBs");

                    b.Navigation("RiderStationDB")
                        .IsRequired();

                    b.Navigation("RiderWageDBs");
                });

            modelBuilder.Entity("takeout_tj.Models.Rider.StationDB", b =>
                {
                    b.Navigation("MerchantStationDBs");

                    b.Navigation("RiderStationDBs");
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserAddressDB", b =>
                {
                    b.Navigation("OrderDBs");

                    b.Navigation("UserDefaultAddressDB")
                        .IsRequired();
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserCouponDB", b =>
                {
                    b.Navigation("OrderCouponDB");
                });

            modelBuilder.Entity("takeout_tj.Models.User.UserDB", b =>
                {
                    b.Navigation("CouponPurchaseDBs");

                    b.Navigation("FavoriteMerchantDBs");

                    b.Navigation("OrderUserDBs");

                    b.Navigation("UserAddressDBs");

                    b.Navigation("UserCouponDBs");

                    b.Navigation("UserDefaultAddressDB")
                        .IsRequired();

                    b.Navigation("shoppingCartDBs");
                });
#pragma warning restore 612, 618
        }
    }
}
