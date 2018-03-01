using GomokuBase.Serialize;

namespace GomokuBase.Base
{
    public class BoardPoint : ISerialize
    {
        public int X_;
        public int Y_;

        public BoardPoint()
        {
            X_ = 0;
            Y_ = 0;
        }

        public BoardPoint(int X, int Y)
        {
            X_ = X;
            Y_ = Y;
        }

        public void Serialize(SerializeHelper Helper)
        {
            Helper.WriteInt32(X_);
            Helper.WriteInt32(Y_);
        }

        public void Deserialize(DeserializeHelper Helper)
        {
            X_ = Helper.ReadInt32();
            Y_ = Helper.ReadInt32();
        }
    }
}
