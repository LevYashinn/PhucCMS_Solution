# 👟 Trạm Giày Sneaker (Sneaker Station) - E-commerce Platform

![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![.NET Core](https://img.shields.io/badge/.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)
![Status](https://img.shields.io/badge/Status-Active-success.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)

**Trạm Giày Sneaker** là một hệ thống Website thương mại điện tử chuyên cung cấp các sản phẩm giày thể thao chính hãng. Hệ thống được xây dựng với kiến trúc Client-Server hiện đại, phân tách hoàn toàn giữa Backend (API) và Frontend (UI), mang lại hiệu năng cao và trải nghiệm người dùng mượt mà.

---

## 🌟 Chức năng nổi bật (Features)

### 👨‍💻 Phía Khách hàng (Client Side)
* **Quản lý tài khoản:** Đăng ký, Đăng nhập (Mã hóa mật khẩu), Quên mật khẩu qua Email (OTP).
* **Mua sắm:** Xem danh mục, Tìm kiếm sản phẩm, Lọc theo giá/loại.
* **Giỏ hàng (Cart):** Thêm/Sửa/Xóa sản phẩm, tự động chặn nếu vượt quá số lượng tồn kho. Quản lý giỏ hàng riêng biệt cho từng user.
* **Thanh toán & Đơn hàng:** Đặt hàng, theo dõi lịch sử và trạng thái đơn hàng.
* **Tin tức (Blog):** Xem các bài viết hướng dẫn, khuyến mãi, kiến thức sneaker.

### 🔐 Phía Quản trị viên (Admin Side)
* **Dashboard:** Thống kê doanh thu, số lượng đơn hàng, sản phẩm sắp hết hàng.
* **Quản lý Sản phẩm & Danh mục:** Thêm, sửa, xóa, upload hình ảnh vật lý.
* **Quản lý Đơn hàng:** Duyệt đơn, cập nhật trạng thái giao hàng.
* **Quản lý Bài viết (Blog):** Trình soạn thảo văn bản CKEditor.

---

## 💻 Công nghệ sử dụng (Tech Stack)

* **Frontend:** ReactJS (Functional Components, Hooks), React Router DOM, Axios, Context API, Bootstrap 5.
* **Backend:** C# ASP.NET Core Web API (MVC Architecture), Entity Framework Core.
* **Database:** Microsoft SQL Server.
* **Công cụ phụ trợ:** Swagger (Test API), SendGrid/SMTP (Gửi Email OTP), MemoryCache.

---

## 🚀 Hướng dẫn cài đặt và chạy dự án (Getting Started)

Để chạy dự án này trên máy tính cá nhân của bạn (Local Environment), vui lòng làm theo các bước dưới đây.

### 1️⃣ Yêu cầu phần mềm (Prerequisites)
Bạn cần phải cài đặt sẵn các phần mềm sau trước khi chạy code:
* [Node.js](https://nodejs.org/en/) (Phiên bản v16.x trở lên)
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (Hỗ trợ workload ASP.NET và web development)
* [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
* [Git](https://git-scm.com/)

---

### 2️⃣ Cài đặt và cấu hình Backend (ASP.NET Core API)

**Bước 1:** Clone dự án về máy
```bash
git clone [https://github.com/LevYashinn/PhucCMS_Solution/]

Cấu trúc thư mục (Folder Structure)
Frontend/
├── public/               # File tĩnh (index.html, favicon)
├── src/
│   ├── api/              # Cấu hình Axios và chặn lỗi (Interceptors)
│   ├── assets/           # Hình ảnh, icon tĩnh
│   ├── components/       # Các component dùng chung (Header, Footer, PostCard...)
│   ├── context/          # Quản lý State toàn cục (CartContext)
│   ├── pages/            # Các trang giao diện (Home, Shop, Blog, Auth...)
│   ├── services/         # Gọi API tương tác với Backend
│   ├── App.js            # File định tuyến (Routing) chính
│   └── index.js          # File gốc khởi chạy React
├── .env                  # Cấu hình biến môi trường
└── package.json          # Danh sách thư viện npm

Backend/
├── Controllers/          # Xử lý Request/Response (API Endpoints)
├── Data/
│   ├── Entities/         # Models đại diện cho các bảng trong SQL
│   └── ApplicationDbContext.cs # Cấu hình Entity Framework
├── Models/               # Data Transfer Objects (DTOs)
├── wwwroot/              # Chứa file tĩnh được upload từ Admin (Hình ảnh)
├── appsettings.json      # File cấu hình Server, Database
└── Program.cs            # Cấu hình Middleware, CORS, Services
9. Giải pháp thuật toán nổi bật
1. Thuật toán xử lý Giỏ hàng (CartContext)
Phân tách User: Giỏ hàng sử dụng localStorage nhưng tên Key được gắn mã ID động (cart_user_5). Nếu chưa đăng nhập, sử dụng cart_guest. Khi User đăng xuất, hệ thống tự động tải lại context để trả về giỏ hàng trống.

Kiểm soát Tồn kho (Stock Validation): Hàm addToCart được lập trình để trước khi cộng số lượng sẽ so sánh với StockQuantity từ Backend. Nếu Item in Cart + Want to Add > Stock, hệ thống hiện cảnh báo SweetAlert2 ngay lập tức và hủy lệnh thêm.

2. Thuật toán Bảo mật Quên mật khẩu bằng OTP
Không lưu OTP vào CSDL để tránh rác. Sử dụng IMemoryCache của .NET.

Mã OTP 6 số được sinh ngẫu nhiên và Set thời gian sống (TTL) đúng 5 phút:
_cache.Set($"OTP_{request.Email}", otp, TimeSpan.FromMinutes(5));

Gửi Email trực tiếp cho khách qua hệ thống SmtpClient sử dụng giao thức bảo mật SSL/TLS.

3. Thuật toán Phân trang (Pagination) Tối ưu hóa
Backend (Admin): Áp dụng Server-side Pagination để không bị sập Server khi DB có hàng triệu dòng. Query Skip((page - 1) * pageSize).Take(pageSize) tạo ra câu lệnh SQL tối ưu.

Frontend (Shop/Blog): Áp dụng Client-side Pagination với cú pháp .slice(indexOfFirstItem, indexOfLastItem) kết hợp "Smart Reset" (Tự động đưa về Page 1 khi đổi danh mục hoặc gõ từ khóa tìm kiếm mới).

10. Xử lý sự cố thường gặp (Troubleshooting)
Lỗi 1: Đăng nhập được nhưng gọi API lấy dữ liệu bị lỗi CORS Policy

Nguyên nhân: ReactJS gọi API sang một cổng khác chưa được C# cho phép.

Giải pháp: Mở file Program.cs ở Backend, kiểm tra phần cấu hình CORS xem đã có AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() chưa.

Lỗi 2: Hình ảnh sản phẩm bị "tàng hình" (Lỗi 404)

Nguyên nhân: Biến IMAGE_BASE_URL cấu hình trong .env chưa đúng với cổng Backend đang chạy.

Giải pháp: Mở .env, sửa lại REACT_APP_IMAGE_BASE_URL cho khớp cổng (VD: http://localhost:5226). Sau đó khởi động lại React (Ctrl+C -> npm start).

Lỗi 3: Gửi mã OTP báo lỗi 500 (Lỗi Email)

Nguyên nhân: Gmail App Password bị sai hoặc hết hạn.

Giải pháp: Vào tài khoản Google, mục Bảo mật -> Mật khẩu ứng dụng (App Passwords) tạo mật khẩu mới 16 ký tự và thay vào biến appPassword trong AuthController.cs.

11. Đóng góp & Giấy phép
Dự án này là mã nguồn mở (Open Source). Chúng tôi hoan nghênh mọi đóng góp (Pull Request) từ cộng đồng để cải thiện tính năng:

Fork dự án

Tạo nhánh mới (git checkout -b feature/NewFeature)

Commit thay đổi (git commit -m 'Add NewFeature')

Push lên nhánh (git push origin feature/NewFeature)

Mở Pull Request

📝 Bản quyền (License)
Dự án được phân phối dưới giấy phép MIT License. Bạn có quyền sử dụng, sao chép, sửa đổi, hợp nhất, xuất bản tự do.

Liên hệ với tác giả:
Email: phuc512dz@gmail.com
GitHub: https://github.com/LevYashinn/PhucCMS_Solution
