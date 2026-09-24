WebDocTruyen - Hệ Thống Quản Lý & Đọc Truyện Online
Dự án thực hành phát triển hệ thống: Xây dựng nền tảng đọc truyện trực tuyến đa nền tảng (Web Application & Mobile Application) kết nối tập trung qua hệ thống RESTful API, sử dụng C# ASP.NET Core / MVC 5, Entity Framework và Microsoft SQL Server.   

Thành Viên Thực Hiện Dự Án (28/09/2026 - 01/12/2026)
   
Product Owner: Dương Lâm Tuấn Anh (TuanAnhAz123)   

Scrum Master / Dev: Phạm Nguyễn Hoàng Phúc (Saigoz)   

Team Lead / Dev: Đặng Duy An (dangduyan96-web)   

Thành viên Dev: Hoàng Văn Nam (NamHaong)   

Thành viên Dev: Nguyễn Anh Tuấn (anhtuan308)   

Thành viên Dev: Tsằn Hiền Đức (duchien1231-bit)   

🛠️ Công nghệ sử dụng
Backend: C# (.NET / ASP.NET MVC 5 & RESTful API Core).   

ORM: Entity Framework Core / EF6 (Database First).   
+ 1

Frontend Web: HTML5, CSS3, JavaScript, Bootstrap 5.

Mobile App: Android Native (Java/Kotlin) / Android Application.

Xác thực: Session kết hợp JWT (JSON Web Tokens) lưu trong HttpOnly Cookies (Web) & Authorization Header (Mobile App).

Cơ sở dữ liệu: Microsoft SQL Server (db68785).   

Thanh toán: Tích hợp cổng thanh toán VNPay và PayPal.

✨ Các tính năng chính
📖 Dành cho Người đọc (Web & Mobile App)
Duyệt & Tìm kiếm truyện: Xem danh sách truyện mới cập nhật, truyện hot/đọc nhiều, lọc theo thể loại (TheLoai) hoặc tìm kiếm theo tên truyện, tác giả.

Chi tiết truyện: Xem thông tin ảnh bìa, giới thiệu, tác giả, trạng thái phát hành, danh sách chương kèm trạng thái truy cập (Miễn phí / Trả phí).

Đọc truyện & Lưu tiến độ: Đọc các chương truyện miễn phí; tự động ghi nhớ vị trí/chương đang đọc gần nhất (TienDoDoc) để tiếp tục đọc ở lần truy cập sau.

Mua quyền đọc & Đăng ký Gói cước:

Thanh toán theo truyện: Mua lẻ từng bộ truyện trả phí vĩnh viễn (TruyenDaMua).

Gói đọc theo tháng: Đăng ký các gói VIP đọc truyện theo thời hạn (NguoiDungGoiDoc).

Cổng thanh toán: Hỗ trợ thanh toán an toàn qua VNPay và PayPal.

Tương tác & Cộng đồng: Đánh dấu truyện yêu thích (TruyenYeuThich), đánh giá sao (DanhGiaTruyen), thảo luận/bình luận theo từng chương (BinhLuanChuong).

Quản lý tài khoản: Đăng ký, đăng nhập (JWT), xem lịch sử giao dịch (LichSuGiaoDich), quản lý gói cước đang sử dụng và danh sách truyện đã mua.

🛡️ Dành cho Quản trị viên (Admin Area)
Bảng điều khiển (Dashboard): Xem tổng quan số lượng thành viên, lượt đọc toàn hệ thống và biểu đồ doanh thu.

Quản lý Truyện & Đa thể loại: Thêm, sửa, xóa truyện; thiết lập ảnh bìa, tác giả, trạng thái (Đang phát hành / Hoàn thành) và gán nhiều thể loại cho một bộ truyện (TruyenTheLoai).   

Quản lý Chương truyện: Thêm chương mới, sửa nội dung chương, thiết lập số thứ tự, hẹn giờ phát hành và cài đặt trạng thái chương (Miễn phí / Khóa trả phí) (ChuongTruyen).   

Thiết lập Chính sách giá & Gói cước: Cài đặt giá tiền cho từng bộ truyện trả phí; tạo và quản lý các gói đọc VIP theo tháng (GoiDoc).   

Kiểm duyệt nội dung & Thảo luận: Theo dõi, xử lý hoặc xóa các bình luận vi phạm quy định cộng đồng (BinhLuanChuong).   

Quản lý Người dùng & Phân quyền: Quản lý danh sách tài khoản (NguoiDung), phân cấp vai trò (VaiTro), kích hoạt/khóa tài khoản vi phạm.   

Thống kê & Báo cáo:

Thống kê lượt đọc theo truyện, theo chương, lượt theo dõi.

Thống kê doanh thu chi tiết từ Gói cước tháng và Doanh thu mua lẻ từng bộ truyện.

🔑 Tài khoản thử nghiệm (Demo Accounts)
1. Tài khoản Quản trị (Admin)
Đường dẫn Web Admin: /Admin/Home_Page

Email: admin@gmail.com

