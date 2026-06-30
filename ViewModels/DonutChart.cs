using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace JobMore.ViewModels
{
    /// <summary>도넛 차트 한 조각.</summary>
    public class DonutSlice
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string Percent { get; set; }
        public string ColorHex { get; set; }
        public Brush Fill { get; set; }
        public Geometry Geometry { get; set; }
    }

    public static class DonutChart
    {
        private static readonly string[] Palette =
        {
            "#7C5CFC", "#2C9BD6", "#2BB673", "#F0922B", "#E0524F", "#D659B0", "#5A6BD8", "#7A8290"
        };

        // 도넛 도형 좌표 (viewBox 0~200)
        private const double CX = 100, CY = 100, R = 92, IR = 56;

        /// <summary>(이름,건수) 목록 → 도넛 조각들. 항목이 많으면 상위 maxItems + '기타'로 묶음.</summary>
        public static List<DonutSlice> Build(IEnumerable<KeyValuePair<string, int>> data, int maxItems = 6)
        {
            var list = data.Where(kv => kv.Value > 0)
                           .OrderByDescending(kv => kv.Value)
                           .ToList();
            if (list.Count > maxItems)
            {
                var top = list.Take(maxItems - 1).ToList();
                int rest = list.Skip(maxItems - 1).Sum(kv => kv.Value);
                top.Add(new KeyValuePair<string, int>("기타", rest));
                list = top;
            }

            int total = list.Sum(kv => kv.Value);
            var result = new List<DonutSlice>();
            if (total == 0) return result;

            double start = 0;
            int ci = 0;
            foreach (var kv in list)
            {
                double frac = (double)kv.Value / total;
                double sweep = frac * 360.0;
                if (sweep >= 359.999) sweep = 359.999; // 단일 항목 보정
                string hex = Palette[ci % Palette.Length];
                result.Add(new DonutSlice
                {
                    Name = kv.Key,
                    Count = kv.Value,
                    Percent = $"{frac * 100:0}%",
                    ColorHex = hex,
                    Fill = (Brush)new BrushConverter().ConvertFromString(hex),
                    Geometry = Ring(start, start + sweep)
                });
                start += sweep;
                ci++;
            }
            return result;
        }

        private static Point P(double angleDeg, double radius)
        {
            double t = (angleDeg - 90) * Math.PI / 180.0;
            return new Point(CX + radius * Math.Cos(t), CY + radius * Math.Sin(t));
        }

        private static Geometry Ring(double a0, double a1)
        {
            bool large = (a1 - a0) > 180;
            var p1o = P(a0, R);
            var p2o = P(a1, R);
            var p2i = P(a1, IR);
            var p1i = P(a0, IR);

            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(p1o, true, true);
                ctx.ArcTo(p2o, new Size(R, R), 0, large, SweepDirection.Clockwise, true, false);
                ctx.LineTo(p2i, true, false);
                ctx.ArcTo(p1i, new Size(IR, IR), 0, large, SweepDirection.Counterclockwise, true, false);
            }
            g.Freeze();
            return g;
        }
    }
}
