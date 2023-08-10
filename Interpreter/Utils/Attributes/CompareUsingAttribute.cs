using System;

namespace Bloc.Utils.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CompareUsingAttribute<T> : Attribute { }