using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class Field
    {
        public int Count;
        public Platform[][] fieldData;

        public Platform this[float y, float x]
        {
            get
            {
                return fieldData[(int)y][(int)x];
            }
            set
            {
                fieldData[(int)y][(int)x] = value;
            }
        }

        public Field(int _count)
        {
            Count = _count;
            Initialize(_count);
        }

        private void Initialize(int _count)
        {
            fieldData = new Platform[_count][];
            for (int i = 0; i < _count; i++)
            {
                fieldData[i] = new Platform[_count];
                for (int j = 0; j < _count; j++)
                {
                    if (i == 0 || i == _count-1)
                    {
                        fieldData[i][j] = new Platform(true, "");
                        continue;
                    }
                    if(j == 0 || j == _count - 1)
                    {
                        fieldData[i][j] = new Platform(true, "");
                        continue;
                    }
                    Random r = new Random();
                    fieldData[i][j] = new Platform(r.NextDouble() > 0.8, GetRotation(r.NextDouble()));
                }
            }
        }

        private string GetRotation(double _r)
        {
            if (_r >= 0 && _r < 0.25) { 
                return "right";
            }
            if (_r >= 0.25 && _r < 0.5) { 
                return "up"; 
            }
            if (_r >= 0.5 && _r < 0.75) { 
                return "left";
            }
            return "down";
        }
    }
}
