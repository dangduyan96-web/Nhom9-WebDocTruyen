# -*- coding: utf-8 -*-
"""
Sinh DATASET mẫu cho đồ án WebDocTruyen (Nhóm 9) có suy luận nghiệp vụ.
Chạy:  python build_dataset.py
Đầu ra: datasets/*.csv  và  SampleData_WebDocTruyen.sql

Các ràng buộc/suy luận được áp dụng (xem thêm BaoCao_ThucHanh4.md):
 - 1 Admin, 1 Biên tập viên, 5 Tác giả, 33 Thành viên; 2 tài khoản bị khóa.
 - Truyện do Tác giả tạo; ngày tạo truyện sau ngày tạo tài khoản của tác giả.
 - Truyện Free: mọi chương miễn phí. Truyện Paid: chỉ 3 chương đầu miễn phí, giá phụ thuộc trạng thái.
 - Lượt xem truyện = tổng lượt xem các chương; lượt xem chương giảm dần theo thứ tự chương.
 - Người dùng chỉ yêu thích / đọc / đánh giá / bình luận truyện thuộc thể loại họ quan tâm và họ có quyền đọc
   (truyện Free, hoặc đã mua, hoặc đang có gói VIP).
 - Tiến độ đọc không vượt quá số chương đã phát hành; bình luận chỉ ở chương nằm trong tiến độ đọc.
 - Số sao khớp với nội dung đánh giá (tích cực / trung lập / tiêu cực).
 - Mỗi giao dịch thành công sinh ra đúng 1 bản ghi TruyenDaMua hoặc NguoiDungGoiDoc; số tiền = giá - mã giảm giá.
 - Thời gian luôn hợp lý: ngày hành động > ngày tạo tài khoản, <= hôm nay (2026-11-12).
"""
import csv, os, random, base64, hashlib, unicodedata, re
from datetime import datetime, timedelta

random.seed(9)
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, 'datasets')
os.makedirs(OUT, exist_ok=True)
NOW = datetime(2026, 11, 12, 21, 0, 0)

def strip_accents(s):
    s = s.replace('đ', 'd').replace('Đ', 'D')
    return ''.join(c for c in unicodedata.normalize('NFD', s) if unicodedata.category(c) != 'Mn')
def slugify(s):
    return re.sub(r'[^a-z0-9]+', '-', strip_accents(s).lower()).strip('-')
def rdate(a, b):
    if b <= a: return a
    return a + timedelta(seconds=random.randint(0, int((b - a).total_seconds())))
def fmt(d): return d.strftime('%Y-%m-%d %H:%M:%S')
def q(s):
    return "N'" + str(s).replace("'", "''") + "'"
def pw_hash(password):
    """ASP.NET Core Identity PasswordHasher v3 (PBKDF2-HMACSHA256, 10000 vòng) -> đăng nhập được bằng mật khẩu thật."""
    salt = hashlib.sha256(b'nhom9-salt').digest()[:16]
    sub = hashlib.pbkdf2_hmac('sha256', password.encode(), salt, 10000, 32)
    blob = b'\x01' + (1).to_bytes(4, 'big') + (10000).to_bytes(4, 'big') + (16).to_bytes(4, 'big') + salt + sub
    return base64.b64encode(blob).decode()
PWD = pw_hash('Nhom9@123')

# ---------------------------------------------------------------- VaiTro, TheLoai, GoiDoc, KhuyenMai
vaitro = [(1, 'Admin', 'Quản trị viên hệ thống'), (2, 'Biên tập viên', 'Duyệt và quản lý nội dung'),
          (3, 'Tác giả', 'Đăng truyện và chương'), (4, 'Thành viên', 'Độc giả thông thường')]
theloai = [(1, 'Tiên hiệp', 'Tu luyện, đắc đạo thành tiên'), (2, 'Huyền huyễn', 'Thế giới huyền ảo, đấu khí và ma pháp'),
           (3, 'Kiếm hiệp', 'Giang hồ, võ lâm, hiệp nghĩa'), (4, 'Ngôn tình', 'Chuyện tình cảm lãng mạn'),
           (5, 'Đô thị', 'Bối cảnh đời sống hiện đại'), (6, 'Khoa huyễn', 'Khoa học viễn tưởng, tương lai'),
           (7, 'Trinh thám', 'Phá án, bí ẩn, hồi hộp'), (8, 'Lịch sử', 'Bối cảnh các thời kỳ lịch sử')]
goidoc = [(1, 'Gói 1 tuần', 15000, 7, 'Đọc không giới hạn 7 ngày'), (2, 'Gói 1 tháng', 50000, 30, 'Đọc không giới hạn 30 ngày'),
          (3, 'Gói 3 tháng', 129000, 90, 'Đọc không giới hạn 90 ngày'), (4, 'Gói 1 năm', 450000, 365, 'Đọc không giới hạn 365 ngày')]
