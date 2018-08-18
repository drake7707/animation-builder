using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFBattleGenerator.BLL
{
    class RandomNumber
    {
        private static Random rand = null;

        public static int GetRandomNr(int min, int max)
        {
            if (rand == null)
                rand = new Random();

            return rand.Next(min, max);
        }
    }
}
