using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class TheLoai
{
    public int MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Truyen> MaTruyens { get; set; } = new List<Truyen>();
}
