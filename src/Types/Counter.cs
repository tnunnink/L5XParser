﻿using L5Sharp.Core;

// ReSharper disable InconsistentNaming RSLogix naming

namespace L5Sharp.Types
{
    public sealed class Counter : Predefined
    {
        public Counter() : base(nameof(Counter).ToUpper())
        {
            RegisterMemberFields();
        }
        
        public Counter(Dint pre) : this()
        {
            PRE.DataType.SetValue(pre);
        }
        
        public override string Description => $"RSLogix {Name} DataType";
        protected override IDataType New()
        {
            return new Counter();
        }
        
        public IMember<Dint> PRE = Member.OfType<Dint>(nameof(PRE));
        public IMember<Dint> ACC = Member.OfType<Dint>(nameof(ACC));
        public IMember<Bool> CU = Member.OfType<Bool>(nameof(CU));
        public IMember<Bool> CD = Member.OfType<Bool>(nameof(CD));
        public IMember<Bool> DN = Member.OfType<Bool>(nameof(DN));
        public IMember<Bool> OV = Member.OfType<Bool>(nameof(OV));
        public IMember<Bool> UN = Member.OfType<Bool>(nameof(UN));
    }
}