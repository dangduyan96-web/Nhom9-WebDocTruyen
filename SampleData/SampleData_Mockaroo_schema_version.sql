/* =====================================================================
   DỮ LIỆU MẪU - ĐỒ ÁN WebDocTruyen (Nhóm 9)
   - NguoiDung (30), TheLoai (6), Truyen (12), ChuongTruyen (36), GoiDoc (3) và 11 bảng còn lại: tất cả sinh bằng Mockaroo (schema đã lưu trong tài khoản)
   Thứ tự chạy tuân theo khóa ngoại. Chạy trên DB TEST/LOCAL, KHÔNG chạy lên DB production.
   ===================================================================== */

-- ---------- 1. VaiTro (Mockaroo, 4 dòng; MaVaiTro không tự tăng) ----------
INSERT INTO VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
 (1, N'Admin',        N'Quản trị viên hệ thống'),
 (2, N'Biên tập viên',N'Duyệt và quản lý nội dung'),
 (3, N'Tác giả',      N'Đăng truyện và chương'),
 (4, N'Thành viên',   N'Độc giả thông thường');

-- ---------- 2. NguoiDung (Mockaroo, 30 dòng) ----------
SET IDENTITY_INSERT NguoiDung ON;
INSERT INTO NguoiDung (MaNguoiDung, TenDangNhap, HoTen, Email, MaVaiTro, NgayTao, MatKhauHash, IsKhoa) VALUES
(1, 'bpizzey0', 'Bryn Pizzey', 'bpizzey0@apple.com', 1, '2026-10-21 18:23:48', '$2a$04$fyJzuVBqNZOIr0Kg3DU8uuSwVABkp3z1bd8jH9MPnSv4tWSm2U10e', 0),
(2, 'gpaydon1', 'Gloria Paydon', 'gpaydon1@liveinternet.ru', 3, '2026-07-31 11:54:27', '$2a$04$bGdTc98FYPRL4tQlcio3n.0/UC3OkshwnTogvzBPG2n.DiW/sbtZm', 0),
(3, 'ncrosland2', 'Nessie Crosland', 'ncrosland2@abc.net.au', 3, '2026-05-14 13:05:12', '$2a$04$Zh/2xMoghcNs69n1enUCT.OSffeHSEFTnPYr86tqo06eEhNSbRpZu', 0),
(4, 'skirsche3', 'Sydney Kirsche', 'skirsche3@time.com', 3, '2026-06-17 03:17:35', '$2a$04$whOKNRdgdEa.8thO03czCeRoRAfILIW.0iSnOvxr1mqg2uzrdPaTS', 0),
(5, 'dstearndale4', 'Dale Stearndale', 'dstearndale4@dyndns.org', 4, '2026-06-09 13:47:54', '$2a$04$fY9bp8uRIr9y495S4BbBA.8eArpLl7kFWj9UKYbu1xR8WDHoSyzXa', 0),
(6, 'rtemblett5', 'Rudyard Temblett', 'rtemblett5@purevolume.com', 4, '2026-01-28 01:50:27', '$2a$04$u9EDuJrAvxAnEeyf73241.ewSXSYaKiKAMuN.RVTjVrVktYif0Bdy', 1),
(7, 'dpennacci6', 'Druci Pennacci', 'dpennacci6@scribd.com', 4, '2026-03-28 22:48:56', '$2a$04$qtPemkoMFGXp.XYAa8Hmf.En8WqgjumH.VNEE./zoODAuATpFeR0i', 0),
(8, 'pstrelitzer7', 'Phil Strelitzer', 'pstrelitzer7@arizona.edu', 3, '2026-04-17 18:23:54', '$2a$04$kLCh4Uv8x3Q7A0ZZcx8IDOcmlouB514ba6P9ae.gzQmE958HutoKm', 0),
(9, 'rhamflett8', 'Rachel Hamflett', 'rhamflett8@jalbum.net', 4, '2026-07-21 13:52:37', '$2a$04$qgcSpPUk3B6/tgV.Ghyafe0ivkcWIN4yXL5oPiioTpOImuAszljyO', 0),
(10, 'ahearnes9', 'Alma Hearnes', 'ahearnes9@hugedomains.com', 4, '2026-10-24 16:45:15', '$2a$04$MeeTYbJKR.wMy/wlyMCfb.UhxjrYw4Ikr9fViY73sfD53mt2tu5Ru', 0),
(11, 'apavinesea', 'Annabella Pavinese', 'apavinesea@microsoft.com', 3, '2026-09-20 04:19:32', '$2a$04$sYWm0p.2i0kcKuhE2FfmZeS7blPE5m8szZ/zJGiajGzTZBXfWH/4S', 0),
(12, 'iollertonb', 'Ilyssa Ollerton', 'iollertonb@bloomberg.com', 4, '2025-11-29 21:56:55', '$2a$04$BdBbhfDlPHIClNO3dKwAAuCKmH/f3fvnuZuQe8poPF8SC/0zRoQSK', 0),
(13, 'lcrissilc', 'Lin Crissil', 'lcrissilc@marriott.com', 4, '2025-12-03 01:15:30', '$2a$04$ThY0VJ86uH6Vu1HNzFBi.Ox4osxTv8MkA3/AI9aoT3HCJsHmQ0qWG', 0),
(14, 'ahameld', 'Ashlin Hamel', 'ahameld@tiny.cc', 4, '2026-06-06 16:55:33', '$2a$04$1AAHnAvUm2FnJ/bQkf5RYu0bYmVd5yTepNGODPjNOo8BVmIyZdufC', 1),
(15, 'kgreime', 'Kelwin Greim', 'kgreime@cnbc.com', 3, '2026-10-06 10:16:50', '$2a$04$4XJduGCOdQlWfwtz.cH/uusoelx55AM6HNyJ6zy7wCgSmGOLK629y', 0),
(16, 'clantf', 'Clareta Lant', 'clantf@vinaora.com', 4, '2026-03-01 17:12:00', '$2a$04$BWjDDoodeH2wNLiPW5p7rOgOpt8yV6PQ0PaUOAtSMMQmSQ/17cHd6', 0),
(17, 'bsarreg', 'Betteanne Sarre', 'bsarreg@networkadvertising.org', 4, '2026-03-01 10:03:41', '$2a$04$jEz3fEjNyIuAfYoRzsc4wuXl1dys8lxGKhLYjW9Uh4GJ1QPfRgWfG', 0),
(18, 'lcoppinh', 'Lorianne Coppin', 'lcoppinh@dailymotion.com', 3, '2026-10-27 18:54:27', '$2a$04$hV7ow/kyFSTSDMMRGZTk0uZ5N0M7cubfTO7KHk6zD7jEVJzFE9zJy', 0),
(19, 'asneydi', 'Amaleta Sneyd', 'asneydi@newsvine.com', 4, '2026-08-29 23:39:06', '$2a$04$QKT7NKjWIUZ81dRdwLYHreS.9K8aW9ygqiudCvjAXdf6pGkmdsHsO', 0),
(20, 'scarlyonj', 'Suzanna Carlyon', 'scarlyonj@google.ca', 4, '2026-08-04 17:16:22', '$2a$04$ZeqYNECdXPE/r/Y9bNlbaO322vvQroN6GBBl7JP8CFlopjlvd02ye', 0),
(21, 'ctriponk', 'Con Tripon', 'ctriponk@infoseek.co.jp', 3, '2026-03-01 06:53:35', '$2a$04$X/vLF0clIrlTN9hPxFy30./adYN0En2z5Wr09/9jjfXTGUweeb1ca', 0),
(22, 'jlorrainel', 'Jessamyn Lorraine', 'jlorrainel@slashdot.org', 4, '2026-06-17 20:01:10', '$2a$04$6q.enhsPY8bxKAdQwhDkAeSU39Yb25CZF8.8ZenAQXVPBSqNDRaEi', 0),
(23, 'ikelsellm', 'Iona Kelsell', 'ikelsellm@utexas.edu', 3, '2026-10-10 20:04:21', '$2a$04$RV7hszveSIkErWLbcyB.3.uuSgbYWWQmwhi9OHKZ/wTGsc0UE2gs2', 0),
(24, 'nsumshonn', 'Noam Sumshon', 'nsumshonn@huffingtonpost.com', 4, '2026-03-10 23:35:27', '$2a$04$OX/t7zk5twVOBaHvsRDZPOfXzry9cMh9JY1CsvaQneqH/f0ZcIZfm', 0),
(25, 'ljeanessono', 'Lazaro Jeanesson', 'ljeanessono@imdb.com', 4, '2026-05-03 14:46:50', '$2a$04$370lq.zEuv1AvHyf3pHQjOvXtAXYvAW6hThE/f7/kTQnBkeoEhlXm', 0),
(26, 'dwedgep', 'Dione Wedge', 'dwedgep@deliciousdays.com', 4, '2026-07-08 03:07:37', '$2a$04$AHdf03EJ0l4HhZ4y15mHU.zl6wblhqGXUt0xDxx0kfyKCjoFDJmne', 0),
(27, 'hwalentaq', 'Hogan Walenta', 'hwalentaq@e-recht24.de', 4, '2026-08-23 23:08:36', '$2a$04$heq2FI8wtlpXXyN/MJ488OOaYImkpR/aH0XbCwUWG21J/LQILO1Su', 0),
(28, 'mannakinr', 'Melody Annakin', 'mannakinr@intel.com', 4, '2026-01-06 16:11:39', '$2a$04$DZj8VVDa0rffuaBi5Lpdse0PJIcVzrOBwUE.ay08muOnakhmSDKcS', 0),
(29, 'jiltchevs', 'Joscelin Iltchev', 'jiltchevs@flavors.me', 4, '2026-07-21 02:53:15', '$2a$04$GoG/ktFEH/U.xVXFA0suCudFxjGvppbOZUiPPq8X4EigPFOfS5jYe', 0),
(30, 'sfaucettt', 'Shaughn Faucett', 'sfaucettt@opensource.org', 4, '2026-06-03 23:40:06', '$2a$04$uEqaIVC0t0Dy7L/N3NGtt.AT6dQw9SCR65Qfn4nz9HD6PL0BWU/mS', 0);
SET IDENTITY_INSERT NguoiDung OFF;

