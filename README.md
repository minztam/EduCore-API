EduCore.API là hệ thống backend mạnh mẽ dành cho nền tảng quản lý khóa học trực tuyến (E-learning), hỗ trợ đầy đủ các tính năng từ quản lý nội dung, thanh toán VNPAY, cho đến chat realtime.

## 🚀 Công nghệ sử dụng

Framework: ASP.NET Core 8.0, Entity Framework Core 8.0
Database: PostgreSQL
Realtime: SignalR (Notifications & Chat)
Third-party: Cloudinary (Media), VNPAY (Payment), Google OAuth (Auth)
API Documentation: Swagger (Swashbuckle)

## 🌐 Hệ sinh thái dự án

Dự án này là phần Backend (API) của hệ thống EduCore. Để có trải nghiệm đầy đủ, bạn có thể tham khảo Frontend tại đây:

- **Frontend Repository**: [EduCore-Frontend](https://github.com/minztam/EduCore-Frontend)
- **Công nghệ**: React, TailwindCSS, Axios, SignalR Client, v.v.

## 📂 Cấu trúc dự án

- `EduCore.API/`: mã nguồn chính của backend.
  - `Controllers/`: các API controller xử lý yêu cầu HTTP.
  - `Data/`: cấu hình DbContext, Entities và DTOs.
  - `Entities/`: các lớp thực thể (models) ánh xạ bảng cơ sở dữ liệu.
  - `DTOs/`: kiểu dữ liệu dùng cho request/response, chia theo module.
  - `Repositories/`: logic truy xuất dữ liệu (Interfaces + Implementations).
  - `Service/`: các dịch vụ nghiệp vụ, cấu hình dịch vụ, helper.
  - `Hubs/`: SignalR hub cho chat và thông báo realtime.
  - `Migrations/`: tập tin migration của Entity Framework Core.
  - `Properties/`: cấu hình launchSettings.
  - `Program.cs`: điểm khởi động ứng dụng và cấu hình dịch vụ.
  - `appsettings.json` / `appsettings.Development.json`: cấu hình ứng dụng.

## 🛠 Hướng dẫn cài đặt

1. Yêu cầu
   .NET 8 SDK
   PostgreSQL Database
   Tài khoản Cloudinary, VNPAY, Google Developer Console

2. Thiết lập

# Clone dự án

git clone <your-repo-url>

# Sao chép file cấu hình mẫu

cp appsettings.json appsettings.Development.json

3. Điền thông tin vào `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "REPLACED_BY_RENDER_ENV",
    "SupabaseConnection": "REPLACED_BY_RENDER_ENV"
  },
  "Google": {
    "ClientId": "REPLACED_BY_RENDER_ENV",
    "ClientSecret": "REPLACED_BY_RENDER_ENV"
  },
  "EmailSettings": {
    "SenderEmail": "REPLACED_BY_RENDER_ENV",
    "SenderName": "EduCore",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Password": "REPLACED_BY_RENDER_ENV"
  },
  "Jwt": {
    "Key": "REPLACED_BY_RENDER_ENV",
    "Issuer": "EduCoreAPI",
    "Audience": "EduCoreClient",
    "ExpireMinutes": 60
  },
  "CloudinarySettings": {
    "CloudName": "REPLACED_BY_RENDER_ENV",
    "ApiKey": "REPLACED_BY_RENDER_ENV",
    "ApiSecret": "REPLACED_BY_RENDER_ENV"
  },
  "Vnpay": {
    "TmnCode": "REPLACED_BY_RENDER_ENV",
    "HashSecret": "REPLACED_BY_RENDER_ENV",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "REPLACED_BY_RENDER_ENV"
  },
  "FrontendUrl": "REPLACED_BY_RENDER_ENV",
  "TimeZoneId": "SE Asia Standard Time"
}
```

## 💡 Các tính năng chính

1. Auth: Đăng nhập Local / Google OAuth 2.0.
2. Course Management: CRUD khóa học, quản lý chương & bài học (video/tài liệu).
3. E-commerce: Tích hợp thanh toán VNPAY, quản lý lịch sử đơn hàng.
4. Realtime: Hệ thống thông báo và Chat (1:1 & Group) dùng SignalR.
5. Media: Lưu trữ tài nguyên trên Cloudinary.

## 📚 API Documentation

Sau khi chạy ứng dụng, truy cập:
https://localhost:<port>/swagger/index.html để khám phá các Endpoint.
