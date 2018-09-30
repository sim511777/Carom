using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorD = System.Windows.Vector;

namespace Carom {
    class Glb {
        // 원과 선분의 교점
        public static VectorD? FindLineCircleIntersection(VectorD p1, VectorD p2, VectorD cp, double cr) {
            double a, b, c, det;

            VectorD dP1P2 = p2 - p1;
            VectorD dCP1 = p1 - cp;

            a = dP1P2.LengthSquared;
            b = 2 * (dP1P2 * dCP1);
            c = dCP1.LengthSquared - cr * cr;

            det = b * b - 4 * a * c;

            var result = new List<VectorD>();
            
            if (det < 0) {
                // 교점 없음
                return null;
            } else if (det == 0) {
                // 교점 하나(접점)
                double t = (double)(-b / (2 * a));
                VectorD col = p1 + dP1P2*t;
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
                VectorD col1 = p1 + dP1P2*t1;
                VectorD col2 = p1 + dP1P2*t2;
                if (t1 >= 1)
                    return null;
                if (t2 < 0)
                    return null;

                return col1;
            }
        }

        // 선분과 선분의 교점
        public static VectorD? FindLineLineIntersection(VectorD p1, VectorD p2, VectorD p3, VectorD p4) {
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
            return new VectorD(px, py);
        }

        // 원과 점의 충돌을 찾아서 원의 외곽으로 리턴
        public static VectorD? FindCirclePointCollision(VectorD cp, double cr, VectorD p1) {
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
        public static double Distance(VectorD p, double a, double b, double c) {
            return Math.Abs(a*p.X + b*p.Y + c)/Math.Sqrt(a*a+b*b);
        }

        // 수선의 발
        public static VectorD FootOfPerpendicular(VectorD p, VectorD dir, VectorD a) {
            return p + VectorD.Multiply(((a-p) * dir)/dir.LengthSquared, dir);
        }

        // 반사 벡터 (반사면)
        public static VectorD GerReflectMirror(VectorD p, VectorD mirror) {
            var n = mirror/mirror.Length;
            var r = (p * n) * n * 2 - p;
            return r;
        }

        // 반사 벡터 (반사면 법선)
        public static VectorD GerSlidingNormal(VectorD p, VectorD norm) {
            var n = norm/norm.Length;
            var r = p - (p * n) * n;
            return r;
        }
    }

    abstract class CollisionObject {
        public VectorD? colPt = null;
        public VectorD? reflectDir = null;
        public abstract void CheckCollision(VectorD p1, VectorD p2);
    }

    class CollisionObjectSegment : CollisionObject {
        public VectorD p3;
        public VectorD p4;
        public CollisionObjectSegment(VectorD p3, VectorD p4) {
            this.p3 = p3;
            this.p4 = p4;
        }
        public override void CheckCollision(VectorD p1, VectorD p2) {
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
        public VectorD cp;
        public double r;
        public CollisionObjectCircle(VectorD cp, double r) {
            this.cp = cp;
            this.r = r;
        }
        public override void CheckCollision(VectorD p1, VectorD p2) {
            this.colPt = Glb.FindLineCircleIntersection(p1, p2, cp, r);
            if (this.colPt == null)
                this.reflectDir = null;
            else {
                var v = Glb.GerSlidingNormal(p2-p1, this.cp-(VectorD)this.colPt);
                v.Normalize();
                this.reflectDir = v;
            }
        }
    }
}
