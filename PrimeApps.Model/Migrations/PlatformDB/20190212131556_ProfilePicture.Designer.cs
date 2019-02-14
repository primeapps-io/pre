﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PrimeApps.Model.Context;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    [DbContext(typeof(PlatformDBContext))]
    [Migration("20190212131556_ProfilePicture")]
    partial class ProfilePicture
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.App", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("AppDraftId")
                        .HasColumnName("app_draft_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatedById")
                        .HasColumnName("created_by");

                    b.Property<bool>("Deleted")
                        .HasColumnName("deleted");

                    b.Property<string>("Description")
                        .HasColumnName("description")
                        .HasMaxLength(4000);

                    b.Property<string>("Label")
                        .HasColumnName("label")
                        .HasMaxLength(400);

                    b.Property<string>("Logo")
                        .HasColumnName("logo");

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasMaxLength(50);

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.Property<int?>("UpdatedById")
                        .HasColumnName("updated_by");

                    b.Property<bool>("UseTenantSettings")
                        .HasColumnName("use_tenant_settings");

                    b.HasKey("Id");

                    b.HasIndex("AppDraftId");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Deleted");

                    b.HasIndex("Name");

                    b.HasIndex("UpdatedAt");

                    b.HasIndex("UpdatedById");

                    b.ToTable("apps");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.AppSetting", b =>
                {
                    b.Property<int>("AppId")
                        .HasColumnName("app_id");

                    b.Property<string>("AppDomain")
                        .HasColumnName("app_domain");

                    b.Property<string>("AppTheme")
                        .HasColumnName("app_theme")
                        .HasColumnType("jsonb");

                    b.Property<string>("AuthDomain")
                        .HasColumnName("auth_domain");

                    b.Property<string>("AuthTheme")
                        .HasColumnName("auth_theme")
                        .HasColumnType("jsonb");

                    b.Property<string>("Culture")
                        .HasColumnName("culture");

                    b.Property<string>("Currency")
                        .HasColumnName("currency");

                    b.Property<string>("ExternalAuth")
                        .HasColumnName("external_auth")
                        .HasColumnType("jsonb");

                    b.Property<string>("GoogleAnalyticsCode")
                        .HasColumnName("google_analytics_code");

                    b.Property<string>("Language")
                        .HasColumnName("language");

                    b.Property<string>("MailSenderEmail")
                        .HasColumnName("mail_sender_email");

                    b.Property<string>("MailSenderName")
                        .HasColumnName("mail_sender_name");

                    b.Property<int>("RegistrationType")
                        .HasColumnName("registration_type");

                    b.Property<string>("TenantOperationWebhook")
                        .HasColumnName("tenant_operation_webhook");

                    b.Property<string>("TimeZone")
                        .HasColumnName("time_zone");

                    b.HasKey("AppId");

                    b.ToTable("app_settings");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.AppTemplate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<bool>("Active")
                        .HasColumnName("active");

                    b.Property<int>("AppId")
                        .HasColumnName("app_id");

                    b.Property<string>("Content")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatedById")
                        .HasColumnName("created_by");

                    b.Property<bool>("Deleted")
                        .HasColumnName("deleted");

                    b.Property<string>("Language")
                        .HasColumnName("language");

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasMaxLength(200);

                    b.Property<string>("Settings")
                        .HasColumnName("settings")
                        .HasColumnType("jsonb");

                    b.Property<string>("Subject")
                        .HasColumnName("subject")
                        .HasMaxLength(200);

                    b.Property<string>("SystemCode")
                        .HasColumnName("system_code");

                    b.Property<int>("Type")
                        .HasColumnName("type");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.Property<int?>("UpdatedById")
                        .HasColumnName("updated_by");

                    b.HasKey("Id");

                    b.HasIndex("Active");

                    b.HasIndex("AppId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Language");

                    b.HasIndex("Name");

                    b.HasIndex("SystemCode");

                    b.HasIndex("Type");

                    b.HasIndex("UpdatedById");

                    b.ToTable("app_templates");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.Component", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatedById")
                        .HasColumnName("created_by");

                    b.Property<bool>("Deleted")
                        .HasColumnName("deleted");

                    b.Property<int>("ModuleId")
                        .HasColumnName("module_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(15);

                    b.Property<int>("Order")
                        .HasColumnName("order");

                    b.Property<int>("Place")
                        .HasColumnName("place");

                    b.Property<int>("Status")
                        .HasColumnName("status");

                    b.Property<int>("Type")
                        .HasColumnName("type");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.Property<int?>("UpdatedById")
                        .HasColumnName("updated_by");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModuleId");

                    b.HasIndex("Name");

                    b.HasIndex("Place");

                    b.HasIndex("Status");

                    b.HasIndex("Type");

                    b.HasIndex("UpdatedById");

                    b.ToTable("components");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.ExchangeRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("Date")
                        .HasColumnName("date");

                    b.Property<int>("Day")
                        .HasColumnName("day");

                    b.Property<decimal>("Eur")
                        .HasColumnName("eur");

                    b.Property<int>("Month")
                        .HasColumnName("month");

                    b.Property<decimal>("Usd")
                        .HasColumnName("usd");

                    b.Property<int>("Year")
                        .HasColumnName("year");

                    b.HasKey("Id");

                    b.HasIndex("Date");

                    b.HasIndex("Day");

                    b.HasIndex("Month");

                    b.HasIndex("Year");

                    b.ToTable("exchange_rates");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.Function", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .HasColumnName("content");

                    b.Property<int>("ContentType")
                        .HasColumnName("content_type");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatedById")
                        .HasColumnName("created_by");

                    b.Property<bool>("Deleted")
                        .HasColumnName("deleted");

                    b.Property<string>("Dependencies")
                        .HasColumnName("dependencies");

                    b.Property<string>("Handler")
                        .IsRequired()
                        .HasColumnName("handler");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnName("label")
                        .HasMaxLength(300);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(100);

                    b.Property<int>("Runtime")
                        .HasColumnName("runtime");

                    b.Property<int>("Status")
                        .HasColumnName("status");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.Property<int?>("UpdatedById")
                        .HasColumnName("updated_by");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Handler");

                    b.HasIndex("Name");

                    b.HasIndex("Runtime");

                    b.HasIndex("Status");

                    b.HasIndex("UpdatedById");

                    b.ToTable("functions");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.PlatformUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnName("first_name");

                    b.Property<bool>("IsIntegrationUser")
                        .HasColumnName("is_integration_user");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnName("last_name");

                    b.Property<string>("ProfilePicture")
                        .IsRequired()
                        .HasColumnName("profile_picture");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("Email");

                    b.HasIndex("FirstName");

                    b.HasIndex("Id");

                    b.HasIndex("LastName");

                    b.HasIndex("UpdatedAt");

                    b.ToTable("users");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.PlatformUserSetting", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.Property<string>("Culture")
                        .HasColumnName("culture");

                    b.Property<string>("Currency")
                        .HasColumnName("currency");

                    b.Property<string>("Language")
                        .HasColumnName("language");

                    b.Property<string>("Phone")
                        .HasColumnName("phone");

                    b.Property<string>("TimeZone")
                        .HasColumnName("time_zone");

                    b.HasKey("UserId");

                    b.HasIndex("Culture");

                    b.HasIndex("Currency");

                    b.HasIndex("Language");

                    b.HasIndex("Phone");

                    b.HasIndex("TimeZone");

                    b.ToTable("user_settings");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.PlatformWarehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<bool>("Completed")
                        .HasColumnName("completed");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatedById")
                        .HasColumnName("created_by");

                    b.Property<string>("DatabaseName")
                        .HasColumnName("database_name");

                    b.Property<string>("DatabaseUser")
                        .HasColumnName("database_user");

                    b.Property<bool>("Deleted")
                        .HasColumnName("deleted");

                    b.Property<string>("PowerbiWorkspaceId")
                        .HasColumnName("powerbi_workspace_id");

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.Property<int?>("UpdatedById")
                        .HasColumnName("updated_by");

                    b.HasKey("Id");

                    b.HasIndex("Completed");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DatabaseName");

                    b.HasIndex("TenantId");

                    b.HasIndex("UpdatedById");

                    b.ToTable("warehouses");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("AppId")
                        .HasColumnName("app_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatedById")
                        .HasColumnName("created_by");

                    b.Property<bool>("Deleted")
                        .HasColumnName("deleted");

                    b.Property<Guid>("GuidId")
                        .HasColumnName("guid_id");

                    b.Property<int>("OwnerId")
                        .HasColumnName("owner_id");

                    b.Property<string>("Title")
                        .HasColumnName("title");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnName("updated_at");

                    b.Property<int?>("UpdatedById")
                        .HasColumnName("updated_by");

                    b.Property<bool>("UseUserSettings")
                        .HasColumnName("use_user_settings");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Deleted");

                    b.HasIndex("GuidId");

                    b.HasIndex("OwnerId");

                    b.HasIndex("UpdatedAt");

                    b.HasIndex("UpdatedById");

                    b.ToTable("tenants");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.TenantLicense", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id");

                    b.Property<int>("AnalyticsLicenseCount")
                        .HasColumnName("analytics_license_count");

                    b.Property<DateTime?>("DeactivatedAt")
                        .HasColumnName("deactivated_at");

                    b.Property<bool>("IsDeactivated")
                        .HasColumnName("is_deactivated");

                    b.Property<bool>("IsPaidCustomer")
                        .HasColumnName("is_paid_customer");

                    b.Property<bool>("IsSuspended")
                        .HasColumnName("is_suspended");

                    b.Property<int>("ModuleLicenseCount")
                        .HasColumnName("module_license_count");

                    b.Property<int>("SipLicenseCount")
                        .HasColumnName("sip_license_count");

                    b.Property<DateTime?>("SuspendedAt")
                        .HasColumnName("suspended_at");

                    b.Property<int>("UserLicenseCount")
                        .HasColumnName("user_license_count");

                    b.HasKey("TenantId");

                    b.HasIndex("DeactivatedAt");

                    b.HasIndex("IsDeactivated");

                    b.HasIndex("IsPaidCustomer");

                    b.HasIndex("IsSuspended");

                    b.HasIndex("SuspendedAt");

                    b.ToTable("tenant_licenses");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.TenantSetting", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id");

                    b.Property<string>("Culture")
                        .HasColumnName("culture");

                    b.Property<string>("Currency")
                        .HasColumnName("currency");

                    b.Property<string>("CustomColor")
                        .HasColumnName("custom_color");

                    b.Property<string>("CustomDescription")
                        .HasColumnName("custom_description");

                    b.Property<string>("CustomDomain")
                        .HasColumnName("custom_domain");

                    b.Property<string>("CustomFavicon")
                        .HasColumnName("custom_favicon");

                    b.Property<string>("CustomImage")
                        .HasColumnName("custom_image");

                    b.Property<string>("CustomTitle")
                        .HasColumnName("custom_title");

                    b.Property<bool>("HasSampleData")
                        .HasColumnName("has_sample_data");

                    b.Property<string>("Language")
                        .HasColumnName("language");

                    b.Property<string>("Logo")
                        .HasColumnName("logo");

                    b.Property<string>("MailSenderEmail")
                        .HasColumnName("mail_sender_email");

                    b.Property<string>("MailSenderName")
                        .HasColumnName("mail_sender_name");

                    b.Property<string>("TimeZone")
                        .HasColumnName("time_zone");

                    b.HasKey("TenantId");

                    b.HasIndex("CustomDomain");

                    b.ToTable("tenant_settings");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.UserTenant", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id");

                    b.HasKey("UserId", "TenantId");

                    b.HasIndex("TenantId");

                    b.HasIndex("UserId");

                    b.ToTable("user_tenants");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.App", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.AppSetting", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.App", "App")
                        .WithOne("Setting")
                        .HasForeignKey("PrimeApps.Model.Entities.Platform.AppSetting", "AppId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.AppTemplate", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.App", "App")
                        .WithMany("Templates")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.Component", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.Function", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.PlatformUserSetting", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "User")
                        .WithOne("Setting")
                        .HasForeignKey("PrimeApps.Model.Entities.Platform.PlatformUserSetting", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.PlatformWarehouse", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.Tenant", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.App", "App")
                        .WithMany("Tenants")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "Owner")
                        .WithMany("TenantsAsOwner")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById");
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.TenantLicense", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.Tenant", "Tenant")
                        .WithOne("License")
                        .HasForeignKey("PrimeApps.Model.Entities.Platform.TenantLicense", "TenantId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.TenantSetting", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.Tenant", "Tenant")
                        .WithOne("Setting")
                        .HasForeignKey("PrimeApps.Model.Entities.Platform.TenantSetting", "TenantId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PrimeApps.Model.Entities.Platform.UserTenant", b =>
                {
                    b.HasOne("PrimeApps.Model.Entities.Platform.Tenant", "Tenant")
                        .WithMany("TenantUsers")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PrimeApps.Model.Entities.Platform.PlatformUser", "PlatformUser")
                        .WithMany("TenantsAsUser")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
