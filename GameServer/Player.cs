using System.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class Player
    {
        public Vector2 position;
        public string userName;
        public bool pickedUpArtifact;

        public Player(Vector2 _position, int _id)
        {
            position = _position;
            Server.players.Add(_id, this);
        }
    }
}
