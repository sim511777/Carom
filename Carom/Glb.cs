using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Carom {
    class Glb {
        // 원과 선분의 교점
        public static Vector? FindLineCircleIntersection(Vector p1, Vector p2, Vector cp, double cr) {
            double a, b, c, det;

            Vector dP1P2 = p2 - p1;
            Vector dCP1 = p1 - cp;

            a = dP1P2.LengthSquared;
            b = 2 * (dP1P2 * dCP1);
            c = dCP1.LengthSquared - cr * cr;

            det = b * b - 4 * a * c;

            var result = new List<Vector>();
            
            if (det < 0) {
                // 교점 없음
                return null;
            } else if (det == 0) {
                // 교점 하나(접점)
                double t = (double)(-b / (2 * a));
                Vector col = p1 + dP1P2*t;
                return null;
            } else {
                // 교점 두개
                double t1 = (double)((-b - Math.Sqrt(det)) / (2 * a));
                double t2 = (double)((-b + Math.Sqrt(det)) / (2 * a));
                double tt = 0;
                if (Math.Abs(t1) > Math.Abs(t2)) {
                    tt = t1;
                    t1 = t2;
                    t2 = tt;
                }
                Vector col1 = p1 + dP1P2*t1;
                Vector col2 = p1 + dP1P2*t2;
                if (t1 >= 1)
                    return null;
                if (t2 < 0)
                    return null;

                return col1;
            }
        }

        // 선분과 선분의 교점
        public static Vector? FindLineLineIntersection(Vector p1, Vector p2, Vector p3, Vector p4) {
            double under = (p2.Y-p1.Y)*(p4.X-p3.X)-(p2.X-p1.X)*(p4.Y-p3.Y);
            if(under==0)    // 평행
                return null;
 
            double _t = (p2.X-p1.X)*(p3.Y-p1.Y) - (p2.Y-p1.Y)*(p3.X-p1.X);
            double _s = (p4.X-p3.X)*(p3.Y-p1.Y) - (p4.Y-p3.Y)*(p3.X-p1.X); 
 
            double t = _t/under;
            double s = _s/under;

            if (s < 0 || s >= 1)    // 교점이 선분 밖에 있음
                return null;
            if (s == 0 && under > 0)
                return null;
 
            double px = p3.X + t * (p4.X-p3.X);
            double py = p3.Y + t * (p4.Y-p3.Y);
            return new Vector(px, py);
        }

        // 원과 점의 충돌을 찾아서 원의 외곽으로 리턴
        public static Vector? FindCirclePointCollision(Vector cp, double cr, Vector p1) {
            var dist = (cp - p1).Length;
            if (dist < cr) {
                var v = p1-cp;
                v.Normalize();
                return cp + v*cr;
            }
            else
                return null;
        }

        // 점과 직선의 거리
        public static double Distance(Vector p, double a, double b, double c) {
            return Math.Abs(a*p.X + b*p.Y + c)/Math.Sqrt(a*a+b*b);
        }

        // 수선의 발
        public static Vector FootOfPerpendicular(Vector p, Vector dir, Vector a) {
            return p + Vector.Multiply(((a-p) * dir)/dir.LengthSquared, dir);
        }

        // 반사 벡터 (반사면)
        public static Vector GerReflectMirror(Vector p, Vector mirror) {
            var n = mirror/mirror.Length;
            var r = (p * n) * n * 2 - p;
            return r;
        }

        // 반사 벡터 (반사면 법선)
        public static Vector GerSlidingNormal(Vector p, Vector norm) {
            var n = norm/norm.Length;
            var r = p - (p * n) * n;
            return r;
        }
    }

    abstract class CollisionObject {
        public Vector? colPt = null;
        public Vector? reflectDir = null;
        public abstract void CheckCollision(Vector p1, Vector p2);
    }

    class CollisionObjectSegment : CollisionObject {
        public Vector p3;
        public Vector p4;
        public CollisionObjectSegment(Vector p3, Vector p4) {
            this.p3 = p3;
            this.p4 = p4;
        }
        public override void CheckCollision(Vector p1, Vector p2) {
            this.colPt = Glb.FindLineLineIntersection(p1, p2, p3, p4);
            if (this.colPt == null)
                this.reflectDir = null;
            else {
                var v = Glb.GerReflectMirror(p2-p1, p4-p3);
                v.Normalize();
                this.reflectDir = v;
            }
        }
    }

    class CollisionObjectCircle : CollisionObject {
        public Vector cp;
        public double r;
        public CollisionObjectCircle(Vector cp, double r) {
            this.cp = cp;
            this.r = r;
        }
        public override void CheckCollision(Vector p1, Vector p2) {
            this.colPt = Glb.FindLineCircleIntersection(p1, p2, cp, r);
            if (this.colPt == null)
                this.reflectDir = null;
            else {
                var v = Glb.GerSlidingNormal(p2-p1, this.cp-(Vector)this.colPt);
                v.Normalize();
                this.reflectDir = v;
            }
        }
    }
}
