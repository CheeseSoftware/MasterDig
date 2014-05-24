using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig
{
    static class BetterMath
    {
        public static int Fibonacci(int i)
        {
            if (i < 1)
            {
                return 0;
            }
            else
            {
                int j = 1;
                int k = 0;


                for (int l = 1; l < i; l++)
                {
                    j += k;
                    k = j;
                }

                return j;
            }
        }
    }
}
