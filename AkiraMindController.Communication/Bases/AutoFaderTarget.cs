using AkiraMindController.Communication.Utils;
using System;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.Bases
{
    [Serializable]
    public struct AutoFaderTarget
    {
        //just for debugger inspector
        public ValueRange[] damageRanges;
        public ValueRange[] bellRanges;
        public ValueRange[] targetRanges;

        public ValueRange moveableRange;

        public ValueRange targetPlaceRange;
        public float finalTargetFrame;

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(" ", damageRanges.Select(x => Json.Serialize(x)).ToArray()));
            sb.Append('\n');
            sb.Append(string.Join(" ", bellRanges.Select(x => Json.Serialize(x)).ToArray()));
            sb.Append('\n');
            sb.Append(string.Join(" ", targetRanges.Select(x => Json.Serialize(x)).ToArray()));
            sb.Append('\n');
            sb.Append(Json.Serialize(moveableRange));
            sb.Append('\n');
            sb.Append(Json.Serialize(targetPlaceRange));
            sb.Append('\n');
            sb.Append(finalTargetFrame);
            return sb.ToString();
        }

        public void Deerialize(string str)
        {
            var split = str.Split('\n');

            ValueRange[] des(string s) => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(Json.Deserialize<ValueRange>).ToArray();

            damageRanges = des(split[0]);
            bellRanges = des(split[1]);
            targetRanges = des(split[2]);

            moveableRange = Json.Deserialize<ValueRange>(split[3]);
            targetPlaceRange = Json.Deserialize<ValueRange>(split[4]);
            finalTargetFrame = float.Parse(split[5]);
        }
    }
}

