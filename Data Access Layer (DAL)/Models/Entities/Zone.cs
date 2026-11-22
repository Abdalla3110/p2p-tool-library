using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class Zone
{
    public int ZoneId { get; set; }

    public string Name { get; set; } = null!;

    public int? LibrarianUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Broadcast> Broadcasts { get; set; } = new List<Broadcast>();

    public virtual ICollection<InventoryAudit> InventoryAudits { get; set; } = new List<InventoryAudit>();

    public virtual ICollection<LibrarianTask> LibrarianTasks { get; set; } = new List<LibrarianTask>();

    public virtual User? LibrarianUser { get; set; }

    public virtual ICollection<Tool> Tools { get; set; } = new List<Tool>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
