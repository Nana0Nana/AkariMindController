using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace AkariMindControllers.Utils
{
    public struct ValueRange
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public ValueRange(float min, float max) { Min = min; Max = max; }

        public override string ToString() => $"[{Min} - {Max}]";

        public static IEnumerable<ValueRange> Union(IEnumerable<ValueRange> list)
        {
            var itor = list.OrderBy(x => x.Min).GetEnumerator();
            if (itor.MoveNext())
            {
                var cur = itor.Current;

                while (itor.MoveNext())
                {
                    var append = itor.Current;

                    if (append.Min > cur.Max)
                    {
                        /*
                         |----cur----|
                                       |----append----|
                         */
                        yield return cur;
                        cur = append;
                        continue;
                    }
                    else
                    {
                        /*
                        |----cur----|
                                |----append----|
                        |--------newCur--------|
                        */

                        cur.Max = append.Max;
                    }
                }

                yield return cur;
            }
        }
        public static IEnumerable<ValueRange> Union(params ValueRange[] list) => Union(list.AsEnumerable());

        public static IEnumerable<ValueRange> Except(ValueRange r, IEnumerable<ValueRange> list)
        {
            var itor = Union(list).GetEnumerator();

            var min = r.Min;
            var max = r.Max;

            while (itor.MoveNext())
            {
                var cur = itor.Current;

                if (min < cur.Min && cur.Min < max)
                {
                    var gen = new ValueRange(min, cur.Min);
                    yield return gen;
                }

                min = cur.Max;
            }

            if (min < max)
            {
                var gen = new ValueRange(min, max);
                yield return gen;
            }
        }
        public static IEnumerable<ValueRange> Except(ValueRange r, params ValueRange[] list) => Except(r, list.AsEnumerable());
    }
}