khuyenmai = [(1, 'WELCOME10', 10000, '2026-01-01', '2026-12-31', 1), (2, 'TET2026', 20000, '2026-01-15', '2026-03-01', 0),
             (3, 'HE2026', 15000, '2026-06-01', '2026-08-31', 0), (4, 'BLACKFRIDAY', 30000, '2026-11-20', '2026-11-30', 1)]

# ---------------------------------------------------------------- NguoiDung
HO = ['Nguyễn', 'Trần', 'Lê', 'Phạm', 'Hoàng', 'Huỳnh', 'Phan', 'Vũ', 'Võ', 'Đặng', 'Bùi', 'Đỗ', 'Hồ', 'Ngô', 'Dương', 'Lý']
DEM_NAM = ['Văn', 'Hữu', 'Minh', 'Quốc', 'Đức', 'Thành', 'Gia', 'Anh', 'Tuấn', 'Hoàng']
DEM_NU = ['Thị', 'Ngọc', 'Thu', 'Khánh', 'Bảo', 'Thanh', 'Mai', 'Phương', 'Diệu', 'Hồng']
TEN_NAM = ['Nam', 'Long', 'Hiếu', 'Khoa', 'Đạt', 'Phúc', 'Tài', 'Bình', 'Dũng', 'Hải', 'Kiệt', 'Sơn', 'Thắng', 'Huy', 'Việt', 'Trung', 'Quân', 'Toàn']
TEN_NU = ['Lan', 'Hương', 'Linh', 'Trang', 'Mai', 'Ngân', 'Thảo', 'Vy', 'Yến', 'Chi', 'Hà', 'My', 'Nhi', 'Quỳnh', 'Trâm', 'Uyên']
used_names = set()
def make_name(gender):
    while True:
        ho = random.choice(HO); dem = random.choice(DEM_NAM if gender == 'm' else DEM_NU)
        ten = random.choice(TEN_NAM if gender == 'm' else TEN_NU)
        full = f'{ho} {dem} {ten}'
        if full not in used_names:
            used_names.add(full); return full
users = []   # dict
def add_user(role, name, username, created, locked=0, interest=None):
    uid = len(users) + 1
    users.append(dict(id=uid, role=role, username=username, name=name,
                      email=f'{username}@gmail.com', created=created, locked=locked, interest=interest or []))
add_user(1, 'Quản trị viên Nhóm 9', 'admin', datetime(2025, 8, 15, 9, 0, 0))
add_user(2, 'Biên tập viên Nhóm 9', 'bientap', datetime(2025, 8, 20, 9, 0, 0))
author_users = []
for i in range(5):
    g = random.choice('mf'); nm = make_name(g)
    uname = strip_accents(nm.split()[-1]).lower() + strip_accents(nm.split()[0])[0].lower() + '.tacgia'
    add_user(3, nm, uname, rdate(datetime(2025, 8, 25), datetime(2025, 10, 30)))
    author_users.append(users[-1]['id'])
GENRE_SETS = [[1, 2], [2, 6], [3, 8], [4, 5], [5, 7], [7, 8], [1, 3], [2, 4]]
for i in range(33):
    g = random.choice('mf'); nm = make_name(g)
    uname = (strip_accents(nm.split()[-1]) + strip_accents(nm.split()[0])[0] + str(random.randint(1, 99))).lower()
    add_user(4, nm, uname, rdate(datetime(2025, 10, 1), datetime(2026, 10, 25)), interest=random.choice(GENRE_SETS))
for u in random.sample([x for x in users if x['role'] == 4], 2):
    u['locked'] = 1   # tài khoản vi phạm bị khóa
uid_of = {u['id']: u for u in users}

