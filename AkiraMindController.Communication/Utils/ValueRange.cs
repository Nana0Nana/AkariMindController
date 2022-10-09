using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace AkiraMindController.Communication.Utils
{
    [Serializable]
    public struct ValueRange
    {
        public float min;
        public float max;

        public ValueRange(float min, float max) { this.min = min; this.max = max; }

        public override string ToString() => $"[{min} - {max}]";

        public static IEnumerable<ValueRange> Union(IEnumerable<ValueRange> list)
        {
            var itor = list.OrderBy(x => x.min).GetEnumerator();
            if (itor.MoveNext())
            {
                var cur = itor.Current;

                while (itor.MoveNext())
                {
                    var append = itor.Current;

                    if (append.min > cur.max)
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

                        cur.max = append.max;
                    }
                }

                yield return cur;
            }
        }
        public static IEnumerable<ValueRange> Union(params ValueRange[] list) => Union(list.AsEnumerable());

        public static IEnumerable<ValueRange> Except(ValueRange r, IEnumerable<ValueRange> list)
        {
            var itor = Union(list).GetEnumerator();

            var min = r.min;
            var max = r.max;

            while (itor.MoveNext())
            {
                var cur = itor.Current;

                if (min < cur.min && cur.min < max)
                {
                    var gen = new ValueRange(min, cur.min);
                    yield return gen;
                }

                min = cur.max;
            }

            if (min < max)
            {
                var gen = new ValueRange(min, max);
                yield return gen;
            }
        }
        public static IEnumerable<ValueRange> Except(ValueRange r, params ValueRange[] list) => Except(r, list.AsEnumerable());

        public static IEnumerable<ValueRange> Intersect(IEnumerable<ValueRange> range)
        {
            var itor = range.OrderBy(x => x.min).GetEnumerator();

            IEnumerable<ValueRange> IntersectInternal()
            {
                if (itor.MoveNext())
                {
                    var prev = itor.Current;

                    while (itor.MoveNext())
                    {
                        var cur = itor.Current;

                        if (cur.min <= prev.max)
                        {
                            if (cur.max < prev.max)
                            {
                                yield return new ValueRange(cur.min, cur.max);
                                continue;
                            }
                            else
                            {
                                yield return new ValueRange(cur.min, prev.max);
                            }
                        }

                        prev = cur;
                    }
                }
            }

            return Union(IntersectInternal());
        }

        public static IEnumerable<ValueRange> Intersect(params ValueRange[] list) => Intersect(list.AsEnumerable());
    }
}