-- ---------- 3. TheLoai (Mockaroo, 6 dòng) ----------
SET IDENTITY_INSERT TheLoai ON;
INSERT INTO TheLoai (MaTheLoai, TenTheLoai, Slug, MoTa) VALUES
(1, N'Tiên hiệp', 'tien-hiep', N'Tu luyện thành tiên'),
(2, N'Huyền huyễn', 'huyen-huyen', N'Thế giới huyền ảo'),
(3, N'Kiếm hiệp', 'kiem-hiep', N'Giang hồ võ lâm'),
(4, N'Ngôn tình', 'ngon-tinh', N'Tình cảm lãng mạn'),
(5, N'Đô thị', 'do-thi', N'Bối cảnh hiện đại'),
(6, N'Khoa huyễn', 'khoa-huyen', N'Khoa học viễn tưởng');
SET IDENTITY_INSERT TheLoai OFF;

-- ---------- 4. Truyen (Mockaroo, 12 dòng; bổ sung NguoiTao, ngày) ----------
SET IDENTITY_INSERT Truyen ON;
INSERT INTO Truyen (MaTruyen, TenTruyen, Slug, TacGia, MoTa, TrangThai, ChinhSachTruyCap, GiaBan, LuotXem, NguoiTao) VALUES
(1, N'Tiên Nghịch', 'truyen-1', 'Denys Brain', N'Hành trình tu luyện đầy gian nan', N'Đang phát hành', 'Free', 0, 4197, 2),
(2, N'Vũ Động Càn Khôn', 'truyen-2', 'Oby Chevin', N'Truyện tiên hiệp hấp dẫn', N'Đang phát hành', 'Free', 0, 1176, 3),
(3, N'Kiếm Lai', 'truyen-3', 'Pollyanna Wigin', N'Hành trình tu luyện đầy gian nan', N'Hoàn thành', 'Free', 0, 3793, 4),
(4, N'Tiên Nghịch 2', 'truyen-4', 'Wilburt Martinets', N'Một thế giới huyền huyễn rộng lớn', N'Đang phát hành', 'Free', 0, 1389, 2),
(5, N'Ma Thiên Ký', 'truyen-5', 'Prissie Harland', N'Một thế giới huyền huyễn rộng lớn', N'Đang phát hành', 'Paid', 30000, 1567, 8),
(6, N'Vũ Động Càn Khôn 2', 'truyen-6', 'Artemis Lowder', N'Hành trình tu luyện đầy gian nan', N'Đang phát hành', 'Paid', 30000, 1508, 11),
(7, N'Tiên Nghịch 3', 'truyen-7', 'Crystie Frayling', N'Hành trình tu luyện đầy gian nan', N'Hoàn thành', 'Paid', 30000, 4871, 3),
(8, N'Nguyên Tôn', 'truyen-8', 'Zorine Passmore', N'Truyện tiên hiệp hấp dẫn', N'Đang phát hành', 'Free', 0, 2256, 15),
(9, N'Thôn Phệ Tinh Không', 'truyen-9', 'Guy Ragless', N'Truyện tiên hiệp hấp dẫn', N'Đang phát hành', 'Free', 0, 2272, 18),
(10, N'Hắc Ám Chi Tâm', 'truyen-10', 'Yancey Mathevet', N'Một thế giới huyền huyễn rộng lớn', N'Đang phát hành', 'Free', 0, 2723, 21),
(11, N'Linh Vực', 'truyen-11', 'Johnathan Swyer-Sexey', N'Hành trình tu luyện đầy gian nan', N'Đang phát hành', 'Free', 0, 4522, 23),
(12, N'Long Tộc', 'truyen-12', 'Hanna Rouke', N'Một thế giới huyền huyễn rộng lớn', N'Hoàn thành', 'Free', 0, 1817, 2);
SET IDENTITY_INSERT Truyen OFF;
-- (Mockaroo cho phép tên truyện trùng nhau, nên các bản trùng đã được đánh số "2", "3" để dễ phân biệt)

