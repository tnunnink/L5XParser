﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace L5Sharp.Primitives
{
    public class Dimensions : IEquatable<Dimensions>
    {
        private Dimensions()
        {
        }

        public Dimensions(ushort x)
        {
            X = x;
        }

        public Dimensions(ushort x, ushort y) : this(x)
        {
            if (y == 0)
                throw new ArgumentException("Value must be non zero");

            Y = y;
        }

        public Dimensions(ushort x, ushort y, ushort z) : this(x, y)
        {
            if (z == 0)
                throw new ArgumentException("Value must be non zero");

            Z = z;
        }

        public ushort X { get; }
        public ushort Y { get; }
        public ushort Z { get; }
        public int Length => Z > 0 ? X * Y * Z : Y > 0 ? X * Y : X;

        public static Dimensions Empty => new Dimensions();

        public static Dimensions Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            //todo regex validate string input

            if (value == string.Empty)
                return Empty;

            var numbers = value.Split(' ').Select(v => Convert.ToUInt16(v)).ToList();

            if (numbers.Count > 3)
                throw new InvalidOperationException();

            return numbers.Count switch
            {
                3 => new Dimensions(numbers[0], numbers[1], numbers[2]),
                2 => new Dimensions(numbers[0], numbers[1]),
                1 => new Dimensions(numbers[0]),
                _ => Empty
            };
        }
        
        public override string ToString()
        {
            return Z > 0 ? $"{X} {Y} {Z}"
                : Y > 0 ? $"{X} {Y}"
                : X > 0 ? $"{X}" : string.Empty;
        }

        public IEnumerable<string> GenerateIndices()
        {
            var indices = new List<string>();

            for (ushort i = 0; i < X; i++)
            {
                if (Y == 0)
                    indices.Add(GenerateIndex(i));

                for (ushort j = 0; j < Y; j++)
                {
                    if (Z == 0)
                        indices.Add(GenerateIndex(i, j));

                    for (ushort k = 0; k < Z; k++)
                        indices.Add(GenerateIndex(i, j, k));
                }
            }

            return indices;
        }
        
        public bool Equals(Dimensions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Dimensions)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(Dimensions left, Dimensions right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Dimensions left, Dimensions right)
        {
            return !Equals(left, right);
        }

        private static string GenerateIndex(ushort x)
        {
            return $"[{x}]";
        }

        private static string GenerateIndex(ushort x, ushort y)
        {
            return $"[{x},{y}]";
        }

        private static string GenerateIndex(ushort x, ushort y, ushort z)
        {
            return $"[{x},{y},{z}]";
        }
    }
}