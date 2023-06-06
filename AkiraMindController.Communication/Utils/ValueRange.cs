using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using static AkiraMindController.Communication.Utils.ValueRange.FlagValue;

namespace AkiraMindController.Communication.Utils
{
    [Serializable]
    public struct ValueRange
    {
        public float min;
        public float max;

        public ValueRange(float min, float max) { this.min = min; this.max = max; }

        public override string ToString() => $"[{min} ~ {max}]";

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

        public struct FlagValue
        {
            public enum Flag
            {
                EnterA,
                LeaveA,
                EnterB,
                LeaveB,
            }

            public readonly float Value;
            public readonly Flag Flags;

            public FlagValue(float x, Flag f)
            {
                Flags = f;
                Value = x;
            }
        }

        public static IEnumerable<ValueRange> Except(IEnumerable<ValueRange> a, IEnumerable<ValueRange> b)
        {
            var itorA = Union(a).SelectMany(x => new FlagValue[] { new(x.min, Flag.EnterA), new(x.max, Flag.LeaveA) });
            var itorB = Union(b).SelectMany(x => new FlagValue[] { new(x.min, Flag.EnterB), new(x.max, Flag.LeaveB) });

            var itor = itorA.Concat(itorB).OrderBy(x => x.Value).GetEnumerator();
            var value = 0;

            void applyFlag(Flag f)
            {
                value += f switch
                {
                    Flag.EnterA or Flag.LeaveB => 1,
                    Flag.EnterB or Flag.LeaveA => -1,
                    _ => 0
                };
            }

            if (itor.MoveNext())
            {
                var prev = itor.Current;
                applyFlag(prev.Flags);

                while (itor.MoveNext())
                {
                    var cur = itor.Current;

                    if (value == 1 && prev.Value != cur.Value)
                        yield return new(prev.Value, cur.Value);

                    applyFlag(cur.Flags);
                    prev = itor.Current;
                }
            }
        }

        public static IEnumerable<ValueRange> Intersect(IEnumerable<ValueRange> a, IEnumerable<ValueRange> b)
        {
            var itorA = Union(a).SelectMany(x => new FlagValue[] { new(x.min, Flag.EnterA), new(x.max, Flag.LeaveA) });
            var itorB = Union(b).SelectMany(x => new FlagValue[] { new(x.min, Flag.EnterB), new(x.max, Flag.LeaveB) });

            var itor = itorA.Concat(itorB).OrderBy(x => x.Value).GetEnumerator();
            var value = 0;

            void applyFlag(Flag f)
            {
                value += f switch
                {
                    Flag.EnterA  or Flag.EnterB => 1,
                    Flag.LeaveA or Flag.LeaveB => -1,
                    _ => 0
                };
            }

            if (itor.MoveNext())
            {
                var prev = itor.Current;
                applyFlag(prev.Flags);

                while (itor.MoveNext())
                {
                    var cur = itor.Current;

                    if (value == 2 && prev.Value != cur.Value)
                        yield return new(prev.Value, cur.Value);

                    applyFlag(cur.Flags);
                    prev = itor.Current;
                }
            }
        }
    }
}
