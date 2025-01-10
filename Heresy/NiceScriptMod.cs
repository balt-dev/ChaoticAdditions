using System.Runtime.Serialization;
using System.Text;
using GDWeave.Godot;
using GDWeave.Godot.Variants;
using GDWeave.Modding;
using Serilog;

namespace Heresy;

public class NoMatchException : Exception {
    public NoMatchException() {}

    public NoMatchException(string? message) : base(message) {}

    public NoMatchException(string? message, Exception? innerException) : base(message, innerException) {}
}


public class TokenCursor {
    public TokenCursor(IEnumerable<Token> tokens) {
        this.List = new(tokens.ToArray());
        this.Head = this.List.First;
    }

    public LinkedList<Token> List;
    public LinkedListNode<Token>? Head;

    public void MatchAhead(IWaiter waiter, string? label = null) {
        if (!TryMatchAhead(waiter)) {
            throw new NoMatchException($"Could not match pattern {label ?? "<unlabeled>"}");
        }
    }
    public bool TryMatchAhead(IWaiter waiter) {
        var cur = Head;
        while (cur != null) {
            if (waiter.Check(cur.Value)) {
                Head = cur;
                return true;
            }
            cur = cur.Next;
        }
        return false;
    }

    public void MatchBehind(IWaiter waiter, string? label = null) {
        if (!TryMatchBehind(waiter)) {
            throw new NoMatchException($"Could not match pattern {label ?? "<unlabeled>"}");
        }
    }

    public bool TryMatchBehind(IWaiter waiter) {
        var cur = Head;
        while (cur != null) {
            if (waiter.Check(cur.Value)) {
                Head = cur;
                return true;
            }
            cur = cur.Next;
        }
        return false;
    }

    private static IEnumerable<Token> ToTokens(object? tokenlike) {
        if (tokenlike is Token tok) yield return tok;
        else if (tokenlike is TokenType ty) yield return new(ty);
        else if (tokenlike is byte a) yield return new ConstantToken(new IntVariant(a));
        else if (tokenlike is sbyte b) yield return new ConstantToken(new IntVariant(b));
        else if (tokenlike is short c) yield return new ConstantToken(new IntVariant(c));
        else if (tokenlike is ushort d) yield return new ConstantToken(new IntVariant(d));
        else if (tokenlike is int e) yield return new ConstantToken(new IntVariant(e));
        else if (tokenlike is uint f) yield return new ConstantToken(new IntVariant(f));
        else if (tokenlike is long g) yield return new ConstantToken(new IntVariant(g));
        else if (tokenlike is double h) yield return new ConstantToken(new RealVariant(h));
        else if (tokenlike is float i) yield return new ConstantToken(new RealVariant(i));
        else if (tokenlike is bool j) yield return new ConstantToken(new BoolVariant(j));
        else if (tokenlike is string k) yield return new ConstantToken(new StringVariant(k));
        else if (tokenlike is null) yield return new ConstantToken(new NilVariant());
        else if (tokenlike is NiceScriptMod.Identifier id) yield return new IdentifierToken(id.name);
        else if (tokenlike is NiceScriptMod.Wrapped wrap) {
            yield return wrap.start;
            foreach (var val in wrap.val)
                foreach (var t in ToTokens(val)) yield return t;
            yield return wrap.end;
        }
        else if (tokenlike is NiceScriptMod.Delimited delim) {
            var iter = ToTokens(delim.val).GetEnumerator();
            var last = iter.Current;
            while (iter.MoveNext()) {
                yield return last;
                foreach (var t in ToTokens(delim.delimiter)) yield return t;
                last = iter.Current;
            }
            yield return last;
        }
        else throw new NotImplementedException($"Attempted to convert object ({tokenlike?.GetType()}) to token");
    }

    public void Patch(IEnumerable<object> tok_objs) {
        foreach (var obj in tok_objs) {
            var toks = ToTokens(obj);
            if (Head == null) {
                List = new(toks);
                Head = List.Last;
                return;
            }
            
            foreach (var tok in toks) {
                var node = new LinkedListNode<Token>(tok);
                List.AddAfter(Head, node);
                Head = node;
            }
        }
    }

    public Token? PopToNext() {
        if (Head == null) {
            return null;
        }
        
        List.Remove(Head);
        var cur = Head;
        Head = Head.Next;
        return cur.Value;
    }

    public Token? PopToPrevious() {
        if (Head == null) {
            return null;
        }
        
        List.Remove(Head);
        var cur = Head;
        Head = Head.Previous;
        return cur.Value;
    }

    public string Display() {
        return string.Join(" ", DisplayStrings());
    }

