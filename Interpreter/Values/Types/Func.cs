using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using Bloc.Funcs;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Func : Value, IPattern, IInvokable
{
    private readonly FuncType _type;
    private readonly CaptureMode _mode;

    private readonly VariableCollection _toplvlVariables;
    private readonly VariableCollection _outerVariables;
    private readonly string? _packingParameterName;
    private readonly string? _kwPackingParameterName;
    private readonly List<Parameter> _parameters;
    private readonly List<Statement> _statements;

    [DoNotCompare]
    private readonly Dictionary<string, LabelInfo> _labels;

    internal Func()
    {
        _type = FuncType.Synchronous;
        _mode = CaptureMode.None;

        _toplvlVariables = new();
        _outerVariables = new();
        _packingParameterName = null;
        _kwPackingParameterName = null;
        _parameters = new();
        _statements = new();
        _labels = new();
    }

    internal Func(
        FuncType type,
        CaptureMode mode,
        VariableCollection toplvlVariables,
        VariableCollection outerVariables,
        string? packingParameterName,
        string? kwPackingParameterName,
        List<Parameter> parameters,
        List<Statement> statements)
    {
        _type = type;
        _mode = mode;
        _toplvlVariables = toplvlVariables;
        _outerVariables = outerVariables;
        _packingParameterName = packingParameterName;
        _kwPackingParameterName = kwPackingParameterName;
        _parameters = parameters;
        _statements = statements;

        _labels = StatementHelper.GetLabels(statements);
    }

    public IPatternNode GetRoot() => new PredicatePattern(this);
    public override ValueType GetType() => ValueType.Func;
    public override string ToString() => "[func]";

    internal Task InvokeAsync(Call parent)
    {
        var call = new Call(parent, _toplvlVariables, _outerVariables, new());

        return new Task(() => Execute(call));
    }

    public Value Invoke(List<Value?> args, Dictionary<string, Value> kwargs, Call parent)
    {
        var arguments = new VariableCollection();

        foreach (var parameter in _parameters)
        {
            Value? value = null;

            switch (parameter.Type)
            {
                case ParameterType.KeywordOnly:
                case ParameterType.Standard when kwargs.ContainsKey(parameter.Name):
                    value = kwargs.Remove(parameter.Name, out var val)
                        ? val.GetOrCopy(true)
                        : parameter.DefaultValue?.Copy(true)
                            ?? throw new Throw($"A value must be provided for the required parameter '{parameter.Name}'");
                    break;

                case ParameterType.PositionalOnly:
                case ParameterType.Standard:
                    if (args.Count > 0)
                    {
                        value = args[0]?.GetOrCopy(true);
                        args.RemoveAt(0);
                    }

                    value ??= parameter.DefaultValue?.Copy(true)
                        ?? throw new Throw($"A value must be provided for the required parameter '{parameter.Name}'");
                    break;

                default:
                    throw new System.Exception();
            }

            arguments.Add(true, parameter.Name, value);
        }

        if (_packingParameterName is not null)
        {
            var values = args.OfType<Value>().ToList();
            var packingArray = new Array(values).GetOrCopy(true);

            arguments.Add(true, _packingParameterName, packingArray);
        }
        else if (args.Count > 0)
        {
            throw new Throw($"To many positional arguments were provided");
        }

        if (_kwPackingParameterName is not null)
        {
            var packingStruct = new Struct(kwargs).GetOrCopy(true);

            arguments.Add(true, _kwPackingParameterName, packingStruct);
        }
        else if (kwargs.Count > 0)
        {
            throw new Throw($"This function does not have a parameter named '{kwargs.First().Key}'");
        }

        var call = new Call(parent, _toplvlVariables, _outerVariables, arguments);

        return _type switch
        {
            FuncType.Asynchronous => new Task(() => Execute(call)),
            FuncType.Generator => new Iter(call, _statements),
            _ => Execute(call),
        };
    }

    private Value Execute(Call call)
    {
        try
        {
            for (int i = 0; i < _statements.Count; i++)
            {
                switch (_statements[i].Execute(call).FirstOrDefault())
                {
                    case Continue:
                        throw new Throw("A continue statement can only be used inside a loop");

                    case Break:
                        throw new Throw("A break statement can only be used inside a loop");

                    case Yield:
                        throw new Throw("A yield statement can only be used inside a generator");

                    case Throw @throw:
                        throw @throw;

                    case Return @return:
                        return @return.Value;

                    case Goto @goto:
                        if (_labels.TryGetValue(@goto.Label, out var label))
                        {
                            if (++label.Count > call.Engine.Options.JumpLimit)
                                throw new Throw("The jump limit was reached.");

                            i = label.Index - 1;

                            continue;
                        }

                        throw new Throw("No such label in scope");
                }
            }

            return Void.Value;
        }
        finally
        {
            call.Destroy();
        }
    }

    internal static Func Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Func func] => func,
            [_] => throw new Throw($"'func' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'func' does not have a constructor that takes {values.Count} arguments")
        };
    }

    internal sealed record Parameter(string Name, Value? DefaultValue, ParameterType Type);
}