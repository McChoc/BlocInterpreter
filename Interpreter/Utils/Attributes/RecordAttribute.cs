using System;

namespace Bloc.Utils.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RecordAttribute : Attribute { }