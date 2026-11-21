using System.Text.Json;

namespace WebDocTruyen.Models
{
    public class QuocGiaItem
    {
        public int MaQuocGia { get; set; }
        public string TenQuocGia { get; set; } = string.Empty;
        public string? MaIso { get; set; }
        public string? MoTa { get; set; }
    }

    public static class QuocGiaStore
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "quocgia.json");
        private static readonly object LockObj = new object();

        private static readonly List<QuocGiaItem> DefaultList = new List<QuocGiaItem>
        {
            new QuocGiaItem { MaQuocGia = 1, TenQuocGia = "Trung Quốc", MaIso = "CN", MoTa = "Truyện Manhua, Tiên hiệp, Huyền huyễn" },
            new QuocGiaItem { MaQuocGia = 2, TenQuocGia = "Việt Nam", MaIso = "VN", MoTa = "Tác phẩm tác giả Việt Nam sáng tác" },
            new QuocGiaItem { MaQuocGia = 3, TenQuocGia = "Hàn Quốc", MaIso = "KR", MoTa = "Truyện Manhwa, Thợ săn, Hầm ngục" },
            new QuocGiaItem { MaQuocGia = 4, TenQuocGia = "Nhật Bản", MaIso = "JP", MoTa = "Truyện Manga, Light Novel, Isekai" },
            new QuocGiaItem { MaQuocGia = 5, TenQuocGia = "Mỹ", MaIso = "US", MoTa = "Comic, Siêu anh hùng, Viễn tưởng Âu Mỹ" }
        };

        public static List<QuocGiaItem> GetAll()
        {
            lock (LockObj)
            {
                try
                {
                    if (!File.Exists(FilePath))
                    {
                        string dir = Path.GetDirectoryName(FilePath)!;
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        SaveAll(DefaultList);
                        return new List<QuocGiaItem>(DefaultList);
                    }
                    string json = File.ReadAllText(FilePath);
                    var items = JsonSerializer.Deserialize<List<QuocGiaItem>>(json);
                    return items ?? new List<QuocGiaItem>(DefaultList);
                }
                catch
                {
                    return new List<QuocGiaItem>(DefaultList);
                }
            }
        }

        public static void SaveAll(List<QuocGiaItem> items)
        {
            lock (LockObj)
            {
                string dir = Path.GetDirectoryName(FilePath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
        }

        public static QuocGiaItem? GetById(int id)
        {
            return GetAll().FirstOrDefault(q => q.MaQuocGia == id);
        }

        public static void Add(QuocGiaItem item)
        {
            var list = GetAll();
            int newId = list.Any() ? list.Max(x => x.MaQuocGia) + 1 : 1;
            item.MaQuocGia = newId;
            list.Add(item);
            SaveAll(list);
        }

        public static void Update(QuocGiaItem item)
        {
            var list = GetAll();
            var existing = list.FirstOrDefault(x => x.MaQuocGia == item.MaQuocGia);
            if (existing != null)
            {
                existing.TenQuocGia = item.TenQuocGia;
                existing.MaIso = item.MaIso;
                existing.MoTa = item.MoTa;
                SaveAll(list);
            }
        }

        public static void Delete(int id)
        {
            var list = GetAll();
            list.RemoveAll(x => x.MaQuocGia == id);
            SaveAll(list);
        }

        // Quản lý Quốc gia gán cho từng truyện qua file JSON phụ
        private static readonly string TruyenQuocGiaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "truyen_quocgia.json");

        public static Dictionary<int, string> GetAllTruyenQuocGia()
        {
            lock (LockObj)
            {
                try
                {
                    if (!File.Exists(TruyenQuocGiaFilePath)) return new Dictionary<int, string>();
                    string json = File.ReadAllText(TruyenQuocGiaFilePath);
                    return JsonSerializer.Deserialize<Dictionary<int, string>>(json) ?? new Dictionary<int, string>();
                }
                catch
                {
                    return new Dictionary<int, string>();
                }
            }
        }

        public static void SetQuocGiaChoTruyen(int maTruyen, string? quocGia)
        {
            if (string.IsNullOrWhiteSpace(quocGia)) return;
            lock (LockObj)
            {
                try
                {
                    var dict = GetAllTruyenQuocGia();
                    dict[maTruyen] = quocGia;
                    string dir = Path.GetDirectoryName(TruyenQuocGiaFilePath)!;
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(TruyenQuocGiaFilePath, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch { }
            }
        }

        public static string GetQuocGiaCuaTruyen(int maTruyen, string? tenTruyen, string? tacGia)
        {
            var dict = GetAllTruyenQuocGia();
            if (dict.TryGetValue(maTruyen, out var qg) && !string.IsNullOrWhiteSpace(qg))
            {
                return qg;
            }
            return DoanQuocGia(tenTruyen, tacGia);
        }

        // Bổ trợ ánh xạ quốc gia mặc định cho truyện dựa trên tên hoặc tác giả nếu truyện chưa có quốc gia
        public static string DoanQuocGia(string? tenTruyen, string? tacGia)
        {
            if (string.IsNullOrWhiteSpace(tenTruyen)) return "Trung Quốc";

            string lower = (tenTruyen + " " + (tacGia ?? "")).ToLower();
            if (lower.Contains("hà nội") || lower.Contains("sài gòn") || lower.Contains("phố cổ") || lower.Contains("triều đại") ||
                lower.Contains("đặng minh khôi") || lower.Contains("lam phương thảo"))
            {
                return "Việt Nam";
            }
            if (lower.Contains("solo leveling") || lower.Contains("manhwa") || lower.Contains("tháp") || lower.Contains("hàn quốc"))
            {
                return "Hàn Quốc";
            }
            if (lower.Contains("manga") || lower.Contains("boruto") || lower.Contains("vagabond") || lower.Contains("one piece") ||
                lower.Contains("naruto") || lower.Contains("nhật bản") || lower.Contains("isekai"))
            {
                return "Nhật Bản";
            }
            if (lower.Contains("marvel") || lower.Contains("dc") || lower.Contains("mỹ") || lower.Contains("comic"))
            {
                return "Mỹ";
            }

            return "Trung Quốc";
        }
    }
}