# ---------------------------------------------------------------- Truyen (có suy luận)
# (tên, tác giả hiển thị, thể loại, trạng thái, chính sách, số chương, độ phổ biến 1-10, nhân vật chính, mô tả)
STORIES = [
 ('Tiên Nghịch', 'Nhĩ Căn', [1, 2], 'Hoàn thành', 'Paid', 12, 9, 'Vương Lâm', 'Một thiếu niên nghèo khó bước lên con đường nghịch thiên tu tiên, chống lại số mệnh đã định.'),
 ('Đấu Phá Thương Khung', 'Thiên Tàm Thổ Đậu', [2, 1], 'Hoàn thành', 'Free', 12, 10, 'Tiêu Viêm', 'Thiên tài đấu khí sa sút, nhờ dược lão thức tỉnh mà trở lại đỉnh cao đại lục.'),
 ('Vũ Động Càn Khôn', 'Thiên Tàm Thổ Đậu', [2], 'Đang phát hành', 'Free', 10, 8, 'Lâm Động', 'Thiếu niên Lâm Động dùng tổ phù huyền bí từng bước leo lên đỉnh cao võ đạo.'),
 ('Phàm Nhân Tu Tiên', 'Vong Ngữ', [1], 'Hoàn thành', 'Paid', 12, 9, 'Hàn Lập', 'Một người bình thường, tư chất tầm thường, nhờ cẩn trọng và một bình nhỏ bí ẩn mà tu tiên.'),
 ('Thôn Phệ Tinh Không', 'Ngã Cật Tây Hồng Thị', [6, 2], 'Hoàn thành', 'Paid', 10, 7, 'La Phong', 'Thời đại võ giả vũ trụ, La Phong từ một thiếu niên nghèo trở thành cường giả tinh không.'),
 ('Kiếm Lai', 'Phong Hỏa Hí Chư Hầu', [3, 2], 'Đang phát hành', 'Paid', 10, 8, 'Trần Bình An', 'Cậu bé mồ côi ở trấn nhỏ, mang theo thanh kiếm gãy đi qua giang hồ tìm lại chính mình.'),
 ('Nguyên Tôn', 'Thiên Tàm Thổ Đậu', [2], 'Đang phát hành', 'Free', 8, 6, 'Chu Nguyên', 'Đại thiên thế giới thuộc về những người nắm giữ nguyên khí, Chu Nguyên bước lên con đường vương giả.'),
 ('Hắc Ám Chi Tâm', 'Lam Phương Thảo', [2, 4], 'Hoàn thành', 'Free', 8, 4, 'Mộc Thanh', 'Cô gái mang trong tim sức mạnh bóng tối phải chọn giữa tình yêu và lời nguyền gia tộc.'),
 ('Người Giữ Đèn Phố Cổ', 'Đặng Minh Khôi', [5, 7], 'Đang phát hành', 'Free', 9, 6, 'Khôi', 'Người thợ sửa đèn ở phố cổ Hội An vô tình cuốn vào chuỗi vụ mất tích liên quan đến những ngọn đèn lồng cổ.'),
 ('Mùa Thu Hà Nội Ấy', 'Lam Phương Thảo', [4, 5], 'Hoàn thành', 'Paid', 10, 7, 'Thu Hà', 'Hai người xa lạ gặp nhau giữa mùa thu Hà Nội, một năm sau họ mới nhận ra mình đã là một phần cuộc đời nhau.'),
 ('Hồ Sơ Mật Sài Gòn 1975', 'Đặng Minh Khôi', [7, 8], 'Hoàn thành', 'Paid', 9, 6, 'Thiếu tá Hùng', 'Những ngày cuối tháng Tư, một hồ sơ mật biến mất khỏi dinh Độc Lập và viên sĩ quan điều tra phải tìm ra sự thật.'),
 ('Linh Vực', 'Thiên Tàm Thổ Đậu', [2], 'Đang phát hành', 'Free', 6, 3, 'Cổ Vực', 'Một vùng đất chỉ tồn tại trong mộng cảnh, nơi những linh hồn lạc lối tìm đường về.'),
 ('Long Tộc', 'Giang Nam', [2, 3], 'Hoàn thành', 'Paid', 10, 7, 'Lộ Minh Phi', 'Chàng trai bình thường nhận thư mời vào học viện bí ẩn và biết mình mang dòng máu rồng.'),
 ('Ma Thiên Ký', 'Bát Nguyệt Trường An', [1, 4], 'Đang phát hành', 'Paid', 8, 5, 'Diệp Thiên', 'Nữ ma tu bị phản bội trùng sinh về mười năm trước, quyết viết lại số phận của mình.'),
 ('Kẻ Trộm Ký Ức', 'Lam Phương Thảo', [6, 7], 'Đang phát hành', 'Free', 7, 5, 'Nhã', 'Năm 2087, một tên trộm có thể đánh cắp ký ức người khác bị cuốn vào âm mưu của tập đoàn công nghệ.'),
 ('Triều Đại Sau Ngai Vàng', 'Đặng Minh Khôi', [8, 3], 'Đang phát hành', 'Paid', 8, 5, 'Lý Phúc', 'Câu chuyện về một thái giám và cuộc cờ quyền lực trong hậu cung triều Lê.'),
]
CH_TITLES = {
 1: ['Thiếu niên nghèo khó', 'Bước chân vào tông môn', 'Cơ duyên bất ngờ', 'Luyện khí tầng một', 'Đối đầu đệ tử nội môn', 'Bí cảnh mở ra', 'Phá cảnh', 'Địch nhân xuất hiện', 'Sinh tử đại chiến', 'Trúc cơ thành công', 'Tiên môn đại hội', 'Nghịch thiên cải mệnh'],
 2: ['Phế vật trở về', 'Lão sư bí ẩn', 'Đấu khí thức tỉnh', 'Gia tộc đại hội', 'Dị hỏa hiện thế', 'Đấu giá hội', 'Thiên tài thanh niên', 'Chiến đấu ở học viện', 'Cường địch ngoài ải', 'Đột phá bình cảnh', 'Chung kết đại hội', 'Đỉnh cao đại lục'],
 3: ['Trấn nhỏ ven sông', 'Thanh kiếm gãy', 'Gặp lại cố nhân', 'Rượu và giang hồ', 'Một kiếm trấn núi', 'Quán trọ đêm mưa', 'Hiệp nghĩa', 'Ân oán giang hồ', 'Kiếm khách vô danh', 'Kiếm lai'],
 4: ['Lần đầu gặp gỡ', 'Hiểu lầm nhỏ', 'Ngày mưa', 'Lời hứa mùa thu', 'Chia xa', 'Thư tay chưa gửi', 'Trở lại', 'Cuối cùng cũng nói', 'Hạnh phúc giản dị', 'Mùa sau'],
 5: ['Cuộc gọi lúc nửa đêm', 'Con hẻm cũ', 'Người lạ trong quán cà phê', 'Dấu vết đầu tiên', 'Căn phòng khóa kín', 'Nhân chứng biến mất', 'Chiếc đèn lồng', 'Sự thật lộ diện', 'Kết thúc bất ngờ'],
 6: ['Năm 2087', 'Ký ức lạ', 'Công ty Helix', 'Kẻ săn đuổi', 'Dữ liệu bị xóa', 'Cuộc đào thoát', 'Bản sao', 'Quyết định cuối'],
 7: ['Hồ sơ biến mất', 'Dinh Độc Lập', 'Người lính cuối cùng', 'Cuốn sổ ghi chép', 'Mật thư', 'Cuộc truy đuổi', 'Bến Bạch Đằng', 'Lời khai', 'Sự thật 1975'],
 8: ['Chiếu chỉ', 'Hậu cung', 'Lời thì thầm', 'Bữa tiệc triều đình', 'Ván cờ quyền lực', 'Phản bội', 'Chạy trốn', 'Hoàng hôn'],
}
CH_SENT = {
 1: ['{h} ngồi xuống bên vách đá, cảm nhận linh khí trong thiên địa chậm rãi tụ lại.', 'Tiếng gió xuyên qua rừng trúc, mang theo hơi lạnh của buổi sớm.', 'Ánh mắt {h} chợt sáng lên: cơ duyên mà y chờ đợi bấy lâu rốt cuộc đã xuất hiện.'],
 2: ['{h} siết chặt nắm tay, đấu khí trong đan điền cuộn trào như sóng lớn.', 'Cả hội trường im phăng phắc khi cái tên {h} được xướng lên.', 'Một cơn gió lạnh quét qua, báo hiệu trận chiến sắp bắt đầu.'],
 3: ['Mưa bụi rơi trên mái hiên, {h} đặt thanh kiếm xuống bàn và rót một chén rượu.', 'Giang hồ rộng lớn, nhưng người biết {h} chẳng được mấy ai.', 'Tiếng vó ngựa xa dần trên con đường đất đỏ.'],
 4: ['{h} đứng dưới tán cây vàng, gió thu thổi tung mái tóc.', 'Cô nhớ về lời hẹn năm ấy, tim bỗng nhói lên một nhịp.', 'Tin nhắn đến, chỉ vỏn vẹn vài chữ nhưng đủ làm cô bối rối cả ngày.'],
 5: ['{h} nhìn con hẻm tối, linh cảm có điều gì đó không ổn.', 'Ngọn đèn đường chập chờn, chiếu lên những dấu chân còn mới.', 'Chiếc điện thoại rung lên trong túi, số lạ gọi đến lần thứ ba.'],
 6: ['Màn hình hiện lên dòng cảnh báo đỏ, {h} biết mình chỉ còn vài phút.', 'Thành phố lơ lửng giữa những tầng mây nhân tạo, rực rỡ nhưng lạnh lẽo.', 'Ký ức không phải của cô lại hiện lên rõ ràng đến đáng sợ.'],
 7: ['{h} lật từng trang hồ sơ dưới ánh đèn bàn vàng vọt.', 'Tiếng pháo xa xa vọng lại, thành phố sắp thay đổi.', 'Có người đã xóa dấu vết, nhưng chưa xóa hết.'],
 8: ['{h} cúi đầu bước qua cánh cửa son, tim đập thình thịch.', 'Trong cung, mọi lời nói đều có tai nghe.', 'Tờ chiếu chỉ cuộn trong tay áo nặng tựa ngàn cân.'],
}
def genre_group(genres):
    g = genres[0]
    return {1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 5, 8: 8}.get(g, 1) if g != 8 else 8
