using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class VUpcomingEvent
{
    public int EventId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public string Venue { get; set; } = null!;

    public string Organizer { get; set; } = null!;

    public int? RegisteredCount { get; set; }
}
