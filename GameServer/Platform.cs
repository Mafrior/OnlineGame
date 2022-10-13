using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class Platform
    {
        public bool isWater;
        public string rotation;

        public Platform(bool _isPlatform, string _rotation)
        {
            isWater = _isPlatform;
            rotation = _rotation;
        }
    }
}
