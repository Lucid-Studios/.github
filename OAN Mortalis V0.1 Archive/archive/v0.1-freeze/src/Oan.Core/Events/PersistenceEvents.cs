using System;

namespace Oan.Core.Events
{
    public class TipSnapshotLoadAttemptedEvent 
    { 
        public string PathHash { get; set; } = ""; 
    }

    public class TipSnapshotLoadedEvent 
    { 
        public int TheaterCount { get; set; } 
    }

    public class TipSnapshotMissingEvent 
    { 
    }

    public class TipSnapshotRejectedEvent 
    { 
        public string ReasonCode { get; set; } = ""; 
    }

    public class TipSnapshotWrittenEvent 
    { 
        public int TheaterCount { get; set; } 
    }
}
