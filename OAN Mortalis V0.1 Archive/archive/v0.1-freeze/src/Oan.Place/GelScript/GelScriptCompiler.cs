using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Oan.Place.GelScript
{
    public class GelScriptCompiler
    {
        public GelCompilationResult Compile(string source)
        {
            var result = new GelCompilationResult();
            var lines = source.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var variables = new Dictionary<string, string>(); // Name -> Value

            // GEL0 currently compiles one statement -> one Bridge IR intent.
            // Complex logic control flow is not yet supported in this toy compiler.

            int lineNum = 0;
            foreach (var rawLine in lines)
            {
                lineNum++;
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

                // 1. Variable Declaration: var name = value
                var varMatch = Regex.Match(line, @"^var\s+(\w+)\s*=\s*(.+)$");
                if (varMatch.Success)
                {
                    string name = varMatch.Groups[1].Value;
                    string val = varMatch.Groups[2].Value.Trim();
                    variables[name] = ResolveValue(val, variables);
                    continue;
                }

                // 2. Function Call: Func(arg1, arg2)
                var callMatch = Regex.Match(line, @"^(\w+)\((.*)\)$");
                if (callMatch.Success)
                {
                    string funcName = callMatch.Groups[1].Value;
                    string argsRaw = callMatch.Groups[2].Value;
                    
                    var args = SplitArgs(argsRaw)
                        .Select(a => ResolveValue(a, variables))
                        .ToList();

                    // Generate Bridge IR S-Expression
                    // (oan.intent (id "GUID") (sli "default") (kind "Func") (args (k "v")...))
                    
                    string id = Guid.NewGuid().ToString();
                    string sli = "sli.gen.default"; // Placeholder SLI
                    string kind = funcName; // Assume FuncName == Kind for now

                    string sexpr = $"(oan.intent (id \"{id}\") (sli \"{sli}\") (kind \"{kind}\")";

                    if (args.Count > 0)
                    {
                        sexpr += " (args";
                        var kvPairs = MapArgs(funcName, args);
                        foreach (var kv in kvPairs)
                        {
                            sexpr += $" ({kv.Key} \"{kv.Value}\")";
                        }
                        sexpr += ")";
                    }
                    else 
                    {
                         // Even empty args might be needed or valid
                    }
                    sexpr += ")";

                    result.BridgeIrOps.Add(sexpr);
                    continue;
                }

                result.Errors.Add($"Line {lineNum}: Unknown statement '{line}'");
                result.Success = false;
            }

            if (result.Errors.Count == 0) result.Success = true;
            return result;
        }

        private List<string> SplitArgs(string argsRaw)
        {
            // Naive split by comma, ignoring quotes for now (toy compiler)
            return argsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .ToList();
        }

        private string ResolveValue(string val, Dictionary<string, string> vars)
        {
            if (vars.ContainsKey(val)) return vars[val];
            if (val.StartsWith("\"") && val.EndsWith("\"")) return val.Trim('"');
            return val;
        }

        private Dictionary<string, string> MapArgs(string func, List<string> args)
        {
            var map = new Dictionary<string, string>();
            switch (func)
            {
                case "MoveTo":
                    if (args.Count >= 1) map["x"] = args[0];
                    if (args.Count >= 2) map["y"] = args[1];
                    break;
                case "Say":
                    if (args.Count >= 1) map["text"] = args[0];
                    break;
                case "Emote":
                    if (args.Count >= 1) map["emoji"] = args[0];
                    break;
                case "LookAt":
                     if (args.Count >= 1) map["target"] = args[0];
                     break;
                case "Interact":
                     if (args.Count >= 1) map["target"] = args[0];
                     break;
                 case "Stop":
                     // No args
                     break;
                default:
                    // Generic arg mapping
                    for(int i=0; i<args.Count; i++) map[$"arg{i}"] = args[i];
                    break;
            }
            return map;
        }
    }
}
