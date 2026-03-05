using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrderTracking.Infrastructure.Persistence;

#nullable disable

namespace OrderTracking.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OrderTracking.Domain.Orders.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("Id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("description");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("order_number");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("status");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<uint>("xmin")
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("orders");
                });

            modelBuilder.Entity("OrderTracking.Infrastructure.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("Attempts")
                        .HasColumnType("integer")
                        .HasColumnName("attempts");

                    b.Property<DateTimeOffset>("OccurredAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_at");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("payload");

                    b.Property<DateTimeOffset?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_at");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("type");

                    b.Property<string>("LastError")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)")
                        .HasColumnName("last_error");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Status", "OccurredAt" })
                        .HasDatabaseName("ix_outbox_status_occurred_at");

                    b.HasIndex("ProcessedAt")
                        .HasDatabaseName("ix_outbox_processed_at");

                    b.ToTable("outbox_messages");
                });

            modelBuilder.Entity("OrderTracking.Infrastructure.Idempotency.ProcessedEvent", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid")
                        .HasColumnName("event_id");

                    b.Property<DateTimeOffset>("ProcessedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_at");

                    b.HasKey("EventId");

                    b.HasIndex("ProcessedAt")
                        .HasDatabaseName("ix_processed_events_processed_at");

                    b.ToTable("processed_events");
                });
#pragma warning restore 612, 618
        }
    }
}
