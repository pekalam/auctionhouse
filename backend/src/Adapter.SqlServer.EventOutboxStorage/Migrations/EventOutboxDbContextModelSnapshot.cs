﻿// <auto-generated />
using System;
using Adapter.SqlServer.EventOutbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Adapter.SqlServer.EventOutboxStorage.Migrations
{
    [DbContext(typeof(EventOutboxDbContext))]
    partial class EventOutboxDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Adapter.SqlServer.EventOutbox.DbOutboxItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Processed")
                        .HasColumnType("bit");

                    b.Property<int>("ReadModelNotifications")
                        .HasColumnType("int");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("OutboxItems");
                });

            modelBuilder.Entity("Adapter.SqlServer.EventOutbox.DbOutboxItem", b =>
                {
                    b.OwnsOne("Common.Application.Commands.CommandContext", "CommandContext", b1 =>
                        {
                            b1.Property<long>("DbOutboxItemId")
                                .HasColumnType("bigint");

                            b1.Property<bool>("HttpQueued")
                                .HasColumnType("bit");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<Guid?>("User")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<bool>("WSQueued")
                                .HasColumnType("bit");

                            b1.HasKey("DbOutboxItemId");

                            b1.ToTable("OutboxItems");

                            b1.WithOwner()
                                .HasForeignKey("DbOutboxItemId");

                            b1.OwnsOne("Common.Application.Commands.CommandId", "CommandId", b2 =>
                                {
                                    b2.Property<long>("CommandContextDbOutboxItemId")
                                        .HasColumnType("bigint");

                                    b2.Property<string>("Id")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("CommandContextDbOutboxItemId");

                                    b2.ToTable("OutboxItems");

                                    b2.WithOwner()
                                        .HasForeignKey("CommandContextDbOutboxItemId");
                                });

                            b1.OwnsOne("Common.Application.Events.CorrelationId", "CorrelationId", b2 =>
                                {
                                    b2.Property<long>("CommandContextDbOutboxItemId")
                                        .HasColumnType("bigint");

                                    b2.Property<string>("Value")
                                        .IsRequired()
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("CommandContextDbOutboxItemId");

                                    b2.ToTable("OutboxItems");

                                    b2.WithOwner()
                                        .HasForeignKey("CommandContextDbOutboxItemId");
                                });

                            b1.Navigation("CommandId")
                                .IsRequired();

                            b1.Navigation("CorrelationId")
                                .IsRequired();
                        });

                    b.Navigation("CommandContext")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}