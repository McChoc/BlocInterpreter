﻿using Bloc.Values;

namespace Bloc.Variables
{
    internal class ArrayVariable : Variable
    {
        private readonly Array _parent;

        internal ArrayVariable(Value value, Array parent) : base(value)
        {
            _parent = parent;
        }

        public override void Delete()
        {
            _parent.Values.Remove(this);

            base.Delete();
        }
    }
}