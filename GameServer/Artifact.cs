using System.Numerics;

namespace GameServer
{
    class Artifact
    {
        public bool isPackedUp;
        public Vector2 postition;

        public Artifact(Vector2 _pos)
        {
            postition = _pos;
        }
    }
}
