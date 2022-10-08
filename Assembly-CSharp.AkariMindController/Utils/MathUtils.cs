using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.Utils
{
    internal class MathUtils
    {
        public static double CalculateXFromTwoPointFormFormula(double y, double x1, double y1, double x2, double y2)
        {
            var by = y2 - y1;
            var bx = x2 - x1;

            if (by == 0)
                return x1;

            return (y - y1) * 1.0 / by * bx + x1;
        }
    }
}
