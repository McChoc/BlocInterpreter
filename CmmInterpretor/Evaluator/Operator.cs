using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;

namespace CmmInterpretor
{
    public static class Operator
    {
        public static IResult Plus(IValue v)
        {
            if (v.Implicit(out Number num))
                return new Number(+num.Value);

            return new Throw($"Cannot apply operator '+' on type {v.Type.ToString().ToLower()}");
        }

        public static IResult Minus(IValue v)
        {
            if (v.Implicit(out Number num))
                return new Number(-num.Value);

            return new Throw($"Cannot apply operator '-' on type {v.Type.ToString().ToLower()}");
        }

        public static IResult Add (IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value + numB.Value);

            if (a.Implicit(out Array arrA) && b.Implicit(out Array arrB))
            {
                var list = new List<IValue>();
                list.AddRange(((Array)arrA.Copy()).Values);
                list.AddRange(((Array)arrB.Copy()).Values);
                return new Array(list);
            }

            {
                if (a.Implicit(out Array arr))
                {
                    var array = (Array)arr.Copy();
                    array.Values.Add(b.Copy());
                    return array;
                }
            }

            {
                if (b.Implicit(out Array arr))
                {
                    var array = (Array)arr.Copy();
                    array.Values.Insert(0, a.Copy());
                    return array;
                }
            }

            if (a.Implicit(out String strA) && b.Implicit(out String strB))
                return new String(strA.Value + strB.Value);

