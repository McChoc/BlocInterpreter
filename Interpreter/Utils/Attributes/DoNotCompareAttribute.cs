using System;

namespace Bloc.Utils.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DoNotCompareAttribute : Attribute { }