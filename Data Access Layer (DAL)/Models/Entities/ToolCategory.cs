using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class ToolCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Icon { get; set; }

    public int? ParentCategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ToolCategory? ParentCategory { get; set; }

    public virtual ICollection<ToolCategory> Children { get; set; } = new List<ToolCategory>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<Tool> Tools { get; set; } = new List<Tool>();
}
