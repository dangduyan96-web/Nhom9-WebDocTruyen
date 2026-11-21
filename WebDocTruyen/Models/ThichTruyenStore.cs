using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WebDocTruyen.Models
{
    public class ThichTruyenStore
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "thichtruyen.json");
        private static readonly object LockObj = new object();

        // Lưu danh sách { MaNguoiDung, MaTruyen, NgayTao }
        public class UserThichItem
        {
            public int MaNguoiDung { get; set; }
            public int MaTruyen { get; set; }
            public DateTime NgayTao { get; set; } = DateTime.Now;
        }

        private static List<UserThichItem> ReadData()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    string dir = Path.GetDirectoryName(FilePath)!;
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(FilePath, "[]");
                    return new List<UserThichItem>();
                }
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<UserThichItem>>(json) ?? new List<UserThichItem>();
            }
            catch
            {
                return new List<UserThichItem>();
            }
        }

        private static void SaveData(List<UserThichItem> list)
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }

        public static bool DaThich(int maNguoiDung, int maTruyen)
        {
            lock (LockObj)
            {
                var list = ReadData();
                return list.Exists(x => x.MaNguoiDung == maNguoiDung && x.MaTruyen == maTruyen);
            }
        }

        public static bool ToggleThich(int maNguoiDung, int maTruyen)
        {
            lock (LockObj)
            {
                var list = ReadData();
                var item = list.Find(x => x.MaNguoiDung == maNguoiDung && x.MaTruyen == maTruyen);
                bool result;
                if (item != null)
                {
                    list.Remove(item);
                    result = false;
                }
                else
                {
                    list.Add(new UserThichItem
                    {
                        MaNguoiDung = maNguoiDung,
                        MaTruyen = maTruyen,
                        NgayTao = DateTime.Now
                    });
                    result = true;
                }
                SaveData(list);
                return result;
            }
        }

        public static int DemSoThich(int maTruyen, int baseCount = 0)
        {
            lock (LockObj)
            {
                var list = ReadData();
                int userLikes = list.FindAll(x => x.MaTruyen == maTruyen).Count;
                return Math.Max(baseCount, userLikes);
            }
        }
    }
}
