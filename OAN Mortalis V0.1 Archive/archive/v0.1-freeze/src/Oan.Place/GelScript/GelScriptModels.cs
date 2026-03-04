using System;
using System.Collections.Generic;

namespace Oan.Place.GelScript
{
    public class GelScript
    {
        public string Source { get; set; } = string.Empty;
        public List<GelStatement> Body { get; set; } = new();
    }

    public abstract class GelStatement { }

    public class GelCall : GelStatement
    {
        public string FunctionName { get; set; } = string.Empty;
        public List<object> Arguments { get; set; } = new();
    }
    
    public class GelAssignment : GelStatement
    {
        public string VariableName { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    // Compiler Result
    public class GelCompilationResult
    {
        public bool Success { get; set; }
        public List<string> BridgeIrOps { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}

