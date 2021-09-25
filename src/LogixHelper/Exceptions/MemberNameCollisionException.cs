﻿using System;

namespace LogixHelper.Exceptions
{
    public class MemberNameCollisionException : Exception
    {
        public MemberNameCollisionException()
        {
        }
        
        public MemberNameCollisionException(string message)
            : base(message)
        {
        }
        
        public MemberNameCollisionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}