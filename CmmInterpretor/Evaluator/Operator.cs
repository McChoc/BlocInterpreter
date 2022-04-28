using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmmInterpretor
{
    public static class Operator
    {
        public static IResult Positive(Value v)
        {
            if (v.Implicit(out Number num))
                return new Number(+num.Value);

            return new Throw($"Cannot apply operator '+' on type {v.TypeOf().ToString().ToLower()}");
        }

        public static IResult Negative(Value v)
        {
            if (v.Implicit(out Number num))
                return new Number(-num.Value);

            return new Throw($"Cannot apply operator '-' on type {v.TypeOf().ToString().ToLower()}");
        }

        public static IResult Add (Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value + numB.Value);

            if (a is Array && b is Array)
            {
                var list = new List<Value>();
                list.AddRange(((Array)a.Copy()).Variables);
                list.AddRange(((Array)b.Copy()).Variables);
                return new Array(list);
            }

            if (a is Array)
            {
                var array = (Array)a.Copy();
                array.Variables.Add(b.Copy());
                return array;
            }

            if (b is Array)
            {
                var array = (Array)b.Copy();
                array.Variables.Insert(0, a.Copy());
                return array;
            }

            if (a.Implicit(out String strA) && b.Implicit(out String strB))
                return new String(strA.Value + strB.Value);

            return new Throw($"Cannot apply operator '+' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Substract(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value - numB.Value);

            return new Throw($"Cannot apply operator '-' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Compare(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(System.Math.Sign(numA.Value - numB.Value));

            return new Throw($"Cannot apply operator '-' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Multiply(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value * numB.Value);

            {
                if (a is Array && b is Number num)
                {
                    var list = new List<Value>();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        list.AddRange(((Array)a.Copy()).Variables);

                    return new Array(list);
                }
            }

            {
                if (a is Number num && b is Array)
                {
                    var list = new List<Value>();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        list.AddRange(((Array)b.Copy()).Variables);

                    return new Array(list);
                }
            }

            {
                if (a is String str && b is Number num)
                {
                    var builder = new StringBuilder();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        builder.Append(str.Value);

                    return new String(builder.ToString());
                }
            }

            {
                if (a is Number num && b is String str)
                {
                    var builder = new StringBuilder();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        builder.Append(str.Value);

                    return new String(builder.ToString());
                }
            }

            return new Throw($"Cannot apply operator '*' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Divide(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value / numB.Value);

            return new Throw($"Cannot apply operator '/' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Remainder(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value % numB.Value);

            return new Throw($"Cannot apply operator '%' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Power(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(System.Math.Pow(numA.Value, numB.Value));

            return new Throw($"Cannot apply operator '**' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Root(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(System.Math.Pow(numA.Value, 1 / numB.Value));

            return new Throw($"Cannot apply operator '//' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Logarithm(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(System.Math.Log(numA.Value, numB.Value));

            return new Throw($"Cannot apply operator '%%' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Reverse(Value v)
        {
            if (v.Implicit(out Number num))
                return new Number(~num.ToInt());

            if (v.Implicit(out Type type))
                return new Type(Type.Any.Value.Where(t => !type.Value.Contains(t)).ToHashSet());

            return new Throw($"Cannot apply operator '~' on type {v.TypeOf().ToString().ToLower()}");
        }

        public static IResult And(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() & numB.ToInt());

            if (a.Implicit(out Type typeA) && b.Implicit(out Type typeB))
            {
                var types = new HashSet<VariableType>();

                foreach (VariableType type in System.Enum.GetValues(typeof(VariableType)))
                {
                    if (typeA.Value.Contains(type) && typeB.Value.Contains(type))
                        types.Add(type);
                }

                return new Type(types);
            }

            return new Throw($"Cannot apply operator '&' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Or(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() | numB.ToInt());

            if (a.Implicit(out Type typeA) && b.Implicit(out Type typeB))
            {
                var types = new HashSet<VariableType>();

                foreach (VariableType type in System.Enum.GetValues(typeof(VariableType)))
                    if (typeA.Value.Contains(type) || typeB.Value.Contains(type))
                        types.Add(type);

                return new Type(types);
            }

            return new Throw($"Cannot apply operator '|' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Xor(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() ^ numB.ToInt());

            if (a.Implicit(out Type typeA) && b.Implicit(out Type typeB))
            {
                var types = new HashSet<VariableType>();

                foreach (VariableType type in System.Enum.GetValues(typeof(VariableType)))
                    if (typeA.Value.Contains(type) != typeB.Value.Contains(type))
                        types.Add(type);

                return new Type(types);
            }

            return new Throw($"Cannot apply operator '^' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Left(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() << numB.ToInt());

            return new Throw($"Cannot apply operator '<<' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Right(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() >> numB.ToInt());

            return new Throw($"Cannot apply operator '>>' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Not(Value v)
        {
            if (v.Implicit(out Bool b))
                return new Bool(!b.Value);

            return new Throw($"Cannot apply operator '!' on type {v.TypeOf().ToString().ToLower()}");
        }

        public static IResult Less(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value < numB.Value);

            return new Throw($"Cannot apply operator '<' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult LessOrEqual(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value <= numB.Value);

            return new Throw($"Cannot apply operator '<=' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Greater(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value > numB.Value);

            return new Throw($"Cannot apply operator '>' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult GreaterOrEqual(Value a, Value b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value >= numB.Value);

            return new Throw($"Cannot apply operator '>=' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult In(Value a, Value b)
        {
            if (b.Implicit(out Array arr))
                return new Bool(arr.Variables.Any(v => v.Equals(a)));

            if (a.Implicit(out String sub) && b.Implicit(out String str))
                return new Bool(str.Value.Contains(sub.Value));

            return new Throw($"Cannot apply operator 'in' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult Is(Value a, Value b)
        {
            if (b.Implicit(out Type type))
                return new Bool(type.Value.Contains(a.TypeOf()));

            return new Throw($"Cannot apply operator 'is' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }

        public static IResult As(Value a, Value b)
        {
            if (b.Implicit(out Type type))
            {
                return type.Value.Single() switch
                {
                    VariableType.Void => new Void(),
                    VariableType.Null => new Null(),
                    VariableType.Bool => a.Explicit<Bool>(),
                    VariableType.Number => a.Explicit<Number>(),
                    VariableType.Range => a.Explicit<Range>(),
                    VariableType.String => a.Explicit<String>(),
                    VariableType.Tuple => a.Explicit<Tuple>(),
                    VariableType.Array => a.Explicit<Array>(),
                    VariableType.Struct => a.Explicit<Struct>(),
                    VariableType.Function => a.Explicit<Function>(),
                    VariableType.Task => a.Explicit<Task>(),
                    VariableType.Reference => a.Explicit<Reference>(),
                    VariableType.Complex => a.Explicit<Complex>(),
                    VariableType.Type => a.Explicit<Type>(),
                    _ => throw new System.Exception(),
                };
            }

            return new Throw($"Cannot apply operator 'as' on operands of types {a.TypeOf().ToString().ToLower()} and {b.TypeOf().ToString().ToLower()}");
        }
    }
}
