using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class QuocGia
{
    public int MaQuocGia { get; set; }

    public string TenQuocGia { get; set; } = null!;

    public string? MaIso { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
