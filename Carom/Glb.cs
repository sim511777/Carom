using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Carom {
    class Glb {
        // 원과 선분의 교점
        public static Vector2? FindCircleLineSegIntersection(Vector2 p1, Vector2 p2, Vector2 cp, float cr) {
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
                return null;
            } else if (det == 0) {
                // 교점 하나(접점)
                return null;
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
                    return col1;
                    result.Add(col1);
                }

                return null;
                if (t2 >= 0 && t2 <= 1) {
                    // 교점이 선분 안에 포함
                    //result.Add(col2);
                }
            }
        }

        // 선분과 선분의 교점
        public static Vector2? FindLineSegIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            float under = (p2.Y-p1.Y)*(p4.X-p3.X)-(p2.X-p1.X)*(p4.Y-p3.Y);
            if(under==0)    // 평행
                return null;
 
            float _t = (p2.X-p1.X)*(p3.Y-p1.Y) - (p2.Y-p1.Y)*(p3.X-p1.X);
            float _s = (p4.X-p3.X)*(p3.Y-p1.Y) - (p4.Y-p3.Y)*(p3.X-p1.X); 
 
            float t = _t/under;
            float s = _s/under; 
 
            if(t<=0.0 || t>=1.0 || s<=0.0 || s>=1.0)    // 교점이 선분 밖에 있음
                return null;
 
            float px = p3.X + t * (p4.X-p3.X);
            float py = p3.Y + t * (p4.Y-p3.Y);
            return new Vector2(px, py);
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

        // 반사 벡터 (반사면)
        public static Vector2 GerReflectMirror(Vector2 p, Vector2 mirror) {
            var n = Vector2.Normalize(mirror);
            var r = Vector2.Dot(p, n) * n * 2 - p;
            return r;
        }

        // 반사 벡터 (반사면 법선)
        public static Vector2 GerSlidingNormal(Vector2 p, Vector2 norm) {
            var n = Vector2.Normalize(norm);
            var r = p - Vector2.Dot(p, n) * n;
            return r;
        }
    }

    abstract class CollisionObject {
        public Vector2? colPt = null;
        public Vector2? reflectDir = null;
        public abstract void CheckCollision(Vector2 p1, Vector2 p2);
    }

    class CollisionObjectSegment : CollisionObject {
        public Vector2 p3;
        public Vector2 p4;
        public CollisionObjectSegment(Vector2 p3, Vector2 p4) {
            this.p3 = p3;
            this.p4 = p4;
        }
        public override void CheckCollision(Vector2 p1, Vector2 p2) {
            this.colPt = Glb.FindLineSegIntersection(p1, p2, p3, p4);
            if (this.colPt == null)
                this.reflectDir = null;
            else
                this.reflectDir = Vector2.Normalize(Glb.GerReflectMirror(p2-p1, p4-p3));
        }
    }

    class CollisionObjectCircle : CollisionObject {
        public Vector2 cp;
        public float r;
        public CollisionObjectCircle(Vector2 cp, float r) {
            this.cp = cp;
            this.r = r;
        }
        public override void CheckCollision(Vector2 p1, Vector2 p2) {
            this.colPt = Glb.FindCircleLineSegIntersection(p1, p2, cp, r);
            if (this.colPt == null)
                this.reflectDir = null;
            else
                this.reflectDir = Vector2.Normalize(Glb.GerSlidingNormal(p2-p1, this.cp-(Vector2)this.colPt));
        }
    }
}
