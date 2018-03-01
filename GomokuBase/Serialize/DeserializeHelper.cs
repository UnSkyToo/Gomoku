using System;
using System.Text;

namespace GomokuBase.Serialize
{
    public class DeserializeHelper
    {
        private readonly byte[] Buffer_;
        private int Index_;

        public DeserializeHelper(byte[] Buffer)
        {
            Buffer_ = Buffer;
            Index_ = 0;
        }

        public bool ReadBool()
        {
            if (ReadInt8() == 1)
            {
                return true;
            }

            return false;
        }

        public bool[] ReadBoolArray()
        {
            var Length = ReadInt32();
            var Value = new bool[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadBool();
            }
            return Value;
        }

        public byte ReadInt8()
        {
            return Buffer_[Index_++];
        }

        public byte[] ReadInt8Array()
        {
            var Length = ReadInt32();
            var Value = new byte[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadInt8();
            }
            return Value;
        }

        public short ReadInt16()
        {
            var Value = (short) ((int) ReadInt8() | ((int) ReadInt8()) << 8);
            return Value;
        }

        public short[] ReadInt16Array()
        {
            var Length = ReadInt32();
            var Value = new short[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadInt16();
            }
            return Value;
        }

        public int ReadInt32()
        {
            var Value = (int) ((int) ReadInt8() | ((int) ReadInt8()) << 8 | ((int) ReadInt8()) << 16 |
                               ((int) ReadInt8()) << 24);
            return Value;
        }

        public int[] ReadInt32Array()
        {
            var Length = ReadInt32();
            var Value = new int[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadInt32();
            }
            return Value;
        }

        public string ReadString()
        {
            var Len = ReadInt32();
            var Buffer = new byte[Len];

            for (var Index = 0; Index < Len; ++Index)
            {
                Buffer[Index] = ReadInt8();
            }

            var Value = Encoding.UTF8.GetString(Buffer);
            return Value;
        }

        public string[] ReadStringArray()
        {
            var Length = ReadInt32();
            var Value = new string[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadString();
            }
            return Value;
        }
        
        public T ReadEnum<T>()
        {
            var Value = ReadInt32();
            return (T)Enum.ToObject(typeof(T), Value);
        }

        public T[] ReadEnumArray<T>()
        {
            var Length = ReadInt32();
            var Value = new T[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadEnum<T>();
            }
            return Value;
        }

        public Enum ReadEnum(Type EnumType)
        {
            var Value = ReadInt32();
            return (Enum)Enum.ToObject(EnumType, Value);
        }

        public Enum[] ReadEnumArray(Type EnumType)
        {
            var Length = ReadInt32();
            var Value = new Enum[Length];
            for (var Index = 0; Index < Length; ++Index)
            {
                Value[Index] = ReadEnum(EnumType);
            }
            return Value;
        }

        public T ReadObject<T>() where T : ISerialize, new()
        {
            var Value = new T();
            Value.Deserialize(this);
            return Value;
        }

        public object[] ReadObjectArray(Type ObjectType)
        {
            var Length = ReadInt32();
            var Value = new ISerialize[Length];

            for (var Index = 0; Index < Length; ++Index)
            {
                var Obj = Activator.CreateInstance(ObjectType) as ISerialize;
                Obj.Deserialize(this);
                Value[Index] = Obj;
            }

            return Value;
        }
    }
}