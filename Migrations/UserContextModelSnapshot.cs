﻿// <auto-generated />
using Mercury_Backend.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;

namespace Mercury_Backend.Migrations
{
    [DbContext(typeof(UserContext))]
    partial class UserContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Mercury_Backend.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("AvatarId")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Brief")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("Credit")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("Grade")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Major")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("NickName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Password")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Phone")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("RealName")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Role")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
