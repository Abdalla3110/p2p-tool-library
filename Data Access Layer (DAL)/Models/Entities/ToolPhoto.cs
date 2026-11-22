using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class ToolPhoto
{
    public int PhotoId { get; set; }

    public int ToolId { get; set; }

    public string Url { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }

    public DocumentTypeEnum? DocumentType { get; set; }

    public DateOnly? WarrantyExpiry { get; set; }

    public virtual Tool Tool { get; set; } = null!;
}
