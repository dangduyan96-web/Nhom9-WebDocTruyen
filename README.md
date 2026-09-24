# WebDocTruyen - Hệ Thống Quản Lý Đọc Truyện Trực Tuyến

Dự án phát triển hệ thống quản lý và đọc truyện trực tuyến đa nền tảng (Web Application & Mobile Application) kết nối tập trung qua RESTful API.

---

## Thành Viên Thực Hiện Dự Án
- **Product Owner**: Dương Lâm Tuấn Anh (`TuanAnhAz123`)
- **Scrum Master / Dev**: Phạm Nguyễn Hoàng Phúc (`Saigoz`)
- **Team Lead / Dev**: Đặng Duy An (`dangduyan96-web`)
- **Thành Viên Dev**: Hoàng Văn Nam (`NamHaong`)
- **Thành Viên Dev**: Nguyễn Anh Tuấn (`anhtuan308`)
- **Thành Viên Dev**: Tsằn Hiền Đức (`duchien1231-bit`)

---

## 🛠️ Công nghệ sử dụng
- **Backend:** C# (.NET Framework 4.8 / .NET Core), ASP.NET MVC 5 & Web API.
- **ORM:** Entity Framework Core (Database First).
- **Frontend Web:** HTML5, CSS3, JavaScript, Bootstrap 5, AJAX.
- **Mobile App:** Android Native (Java/Kotlin).
- **Xác thực:** Session kết hợp JWT (JSON Web Tokens) lưu trong HttpOnly Cookies (Web) & Authorization Bearer Header (Mobile App).
- **Cơ sở dữ liệu:** Microsoft SQL Server.
- **Thanh toán:** Tích hợp cổng thanh toán PayPal, VNPay.

---

## ✨ Các tính năng chính

### 📚 Dành cho Người đọc (Web & Mobile App)
1. **Duyệt & Tìm kiếm truyện:** Xem danh mục, truyện mới cập nhật, truyện hot, tìm kiếm theo tên tác giả, thể loại.
2. **Đọc truyện & Lưu tiến độ:** Đọc các chương miễn phí/trả phí, hệ thống tự động lưu vị trí/chương đang đọc gần nhất (`dbo.TienDoDoc`).
3. **Theo dõi & Yêu thích:** Đánh dấu truyện yêu thích (`dbo.TruyenYeuThich`) và nhận thông báo khi có chương mới.
4. **Thảo luận & Bình luận:** Đăng nhận xét, trao đổi tại từng chương truyện (`dbo.BinhLuanChuong`).
5. **Thanh toán & Mua quyền đọc:** Đăng ký gói đọc theo tháng (`dbo.GoiDoc`) hoặc mua lẻ từng bộ truyện (`dbo.TruyenDaMua`) qua VNPay / PayPal.
6. **Quản lý tài khoản:** Đăng ký, đăng nhập, đổi mật khẩu và xem lịch sử giao dịch thanh toán (`dbo.LichSuGiaoDich`).

### 🛡️ Dành cho Quản trị viên (Admin Area)
1. **Quản lý Truyện & Thể loại:** Thêm/sửa/xóa bộ truyện, cập nhật ảnh bìa, tác giả, trạng thái phát hành, gán đa thể loại (`dbo.TruyenTheLoai`).
2. **Quản lý Chương truyện:** Thêm/sửa/xóa chương, thiết lập thời gian phát hành và trạng thái miễn phí/trả phí.
3. **Thiết lập Chính sách giá & Gói cước:** Quản lý danh mục gói đọc theo tháng, thiết lập giá mua lẻ từng bộ truyện.
4. **Kiểm duyệt nội dung & Người dùng:** Theo dõi/xóa các bình luận vi phạm quy định cộng đồng, quản lý danh sách tài khoản thành viên.
5. **Báo cáo & Thống kê:** Thống kê lượt đọc, tương tác, doanh thu theo gói cước tháng và bán lẻ truyện theo khoảng thời gian.

---

## 🔑 Tài khoản thử nghiệm (Demo Accounts)

### 1. Tài khoản Quản trị (Admin)
- **Đường dẫn:** `/Admin/Home`
- **Email:** `admin@doctruyen.com`
- **Mật khẩu:** `123456`

### 2. Tài khoản Thành viên (Member)
- **Email:** `member@doctruyen.com`
- **Mật khẩu:** `123456`

