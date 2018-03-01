using System;
using System.Reflection;
using GomokuBase.Msg;

namespace GomokuBase.Serialize
{
    public static class MsgSerialize
    {
        public static byte[] Serialize(IMsg Msg)
        {
            var Helper = new SerializeHelper();
            var Fields = Msg.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var Filed in Fields)
            {
                if (Filed.FieldType == typeof(bool))
                {
                    Helper.WriteBool((bool)Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(bool[]))
                {
                    Helper.WriteBoolArray((bool[])Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(byte))
                {
                    Helper.WriteInt8((byte)Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(byte[]))
                {
                    Helper.WriteInt8Array((byte[])Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(short))
                {
                    Helper.WriteInt16((short)Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(short[]))
                {
                    Helper.WriteInt16Array((short[])Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(int))
                {
                    Helper.WriteInt32((int)Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(int[]))
                {
                    Helper.WriteInt32Array((int[])Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(string))
                {
                    Helper.WriteString((string)Filed.GetValue(Msg));
                }
                else if (Filed.FieldType == typeof(string[]))
                {
                    Helper.WriteStringArray((string[])Filed.GetValue(Msg));
                }
                else if (Filed.FieldType.BaseType == typeof(Enum))
                {
                    Helper.WriteEnum((Enum)Filed.GetValue(Msg));
                }
                else if (Filed.FieldType.BaseType == typeof(Array))
                {
                    var ArrayType = Filed.FieldType.GetElementType();

                    if (ArrayType != null && ArrayType.BaseType == typeof(Enum))
                    {
                        var EnumList = Filed.GetValue(Msg) as Array;
                        var Enums = new Enum[EnumList.Length];
                        var Index = 0;
                        foreach (var Data in EnumList)
                        {
                            Enums[Index++] = (Enum)Data;
                        }
                        Helper.WriteEnumArray(Enums);
                    }
                    else if (ArrayType != null && typeof(ISerialize).IsAssignableFrom(ArrayType))
                    {
                        Helper.WriteObjectArray((ISerialize[])Filed.GetValue(Msg));
                    }
                }
                else if (typeof(ISerialize).IsAssignableFrom(Filed.FieldType))
                {
                    (Filed.GetValue(Msg) as ISerialize).Serialize(Helper);
                }
            }

            return Helper.GetBuffer();
        }

        public static object Deserialize(Type MsgType, byte[] Buffer)
        {
            var Helper = new DeserializeHelper(Buffer);
            var Msg = Activator.CreateInstance(MsgType);
            var Fields = Msg.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var Filed in Fields)
            {
                if (Filed.FieldType == typeof(bool))
                {
                    Filed.SetValue(Msg, Helper.ReadBool());
                }
                else if (Filed.FieldType == typeof(bool[]))
                {
                    Filed.SetValue(Msg, Helper.ReadBoolArray());
                }
                else if (Filed.FieldType == typeof(byte))
                {
                    Filed.SetValue(Msg, Helper.ReadInt8());
                }
                else if (Filed.FieldType == typeof(byte[]))
                {
                    Filed.SetValue(Msg, Helper.ReadInt8Array());
                }
                else if (Filed.FieldType == typeof(short))
                {
                    Filed.SetValue(Msg, Helper.ReadInt16());
                }
                else if (Filed.FieldType == typeof(short[]))
                {
                    Filed.SetValue(Msg, Helper.ReadInt16Array());
                }
                else if (Filed.FieldType == typeof(int))
                {
                    Filed.SetValue(Msg, Helper.ReadInt32());
                }
                else if (Filed.FieldType == typeof(int[]))
                {
                    Filed.SetValue(Msg, Helper.ReadInt32Array());
                }
                else if (Filed.FieldType == typeof(string))
                {
                    Filed.SetValue(Msg, Helper.ReadString());
                }
                else if (Filed.FieldType == typeof(string[]))
                {
                    Filed.SetValue(Msg, Helper.ReadStringArray());
                }
                else if (Filed.FieldType.BaseType == typeof(Enum))
                {
                    Filed.SetValue(Msg, Helper.ReadEnum(Filed.FieldType));
                }
                else if (Filed.FieldType.BaseType == typeof(Array))
                {
                    var ArrayType = Filed.FieldType.GetElementType();

                    if (ArrayType != null && ArrayType.BaseType == typeof(Enum))
                    {
                        var EnumArray = Helper.ReadEnumArray(ArrayType);
                        var ValueArray = Array.CreateInstance(ArrayType, EnumArray.Length);
                        var Index = 0;
                        foreach (var Value in EnumArray)
                        {
                            ValueArray.SetValue(Value, Index++);
                        }
                        Filed.SetValue(Msg, ValueArray);
                    }
                    else if (ArrayType != null && typeof(ISerialize).IsAssignableFrom(ArrayType))
                    {
                        var ObjectArray = Helper.ReadObjectArray(ArrayType);
                        var ValueArray = Array.CreateInstance(ArrayType, ObjectArray.Length);
                        var Index = 0;
                        foreach (var Value in ObjectArray)
                        {
                            ValueArray.SetValue(Value, Index++);
                        }
                        Filed.SetValue(Msg, ValueArray);
                    }
                }
                else if (typeof(ISerialize).IsAssignableFrom(Filed.FieldType))
                {
                    var Obj = Activator.CreateInstance(Filed.FieldType) as ISerialize;
                    Obj.Deserialize(Helper);
                    Filed.SetValue(Msg, Obj);
                }
            }

            return Msg;
        }

        public static T Deserialize<T>(byte[] Buffer)
        {
            return (T)Deserialize(typeof(T), Buffer);
        }
    }
}