def chapter_group(genres):
    g = genres[0]
    return {1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 8: 8}[g] if g in (1, 2, 3, 4, 5, 6, 7, 8) else 1

truyen, chuong, truyen_theloai = [], [], []
for i, (name, tg, gens, status, pol, nch, pop, hero, desc) in enumerate(STORIES, start=1):
    creator = author_users[(i - 1) % len(author_users)]
    created = rdate(uid_of[creator]['created'] + timedelta(days=3), NOW - timedelta(days=120 if status == 'Hoàn thành' else 30))
    if pol == 'Paid':
        price = (35000 if pop >= 8 else 30000) if status == 'Hoàn thành' else 25000
    else:
        price = 0
    group = chapter_group(gens)
    titles = random.sample(CH_TITLES[group], min(nch, len(CH_TITLES[group])))
    while len(titles) < nch: titles.append(random.choice(CH_TITLES[group]))
    base = pop * 90 + random.randint(0, 80)
    last = created
    chs = []
    for n in range(1, nch + 1):
        pub = created + timedelta(days=7 * (n - 1) + random.randint(0, 2), hours=random.randint(0, 10))
        if pub > NOW: pub = NOW - timedelta(days=random.randint(0, 6))
        last = max(last, pub)
        views = max(5, int(base * (0.88 ** (n - 1)) + random.randint(-15, 15)))
        body = ' '.join(random.sample(CH_SENT[group], 3)).replace('{h}', hero)
        chs.append(dict(n=n, title=f'Chương {n}: {titles[n-1]}', body=f'{body} (Chương {n} - {name})',
                        free=1 if (pol == 'Free' or n <= 3) else 0, views=views, pub=pub))
    chuong.append((i, chs))
    truyen.append(dict(id=i, name=name, slug=slugify(name), tg=tg, desc=desc, status=status, pol=pol, price=price,
                       views=sum(c['views'] for c in chs), likes=0, creator=creator, created=created,
                       updated=last if status == 'Đang phát hành' else last + timedelta(days=1), genres=gens, pop=pop, nch=nch))
    for g in gens: truyen_theloai.append((i, g))