Mật khẩu: 123456

2. Tài khoản Thành viên (Member / Reader)
Email: reader@webdoctruyen.com

Mật khẩu: 123456

💻 Hướng dẫn chạy dự án dưới Local
Yêu cầu hệ thống:

Cài đặt Visual Studio 2022 (chọn gói ASP.NET and web development).   

Android Studio (cho dự án Mobile App).

Microsoft SQL Server Management Studio (SSMS).   

Các bước thiết lập Backend & Web:

Clone repository này về máy.

Chạy script powershell -ExecutionPolicy Bypass -File .\Initialize-LocalConfig.ps1.

Mở file WebDocTruyen/appsettings.json (hoặc LocalSecrets.config) và cập nhật chuỗi kết nối SQL Server:
"DefaultConnection": "Server=YOUR_SERVER;Database=db68785;Trusted_Connection=True;"   
+ 2

Mở file solution WebDocTruyen.sln bằng Visual Studio.   

Nhấp chuột phải vào Solution -> Chọn Restore NuGet Packages.   

Khởi chạy các file script SQL trong thư mục /SQL để tạo database db68785 và các bảng dữ liệu (dbo.Truyen, dbo.ChuongTruyen, dbo.NguoiDung,...).   

Nhấn F5 để khởi chạy Web App & RESTful API Service (Swagger UI sẽ tự động mở tại /swagger).   

Chạy ứng dụng Android Mobile:

Mở thư mục WebDocTruyen_Mobile bằng Android Studio.

Cập nhật địa chỉ BASE_URL trong file cấu hình API sang IP local của bạn (vd: [http://10.0.2.2:5000/api/](http://10.0.2.2:5000/api/)).

Khởi chạy trên Android Emulator hoặc thiết bị thật.

Cấu hình bí mật (App Settings)
Mã cấu hình bí mật lưu tại appsettings.Development.json hoặc biến môi trường:
JWT_SECRET, VNPAY_TMN_CODE, VNPAY_HASH_SECRET, PAYPAL_CLIENT_ID, PAYPAL_CLIENT_SECRET, SMTP_USER, SMTP_PASS.   

📝 Nhật ký cập nhật tiến độ công việc - Nhóm Phát Triển
1. Phân hệ Đọc truyện & Xử lý Tiến độ (TienDoDoc)
Tự động lưu vị trí đọc: Xây dựng Middleware / API tự động ghi nhận chương và phần trăm nội dung đọc gần nhất của thành viên vào bảng dbo.TienDoDoc.   

Tiếp tục đọc nhanh: Tích hợp nút "Đọc tiếp" tại Trang chủ và Trang cá nhân, giúp người dùng mở ngay lập tức chương truyện đang đọc dở.

2. Kiểm soát Quyền truy cập & Thanh toán (VNPAY & PayPal)
Thuật toán xác thực quyền đọc: Xử lý logic ưu tiên phân quyền: Nếu thành viên có Gói đọc tháng (NguoiDungGoiDoc) còn hiệu lực HOẶC đã Mua lẻ bộ truyện (TruyenDaMua), hệ thống sẽ mở khóa toàn bộ các chương trả phí.   

Tích hợp cổng thanh toán: Hoàn thiện Callback xử lý giao dịch cho VNPay và PayPal SDK. Cập nhật tự động lịch sử giao dịch vào bảng dbo.LichSuGiaoDich và kích hoạt quyền đọc ngay lập tức sau khi thanh toán thành công.   

3. Quản lý Đa thể loại & Cấu trúc Chương truyện
Xử lý Đa thể loại (TruyenTheLoai): Thiết kế bảng trung gian liên kết giữa dbo.Truyen và dbo.TheLoai, hỗ trợ một bộ truyện thuộc nhiều thể loại khác nhau.   

Quản lý chương (ChuongTruyen): Cho phép Admin tải lên nội dung chương, sắp xếp thứ tự chương (SoThuTu), cài đặt hiển thị Đọc thử (Miễn phí) hoặc Khóa nội dung (Trả phí).   

4. Hệ thống Thảo luận & Kiểm duyệt (BinhLuanChuong)
Thảo luận ngữ cảnh: Mỗi chương truyện có khu vực bình luận riêng biệt (dbo.BinhLuanChuong).   

Công cụ kiểm duyệt Admin: Bổ sung trang Admin cho phép lọc bình luận theo truyện/chương, ẩn hoặc xóa các bình luận spam, vi phạm chuẩn mực cộng đồng.

5. Đồng bộ RESTful API & Mobile Application
Bảo mật JWT & Cookie: Thiết lập cơ chế đăng nhập kép: Xuất JWT Token truyền qua HttpOnly Cookie cho Web MVC và trả về chuỗi Token dạng Bearer cho Mobile App.   

Tối ưu hóa Swagger Documentation: Cập nhật đầy đủ các Endpoint API trong Program.cs sử dụng builder.Services.AddSwaggerGen() giúp dev Mobile dễ dàng tra cứu và kết nối dữ liệu.   
