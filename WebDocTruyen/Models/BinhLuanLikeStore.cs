using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WebDocTruyen.Models
{
    public class BinhLuanLikeStore
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "binhluan_likes.json");
        private static readonly object LockObj = new object();

        public class CommentLikeItem
        {
            public int MaBinhLuan { get; set; }
            public int MaNguoiDung { get; set; }
            public DateTime NgayTao { get; set; } = DateTime.Now;
        }

        private static List<CommentLikeItem> ReadData()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    string dir = Path.GetDirectoryName(FilePath)!;
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(FilePath, "[]");
                    return new List<CommentLikeItem>();
                }
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<CommentLikeItem>>(json) ?? new List<CommentLikeItem>();
            }
            catch
            {
                return new List<CommentLikeItem>();
            }
        }

        private static void SaveData(List<CommentLikeItem> list)
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

        public static bool DaLike(int maNguoiDung, int maBinhLuan)
        {
            lock (LockObj)
            {
                var list = ReadData();
                return list.Exists(x => x.MaNguoiDung == maNguoiDung && x.MaBinhLuan == maBinhLuan);
            }
        }

        public static (bool daLike, int tongLike) ToggleLike(int maNguoiDung, int maBinhLuan)
        {
            lock (LockObj)
            {
                var list = ReadData();
                var item = list.Find(x => x.MaNguoiDung == maNguoiDung && x.MaBinhLuan == maBinhLuan);
                bool daLike;
                if (item != null)
                {
                    list.Remove(item);
                    daLike = false;
                }
                else
                {
                    list.Add(new CommentLikeItem
                    {
                        MaNguoiDung = maNguoiDung,
                        MaBinhLuan = maBinhLuan,
                        NgayTao = DateTime.Now
                    });
                    daLike = true;
                }
                SaveData(list);
                int tongLike = list.FindAll(x => x.MaBinhLuan == maBinhLuan).Count;
                return (daLike, tongLike);
            }
        }

        public static int DemLike(int maBinhLuan)
        {
            lock (LockObj)
            {
                var list = ReadData();
                return list.FindAll(x => x.MaBinhLuan == maBinhLuan).Count;
            }
        }

        public static Dictionary<int, int> DemTatCaLike()
        {
            lock (LockObj)
            {
                var list = ReadData();
                var dict = new Dictionary<int, int>();
                foreach (var item in list)
                {
                    if (!dict.ContainsKey(item.MaBinhLuan))
                        dict[item.MaBinhLuan] = 0;
                    dict[item.MaBinhLuan]++;
                }
                return dict;
            }
        }

        public static HashSet<int> DanhSachDaLikeCuaUser(int maNguoiDung)
        {
            lock (LockObj)
            {
                var list = ReadData();
                var set = new HashSet<int>();
                foreach (var item in list)
                {
                    if (item.MaNguoiDung == maNguoiDung)
                        set.Add(item.MaBinhLuan);
                }
                return set;
            }
        }
    }
}