    IEnumerable<string> DisplayStrings() {
        // Quick and dirty print
        foreach (var tok in List) {
            if (tok is ConstantToken ct) {
                if (ct.Value is NilVariant) yield return "null";
                else if (ct.Value is BoolVariant bv) yield return bv.Value ? "true" : "false";
                else if (ct.Value is StringVariant sv) yield return $"\"{sv.Value}\"";
                else yield return ct.Value.GetValue().ToString()!;
            } else if (tok is IdentifierToken it) {
                yield return it.Name;
            } else switch (tok.Type) {
                case TokenType.Empty: break;
                case TokenType.Identifier: break;
                case TokenType.Constant: break;
                case TokenType.Self:
                    yield return "self"; break;
                case TokenType.BuiltInType:
                    yield return Enum.GetName((VariantType) (tok.AssociatedData ?? 2 << 31)) ?? "<type>";
                    break;
                case TokenType.BuiltInFunc:
                    yield return Enum.GetName((BuiltinFunction) (tok.AssociatedData ?? 2 << 31)) ?? "<fn>";
                    yield return ""; break;
                case TokenType.OpIn:
                    yield return "in"; break;
                case TokenType.OpEqual:
                    yield return "=="; break;
                case TokenType.OpNotEqual:
                    yield return "!="; break;
                case TokenType.OpLess:
                    yield return "<"; break;
                case TokenType.OpLessEqual:
                    yield return "<="; break;
                case TokenType.OpGreater:
                    yield return ">"; break;
                case TokenType.OpGreaterEqual:
                    yield return ">="; break;
                case TokenType.OpAnd:
                    yield return "&&"; break;
                case TokenType.OpOr:
                    yield return "||"; break;
                case TokenType.OpNot:
                    yield return "!"; break;
                case TokenType.OpAdd:
                    yield return "+"; break;
                case TokenType.OpSub:
                    yield return "-"; break;
                case TokenType.OpMul:
                    yield return "*"; break;
                case TokenType.OpDiv:
                    yield return "/"; break;
                case TokenType.OpMod:
                    yield return "%"; break;
                case TokenType.OpShiftLeft:
                    yield return "<<"; break;
                case TokenType.OpShiftRight:
                    yield return ">>"; break;
                case TokenType.OpAssign:
                    yield return "="; break;
                case TokenType.OpAssignAdd:
                    yield return "+="; break;
                case TokenType.OpAssignSub:
                    yield return "-="; break;
                case TokenType.OpAssignMul:
                    yield return "*="; break;
                case TokenType.OpAssignDiv:
                    yield return "/="; break;
                case TokenType.OpAssignMod:
                    yield return "%="; break;
                case TokenType.OpAssignShiftLeft:
                    yield return "<<="; break;
                case TokenType.OpAssignShiftRight:
                    yield return ">>="; break;
                case TokenType.OpAssignBitAnd:
                    yield return "&="; break;
                case TokenType.OpAssignBitOr:
                    yield return "|="; break;
                case TokenType.OpAssignBitXor:
                    yield return "^="; break;
                case TokenType.OpBitAnd:
                    yield return "&"; break;
                case TokenType.OpBitOr:
                    yield return "|"; break;
                case TokenType.OpBitXor:
                    yield return "^"; break;
                case TokenType.OpBitInvert:
                    yield return "~"; break;
                case TokenType.CfIf:
                    yield return "if"; break;
                case TokenType.CfElif:
                    yield return "elif"; break;
                case TokenType.CfElse:
                    yield return "else"; break;
                case TokenType.CfFor:
                    yield return "for"; break;
                case TokenType.CfWhile:
                    yield return "while"; break;
                case TokenType.CfBreak:
                    yield return "break"; break;
                case TokenType.CfContinue:
                    yield return "continue"; break;
                case TokenType.CfPass:
                    yield return "pass"; break;
                case TokenType.CfReturn:
                    yield return "return"; break;
                case TokenType.CfMatch:
                    yield return "match"; break;
                case TokenType.PrFunction:
                    yield return "func"; break;
                case TokenType.PrClass:
                    yield return "class"; break;
                case TokenType.PrClassName:
                    yield return "<class name>"; break;
                case TokenType.PrExtends:
                    yield return "extends"; break;
                case TokenType.PrIs:
                    yield return "is"; break;
                case TokenType.PrOnready:
                    yield return "@onready"; break;
                case TokenType.PrTool:
                    yield return "@tool"; break;
                case TokenType.PrStatic:
                    yield return "static"; break;
                case TokenType.PrExport:
                    yield return "@export"; break;
                case TokenType.PrSetget:
                    yield return "setget"; break;
                case TokenType.PrConst:
                    yield return "const"; break;
                case TokenType.PrVar:
                    yield return "var"; break;
                case TokenType.PrAs:
                    yield return "as"; break;
                case TokenType.PrVoid:
                    yield return "void"; break;
                case TokenType.PrEnum:
                    yield return "enum"; break;
                case TokenType.PrPreload:
                    yield return "preload"; break;
                case TokenType.PrAssert:
                    yield return "assert"; break;
                case TokenType.PrYield:
                    yield return "yield"; break;
                case TokenType.PrSignal:
                    yield return "signal"; break;
                case TokenType.PrBreakpoint:
                    yield return "breakpoint"; break;
                case TokenType.PrRemote:
                    yield return "remote"; break;
                case TokenType.PrSync:
                    yield return "sync"; break;
                case TokenType.PrMaster:
                    yield return "master"; break;
                case TokenType.PrSlave:
                case TokenType.PrPuppet:
                    yield return "puppet"; break;
                case TokenType.PrRemotesync:
                    yield return "remotesync"; break;
                case TokenType.PrMastersync:
                    yield return "mastersync"; break;
                case TokenType.PrPuppetsync:
                    yield return "puppetsync"; break;
                case TokenType.BracketOpen:
                    yield return "["; break;
                case TokenType.BracketClose:
                    yield return "]"; break;
                case TokenType.CurlyBracketOpen:
                    yield return "{"; break;
                case TokenType.CurlyBracketClose:
                    yield return "}"; break;
                case TokenType.ParenthesisOpen:
                    yield return "("; break;
                case TokenType.ParenthesisClose:
                    yield return ")"; break;
                case TokenType.Comma:
                    yield return ","; break;
                case TokenType.Semicolon:
                    yield return ":"; break;
                case TokenType.Period:
                    yield return "."; break;
                case TokenType.QuestionMark:
                    yield return "?"; break;
                case TokenType.Colon:
                    yield return ":"; break;
                case TokenType.Dollar:
                    yield return "$"; break;
                case TokenType.ForwardArrow:
                    yield return "->"; break;
                case TokenType.Newline:
                    yield return "\n";
                    for (var i = 0; i < (tok.AssociatedData ?? 0); i++) yield return "\t";
                    break;
                case TokenType.ConstPi:
                    yield return "PI"; break;
                case TokenType.ConstTau:
                    yield return "TAU"; break;
                case TokenType.Wildcard:
                    yield return "*"; break;
                case TokenType.ConstInf:
                    yield return "INF"; break;
                case TokenType.ConstNan:
                    yield return "NAN"; break;
                case TokenType.Error:
                    yield return "Error"; break;
                case TokenType.Eof:
                    yield return ""; break;
                case TokenType.Cursor:
                    yield return ""; break;
            }
        }
    }
}


