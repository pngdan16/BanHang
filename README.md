# Website Bán Hàng MacBook

![MacBook](https://raw.githubusercontent.com/username/BanHang/main/wwwroot/img/logo.png)

## Tổng quan

Website Bán Hàng MacBook là một ứng dụng web hiện đại được xây dựng bằng ASP.NET Core MVC, cung cấp nền tảng bán các sản phẩm Apple trực tuyến. Ứng dụng triển khai kiến trúc sạch với mẫu repository và phương pháp tiếp cận dựa trên dịch vụ để đảm bảo khả năng bảo trì và mở rộng.

## Tính năng

- **Quản lý Sản phẩm**: Thêm, sửa, xóa và hiển thị sản phẩm với hình ảnh
- **Quản lý Danh mục**: Tổ chức sản phẩm theo danh mục
- **Xác thực Người dùng**: Hệ thống xác thực dựa trên vai trò với các cấp độ quyền khác nhau
- **Quản lý Hình ảnh**: Tải lên và quản lý hình ảnh sản phẩm sử dụng tích hợp Cloudinary
- **Thiết kế Responsive**: Giao diện người dùng responsive dựa trên Bootstrap cho tất cả các loại thiết bị
- **Vai trò Người dùng**: Vai trò Admin, Nhân viên, Khách hàng và Công ty với các quyền khác nhau

## Kiến trúc

Ứng dụng tuân theo mẫu kiến trúc sạch với sự phân tách các mối quan tâm:

- **Models**: Mô hình dữ liệu đại diện cho các thực thể kinh doanh
- **Repositories**: Lớp truy cập dữ liệu với sự tách biệt giữa giao diện và triển khai
- **Services**: Lớp logic nghiệp vụ với sự tách biệt giữa giao diện và triển khai
- **Controllers**: Xử lý các yêu cầu và phản hồi HTTP
- **Views**: Các thành phần giao diện người dùng

## Công nghệ sử dụng

- **Backend**: ASP.NET Core MVC (.NET 9.0)
- **Cơ sở dữ liệu**: Microsoft SQL Server với Entity Framework Core
- **Frontend**: HTML, CSS, JavaScript, Bootstrap 5
- **Xác thực**: ASP.NET Core Identity
- **Lưu trữ Đám mây**: Cloudinary cho lưu trữ hình ảnh
- **Dependency Injection**: Container DI tích hợp sẵn trong ASP.NET Core

## Cài đặt

### Yêu cầu

- .NET 9.0 SDK trở lên
- SQL Server (cục bộ hoặc từ xa)
- Visual Studio 2022 hoặc bất kỳ IDE tương thích nào

### Các bước

1. Clone repository:
   ```
   git clone https://github.com/username/BanHang.git
   cd BanHang
   ```

2. Cấu hình kết nối cơ sở dữ liệu trong `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DeployConnectionString": "Server=YOUR_SERVER;Database=BanHang;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. Cấu hình thiết lập Cloudinary trong `appsettings.json`:
   ```json
   "Cloudinary": {
     "CloudName": "your_cloud_name",
     "ApiKey": "your_api_key",
     "ApiSecret": "your_api_secret"
   }
   ```

4. Áp dụng các migration cơ sở dữ liệu:
   ```
   dotnet ef database update
   ```

5. Chạy ứng dụng:
   ```
   dotnet run
   ```

## Sử dụng

Sau khi cài đặt, ứng dụng sẽ có sẵn tại `https://localhost:5001` hoặc `http://localhost:5000`. 

Vai trò và người dùng mặc định được tạo khi chạy lần đầu:
- Admin: admin@example.com / Admin@123
- Khách hàng: customer@example.com / Customer@123

## Cấu trúc dự án

```
BanHang/
├── Controllers/            # MVC Controllers
├── Models/                 # Mô hình dữ liệu
│   ├── Identity/           # Mô hình người dùng và vai trò
│   └── Settings/           # Mô hình thiết lập ứng dụng
├── Repositories/
│   ├── Implementations/    # Triển khai Repository
│   └── Interfaces/         # Giao diện Repository
├── Services/
│   ├── Implementations/    # Triển khai Service
│   └── Interfaces/         # Giao diện Service
├── Views/                  # MVC Views
│   ├── Home/               # Views của controller Home
│   ├── Product/            # Views của controller Product
│   ├── Category/           # Views của controller Category
│   └── Shared/             # Views và layouts được chia sẻ
├── wwwroot/                # Tệp tĩnh (CSS, JS, hình ảnh)
├── Areas/                  # Các khu vực tính năng (Admin, Identity)
├── DTO/                    # Đối tượng Truyền Dữ liệu
├── Migrations/             # Các migration cơ sở dữ liệu
└── Program.cs              # Điểm vào và cấu hình ứng dụng
```

## Đóng góp

1. Fork repository
2. Tạo nhánh tính năng của bạn (`git checkout -b feature/amazing-feature`)
3. Commit các thay đổi của bạn (`git commit -m 'Thêm một tính năng tuyệt vời'`)
4. Push lên nhánh (`git push origin feature/amazing-feature`)
5. Mở một Pull Request

## Giấy phép

Dự án này được cấp phép theo Giấy phép MIT - xem tệp LICENSE để biết chi tiết.

## Liên hệ

Tên của bạn - your.email@example.com

Liên kết dự án: [https://github.com/username/BanHang](https://github.com/username/BanHang)
