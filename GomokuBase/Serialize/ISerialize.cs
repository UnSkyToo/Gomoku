namespace GomokuBase.Serialize
{
    public interface ISerialize
    {
        void Serialize(SerializeHelper Helper);

        void Deserialize(DeserializeHelper Helper);
    }
}