            return new Throw($"Cannot apply operator '+' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Substract(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value - numB.Value);

            return new Throw($"Cannot apply operator '-' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Compare(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(Sign(numA.Value - numB.Value));

            return new Throw($"Cannot apply operator '<=>' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Multiply(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value * numB.Value);

            {
                if (a.Implicit(out Array arr) && b.Implicit(out Number num))
                {
                    var list = new List<IValue>();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        list.AddRange(((Array)arr.Copy()).Values);

                    return new Array(list);
                }
            }

            {
                if (a.Implicit(out Number num) && b.Implicit(out Array arr))
                {
                    var list = new List<IValue>();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        list.AddRange(((Array)arr.Copy()).Values);

                    return new Array(list);
                }
            }

            {
                if (a.Implicit(out String str) && b.Implicit(out Number num))
                {
                    var builder = new StringBuilder();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        builder.Append(str.Value);

                    return new String(builder.ToString());
                }
            }

            {
                if (a.Implicit(out Number num) && b.Implicit(out String str))
                {
                    var builder = new StringBuilder();

                    int amount = num.ToInt();

                    for (int i = 0; i < amount; i++)
                        builder.Append(str.Value);

                    return new String(builder.ToString());
                }
            }

            return new Throw($"Cannot apply operator '*' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Divide(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value / numB.Value);

            return new Throw($"Cannot apply operator '/' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Remainder(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.Value % numB.Value);

            return new Throw($"Cannot apply operator '%' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Power(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(Pow(numA.Value, numB.Value));

            return new Throw($"Cannot apply operator '**' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Root(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(Pow(numA.Value, 1 / numB.Value));

            return new Throw($"Cannot apply operator '//' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Logarithm(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(Log(numA.Value, numB.Value));

            return new Throw($"Cannot apply operator '%%' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Reverse(IValue v)
        {
            if (v.Implicit(out Number num))
                return new Number(~num.ToInt());

            if (v.Implicit(out TypeCollection type))
                return new TypeCollection(TypeCollection.Any.Value.Where(t => !type.Value.Contains(t)).ToHashSet());

            return new Throw($"Cannot apply operator '~' on type {v.Type.ToString().ToLower()}");
        }

        public static IResult And(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() & numB.ToInt());

            if (a.Implicit(out TypeCollection typeA) && b.Implicit(out TypeCollection typeB))
            {
                var types = new HashSet<VariableType>();

                foreach (VariableType type in System.Enum.GetValues(typeof(VariableType)))
                    if (typeA.Value.Contains(type) && typeB.Value.Contains(type))
                        types.Add(type);

                return new TypeCollection(types);
            }

            return new Throw($"Cannot apply operator '&' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Or(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() | numB.ToInt());

            if (a.Implicit(out TypeCollection typeA) && b.Implicit(out TypeCollection typeB))
            {
                var types = new HashSet<VariableType>();

                foreach (VariableType type in System.Enum.GetValues(typeof(VariableType)))
                    if (typeA.Value.Contains(type) || typeB.Value.Contains(type))
                        types.Add(type);

                return new TypeCollection(types);
            }

            return new Throw($"Cannot apply operator '|' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Xor(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() ^ numB.ToInt());

            if (a.Implicit(out TypeCollection typeA) && b.Implicit(out TypeCollection typeB))
            {
                var types = new HashSet<VariableType>();

                foreach (VariableType type in System.Enum.GetValues(typeof(VariableType)))
                    if (typeA.Value.Contains(type) != typeB.Value.Contains(type))
                        types.Add(type);

                return new TypeCollection(types);
            }

            return new Throw($"Cannot apply operator '^' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Left(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() << numB.ToInt());

            return new Throw($"Cannot apply operator '<<' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Right(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Number(numA.ToInt() >> numB.ToInt());

            return new Throw($"Cannot apply operator '>>' on operands of types {a.Type.ToString().ToLower()}  and  {b.Type.ToString().ToLower()}");
        }

        public static IResult Not(IValue v)
        {
            if (v.Implicit(out Bool b))
                return new Bool(!b.Value);

            return new Throw($"Cannot apply operator '!' on type {v.Type.ToString().ToLower()}");
        }

        public static IResult Less(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value < numB.Value);

            return new Throw($"Cannot apply operator '<' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult LessOrEqual(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value <= numB.Value);

            return new Throw($"Cannot apply operator '<=' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Greater(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value > numB.Value);

            return new Throw($"Cannot apply operator '>' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult GreaterOrEqual(IValue a, IValue b)
        {
            if (a.Implicit(out Number numA) && b.Implicit(out Number numB))
                return new Bool(numA.Value >= numB.Value);

            return new Throw($"Cannot apply operator '>=' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult In(IValue a, IValue b)
        {
            if (b.Implicit(out Array arr))
                return new Bool(arr.Values.Any(v => v.Equals(a)));

            if (a.Implicit(out String sub) && b.Implicit(out String str))
                return new Bool(str.Value.Contains(sub.Value));

            return new Throw($"Cannot apply operator 'in' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult NotIn(IValue a, IValue b)
        {
            if (b.Implicit(out Array arr))
                return new Bool(arr.Values.All(v => !v.Equals(a)));

            if (a.Implicit(out String sub) && b.Implicit(out String str))
                return new Bool(!str.Value.Contains(sub.Value));

            return new Throw($"Cannot apply operator 'not in' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult Is(IValue a, IValue b)
        {
            if (b.Implicit(out TypeCollection type))
                return new Bool(type.Value.Contains(a.Type));

            return new Throw($"Cannot apply operator 'is' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult IsNot(IValue a, IValue b)
        {
            if (b.Implicit(out TypeCollection type))
                return new Bool(!type.Value.Contains(a.Type));

            return new Throw($"Cannot apply operator 'is not' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");
        }

        public static IResult As(IValue a, IValue b)
        {
            if (!b.Implicit(out TypeCollection type))
                return new Throw($"Cannot apply operator 'as' on operands of types {a.Type.ToString().ToLower()} and {b.Type.ToString().ToLower()}");

            if (type.Value.Count != 1)
                return new Throw($"Cannot apply operator 'as' on a composite type");
            
            return type.Value.Single() switch
            {
                VariableType.Void => Void.Value,
                VariableType.Null => Null.Value,
                VariableType t => a.Explicit(t)
            };
        }
    }
}
