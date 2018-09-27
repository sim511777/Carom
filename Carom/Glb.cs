using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Carom {
    class Glb {
        // 원과 선분의 교점
        public static List<Vector2> FindCircleLineSegIntersections(Vector2 cp, float cr, Vector2 p1, Vector2 p2) {
            float a, b, c, det;

            Vector2 dP1P2 = p2 - p1;
            Vector2 dCP1 = p1 - cp;

            a = dP1P2.LengthSquared();
            b = 2 * Vector2.Dot(dP1P2, dCP1);
            c = dCP1.LengthSquared() - cr * cr;

            det = b * b - 4 * a * c;

            var result = new List<Vector2>();
            
            if (det < 0) {
                // 교점 없음
            } else if (det == 0) {
                // 교점 하나(접점)
                float t = (float)(-b / (2 * a));
                Vector2 col = p1 + dP1P2*t;
                if (t >= 0 && t <= 1) {
                    // 교점이 선분 안에 포함
                    //result.Add(col);
                }
            } else {
                // 교점 두개
                float t1 = (float)((-b - Math.Sqrt(det)) / (2 * a));
                float t2 = (float)((-b + Math.Sqrt(det)) / (2 * a));
                Vector2 col1 = p1 + dP1P2*t1;
                Vector2 col2 = p1 + dP1P2*t2;
                if (t1 >= 0 && t1 <= 1) {
                    // 교점이 선분 안에 포함
                    result.Add(col1);
                }
                if (t2 >= 0 && t2 <= 1) {
                    // 교점이 선분 안에 포함
                    //result.Add(col2);
                }
            }

            return result;
        }

        // 선분과 선분의 교점
        public static Vector2? FindLineSegLineSegIntersections(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            return null;
        }

        // 원과 점의 충돌을 찾아서 원의 외곽으로 리턴
        public static Vector2? FindCirclePointCollision(Vector2 cp, float cr, Vector2 p1) {
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
