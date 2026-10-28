using System.ComponentModel.DataAnnotations;

namespace WebDocTruyen.Models;

public sealed class LuuTienDoDocRequest
{
    [Range(1, int.MaxValue)]
    public int MaChuongGanNhat { get; set; }
}