# MaChuong tăng dần theo (truyện, số thứ tự)
chuong_rows = []
cid = 0
chapter_ids = {}  # (truyen, n) -> id
for tid, chs in chuong:
    for c in chs:
        cid += 1
        chapter_ids[(tid, c['n'])] = cid
        chuong_rows.append(dict(id=cid, tid=tid, **c))
story = {t['id']: t for t in truyen}

# ---------------------------------------------------------------- Hành vi độc giả
readers = [u for u in users if u['role'] == 4 and not u['locked']]
def interested(u):
    return [t for t in truyen if set(t['genres']) & set(u['interest'])] or truyen
purchases, subs, trans, favorites, progress, ratings = [], [], [], [], [], []
packs = {g[0]: g for g in goidoc}
tx_id = 0
def add_tx(uid, loai, target, amount, code, method, status, when):
    global tx_id
    tx_id += 1
    trans.append(dict(id=tx_id, uid=uid, loai=loai, target=target, amount=amount, code=code, method=method, status=status, when=when))
    return tx_id
# gói VIP: ~30% độc giả
for u in random.sample(readers, 10):
    pk = random.choice([2, 2, 3, 4, 1])
    start = rdate(u['created'] + timedelta(days=2), NOW - timedelta(days=1))
    code = None; price = packs[pk][2]; amount = price
    if u['created'].year == 2026 and not any(t['uid'] == u['id'] and t['loai'] == 'GoiDoc' for t in trans) and random.random() < 0.5:
        code = 'WELCOME10'; amount = price - 10000
    add_tx(u['id'], 'GoiDoc', pk, amount, code, random.choice(['VNPAY', 'QR', 'PayPal']), 'Thành công', start)
    end = start + timedelta(days=packs[pk][3])
    subs.append(dict(uid=u['id'], pack=pk, start=start, end=end, status='Active' if end > NOW else 'Expired'))
vip_active = {s['uid'] for s in subs if s['status'] == 'Active'}
def has_access(u, t, when):
    if t['pol'] == 'Free': return True
    if any(p['uid'] == u['id'] and p['tid'] == t['id'] for p in purchases): return True
    return any(s['uid'] == u['id'] and s['start'] <= when <= s['end'] for s in subs)
# mua lẻ truyện Paid
for u in readers:
    if random.random() < 0.45:
        cands = [t for t in interested(u) if t['pol'] == 'Paid']
        for t in random.sample(cands, min(len(cands), random.randint(1, 2))):
            when = rdate(max(u['created'], t['created']) + timedelta(days=1), NOW - timedelta(days=1))
            add_tx(u['id'], 'MuaTruyen', t['id'], t['price'], None, random.choice(['VNPAY', 'QR']), 'Thành công', when)
            purchases.append(dict(uid=u['id'], tid=t['id'], when=when, price=t['price']))
# giao dịch không thành công (hủy / đang chờ)
for u in random.sample(readers, 4):
    t = random.choice([x for x in truyen if x['pol'] == 'Paid'])
    add_tx(u['id'], 'MuaTruyen', t['id'], t['price'], None, 'QR', 'Đã hủy', rdate(NOW - timedelta(days=60), NOW - timedelta(days=2)))
for u in random.sample(readers, 2):
    add_tx(u['id'], 'GoiDoc', 2, 50000, None, 'QR', 'Chờ thanh toán', NOW - timedelta(minutes=random.randint(3, 12)))
