﻿// <auto-generated />
using System;
using Lyralabs.TempMailServer.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lyralabs.TempMailServer.Data.Context.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20210511122942_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.5");

            modelBuilder.Entity("Lyralabs.TempMailServer.Data.Entities.MailModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BodyHtml")
                        .HasColumnType("TEXT");

                    b.Property<string>("BodyText")
                        .HasColumnType("TEXT");

                    b.Property<string>("FromAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("FromName")
                        .HasColumnType("TEXT");

                    b.Property<int>("MailboxId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ReceivedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Subject")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MailboxId");

                    b.ToTable("Mails");
                });

            modelBuilder.Entity("Lyralabs.TempMailServer.Data.Entities.MailboxModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PublicKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Mailboxes");
                });

            modelBuilder.Entity("Lyralabs.TempMailServer.Data.Entities.MailModel", b =>
                {
                    b.HasOne("Lyralabs.TempMailServer.Data.Entities.MailboxModel", "Mailbox")
                        .WithMany("Mails")
                        .HasForeignKey("MailboxId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Mailbox");
                });

            modelBuilder.Entity("Lyralabs.TempMailServer.Data.Entities.MailboxModel", b =>
                {
                    b.Navigation("Mails");
                });
#pragma warning restore 612, 618
        }
    }
}
