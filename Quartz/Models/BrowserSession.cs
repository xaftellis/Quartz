using System;
using System.Collections.Generic;

namespace Quartz.Models
{
    public sealed class BrowserSessionModel
    {
        public int Version { get; set; } = 1;
        public Guid ProfileId { get; set; }
        public DateTime SavedAtUtc { get; set; }
        public Guid ActiveWindowId { get; set; }
        public List<SessionWindowModel> Windows { get; set; } = new List<SessionWindowModel>();
    }

    public sealed class SessionWindowModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Maximized { get; set; }
        public string Name { get; set; }
        public int SelectedTabIndex { get; set; }
        public List<SessionTabModel> Tabs { get; set; } = new List<SessionTabModel>();
    }

    public sealed class SessionTabModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public bool IsPinned { get; set; }
        public bool IsMuted { get; set; }
        public double ZoomFactor { get; set; } = 1;
        public double ScrollX { get; set; }
        public double ScrollY { get; set; }
    }
}
