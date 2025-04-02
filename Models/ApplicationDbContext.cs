using BanHang.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus
                {
                    Id = 1,
                    Name = "Chờ xác nhận",
                    Description = "Đơn hàng đang chờ xác nhận",
                },
                new OrderStatus
                {
                    Id = 2,
                    Name = "Đã xác nhận",
                    Description = "Đơn hàng đã được xác nhận",
                },
                new OrderStatus
                {
                    Id = 3,
                    Name = "Đang giao hàng",
                    Description = "Đơn hàng đang được giao",
                },
               new OrderStatus
               {
                   Id = 4,
                   Name = "Đã hoàn thành",
                   Description = "Đơn hàng đã được giao và hoàn thành",
               },
                new OrderStatus
                {
                    Id = 5,
                    Name = "Đã hủy",
                    Description = "Đơn hàng đã bị hủy",
                }
            );

        }



    }
}