trans.sort(key=lambda t: t['when'])
for n, t in enumerate(trans, start=1): t['id'] = n
# yêu thích, tiến độ đọc, đánh giá
fav_count = {t['id']: 0 for t in truyen}
for u in readers:
    pool = interested(u)
    weights = [t['pop'] for t in pool]
    chosen = set()
    for _ in range(random.randint(2, 5)):
        chosen.add(random.choices(pool, weights)[0]['id'])
    for tid in chosen:
        t = story[tid]
        when = rdate(max(u['created'], t['created']) + timedelta(days=1), NOW)
        favorites.append((u['id'], tid, when)); fav_count[tid] += 1
        if has_access(u, t, when):
            last_ch = random.randint(1, t['nch'])
            if t['pol'] == 'Paid' and not any(p['uid'] == u['id'] and p['tid'] == tid for p in purchases) and not any(s['uid'] == u['id'] for s in subs):
                last_ch = min(last_ch, 3)
            progress.append(dict(uid=u['id'], tid=tid, ch=last_ch, reads=random.randint(1, 6),
                                 when=rdate(when, NOW)))
for t in truyen:
    t['likes'] = fav_count[t['id']] * 7 + t['pop'] * 3   # lượt thích ~ tỉ lệ với số người yêu thích
# đánh giá: chỉ người đã đọc >= 3 chương
POS = ['Truyện hay, cốt truyện cuốn hút, đọc một mạch không dừng được.', 'Nhân vật chính phát triển rất tự nhiên, rất đáng đọc.', 'Văn phong mượt, bản dịch tốt, mong ra chương mới đều.']
NEU = ['Phần đầu hơi chậm nhưng càng về sau càng ổn.', 'Truyện tạm được, hợp để đọc giải trí.', 'Cốt truyện ổn nhưng có đoạn hơi dài dòng.']
NEG = ['Nội dung lặp lại nhiều, mình đọc đến chương 5 thì thấy chán.', 'Cốt truyện dễ đoán, nhịp truyện chưa ổn.']
for p in progress:
    if p['ch'] >= 3 and random.random() < 0.6:
        t = story[p['tid']]
        score = t['pop'] / 2.0 + random.uniform(-0.8, 0.8)
        stars = 5 if score >= 4.2 else 4 if score >= 3.3 else 3 if score >= 2.5 else 2
        txt = random.choice(POS if stars >= 4 else NEU if stars == 3 else NEG)
        ratings.append(dict(uid=p['uid'], tid=p['tid'], stars=stars, text=txt, when=rdate(p['when'], NOW)))
# bình luận: ở chương nằm trong tiến độ đọc; có trả lời của tác giả
CM = ['Chương này hay quá, đoạn cuối cuốn thật sự!', 'Mong tác giả ra chương mới sớm.', 'Nhân vật chính ngầu quá, không ngờ lại như vậy.', 'Cảm ơn nhóm dịch đã cập nhật đều.', 'Đoạn này hơi khó hiểu, ai giải thích giúp mình với?', 'Chương hơi ngắn nhưng đọc rất cuốn.']
REPLY = ['Cảm ơn bạn đã ủng hộ truyện nhé!', 'Tác giả sẽ cập nhật chương mới vào cuối tuần.', 'Bạn đọc tiếp các chương sau sẽ rõ hơn nhé.']
comments = []
cmid = 0
for p in progress:
    if random.random() < 0.55:
        t = story[p['tid']]
        n = random.randint(1, p['ch'])
        when = rdate(max(story[p['tid']]['created'], uid_of[p['uid']]['created']) + timedelta(days=1), NOW)
        cmid += 1
        comments.append(dict(id=cmid, ch=chapter_ids[(p['tid'], n)], uid=p['uid'], text=random.choice(CM), parent=None, hidden=0, when=when))
        if random.random() < 0.4:
            cmid += 1
            comments.append(dict(id=cmid, ch=chapter_ids[(p['tid'], n)], uid=t['creator'], text=random.choice(REPLY), parent=cmid - 1, hidden=0, when=when + timedelta(hours=random.randint(1, 20))))
# 1 bình luận spam bị ẩn
cmid += 1
comments.append(dict(id=cmid, ch=1, uid=readers[0]['id'], text='Mua gói đọc giá rẻ tại website abc-xyz.com', parent=None, hidden=1, when=NOW - timedelta(days=9)))

# hỗ trợ: gắn với sự kiện thật
support = []
pending = [t for t in trans if t['status'] == 'Chờ thanh toán']
for tt in pending:
    support.append((tt['uid'], 'User', tt['uid'], 'Mình đã quét mã QR nhưng gói đọc chưa được kích hoạt, nhờ admin kiểm tra giúp.', 0, NOW))
cancelled = [t for t in trans if t['status'] == 'Đã hủy'][:2]
for tt in cancelled:
    support.append((tt['uid'], 'User', tt['uid'], 'Mình bị trừ tiền nhưng giao dịch báo đã hủy, mã giao dịch #%d.' % tt['id'], 1, tt['when'] + timedelta(hours=2)))
    support.append((tt['uid'], 'Admin', 1, 'Chào bạn, hệ thống đã ghi nhận mã #%d và sẽ hoàn tiền trong 24 giờ.' % tt['id'], 1, tt['when'] + timedelta(hours=5)))
