﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheOnlyParty.DiscordBot.DbContexts;

#nullable disable

namespace TheOnlyParty.DiscordBot.Migrations
{
    [DbContext(typeof(DiscordDbContext))]
    [Migration("20220703014021_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.6");

            modelBuilder.Entity("TheOnlyParty.DiscordBot.Models.UserReport", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<int>("NegativeMessages")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositiveMessages")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TotalMessages")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId");

                    b.ToTable("UserReports");
                });
#pragma warning restore 612, 618
        }
    }
}