public abstract class NiceScriptMod(Dictionary<string, OnMatch> actions) : IScriptMod {
    public bool ShouldRun(string path) => actions.ContainsKey(path);

    public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        if (actions.TryGetValue(path, out var callback)) {
            var cursor = new TokenCursor(tokens);
            callback(ref cursor);
            // TODO: Syntax checking on the token stream? Would prevent UNHANDLED EXCEPTION
            return cursor.List;
        } else { return tokens; }
    }

    protected internal static Identifier Ident(string name) => new(name) {};
    protected internal static Wrapped Paren(params object?[] val) => new(val, new(TokenType.ParenthesisOpen), new(TokenType.ParenthesisClose)) {};
    protected internal static Wrapped Brack(params object?[] val) => new(val, new(TokenType.BracketOpen), new(TokenType.BracketClose)) {};
    protected internal static Wrapped Curly(params object?[] val) => new(val, new(TokenType.CurlyBracketOpen), new(TokenType.CurlyBracketClose)) {};
    protected internal static Delimited Delim(object? delimiter, params object?[] val) => new(delimiter, val) {};
    protected internal static Token Nl(uint tabs = 0) => new(TokenType.Newline, tabs);

    protected internal readonly struct Identifier(string name) {
        public readonly string name = name;
    }

    protected internal readonly struct Wrapped(object?[] val, Token start, Token end) {
        public readonly object?[] val = val;
        public readonly Token start = start;
        public readonly Token end = end;
    }

    protected internal readonly struct Delimited(object? delimiter, object?[] val) {
        public readonly object? delimiter = delimiter;
        public readonly object?[] val = val;
    }
}

public delegate void OnMatch(ref TokenCursor cur);