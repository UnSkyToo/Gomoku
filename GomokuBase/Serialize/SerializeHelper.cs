using System;
using System.Text;
using System.Collections.Generic;

namespace GomokuBase.Serialize
{
    public class SerializeHelper
    {
        private readonly List<byte> Buffer_;

        public int Length => Buffer_.Count;

        public SerializeHelper()
        {
            Buffer_ = new List<byte>();
        }

        public byte[] GetBuffer()
        {
            return Buffer_.ToArray();
        }
        
        public void WriteBool(bool Value)
        {
            if (Value)
            {
                WriteInt8(1);
            }
            else
            {
                WriteInt8(0);
            }
        }

        public void WriteBoolArray(bool[] Value)
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteBool(Data);
            }
        }

        public void WriteInt8(byte Value)
        {
            Buffer_.Add(Value);
        }

        public void WriteInt8Array(byte[] Value)
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteInt8(Data);
            }
        }

        public void WriteInt16(short Value)
        {
            Buffer_.Add((byte)(Value & 0x00FF));
            Buffer_.Add((byte)((Value & 0xFF00) >> 8));
        }

        public void WriteInt16Array(short[] Value)
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteInt16(Data);
            }
        }

        public void WriteInt32(int Value)
        {
            Buffer_.Add((byte)(Value & 0x000000FF));
            Buffer_.Add((byte)((Value & 0x0000FF00) >> 8));
            Buffer_.Add((byte)((Value & 0x00FF0000) >> 16));
            Buffer_.Add((byte)((Value & 0xFF000000) >> 24));
        }

        public void WriteInt32Array(int[] Value)
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteInt32(Data);
            }
        }

        public void WriteString(string Value)
        {
            var Buffer = Encoding.UTF8.GetBytes(Value);
            WriteInt32(Buffer.Length);

            foreach (var Data in Buffer)
            {
                WriteInt8(Data);
            }
        }

        public void WriteStringArray(string[] Value)
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteString(Data);
            }
        }

        public void WriteEnum(Enum Value)
        {
            WriteInt32((int)((object)Value));
        }

        public void WriteEnumArray(Enum[] Value)
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteEnum(Data);
            }
        }

        public void WriteObject<T>(T Value) where T : ISerialize
        {
            Value.Serialize(this);
        }
        
        public void WriteObjectArray<T>(T[] Value) where T : ISerialize
        {
            WriteInt32(Value.Length);
            foreach (var Data in Value)
            {
                WriteObject(Data);
            }
        }
    }
}