-- ---------- 5. TruyenTheLoai (Mockaroo, 12 dòng: mỗi truyện 1 thể loại) ----------
INSERT INTO TruyenTheLoai (MaTruyen, MaTheLoai) VALUES
(1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (7, 1), (8, 2), (9, 3), (10, 4), (11, 5), (12, 6);

-- ---------- 6. ChuongTruyen (Mockaroo, 36 dòng: 12 truyện x 3 chương, chương 3 trả phí) ----------
SET IDENTITY_INSERT ChuongTruyen ON;
INSERT INTO ChuongTruyen (MaChuong, MaTruyen, SoThuTuChuong, TieuDe, NoiDung, MienPhi, LuotXem) VALUES
(1, 1, 1, N'Chương 1', N'Nullam porttitor lacus at turpis. Donec posuere metus vitae ipsum.', 1, 94),
(2, 1, 2, N'Chương 2', N'Integer ac leo.', 1, 53),
(3, 1, 3, N'Chương 3', N'Nam congue, risus semper porta volutpat, quam pede lobortis ligula, sit amet eleifend pede libero quis orci.', 0, 93),
(4, 2, 1, N'Chương 1', N'Vestibulum quam sapien, varius ut, blandit non, interdum in, ante. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Duis faucibus accumsan odio.', 1, 29),
(5, 2, 2, N'Chương 2', N'Morbi odio odio, elementum eu, interdum eu, tincidunt in, leo. Maecenas pulvinar lobortis est.', 1, 31),
(6, 2, 3, N'Chương 3', N'Integer a nibh.', 0, 96),
(7, 3, 1, N'Chương 1', N'Duis aliquam convallis nunc.', 1, 53),
(8, 3, 2, N'Chương 2', N'Nulla facilisi.', 1, 12),
(9, 3, 3, N'Chương 3', N'Integer non velit.', 0, 95),
(10, 4, 1, N'Chương 1', N'Donec semper sapien a libero. Nam dui.', 1, 98),
(11, 4, 2, N'Chương 2', N'Vivamus vel nulla eget eros elementum pellentesque. Quisque porta volutpat erat.', 1, 48),
(12, 4, 3, N'Chương 3', N'Maecenas rhoncus aliquam lacus. Morbi quis tortor id nulla ultrices aliquet.', 0, 62),
(13, 5, 1, N'Chương 1', N'Proin risus. Praesent lectus.', 1, 52),
(14, 5, 2, N'Chương 2', N'Maecenas pulvinar lobortis est. Phasellus sit amet erat.', 1, 70),
(15, 5, 3, N'Chương 3', N'Duis aliquam convallis nunc. Proin at turpis a pede posuere nonummy.', 0, 76),
(16, 6, 1, N'Chương 1', N'Morbi sem mauris, laoreet ut, rhoncus aliquet, pulvinar sed, nisl. Nunc rhoncus dui vel sem.', 1, 100),
(17, 6, 2, N'Chương 2', N'Aliquam sit amet diam in magna bibendum imperdiet. Nullam orci pede, venenatis non, sodales sed, tincidunt eu, felis.', 1, 17),
(18, 6, 3, N'Chương 3', N'Nullam molestie nibh in lectus. Pellentesque at nulla.', 0, 46),
(19, 7, 1, N'Chương 1', N'Sed sagittis.', 1, 78),
(20, 7, 2, N'Chương 2', N'Nulla tempus.', 1, 97),
(21, 7, 3, N'Chương 3', N'Pellentesque ultrices mattis odio.', 0, 80),
(22, 8, 1, N'Chương 1', N'Lorem ipsum dolor sit amet, consectetuer adipiscing elit.', 1, 33),
(23, 8, 2, N'Chương 2', N'Morbi sem mauris, laoreet ut, rhoncus aliquet, pulvinar sed, nisl.', 1, 56),
(24, 8, 3, N'Chương 3', N'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Nulla dapibus dolor vel est.', 0, 40),
(25, 9, 1, N'Chương 1', N'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec pharetra, magna vestibulum aliquet ultrices, erat tortor sollicitudin mi, sit amet lobortis sapien sapien non mi. Integer ac neque.', 1, 78),
(26, 9, 2, N'Chương 2', N'Pellentesque ultrices mattis odio. Donec vitae nisi.', 1, 7),
(27, 9, 3, N'Chương 3', N'Maecenas leo odio, condimentum id, luctus nec, molestie sed, justo.', 0, 7),
(28, 10, 1, N'Chương 1', N'Duis consequat dui nec nisi volutpat eleifend.', 1, 80),
(29, 10, 2, N'Chương 2', N'Aenean auctor gravida sem.', 1, 28),
(30, 10, 3, N'Chương 3', N'Nam dui. Proin leo odio, porttitor id, consequat in, consequat ut, nulla.', 0, 14),
(31, 11, 1, N'Chương 1', N'Curabitur in libero ut massa volutpat convallis. Morbi odio odio, elementum eu, interdum eu, tincidunt in, leo.', 1, 47),
(32, 11, 2, N'Chương 2', N'Cras mi pede, malesuada in, imperdiet et, commodo vulputate, justo. In blandit ultrices enim.', 1, 6),
(33, 11, 3, N'Chương 3', N'Nam ultrices, libero non mattis pulvinar, nulla pede ullamcorper augue, a suscipit nulla elit ac nulla.', 0, 74),
(34, 12, 1, N'Chương 1', N'Nam dui. Proin leo odio, porttitor id, consequat in, consequat ut, nulla.', 1, 5),
(35, 12, 2, N'Chương 2', N'In est risus, auctor sed, tristique in, tempus sit amet, sem. Fusce consequat.', 1, 81),
(36, 12, 3, N'Chương 3', N'Donec ut dolor. Morbi vel lectus in quam fringilla rhoncus.', 0, 32);
SET IDENTITY_INSERT ChuongTruyen OFF;

-- ---------- 7. GoiDoc (Mockaroo, 3 dòng) ----------
SET IDENTITY_INSERT GoiDoc ON;
INSERT INTO GoiDoc (MaGoiDoc, TenGoi, GiaTien, SoNgayHieuLuc, MoTa, IsHoatDong) VALUES
(1, N'Gói 1 tháng', 50000, 30, N'Đọc không giới hạn 30 ngày', 1),
(2, N'Gói 3 tháng', 129000, 90, N'Đọc không giới hạn 90 ngày', 1),
(3, N'Gói 1 năm', 450000, 365, N'Đọc không giới hạn 365 ngày', 1);
SET IDENTITY_INSERT GoiDoc OFF;

-- ---------- 8. KhuyenMai (Mockaroo, 3 dòng) ----------
SET IDENTITY_INSERT KhuyenMai ON;
INSERT INTO KhuyenMai (MaKhuyenMai, MaGiamGia, SoTienGiam, NgayBatDau, NgayKetThuc, IsHoatDong) VALUES
(1, 'WELCOME10', 10000, '2026-01-01', '2026-12-31', 1),
(2, 'TET2026', 20000, '2026-01-15', '2026-03-01', 0),
(3, 'HE2026', 15000, '2026-06-01', '2026-08-31', 0);
SET IDENTITY_INSERT KhuyenMai OFF;

-- ---------- 9. NguoiDungGoiDoc (Mockaroo, 8 dòng) ----------
SET IDENTITY_INSERT NguoiDungGoiDoc ON;
INSERT INTO NguoiDungGoiDoc (MaNguoiDungGoiDoc, MaNguoiDung, MaGoiDoc, NgayBatDau, NgayKetThuc, TrangThai) VALUES
(1, 5, 1, '2026-11-02', '2026-12-02', 'Active'),
(2, 7, 2, '2026-10-23', '2027-01-21', 'Active'),
(3, 9, 3, '2026-08-04', '2027-08-04', 'Active'),
(4, 10, 1, '2026-10-03', '2026-11-02', 'Expired'),
(5, 12, 2, '2026-11-07', '2027-02-05', 'Active'),
(6, 13, 1, '2026-11-12', '2026-12-12', 'Active'),
(7, 16, 3, '2026-04-26', '2027-04-26', 'Active'),
(8, 19, 1, '2026-10-28', '2026-11-27', 'Active');
SET IDENTITY_INSERT NguoiDungGoiDoc OFF;

-- ---------- 10. LichSuGiaoDich (Mockaroo, 11 dòng) ----------
SET IDENTITY_INSERT LichSuGiaoDich ON;
INSERT INTO LichSuGiaoDich (MaGiaoDich, MaNguoiDung, LoaiGiaoDich, MaMụcTieu, SoTien, MaGiamGia, PhuongThucThanhToan, TrangThai, NgayTao) VALUES
(1, 5, 'GoiDoc', 1, 50000, NULL, 'QR', N'Thành công', '2026-01-16 07:33:59'),
(2, 7, 'GoiDoc', 2, 119000, 'WELCOME10', 'VNPAY', N'Thành công', '2025-11-14 04:16:10'),
(3, 9, 'GoiDoc', 3, 450000, NULL, 'VNPAY', N'Thành công', '2026-05-14 22:37:09'),
(4, 10, 'GoiDoc', 1, 50000, NULL, 'VNPAY', N'Thành công', '2026-07-29 21:11:56'),
(5, 12, 'GoiDoc', 2, 129000, NULL, 'PayPal', N'Thành công', '2026-03-12 06:08:23'),
(6, 13, 'GoiDoc', 1, 50000, NULL, 'VNPAY', N'Thành công', '2026-06-12 13:20:56'),
(7, 16, 'GoiDoc', 3, 450000, NULL, 'VNPAY', N'Thành công', '2026-04-05 22:05:33'),
(8, 19, 'GoiDoc', 1, 50000, NULL, 'VNPAY', N'Thành công', '2026-07-27 09:27:24'),
(9, 12, 'MuaTruyen', 5, 30000, NULL, 'QR', N'Thành công', '2025-12-23 23:08:53'),
(10, 13, 'MuaTruyen', 6, 30000, NULL, 'QR', N'Thành công', '2025-11-29 18:54:02'),
(11, 16, 'MuaTruyen', 7, 30000, NULL, 'VNPAY', N'Thành công', '2026-10-24 17:39:09');
SET IDENTITY_INSERT LichSuGiaoDich OFF;

-- ---------- 11. TruyenDaMua (Mockaroo, 3 dòng) ----------
INSERT INTO TruyenDaMua (MaNguoiDung, MaTruyen, NgayMua, GiaDaTra) VALUES
(12, 5, '2026-05-25 10:50:45', 30000),
(13, 6, '2026-07-06 03:19:21', 30000),
(16, 7, '2026-03-12 07:02:36', 30000);

-- ---------- 12. TruyenYeuThich (Mockaroo + Dataset, 20 dòng) ----------
INSERT INTO TruyenYeuThich (MaNguoiDung, MaTruyen, NgayTao) VALUES
(1, 12, '2026-10-24 11:52:16'),
(2, 9, '2026-08-25 12:36:14'),
(3, 1, '2026-08-20 12:24:22'),
(4, 8, '2026-06-25 14:10:53'),
(5, 8, '2026-08-04 16:14:38'),
(6, 12, '2026-10-17 05:36:10'),
(7, 12, '2026-02-21 03:37:28'),
(8, 7, '2025-11-23 15:59:05'),
(9, 2, '2026-09-14 09:27:12'),
(10, 1, '2026-08-09 17:40:29'),
(11, 5, '2026-03-23 21:25:19'),
(12, 1, '2026-02-25 18:29:47'),
(13, 1, '2026-07-02 00:38:02'),
(14, 2, '2026-10-02 00:37:20'),
(15, 9, '2026-03-16 01:43:42'),
(16, 4, '2026-10-17 23:26:06'),
(17, 5, '2025-11-21 19:29:29'),
(18, 5, '2026-09-27 11:25:37'),
(19, 8, '2026-03-01 06:39:03'),
(20, 2, '2026-01-13 17:20:13');

-- ---------- 13. TienDoDoc (Mockaroo + Dataset, 20 dòng) ----------
INSERT INTO TienDoDoc (MaNguoiDung, MaTruyen, MaChuongGanNhat, SoLanDoc, NgayCapNhat) VALUES
(1, 7, 20, 2, '2026-05-23 19:51:54'),
(2, 3, 9, 5, '2026-07-27 09:13:51'),
(3, 2, 4, 3, '2026-03-28 15:19:36'),
(4, 2, 4, 4, '2026-02-26 01:13:38'),
(5, 6, 18, 5, '2026-08-10 20:57:22'),
(6, 2, 4, 2, '2026-03-22 06:30:46'),
(7, 12, 35, 4, '2026-02-18 06:16:25'),
(8, 3, 9, 3, '2026-08-07 04:22:29'),
(9, 1, 2, 2, '2026-07-02 23:39:02'),
(10, 8, 22, 5, '2026-10-25 14:39:17'),
(11, 4, 12, 1, '2026-03-14 17:04:12'),
(12, 8, 23, 2, '2026-09-17 23:45:30'),
(13, 7, 20, 1, '2026-10-23 18:31:23'),
(14, 5, 14, 1, '2026-08-06 05:00:39'),
(15, 10, 28, 4, '2025-11-27 09:26:43'),
(16, 8, 23, 2, '2026-10-10 20:55:53'),
(17, 5, 15, 5, '2026-06-14 08:03:31'),
(18, 4, 11, 2, '2026-05-05 18:36:07'),
(19, 3, 8, 3, '2026-01-23 16:27:35'),
(20, 5, 14, 4, '2026-10-01 11:34:12');

-- ---------- 14. DanhGiaTruyen (Mockaroo, 20 dòng) ----------
SET IDENTITY_INSERT DanhGiaTruyen ON;
INSERT INTO DanhGiaTruyen (MaDanhGia, MaNguoiDung, MaTruyen, SoSao, NoiDungDanhGia, NgayTao) VALUES
(1, 1, 6, 5, 'In est risus, auctor sed, tristique in, tempus sit amet, sem. Fusce consequat.', '2026-09-22 08:27:23'),
(2, 2, 11, 3, 'Aenean sit amet justo. Morbi ut odio.', '2026-01-23 10:42:29'),
(3, 3, 4, 5, 'Morbi odio odio, elementum eu, interdum eu, tincidunt in, leo. Maecenas pulvinar lobortis est.', '2025-11-28 22:10:31'),
(4, 4, 9, 5, 'Sed sagittis. Nam congue, risus semper porta volutpat, quam pede lobortis ligula, sit amet eleifend pede libero quis orci.', '2025-11-25 18:06:52'),
(5, 5, 2, 3, 'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec pharetra, magna vestibulum aliquet ultrices, erat tortor sollicitudin mi, sit amet lobortis sapien sapien non mi. Integer ac neque.', '2025-11-28 23:29:05'),
(6, 6, 7, 4, 'Aenean lectus.', '2026-05-07 14:50:08'),
(7, 7, 12, 4, 'Nulla neque libero, convallis eget, eleifend luctus, ultricies eu, nibh. Quisque id justo sit amet sapien dignissim vestibulum.', '2026-11-10 07:08:36'),
(8, 8, 5, 4, 'In hac habitasse platea dictumst. Etiam faucibus cursus urna.', '2026-07-24 02:17:28'),
(9, 9, 10, 5, 'Vivamus metus arcu, adipiscing molestie, hendrerit at, vulputate vitae, nisl. Aenean lectus.', '2026-01-09 12:52:38'),
(10, 10, 3, 3, 'Morbi ut odio.', '2026-05-20 15:58:49'),
(11, 11, 8, 5, 'Ut tellus.', '2026-06-01 23:08:40'),
(12, 12, 1, 3, 'Suspendisse potenti. Nullam porttitor lacus at turpis.', '2026-01-17 04:22:06'),
(13, 13, 6, 5, 'Nam dui. Proin leo odio, porttitor id, consequat in, consequat ut, NULLa.', '2026-02-18 12:43:55'),
(14, 14, 11, 3, 'Suspendisse accumsan tortor quis turpis.', '2025-11-14 00:02:21'),
(15, 15, 4, 4, 'Aliquam sit amet diam in magna bibendum imperdiet.', '2026-09-22 09:07:03'),
(16, 16, 9, 4, 'Nullam varius. Nulla facilisi.', '2026-02-20 02:49:36'),
(17, 17, 2, 3, 'Nunc purus. Phasellus in felis.', '2026-01-06 12:53:21'),
(18, 18, 7, 4, 'Nullam varius.', '2026-08-22 21:50:10'),
(19, 19, 12, 5, 'Nam ultrices, libero non mattis pulvinar, NULLa pede ullamcorper augue, a suscipit NULLa elit ac NULLa.', '2026-08-22 00:03:25'),
(20, 20, 5, 5, 'Quisque id justo sit amet sapien dignissim vestibulum. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Nulla dapibus dolor vel est.', '2026-07-06 17:41:48');
SET IDENTITY_INSERT DanhGiaTruyen OFF;

-- ---------- 15. BinhLuanChuong (Mockaroo + Dataset, 20 dòng) ----------
SET IDENTITY_INSERT BinhLuanChuong ON;
INSERT INTO BinhLuanChuong (MaBinhLuan, MaChuong, MaNguoiDung, NoiDung, IsAn, NgayTao) VALUES
(1, 30, 13, 'Praesent lectus.', 0, '2026-01-14 07:31:35'),
(2, 23, 17, 'Donec dapibus.', 0, '2026-03-31 21:51:06'),
(3, 34, 27, 'Sed accumsan felis.', 0, '2026-01-01 03:43:47'),
(4, 7, 27, 'Maecenas ut massa quis augue luctus tincidunt.', 0, '2026-05-10 20:24:28'),
(5, 17, 6, 'Etiam faucibus cursus urna. Ut tellus.', 0, '2025-11-14 12:55:31'),
(6, 8, 26, 'Sed accumsan felis. Ut at dolor quis odio consequat varius.', 0, '2026-05-28 14:33:32'),
(7, 23, 8, 'Aliquam quis turpis eget elit sodales scelerisque.', 0, '2026-06-25 08:23:35'),
(8, 7, 24, 'Donec posuere metus vitae ipsum.', 0, '2026-11-09 19:10:42'),
(9, 33, 20, 'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Duis faucibus accumsan odio.', 0, '2026-10-11 14:35:56'),
(10, 30, 21, 'Nam ultrices, libero non mattis pulvinar, NULLa pede ullamcorper augue, a suscipit NULLa elit ac NULLa. Sed vel enim sit amet nunc viverra dapibus.', 0, '2026-09-13 13:24:22'),
(11, 10, 1, 'Mauris ullamcorper purus sit amet NULLa. Quisque arcu libero, rutrum ac, lobortis vel, dapibus at, diam.', 0, '2026-09-06 12:03:48'),
(12, 2, 14, 'Praesent blandit. Nam NULLa.', 0, '2026-10-18 08:40:42'),
(13, 20, 12, 'Integer pede justo, lacinia eget, tincidunt eget, tempus vel, pede. Morbi porttitor lorem id ligula.', 0, '2025-11-25 06:53:07'),
(14, 18, 20, 'Phasellus sit amet erat. Nulla tempus.', 0, '2026-02-21 06:35:03'),
(15, 4, 28, 'Vivamus in felis eu sapien cursus vestibulum.', 0, '2026-06-20 06:42:27'),
(16, 23, 6, 'In hac habitasse platea dictumst. Maecenas ut massa quis augue luctus tincidunt.', 0, '2026-05-07 12:49:03'),
(17, 16, 6, 'Nulla tellus. In sagittis dui vel nisl.', 0, '2025-12-12 17:51:24'),
(18, 19, 26, 'Duis aliquam convallis nunc.', 0, '2026-02-06 06:06:13'),
(19, 15, 11, 'Donec quis orci eget orci vehicula condimentum. Curabitur in libero ut massa volutpat convallis.', 0, '2026-08-11 14:51:14'),
(20, 34, 5, 'Nunc nisl.', 0, '2026-11-01 00:03:47');
SET IDENTITY_INSERT BinhLuanChuong OFF;

-- ---------- 16. TinNhanHoTro (Mockaroo, 6 dòng) ----------
SET IDENTITY_INSERT TinNhanHoTro ON;
INSERT INTO TinNhanHoTro (MaTinNhan, MaNguoiDung, VaiTroNguoiGui, MaNguoiGui, NoiDung, DaDoc, NgayTao) VALUES
(1, 5, 'User', 5, N'Mình không thanh toán được gói đọc, nhờ admin hỗ trợ.', 0, '2026-09-10 00:55:37'),
(2, 5, 'Admin', 1, N'Chào bạn, vui lòng gửi mã giao dịch để được kiểm tra.', 0, '2026-03-19 00:38:40'),
(3, 9, 'User', 9, N'Làm sao để đổi ảnh đại diện?', 0, '2026-05-13 19:31:51'),
(4, 12, 'User', 12, N'Truyện bị lỗi không hiển thị chương 5.', 1, '2025-11-25 10:45:13'),
(5, 5, 'User', 5, N'Mình đã gửi mã giao dịch VNPAY rồi nhé.', 0, '2026-04-14 04:46:11'),
(6, 9, 'Admin', 1, N'Đã cộng gói đọc cho bạn, vui lòng tải lại trang.', 1, '2026-03-03 03:45:28');
SET IDENTITY_INSERT TinNhanHoTro OFF;

