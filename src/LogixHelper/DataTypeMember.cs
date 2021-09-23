﻿namespace LogixHelper
{
    public class DataTypeMember
    {
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public short Dimension { get; set; }
        public Radix Radix { get; set; }
        public bool Hidden { get; set; }
        public string Target { get; set; }
        public short BitNumber { get; set; }
        public ExternalAccess ExternalAccess { get; set; }
        public long Size { get; set; }
        public long Offset { get; set; }
        public long MinValue { get; set; }
        public long MaxValue { get; set; }
        public long DefaultValue { get; set; }
        public string Description { get; set; }
    }
}