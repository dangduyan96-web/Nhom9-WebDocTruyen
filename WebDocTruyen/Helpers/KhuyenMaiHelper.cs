using WebDocTruyen.Models;
using Microsoft.EntityFrameworkCore;

namespace WebDocTruyen.Helpers
{
    // Kết quả kiểm tra mã giảm giá
    public record KetQuaKhuyenMai(bool HopLe, string ThongBao, string? MaGiamGia, decimal SoTienGiam, decimal GiaSauGiam);

    // Kiểm tra và tính giảm giá cho mã khuyến mãi (dùng chung cho mua truyện và đăng ký gói đọc)
    public static class KhuyenMaiHelper
    {
        // Số tiền tối thiểu phải thanh toán sau khi giảm (cổng thanh toán / QR cần số tiền > 0)
        public const decimal GiaToiThieu = 1000m;

        private static readonly string[] TrangThaiThanhCong = { "Thành công", "Success", "Hoàn thành" };

        public static string ChuanHoaMa(string? ma)
            => new string((ma ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

        public static async Task<KetQuaKhuyenMai> KiemTraAsync(AppDbContext context, string? ma, decimal giaGoc, int? userId)
        {
            string maChuan = ChuanHoaMa(ma);
            if (string.IsNullOrEmpty(maChuan))
                return Loi("Vui lòng nhập mã giảm giá.", giaGoc);

            var km = await context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(k => k.MaGiamGia == maChuan);
            if (km == null)
                return Loi("Mã giảm giá không tồn tại.", giaGoc);

            var now = DateTime.Now;
            if (km.IsHoatDong != true)
                return Loi("Mã giảm giá hiện không được áp dụng.", giaGoc);
            if (now < km.NgayBatDau)
                return Loi($"Mã giảm giá có hiệu lực từ ngày {km.NgayBatDau:dd/MM/yyyy}.", giaGoc);
            if (now > km.NgayKetThuc)
                return Loi("Mã giảm giá đã hết hạn.", giaGoc);

            // Mỗi tài khoản chỉ được dùng một mã một lần (tính các giao dịch đã thành công)
            if (userId.HasValue && await context.LichSuGiaoDiches.AnyAsync(g =>
                    g.MaNguoiDung == userId.Value && g.MaGiamGia == maChuan && TrangThaiThanhCong.Contains(g.TrangThai)))
                return Loi("Bạn đã sử dụng mã giảm giá này rồi.", giaGoc);

            decimal giam = Math.Min(km.SoTienGiam, Math.Max(giaGoc - GiaToiThieu, 0));
            if (giam <= 0)
                return Loi("Mã giảm giá không áp dụng được cho đơn hàng này.", giaGoc);

            return new KetQuaKhuyenMai(true, $"Áp dụng mã thành công! Bạn được giảm {giam:N0} đ.", km.MaGiamGia, giam, giaGoc - giam);
        }

        private static KetQuaKhuyenMai Loi(string thongBao, decimal giaGoc)
            => new KetQuaKhuyenMai(false, thongBao, null, 0, giaGoc);
    }
}
