using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Event
{
    public int EventId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    public string Venue { get; set; } = null!;

    public int OrganizedBy { get; set; }

    public decimal BudgetAllocated { get; set; }

    public int Capacity { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<EventBudget> EventBudgets { get; set; } = new List<EventBudget>();

    public virtual Member OrganizedByNavigation { get; set; } = null!;

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
}