support.append((readers[3]['id'], 'User', readers[3]['id'], 'Làm sao để đổi ảnh đại diện?', 0, NOW - timedelta(hours=7)))

# ---------------------------------------------------------------- Ghi CSV (dataset)
def w(name, header, rows):
    with open(os.path.join(OUT, name), 'w', encoding='utf-8-sig', newline='') as f:
        cw = csv.writer(f); cw.writerow(header); cw.writerows(rows)
w('VaiTro.csv', ['MaVaiTro', 'TenVaiTro', 'MoTa'], vaitro)
w('TheLoai.csv', ['MaTheLoai', 'TenTheLoai', 'Slug', 'MoTa'], [(a, b, slugify(b), c) for a, b, c in theloai])
w('GoiDoc.csv', ['MaGoiDoc', 'TenGoi', 'GiaTien', 'SoNgayHieuLuc', 'MoTa'], goidoc)
w('KhuyenMai.csv', ['MaKhuyenMai', 'MaGiamGia', 'SoTienGiam', 'NgayBatDau', 'NgayKetThuc', 'IsHoatDong'], khuyenmai)
w('NguoiDung.csv', ['MaNguoiDung', 'MaVaiTro', 'TenDangNhap', 'HoTen', 'Email', 'IsKhoa', 'NgayTao'],
  [(u['id'], u['role'], u['username'], u['name'], u['email'], u['locked'], fmt(u['created'])) for u in users])
w('Truyen.csv', ['MaTruyen', 'TenTruyen', 'Slug', 'TacGia', 'TrangThai', 'ChinhSachTruyCap', 'GiaBan', 'LuotXem', 'LuotThich', 'NguoiTao'],
  [(t['id'], t['name'], t['slug'], t['tg'], t['status'], t['pol'], t['price'], t['views'], t['likes'], t['creator']) for t in truyen])
w('TruyenTheLoai.csv', ['MaTruyen', 'MaTheLoai'], truyen_theloai)
w('ChuongTruyen.csv', ['MaChuong', 'MaTruyen', 'SoThuTuChuong', 'TieuDe', 'MienPhi', 'LuotXem'],
  [(c['id'], c['tid'], c['n'], c['title'], c['free'], c['views']) for c in chuong_rows])

# ---------------------------------------------------------------- Ghi SQL
L = []
def ins(table, cols, rows, ident=False):
    if not rows: return
    L.append(f'-- {table} ({len(rows)} dòng)')
    if ident: L.append(f'SET IDENTITY_INSERT {table} ON;')
    L.append(f'INSERT INTO {table} ({", ".join(cols)}) VALUES')
    L.append(',\n'.join('(' + ', '.join(r) + ')' for r in rows) + ';')
    if ident: L.append(f'SET IDENTITY_INSERT {table} OFF;')
    L.append('')
def n(x): return 'NULL' if x is None else str(x)
L += ['/* =====================================================================',
      '   DỮ LIỆU MẪU - ĐỒ ÁN WebDocTruyen (Nhóm 9)',
      '   Sinh bằng build_dataset.py từ dataset có suy luận nghiệp vụ (xem datasets/*.csv và BaoCao_ThucHanh4.md)',
      "   Mật khẩu của mọi tài khoản mẫu: Nhom9@123 (băm bằng ASP.NET Core PasswordHasher nên đăng nhập được).",
      '   Chạy trên DB TEST/LOCAL, KHÔNG chạy lên DB production. Thứ tự theo khóa ngoại.',
      '   ===================================================================== */', '']
ins('VaiTro', ['MaVaiTro', 'TenVaiTro', 'MoTa'], [(str(a), q(b), q(c)) for a, b, c in vaitro])
ins('NguoiDung', ['MaNguoiDung', 'MaVaiTro', 'TenDangNhap', 'Email', 'MatKhauHash', 'HoTen', 'IsKhoa', 'NgayTao'],
    [(str(u['id']), str(u['role']), q(u['username']), q(u['email']), q(PWD), q(u['name']), str(u['locked']), q(fmt(u['created']))) for u in users], True)
ins('TheLoai', ['MaTheLoai', 'TenTheLoai', 'Slug', 'MoTa'], [(str(a), q(b), q(slugify(b)), q(c)) for a, b, c in theloai], True)
ins('Truyen', ['MaTruyen', 'TenTruyen', 'Slug', 'MoTa', 'TacGia', 'TrangThai', 'ChinhSachTruyCap', 'GiaBan', 'LuotXem', 'LuotThich', 'NguoiTao', 'NgayTao', 'NgayCapNhat'],
    [(str(t['id']), q(t['name']), q(t['slug']), q(t['desc']), q(t['tg']), q(t['status']), q(t['pol']), str(t['price']), str(t['views']), str(t['likes']),
      str(t['creator']), q(fmt(t['created'])), q(fmt(t['updated']))) for t in truyen], True)
