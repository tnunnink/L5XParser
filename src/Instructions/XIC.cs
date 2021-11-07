﻿using System.Collections.Generic;
using System.Linq;
using L5Sharp.Core;
using L5Sharp.Types;

namespace L5Sharp.Instructions
{
    public class XIC : Instruction
    {
        public XIC() : base(nameof(XIC), "Examine If Closed", GetOperands())
        {
        }

        public static NeutralText Of(ITagMember<Bool> dataBit)
        {
            return new NeutralText(new XIC(), dataBit.Name);
        }

        public IMember<IDataType> DataBit => Operands.SingleOrDefault(p => p.Name == nameof(DataBit));

        private static IEnumerable<IMember<IDataType>> GetOperands()
        {
            return new List<IMember<IDataType>>
            {
                new Member<IDataType>(nameof(DataBit), new Bool())
            };
        }
    }
}