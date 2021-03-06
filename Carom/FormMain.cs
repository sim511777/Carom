﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Drawing.Drawing2D;
using Carom.Properties;
using System.Windows;
using System.Windows.Media;
using ShimLib;

namespace Carom {
    public partial class FormMain : Form {
        Vector[] balls;    // Cue1, Cue2, Obj1, Obj2, Dir

        public FormMain() {
            InitializeComponent();
            this.grdProp.SelectedObject = Settings.Default;
            this.InitBalls();
            this.ZoomToTable();
        }

        private void InitBalls() {
            this.balls = new Vector[] {
                new Vector(-Settings.Default.AreaWidth/4, Settings.Default.BallDiameter * 1.1f),
                new Vector(Settings.Default.AreaWidth*4/8-Settings.Default.BallDiameter/2, 0),
                new Vector(Settings.Default.AreaWidth/4, 0),
                new Vector(-Settings.Default.AreaWidth/4, 0),
                new Vector(Settings.Default.AreaWidth/4, -Settings.Default.BallDiameter * 0.8f),
            };
            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        private CollisionObjectCircle firstCollisonObjectCircle = null;
        List<Vector> routes = new List<Vector>();
        private void CalcRoute() {
            var cueDist = Settings.Default.CueDist;
            // 1. Cue1, Dir 볼로 Line생성
            var p1 = balls[0];
            var v = balls[4]-p1;
            v.Normalize();
            var p2 = p1 + v*cueDist;

            double x1 = -Settings.Default.AreaWidth/2+Settings.Default.BallDiameter/2;
            double x2 = +Settings.Default.AreaWidth/2-Settings.Default.BallDiameter/2;
            double y1 = -Settings.Default.AreaHeight/2+Settings.Default.BallDiameter/2;
            double y2 = +Settings.Default.AreaHeight/2-Settings.Default.BallDiameter/2;
            Vector vlt = new Vector(x1, y1);
            Vector vrt = new Vector(x2, y1);
            Vector vrb = new Vector(x2, y2);
            Vector vlb = new Vector(x1, y2);
            
            List<CollisionObject> colObjs = new List<CollisionObject>();
            colObjs.Add(new CollisionObjectSegment(vlt, vrt));
            colObjs.Add(new CollisionObjectSegment(vrt, vrb));
            colObjs.Add(new CollisionObjectSegment(vrb, vlb));
            colObjs.Add(new CollisionObjectSegment(vlb, vlt));
            colObjs.AddRange(this.balls.Skip(1).Take(3).Select(ball => new CollisionObjectCircle(ball, Settings.Default.BallDiameter)));
            
            this.routes.Clear();
            firstCollisonObjectCircle = null;
            bool first = true;
            while (true) {
                colObjs.ForEach(colObj => colObj.CheckCollision(p1, p2));
                var total = colObjs.Where(colObj => colObj.colPt != null).OrderBy(colObj => (p1 - (Vector)colObj.colPt).Length);
                if (total.Count() == 0) {
                    this.routes.Add(p2);
                    break;
                } else {
                    var colObj = total.ElementAt(0);
                    var colPt = (Vector)colObj.colPt;
                    var refDir = (Vector)colObj.reflectDir;
                    this.routes.Add(colPt);
                    if (first == true) {
                        if (colObj is CollisionObjectCircle)
                            firstCollisonObjectCircle = colObj as CollisionObjectCircle;
                        first = false;
                    }
                    cueDist = cueDist - (p1 - colPt).Length; 
                    p1 = colPt;
                    p2 = p1 + refDir * cueDist;
                    if (colObj is CollisionObjectCircle) {
                        bool removed = colObjs.Remove(colObj);
                        if (removed == false)
                            throw new Exception("Remove Failed");
                    }
                }
            }
        }

        private RectangleF WoodRect {
            get {
                return new RectangleF(
                      (float)(-Settings.Default.AreaWidth / 2 - Settings.Default.CushinThickness - Settings.Default.WoodThickness)
                    , (float)(-Settings.Default.AreaHeight / 2 - Settings.Default.CushinThickness - Settings.Default.WoodThickness)
                    , (float)(Settings.Default.AreaWidth + Settings.Default.CushinThickness*2 + Settings.Default.WoodThickness*2)
                    , (float)(Settings.Default.AreaHeight + Settings.Default.CushinThickness*2 + Settings.Default.WoodThickness*2)
                );
            }
        }

        private RectangleF CushinRect {
            get {
                return new RectangleF(
                      (float)(-Settings.Default.AreaWidth / 2 - Settings.Default.CushinThickness )
                    , (float)(-Settings.Default.AreaHeight / 2 - Settings.Default.CushinThickness)
                    , (float)(Settings.Default.AreaWidth + Settings.Default.CushinThickness*2    )
                    , (float)(Settings.Default.AreaHeight + Settings.Default.CushinThickness*2   )
                );
            }
        }

        private RectangleF AreaRect {
            get {
                return new RectangleF(
                      (float)(-Settings.Default.AreaWidth / 2 )
                    , (float)(-Settings.Default.AreaHeight / 2)
                    , (float)(Settings.Default.AreaWidth      )
                    , (float)(Settings.Default.AreaHeight     )
                );
            }
        }

        private void DrawTable(Graphics g) {
            Brush brWood = new SolidBrush(Settings.Default.WoodColor);
            g.FillRectangle(brWood, this.pbxTable.RealToDrawRect(this.WoodRect));
            brWood.Dispose();
            Brush brCushion = new SolidBrush(Settings.Default.CushonColor);
            g.FillRectangle(brCushion, this.pbxTable.RealToDrawRect(this.CushinRect));
            brCushion.Dispose();
            Pen penLine = new Pen(Settings.Default.LineColor);
            var areaRect = this.pbxTable.RealToDrawRect(this.AreaRect);
            g.DrawRectangle(penLine, areaRect.X, areaRect.Y, areaRect.Width, areaRect.Height);
            penLine.Dispose();
        }

        private void DrawPoints(Graphics g) {
            Brush brPoint = new SolidBrush(Settings.Default.PointColor);
            
            List<Vector> pointPts = new List<Vector>();
            double xStep = Settings.Default.AreaWidth / 8;
            for (int i=-4; i<=4; i++) {
                pointPts.Add(new Vector(xStep*i, -Settings.Default.AreaHeight/2-Settings.Default.CushinThickness-Settings.Default.WoodThickness/2));
                pointPts.Add(new Vector(xStep*i, Settings.Default.AreaHeight/2+Settings.Default.CushinThickness+Settings.Default.WoodThickness/2));
            }
            double yStep = Settings.Default.AreaHeight / 4;
            for (int i=-2; i<=2; i++) {
                pointPts.Add(new Vector(-Settings.Default.AreaWidth/2-Settings.Default.CushinThickness-Settings.Default.WoodThickness/2, yStep*i));
                pointPts.Add(new Vector(Settings.Default.AreaWidth/2+Settings.Default.CushinThickness+Settings.Default.WoodThickness/2, yStep*i));
            }
            foreach (var pointPt in pointPts) {
                var realRect = this.VectorToRect(pointPt, (float)Settings.Default.PointDiameter);
                var drawRect = this.pbxTable.RealToDrawRect(realRect);
                g.FillEllipse(brPoint, drawRect);
            }

            brPoint.Dispose();
        }

        private RectangleF VectorToRect(Vector v, float size) {
            return new RectangleF((float)(v.X-size/2), (float)(v.Y-size/2), size, size);
        }

        private void DrawBalls(Graphics g) {
            Brush brCue1 = new SolidBrush(Settings.Default.CueBall1Color);
            Brush brCue2 = new SolidBrush(Settings.Default.CueBall2Color);
            Brush brObj1 = new SolidBrush(Settings.Default.ObjBall1Color);
            Brush brObj2 = new SolidBrush(Settings.Default.ObjBall2Color);

            g.FillEllipse(brCue1, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[0], (float)Settings.Default.BallDiameter)));
            g.FillEllipse(brCue2, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[1], (float)Settings.Default.BallDiameter)));
            g.FillEllipse(brObj1, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[2], (float)Settings.Default.BallDiameter)));
            g.FillEllipse(brObj2, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[3], (float)Settings.Default.BallDiameter)));

            brCue1.Dispose();
            brCue2.Dispose();
            brObj1.Dispose();
            brObj2.Dispose();

            g.DrawEllipse(Pens.Black, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[4], (float)Settings.Default.BallDiameter)));
            
            float r = (float)Settings.Default.BallDiameter / 2;
            Vector dvCue = balls[4] - balls[0];
            var dist = dvCue.Length;
            Vector[] pts = {
                new Vector { X = 0, Y = r },
                new Vector { X = dist, Y = r },
                new Vector { X = dist, Y = -r },
                new Vector { X = 0, Y = -r },
            };

            var theta = -Vector.AngleBetween(dvCue, new Vector(1, 0));
            
            Matrix m = Matrix.Identity;
            m.Rotate(theta);
            m.Transform(pts);
            var polygon = pts.Select(pt => pt + balls[0]).Select(pt => this.pbxTable.RealToDraw(new PointF((float)pt.X, (float)pt.Y))).ToArray();
            Pen pen = new Pen(Color.Black);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            g.DrawLine(pen, polygon[0], polygon[1]);
            g.DrawLine(pen, polygon[2], polygon[3]);
            pen.Dispose();
        }

        private void DrawRoutes(Graphics g) {
            List<PointF> drawRoutes = new List<PointF>();
            drawRoutes.Add(this.pbxTable.RealToDraw(new PointF((float)this.balls[0].X, (float)this.balls[0].Y)));
            foreach (var route in this.routes) {
                var rect = VectorToRect(route, (float)Settings.Default.BallDiameter);
                g.DrawEllipse(Pens.Orange, pbxTable.RealToDrawRect(rect));
                drawRoutes.Add(this.pbxTable.RealToDraw(new PointF((float)route.X, (float)route.Y)));
            }
            
            g.DrawLines(Pens.Orange, drawRoutes.ToArray());
        }

        private void ZoomToTable() {
            var fitRect = this.WoodRect;
            fitRect.Inflate(50, 50);
            this.pbxTable.ZoomToRect(fitRect);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.Default.Save();
        }

        private void pbxTable_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.DrawTable(g);
            this.DrawPoints(g);
            this.DrawBalls(g);
            if (this.chkShowRoute.Checked)
                this.DrawRoutes(g);
            this.pbxTable.DrawCursorPixelInfo(g);
        }

        private void btnZoomFit_Click(object sender, EventArgs e) {
            this.ZoomToTable();
        }

        private void grdProp_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        int pickBallIdx = -1;
        Vector pickOffset;
        Vector pickPt;
        private void pbxTable_MouseDown(object sender, MouseEventArgs e) {
            var ptReal = this.pbxTable.DrawToReal(e.Location);
            var vReal = new Vector(ptReal.X, ptReal.Y);
            int closestIdx = -1;
            double closestDistSq = double.MaxValue;
            for (int i=0; i<this.balls.Length; i++) {
                double distSq = (this.balls[i] - vReal).LengthSquared;
                if (distSq < closestDistSq) {
                    closestIdx = i;
                    closestDistSq = distSq;
                }
            }

            if (closestDistSq <= Settings.Default.BallDiameter*Settings.Default.BallDiameter/4) {
                this.pickBallIdx = closestIdx;
                this.pickPt = vReal;
                this.pickOffset = this.balls[closestIdx] - vReal;
                this.pbxTable.EnableMousePan = false;
            }
        }

        private void pbxTable_MouseUp(object sender, MouseEventArgs e) {
            if (pickBallIdx != -1) {
                pickBallIdx = -1;
                this.pbxTable.EnableMousePan = true;
            }
        }

        private void pbxTable_MouseMove(object sender, MouseEventArgs e) {
            if (this.pickBallIdx == -1)
                return;
            var vOld = this.balls[this.pickBallIdx];
            var ptNew = this.pbxTable.DrawToReal(e.Location);
            var vNew = new Vector(ptNew.X, ptNew.Y) + this.pickOffset;

            if (this.pickBallIdx != 4) {
                vNew.X = vNew.X.Range(-Settings.Default.AreaWidth/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaWidth/2 - Settings.Default.BallDiameter/2);
                vNew.Y = vNew.Y.Range(-Settings.Default.AreaHeight/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaHeight/2 - Settings.Default.BallDiameter/2);
                // 나머지 3개의 볼에 대해 충돌점을 찾는다.
                // 충돌점들 중 가장 가까운 충돌점으로 이동
                var collisions = this.balls
                    .Where((ball, idx) => idx != pickBallIdx && idx != 4)
                    .Select(ball => Glb.FindCirclePointCollision(ball, Settings.Default.BallDiameter, vNew))
                    .Where(ball => ball != null)
                    .OrderBy(collision => ((Vector)collision - vOld).Length);
                if (collisions.Count() > 0) {
                    vNew = (Vector)collisions.ElementAt(0);
                }
            } else {
                var vCollison = Glb.FindCirclePointCollision(balls[0], Settings.Default.BallDiameter, vNew);
                if (vCollison != null)
                    vNew = (Vector)vCollison;
            }
            this.balls[this.pickBallIdx] = vNew;
            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        private void btnInitBalls_Click(object sender, EventArgs e) {
            this.InitBalls();
        }

        private void AddCuePower(double val) {
            Settings.Default.CueDist = (Settings.Default.CueDist + val).Range(10, 100000);
            
            this.grdProp.Refresh();
            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        private void btnCueDisAdd_Click(object sender, EventArgs e) {
            this.AddCuePower(100);
        }

        private void btnCueDistSub_Click(object sender, EventArgs e) {
            this.AddCuePower(-100);
        }

        private void AddCueAngle(double val) {
            var p1 = this.balls[0];
            var p2 = this.balls[4];
            double theta = (Math.PI / 180 * val);
            var m = new System.Windows.Media.Matrix();
            m.RotateAt(theta, p1.X, p1.Y);
            this.balls[4] = p2 * m;

            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        private void btnRotAdd_Click(object sender, EventArgs e) {
            this.AddCueAngle(0.1f);
        }

        private void btnRotSub_Click(object sender, EventArgs e) {
            this.AddCueAngle(-0.1f);
        }

        private void chkShowRoute_CheckedChanged(object sender, EventArgs e) {
            this.pbxTable.Refresh();
        }
    }
}