ins('TruyenTheLoai', ['MaTruyen', 'MaTheLoai'], [(str(a), str(b)) for a, b in truyen_theloai])
ins('ChuongTruyen', ['MaChuong', 'MaTruyen', 'SoThuTuChuong', 'TieuDe', 'NoiDung', 'MienPhi', 'LuotXem', 'NgayPhatHanh'],
    [(str(c['id']), str(c['tid']), str(c['n']), q(c['title']), q(c['body']), str(c['free']), str(c['views']), q(fmt(c['pub']))) for c in chuong_rows], True)
ins('GoiDoc', ['MaGoiDoc', 'TenGoi', 'GiaTien', 'SoNgayHieuLuc', 'MoTa', 'IsHoatDong'], [(str(a), q(b), str(c), str(d), q(e), '1') for a, b, c, d, e in goidoc], True)
ins('KhuyenMai', ['MaKhuyenMai', 'MaGiamGia', 'SoTienGiam', 'NgayBatDau', 'NgayKetThuc', 'IsHoatDong'], [(str(a), q(b), str(c), q(d), q(e), str(f)) for a, b, c, d, e, f in khuyenmai], True)
ins('LichSuGiaoDich', ['MaGiaoDich', 'MaNguoiDung', 'LoaiGiaoDich', 'MaMụcTieu', 'SoTien', 'MaGiamGia', 'PhuongThucThanhToan', 'TrangThai', 'NgayTao'],
    [(str(t['id']), str(t['uid']), q(t['loai']), str(t['target']), str(t['amount']), ("'%s'" % t['code']) if t['code'] else 'NULL', q(t['method']), q(t['status']), q(fmt(t['when']))) for t in trans], True)
ins('TruyenDaMua', ['MaNguoiDung', 'MaTruyen', 'NgayMua', 'GiaDaTra'], [(str(p['uid']), str(p['tid']), q(fmt(p['when'])), str(p['price'])) for p in purchases])
ins('NguoiDungGoiDoc', ['MaNguoiDung', 'MaGoiDoc', 'NgayBatDau', 'NgayKetThuc', 'TrangThai'], [(str(s['uid']), str(s['pack']), q(fmt(s['start'])), q(fmt(s['end'])), q(s['status'])) for s in subs])
ins('TruyenYeuThich', ['MaNguoiDung', 'MaTruyen', 'NgayTao'], [(str(a), str(b), q(fmt(c))) for a, b, c in favorites])
ins('TienDoDoc', ['MaNguoiDung', 'MaTruyen', 'MaChuongGanNhat', 'SoLanDoc', 'NgayCapNhat'],
    [(str(p['uid']), str(p['tid']), str(chapter_ids[(p['tid'], p['ch'])]), str(p['reads']), q(fmt(p['when']))) for p in progress])
ins('DanhGiaTruyen', ['MaNguoiDung', 'MaTruyen', 'SoSao', 'NoiDungDanhGia', 'NgayTao'], [(str(r['uid']), str(r['tid']), str(r['stars']), q(r['text']), q(fmt(r['when']))) for r in ratings])
ins('BinhLuanChuong', ['MaBinhLuan', 'MaChuong', 'MaNguoiDung', 'NoiDung', 'MaBinhLuanCha', 'IsAn', 'NgayTao'],
    [(str(c['id']), str(c['ch']), str(c['uid']), q(c['text']), n(c['parent']), str(c['hidden']), q(fmt(c['when']))) for c in comments], True)
ins('TinNhanHoTro', ['MaNguoiDung', 'VaiTroNguoiGui', 'MaNguoiGui', 'NoiDung', 'DaDoc', 'NgayTao'],
    [(str(a), q(b), str(c), q(d), str(e), q(fmt(f))) for a, b, c, d, e, f in support])
with open(os.path.join(HERE, 'SampleData_WebDocTruyen.sql'), 'w', encoding='utf-8') as f:
    f.write('\n'.join(L))

# ---------------------------------------------------------------- Báo cáo kiểm tra tính nhất quán
print('Người dùng:', len(users), '| Truyện:', len(truyen), '| Chương:', len(chuong_rows))
print('Giao dịch:', len(trans), '| Mua lẻ:', len(purchases), '| Gói:', len(subs), '| Yêu thích:', len(favorites))
print('Tiến độ:', len(progress), '| Đánh giá:', len(ratings), '| Bình luận:', len(comments), '| Hỗ trợ:', len(support))
ok_tx = [t for t in trans if t['status'] == 'Thành công']
assert len(ok_tx) == len(purchases) + len(subs), 'mỗi giao dịch thành công phải có 1 bản ghi mua/gói'
assert all(story[t['id']]['views'] == sum(c['views'] for c in dict(chuong)[t['id']]) for t in truyen)
assert all(p['ch'] <= story[p['tid']]['nch'] for p in progress)
print('Kiểm tra nhất quán: OK')
