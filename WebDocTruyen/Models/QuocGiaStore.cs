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
        private static readonly object LockObj = new object();
        private static readonly string AppDataDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
        private static readonly string FilePath = Path.Combine(AppDataDir, "quocgia.json");
        private static readonly string TruyenQuocGiaFilePath = Path.Combine(AppDataDir, "truyen_quocgia.json");

        private static readonly List<QuocGiaItem> DefaultList = new List<QuocGiaItem>
        {
            new QuocGiaItem { MaQuocGia = 1, TenQuocGia = "Trung Quốc", MaIso = "CN", MoTa = "Truyện Manhua, Tiên hiệp, Huyền huyễn" },
            new QuocGiaItem { MaQuocGia = 2, TenQuocGia = "Việt Nam", MaIso = "VN", MoTa = "Tác phẩm tác giả Việt Nam sáng tác" },
            new QuocGiaItem { MaQuocGia = 3, TenQuocGia = "Hàn Quốc", MaIso = "KR", MoTa = "Truyện Manhwa, Thợ săn, Hầm ngục" },
            new QuocGiaItem { MaQuocGia = 4, TenQuocGia = "Nhật Bản", MaIso = "JP", MoTa = "Truyện Manga, Light Novel, Isekai" },
            new QuocGiaItem { MaQuocGia = 5, TenQuocGia = "Mỹ", MaIso = "US", MoTa = "Comic, Siêu anh hùng, Viễn tưởng Âu Mỹ" }
        };

        private static List<QuocGiaItem> ReadInternal()
        {
            try
            {
                if (!Directory.Exists(AppDataDir))
                    Directory.CreateDirectory(AppDataDir);

                if (!File.Exists(FilePath))
                {
                    var seeded = DefaultList.Select(CloneItem).ToList();
                    WriteInternal(seeded);
                    return seeded;
                }

                string json = File.ReadAllText(FilePath);
                var items = JsonSerializer.Deserialize<List<QuocGiaItem>>(json);
                return items ?? DefaultList.Select(CloneItem).ToList();
            }
            catch
            {
                return DefaultList.Select(CloneItem).ToList();
            }
        }

        private static void WriteInternal(List<QuocGiaItem> items)
        {
            try
            {
                if (!Directory.Exists(AppDataDir))
                    Directory.CreateDirectory(AppDataDir);

                string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Bỏ qua nếu môi trường không cho phép ghi file
            }
        }

        private static QuocGiaItem CloneItem(QuocGiaItem x) => new QuocGiaItem
        {
            MaQuocGia = x.MaQuocGia,
            TenQuocGia = x.TenQuocGia,
            MaIso = x.MaIso,
            MoTa = x.MoTa
        };

        public static List<QuocGiaItem> GetAll()
        {
            lock (LockObj)
            {
                return ReadInternal()
                    .OrderBy(q => q.MaQuocGia)
                    .Select(CloneItem)
                    .ToList();
            }
        }

        public static void SaveAll(List<QuocGiaItem> items)
        {
            lock (LockObj)
            {
                WriteInternal(items);
            }
        }

        public static QuocGiaItem? GetById(int id)
        {
            lock (LockObj)
            {
                var found = ReadInternal().FirstOrDefault(q => q.MaQuocGia == id);
                return found == null ? null : CloneItem(found);
            }
        }

        public static void Add(QuocGiaItem item)
        {
            lock (LockObj)
            {
                var list = ReadInternal();
                int newId = list.Count > 0 ? list.Max(x => x.MaQuocGia) + 1 : 1;
                item.MaQuocGia = newId;
                item.TenQuocGia = (item.TenQuocGia ?? string.Empty).Trim();
                item.MaIso = string.IsNullOrWhiteSpace(item.MaIso) ? null : item.MaIso.Trim().ToUpper();
                item.MoTa = string.IsNullOrWhiteSpace(item.MoTa) ? null : item.MoTa.Trim();
                list.Add(CloneItem(item));
                WriteInternal(list);
            }
        }

        public static void Update(QuocGiaItem item)
        {
            lock (LockObj)
            {
                var list = ReadInternal();
                var existing = list.FirstOrDefault(x => x.MaQuocGia == item.MaQuocGia);
                if (existing != null)
                {
                    existing.TenQuocGia = (item.TenQuocGia ?? string.Empty).Trim();
                    existing.MaIso = string.IsNullOrWhiteSpace(item.MaIso) ? null : item.MaIso.Trim().ToUpper();
                    existing.MoTa = string.IsNullOrWhiteSpace(item.MoTa) ? null : item.MoTa.Trim();
                    WriteInternal(list);
                }
            }
        }

        public static void Delete(int id)
        {
            lock (LockObj)
            {
                var list = ReadInternal();
                list.RemoveAll(x => x.MaQuocGia == id);
                WriteInternal(list);
            }
        }

        public static Dictionary<int, string> GetAllTruyenQuocGia()
        {
            lock (LockObj)
            {
                try
                {
                    if (!File.Exists(TruyenQuocGiaFilePath))
                        return new Dictionary<int, string>();

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
                    if (!Directory.Exists(AppDataDir))
                        Directory.CreateDirectory(AppDataDir);

                    string jsonRead = File.Exists(TruyenQuocGiaFilePath) ? File.ReadAllText(TruyenQuocGiaFilePath) : "{}";
                    var dict = JsonSerializer.Deserialize<Dictionary<int, string>>(jsonRead) ?? new Dictionary<int, string>();
                    dict[maTruyen] = quocGia.Trim();
                    File.WriteAllText(TruyenQuocGiaFilePath, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch
                {
                    // Bỏ qua nếu môi trường không cho phép ghi file
                }
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