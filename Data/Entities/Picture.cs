using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Picture
{
    public int PictureId { get; set; }

    public int EventId { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Event Event { get; set; } = null!;
}
