using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace Carom {
    class Glb {
        // PointF => Vector2
        public static Vector2 PointFToVector2(PointF pt) {
            return new Vector2(pt.X, pt.Y);
        }

        // Vector2 => PointF
        public static PointF Vector2ToPointF(Vector2 v) {
            return new PointF(v.X, v.Y);
        }

        // 원과 선분의 교점
        public static Vector2 FindLineCircleIntersections (Vector2 cp, float cr, Vector2 p1, Vector2 p2) {
            float a, b, c, det;

            Vector2 dP1P2 = p2 - p1;
            Vector2 dCP1 = p1 - cp;

            a = dP1P2.LengthSquared();
            b = 2 * Vector2.Dot(dP1P2, dCP1);
            c = dCP1.LengthSquared() - cr * cr;

            det = b * b - 4 * a * c;

            if (det > 0) {
                // Two solutions.
                float t1 = (float)((-b - Math.Sqrt(det)) / (2 * a));
                float t2 = (float)((-b + Math.Sqrt(det)) / (2 * a));
                if (t1 >= 0 && t1 <= 1)
                    return p1 + dP1P2*t1;
                else if (t2 >= 0 && t2 <= 1)
                    return p1 + dP1P2*t2;
                else if (t1 < 0 && t2 > 1)
                    return p1 + dP1P2*t2;
            }

            return p2;
        }

        // 원과 점의 교점
        public static Vector2? FindPointCircleIntersections (Vector2 cp, float cr, Vector2 p1) {
            var dist = Vector2.Distance(cp, p1);
            if (dist < cr)
                return cp + Vector2.Normalize(p1-cp)*cr;
            else
                return null;
        }

        // 점과 직선의 거리
        public static float Distance(Vector2 p, float a, float b, float c) {
            return Math.Abs(a*p.X + b*p.Y + c)/(float)Math.Sqrt(a*a+b*b);
        }

        // 수선의 발
        public static Vector2 FootOfPerpendicular(Vector2 p, Vector2 dir, Vector2 a) {
            return p + Vector2.Multiply(Vector2.Dot((a-p), dir)/dir.LengthSquared(), dir);
        }
    }
}