---

## 💻 Hướng dẫn chạy dự án dưới Local

1. **Yêu cầu hệ thống:**
   - Visual Studio 2022 (chọn gói *ASP.NET and web development*).
   - Android Studio (dành cho phát triển/kiểm thử ứng dụng Mobile).
   - SQL Server Management Studio (SSMS).

2. **Các bước thiết lập:**
   - Clone repository dự án về máy.
   - Mở SQL Server, tạo cơ sở dữ liệu `db68785` và chạy file script trong thư mục `/SQL` để khởi tạo các bảng (`dbo.Truyen`, `dbo.ChuongTruyen`, `dbo.NguoiDung`, v.v.).
   - Mở file solution `WebDocTruyen.sln` bằng Visual Studio.
   - Cập nhật chuỗi kết nối `DefaultConnection` trong file `appsettings.json` / `web.config` trỏ về SQL Server local của bạn.
   - Restore NuGet Packages và nhấn **F5** để khởi chạy Web App & RESTful API (hỗ trợ Swagger UI tại `/swagger`).
   - Mở project Android bằng Android Studio, cấu hình Base URL trỏ đến API Backend để chạy Mobile App trên máy ảo Android.

---

## 📝 Nhật ký cập nhật tiến độ công việc - Đặng Duy An & Team

#### 1. Phân hệ Backend API & Cơ sở dữ liệu
- **Thiết kế CSDL chuẩn hóa (SQL Server):** Khởi tạo thành công tập hợp các bảng bao gồm `dbo.Truyen`, `dbo.ChuongTruyen`, `dbo.NguoiDung`, `dbo.GoiDoc`, `dbo.TruyenDaMua`, `dbo.TienDoDoc`, `dbo.LichSuGiaoDich`, `dbo.BinhLuanChuong`, v.v.
- **Tích hợp Entity Framework Core:** Kết nối CSDL theo mô hình Database First, tự động sinh các class Model và `DbContext` phục vụ truy vấn.
- **Cấu hình Xác thực JWT & Cookie:** Thiết lập cơ chế cấp phát JWT Token lưu trong HttpOnly Cookie cho trình duyệt Web và trả Bearer Token qua Header cho ứng dụng Mobile.

#### 2. Phân hệ Quản lý Nội dung Truyện & Chương (Admin)
- **Quản lý Đa thể loại (Multi-category):** Xây dựng bảng trung gian `dbo.TruyenTheLoai` cho phép gán một bộ truyện vào nhiều thể loại. Admin chọn thể loại qua Checkbox và tự động gộp chuỗi hiển thị ra màn hình người đọc.
- **Quản lý danh mục chương & Phân quyền truy cập:** Thiết lập thuộc tính cho phép cài đặt trạng thái Miễn phí (Free) hoặc Trả phí (VIP) cho từng chương truyện.
- **Xử lý Upload ảnh bìa:** Upload hình ảnh bìa truyện sử dụng `Guid.NewGuid().ToString()` để đặt tên file chống ghi đè dữ liệu.

#### 3. Phân hệ Đọc truyện & Lưu tiến độ (User / Mobile)
- **Lưu tiến độ đọc tự động (`dbo.TienDoDoc`):** Xây dựng API ghi nhận vị trí chương truyện người đọc đang xem dở, cho phép tiếp tục đọc chính xác ở lần truy cập tiếp theo trên cả Web và App.
- **Khu vực thảo luận chương (`dbo.BinhLuanChuong`):** Tích hợp AJAX tải danh sách bình luận động theo từng chương, cho phép thành viên gửi nhận xét và Admin có thể ẩn/xóa bình luận vi phạm.

#### 4. Tích hợp Thanh toán & Kiểm tra Quyền đọc
- **Xử lý cổng thanh toán:** Tích hợp thành công SDK VNPay và PayPal để xử lý giao dịch mua gói tháng hoặc mua lẻ bộ truyện.
- **Logic kiểm tra quyền truy cập:** Xây dựng Middleware / ActionFilter kiểm tra tự động: Nếu truyện miễn phí -> cho phép đọc; Nếu trả phí -> kiểm tra gói tháng còn hiệu lực (`dbo.NguoiDungGoiDoc`) HOẶC đã mua lẻ truyện (`dbo.TruyenDaMua`) trước khi trả nội dung chương.
