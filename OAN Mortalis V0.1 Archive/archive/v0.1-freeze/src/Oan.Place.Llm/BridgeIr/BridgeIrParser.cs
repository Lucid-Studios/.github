using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Oan.Place.Llm.BridgeIr
{
    public sealed class BridgeIrParser
    {
        private readonly string _input;
        private int _pos;
        private static readonly HashSet<string> _validKinds = new HashSet<string> 
        { 
            "MoveTo", "Say", "Emote", "LookAt", "Interact", "Stop" 
        };

        public BridgeIrParser(string input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _pos = 0;
        }

        public object Parse()
        {
            SkipWhitespace();
            if (Peek() != '(')
                throw new BridgeIrException(BridgeIrErrorCode.PROSE_INPUT, "IR must start with an S-expression: '('");

            var form = ReadSExpr();
            
            if (form.Count == 0) throw new BridgeIrException(BridgeIrErrorCode.UNKNOWN_TOPLEVEL, "Empty S-expression.");

            var head = form[0] as string;
            if (head == "oan.intent") return ParseIntent(form);
            if (head == "oan.raw") return ParseRaw(form);

            throw new BridgeIrException(BridgeIrErrorCode.UNKNOWN_TOPLEVEL, $"Unknown top-level form: {head}");
        }

        private ParsedIntent ParseIntent(List<object> form)
        {
            var result = new ParsedIntent();
            var seenKeys = new HashSet<string>();

            for (int i = 1; i < form.Count; i++)
            {
                if (form[i] is List<object> subForm)
                {
                    if (subForm.Count < 2) continue;
                    var key = subForm[0] as string;
                    if (key == null) continue;

                    if (seenKeys.Contains(key))
                        throw new BridgeIrException(BridgeIrErrorCode.DUPLICATE_FIELD, $"Duplicate field: {key}");
                    seenKeys.Add(key);

                    switch (key)
                    {
                        case "id":
                            result.Id = subForm[1] as string ?? throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "ID must be a string.");
                            break;
                        case "sli":
                            result.SliHandle = subForm[1] as string ?? throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "SLI handle must be a string.");
                            break;
                        case "kind":
                            result.Kind = subForm[1] as string ?? throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "Kind must be a string.");
                            break;
                        case "args":
                            ParseArgs(subForm, result);
                            break;
                        default:
                            throw new BridgeIrException(BridgeIrErrorCode.UNKNOWN_FIELD, $"Unknown field: {key}");
                    }
                }
            }

            if (string.IsNullOrEmpty(result.Id)) throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "Missing required field: id");
            if (string.IsNullOrEmpty(result.SliHandle)) throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "Missing required field: sli");
            if (string.IsNullOrEmpty(result.Kind)) throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "Missing required field: kind");
            if (result.Kind == "MoveTo" && (!result.X.HasValue || !result.Y.HasValue))
                throw new BridgeIrException(BridgeIrErrorCode.MISSING_FIELD, "MoveTo requires x and y arguments.");

            if (!_validKinds.Contains(result.Kind))
                throw new BridgeIrException(BridgeIrErrorCode.UNSUPPORTED_KIND, $"Unsupported kind: {result.Kind}");

            return result;
        }

        private ParsedRaw ParseRaw(List<object> form)
        {
            var result = new ParsedRaw();
            var seenKeys = new HashSet<string>();

            for (int i = 1; i < form.Count; i++)
            {
                if (form[i] is List<object> subForm)
                {
                    if (subForm.Count < 1) continue;
                    var key = subForm[0] as string;
                    if (key == null) continue;

                    if (seenKeys.Contains(key))
                        throw new BridgeIrException(BridgeIrErrorCode.DUPLICATE_FIELD, $"Duplicate field: {key}");
                    seenKeys.Add(key);

                    switch (key)
                    {
                        case "subject":
                            result.Subject = subForm.Count > 1 ? subForm[1] as string : null;
                            break;
                        case "predicate":
                            result.Predicate = subForm.Count > 1 ? subForm[1] as string : null;
                            break;
                        case "scope":
                            result.Scope = subForm.Count > 1 ? subForm[1] as string : null;
                            break;
                        case "constraints":
                            result.Constraints = ParseConstraints(subForm);
                            break;
                        default:
                            throw new BridgeIrException(BridgeIrErrorCode.UNKNOWN_FIELD, $"Unknown field in oan.raw: {key}");
                    }
                }
            }
            return result;
        }

        private Dictionary<string, string> ParseConstraints(List<object> constraintsForm)
        {
            var constraints = new Dictionary<string, string>();
            for (int i = 1; i < constraintsForm.Count; i++)
            {
                if (constraintsForm[i] is List<object> kvForm)
                {
                    if (kvForm.Count < 3 || (kvForm[0] as string) != "kv") continue;
                    var k = kvForm[1] as string;
                    var v = kvForm[2] as string;
                    if (k != null && v != null)
                    {
                        if (constraints.ContainsKey(k))
                            throw new BridgeIrException(BridgeIrErrorCode.DUPLICATE_FIELD, $"Duplicate constraint key: {k}");
                        constraints[k] = v;
                    }
                }
            }
            return constraints;
        }

        private void ParseArgs(List<object> argsForm, ParsedIntent result)
        {
            var seenArgs = new HashSet<string>();
            for (int i = 1; i < argsForm.Count; i++)
            {
                if (argsForm[i] is List<object> arg)
                {
                    if (arg.Count < 2) continue;
                    var key = arg[0] as string;
                    if (key == null) continue;

                    if (seenArgs.Contains(key))
                        throw new BridgeIrException(BridgeIrErrorCode.DUPLICATE_FIELD, $"Duplicate argument: {key}");
                    seenArgs.Add(key);

                    var val = arg[1] as string;
                    if (val == null) throw new BridgeIrException(BridgeIrErrorCode.BAD_NUMBER, $"Value for {key} must be a string.");

                    result.Parameters[key] = val;

                    // Backwards compatibility for X/Y in v0.1 tests
                    if (key == "x" && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double dvx)) 
                    {
                        if (double.IsNaN(dvx) || double.IsInfinity(dvx)) throw new BridgeIrException(BridgeIrErrorCode.NAN_OR_INF, "X cannot be NaN or Inf.");
                        result.X = dvx;
                    }
                    if (key == "y" && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double dvy)) 
                    {
                        if (double.IsNaN(dvy) || double.IsInfinity(dvy)) throw new BridgeIrException(BridgeIrErrorCode.NAN_OR_INF, "Y cannot be NaN or Inf.");
                        result.Y = dvy;
                    }
                }
            }
        }

        private List<object> ReadSExpr()
        {
            Consume('(');
            var list = new List<object>();
            while (true)
            {
                SkipWhitespace();
                if (_pos >= _input.Length) throw new BridgeIrException("IR_MALFORMED", "Unexpected end of input in S-expression.");
                if (Peek() == ')')
                {
                    Consume(')');
                    break;
                }
                if (Peek() == '(')
                {
                    list.Add(ReadSExpr());
                }
                else
                {
                    list.Add(ReadAtom());
                }
            }
            return list;
        }

        private string ReadAtom()
        {
            SkipWhitespace();
            if (Peek() == '"') return ReadString();
            
            int start = _pos;
            while (_pos < _input.Length && !char.IsWhiteSpace(_input[_pos]) && _input[_pos] != '(' && _input[_pos] != ')')
            {
                _pos++;
            }
            if (start == _pos) throw new BridgeIrException("IR_MALFORMED", "Expected atom.");
            return _input.Substring(start, _pos - start);
        }

        private string ReadString()
        {
            Consume('"');
            int start = _pos;
            while (_pos < _input.Length && _input[_pos] != '"')
            {
                _pos++;
            }
            if (_pos >= _input.Length) throw new BridgeIrException("IR_MALFORMED", "Unterminated string.");
            string s = _input.Substring(start, _pos - start);
            Consume('"');
            return s;
        }

        private char Peek() => _pos < _input.Length ? _input[_pos] : '\0';
        private void Consume(char c)
        {
            if (Peek() != c) throw new BridgeIrException("IR_MALFORMED", $"Expected '{c}' at position {_pos}");
            _pos++;
        }
        private void SkipWhitespace()
        {
            while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos])) _pos++;
        }
    }

    public class ParsedIntent
    {
        public string? Id { get; set; }
        public string? SliHandle { get; set; }
        public string? Kind { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class ParsedRaw
    {
        public string? Subject { get; set; }
        public string? Predicate { get; set; }
        public string? Scope { get; set; }
        public Dictionary<string, string> Constraints { get; set; } = new Dictionary<string, string>();
    }

    public class BridgeIrException : Exception
    {
        public string ReasonCode { get; }
        public BridgeIrException(string code, string message) : base(message)
        {
            ReasonCode = code;
        }
    }
}
