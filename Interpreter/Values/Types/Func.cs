using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Funcs;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using ValueType = Bloc.Values.Core.ValueType;

namespace Bloc.Values.Types;

public sealed class Func : Value, IPattern, IInvokable
{
    private readonly FuncType _type;
    private readonly CaptureMode _mode;

    private readonly VariableCollection _captures;

    private readonly Parameter? _argsContainer;
    private readonly Parameter? _kwargsContainer;
    private readonly List<Parameter> _parameters;

    private readonly List<Statement> _statements;
    private readonly Dictionary<string, Label> _labels;

    internal Func()
    {
        _type = FuncType.Synchronous;
        _mode = CaptureMode.None;

        _captures = new();
        _argsContainer = null;
        _kwargsContainer = null;
        _parameters = new();
        _statements = new();
        _labels = new();
    }

    internal Func(
        FuncType type,
        CaptureMode mode,
        VariableCollection captures,
        Parameter? argsContainer,
        Parameter? kwargsContainer,
        List<Parameter> parameters,
        List<Statement> statements)
    {
        _type = type;
        _mode = mode;
        _captures = captures;
        _argsContainer = argsContainer;
        _kwargsContainer = kwargsContainer;
        _parameters = parameters;
        _statements = statements;

        _labels = StatementHelper.GetLabels(statements);
    }

    internal override ValueType GetType() => ValueType.Func;

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

    public override string ToString()
    {
        return "[func]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_type, _mode, _parameters.Count, _statements.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not Func func)
            return false;

        if (_type != func._type)
            return false;

        if (_mode != func._mode)
            return false;

        if (!_parameters.SequenceEqual(func._parameters))
            return false;

        if (!_statements.SequenceEqual(func._statements))
            return false;

        if (_captures.Variables.Count != func._captures.Variables.Count)
            return false;

        foreach (var key in _captures.Variables.Keys)
        {
            if (!func._captures.Variables.ContainsKey(key))
                return false;

            if (!_captures.Variables[key].SequenceEqual(func._captures.Variables[key]))
                return false;
        }

        return true;
    }

    public IPatternNode GetRoot()
    {
        return new PredicatePattern(this);
    }

    public Value Invoke(List<Value> args, Dictionary<string, Value> kwargs, Call parent)
    {
        var @params = new VariableCollection();

        var restArray = new Array();
        var restStruct = new Struct();

        var remainingParameters = new List<Parameter>(_parameters);

        foreach (var (name, value) in kwargs)
        {
            var parameter = remainingParameters.FirstOrDefault(x => x.Name == name);

            if (parameter is not null)
            {
                remainingParameters.Remove(parameter);

                var val = value != Void.Value ? value : parameter.Value?.Copy()
                    ?? throw new Throw($"A non-void value must be provided for the required parameter '{name}'");

                @params.Add(new(true, name, val.GetOrCopy(true), @params));
            }
            else if (_kwargsContainer is not null)
            {
                if (value != Void.Value)
                    restStruct.Values.Add(name, value);
            }
            else
            {
                throw new Throw($"This function does not have a parameter named '{name}'");
            }
        }

        foreach (var value in args)
        {
            if (remainingParameters.Count > 0)
            {
                var parameter = remainingParameters[0];
                remainingParameters.RemoveAt(0);

                var name = parameter.Name;

                var val = value != Void.Value ? value : parameter.Value?.Copy()
                    ?? throw new Throw($"A non-void value must be provided for the required parameter '{name}'");

                @params.Add(new(true, name, val.GetOrCopy(true), @params));
            }
            else if (_argsContainer is not null)
            {
                if (value != Void.Value)
                    restArray.Values.Add(value);
            }
            else
            {
                throw new Throw($"This function does not take this many positional arguments");
            }
        }

        foreach (var parameter in remainingParameters)
        {
            if (parameter.Value is not null)
                @params.Add(new(true, parameter.Name, parameter.Value.GetOrCopy(true), @params));
            else
                throw new Throw($"A non-void value must be provided for the required parameter '{parameter.Name}'");
        }

        if (_argsContainer is not null)
            @params.Add(new(true, _argsContainer.Name, restArray.GetOrCopy(true), @params));

        if (_kwargsContainer is not null)
            @params.Add(new(true, _kwargsContainer.Name, restStruct.GetOrCopy(true), @params));

        var call = new Call(parent, _captures, @params);

        return _type switch
        {
            FuncType.Asynchronous => new Task(() => Execute(call)),
            FuncType.Generator => new Iter(call, _statements),
            _ => Execute(call),
        };
    }

    internal Value Execute(Call call)
    {
        try
        {
            for (var i = 0; i < _statements.Count; i++)
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

    internal sealed record Parameter
    {
        internal string Name { get; }
        internal Value? Value { get; }

        internal Parameter(string name, Value? value)
        {
            Name = name;
            Value = value;
        }

        internal void Deconstruct(out string name, out Value? value)
        {
            name = Name;
            value = Value;
        }
    }
}