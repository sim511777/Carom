using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Numerics;
using Carom.Properties;

namespace Carom {
    public partial class FormMain : Form {
        Vector2[] balls;    // Cue1, Cue2, Obj1, Obj2, Dir

        public FormMain() {
            InitializeComponent();
            this.grdProp.SelectedObject = Settings.Default;
            this.InitBalls();
            this.ZoomToTable();
        }

        private void InitBalls() {
            this.balls = new Vector2[] {
                new Vector2(-Settings.Default.AreaWidth/4, Settings.Default.BallDiameter * 2),
                new Vector2(Settings.Default.AreaWidth*3/8, 0),
                new Vector2(Settings.Default.AreaWidth/4, 0),
                new Vector2(-Settings.Default.AreaWidth/4, 0),
                new Vector2(Settings.Default.AreaWidth/4, -Settings.Default.BallDiameter / 2),
            };
            this.CalcRoute();
            this.Refresh();
        }

        List<Vector2> routes = new List<Vector2>();
        private void CalcRoute() {
            var cueDist = Settings.Default.CueDist;
            // 1. Cue1, Dir 볼로 Line생성
            var p1 = balls[0];
            var p2 = p1 + Vector2.Normalize(balls[4]-p1)*cueDist;

            float x1 = -Settings.Default.AreaWidth/2+Settings.Default.BallDiameter/2;
            float x2 = +Settings.Default.AreaWidth/2-Settings.Default.BallDiameter/2;
            float y1 = -Settings.Default.AreaHeight/2+Settings.Default.BallDiameter/2;
            float y2 = +Settings.Default.AreaHeight/2-Settings.Default.BallDiameter/2;
            
            List<CollisionObject> colObjs = new List<CollisionObject>();
            colObjs.Add(new CollisionObjectSegment(new Vector2(x1, y1), new Vector2(x1, y2)));
            colObjs.Add(new CollisionObjectSegment(new Vector2(x2, y1), new Vector2(x2, y2)));
            colObjs.Add(new CollisionObjectSegment(new Vector2(x1, y1), new Vector2(x2, y1)));
            colObjs.Add(new CollisionObjectSegment(new Vector2(x1, y2), new Vector2(x2, y2)));
            colObjs.AddRange(this.balls.Skip(1).Take(3).Select(ball => new CollisionObjectCircle(ball, Settings.Default.BallDiameter)));
            
            this.routes.Clear();
            while (true) {
                colObjs.ForEach(colObj => colObj.CheckCollision(p1, p2));
                var total = colObjs.Where(colObj => colObj.colPt != null).OrderBy(colObj => Vector2.Distance(p1, (Vector2)colObj.colPt));
                if (total.Count() == 0) {
                    this.routes.Add(p2);
                    break;
                } else {
                    var colObj = total.ElementAt(0);
                    var colPt = (Vector2)colObj.colPt;
                    var refDir = (Vector2)colObj.reflectDir;
                    this.routes.Add(colPt);
                    cueDist = cueDist - Vector2.Distance(p1, colPt); 
                    p1 = colPt;
                    p2 = p1 + refDir * cueDist;
                    if (colObj is CollisionObjectCircle) {
                        colObjs.Remove(colObj);
                    }
                }
            }
        }

        private RectangleF WoodRect {
            get {
                return new RectangleF(
                      -Settings.Default.AreaWidth / 2 - Settings.Default.CushinThickness - Settings.Default.WoodThickness
                    , -Settings.Default.AreaHeight / 2 - Settings.Default.CushinThickness - Settings.Default.WoodThickness
                    , Settings.Default.AreaWidth + Settings.Default.CushinThickness*2 + Settings.Default.WoodThickness*2
                    , Settings.Default.AreaHeight + Settings.Default.CushinThickness*2 + Settings.Default.WoodThickness*2
                );
            }
        }

        private RectangleF CushinRect {
            get {
                return new RectangleF(
                      -Settings.Default.AreaWidth / 2 - Settings.Default.CushinThickness
                    , -Settings.Default.AreaHeight / 2 - Settings.Default.CushinThickness
                    , Settings.Default.AreaWidth + Settings.Default.CushinThickness*2
                    , Settings.Default.AreaHeight + Settings.Default.CushinThickness*2
                );
            }
        }

        private RectangleF AreaRect {
            get {
                return new RectangleF(
                      -Settings.Default.AreaWidth / 2
                    , -Settings.Default.AreaHeight / 2
                    , Settings.Default.AreaWidth
                    , Settings.Default.AreaHeight
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
            
            List<Vector2> pointPts = new List<Vector2>();
            float xStep = Settings.Default.AreaWidth / 8;
            for (int i=-4; i<=4; i++) {
                pointPts.Add(new Vector2(xStep*i, -Settings.Default.AreaHeight/2-Settings.Default.CushinThickness-Settings.Default.WoodThickness/2));
                pointPts.Add(new Vector2(xStep*i, Settings.Default.AreaHeight/2+Settings.Default.CushinThickness+Settings.Default.WoodThickness/2));
            }
            float yStep = Settings.Default.AreaHeight / 4;
            for (int i=-2; i<=2; i++) {
                pointPts.Add(new Vector2(-Settings.Default.AreaWidth/2-Settings.Default.CushinThickness-Settings.Default.WoodThickness/2, yStep*i));
                pointPts.Add(new Vector2(Settings.Default.AreaWidth/2+Settings.Default.CushinThickness+Settings.Default.WoodThickness/2, yStep*i));
            }
            foreach (var pointPt in pointPts) {
                var realRect = this.VectorToRect(pointPt, Settings.Default.PointDiameter);
                var drawRect = this.pbxTable.RealToDrawRect(realRect);
                g.FillEllipse(brPoint, drawRect);
            }

            brPoint.Dispose();
        }

        private RectangleF VectorToRect(Vector2 v, float size) {
            return new RectangleF(v.X-size/2, v.Y-size/2, size, size);
        }

        private void DrawBalls(Graphics g) {
            Brush brCue1 = new SolidBrush(Settings.Default.CueBall1Color);
            Brush brCue2 = new SolidBrush(Settings.Default.CueBall2Color);
            Brush brObj1 = new SolidBrush(Settings.Default.ObjBall1Color);
            Brush brObj2 = new SolidBrush(Settings.Default.ObjBall2Color);

            g.FillEllipse(brCue1, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[0], Settings.Default.BallDiameter)));
            g.FillEllipse(brCue2, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[1], Settings.Default.BallDiameter)));
            g.FillEllipse(brObj1, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[2], Settings.Default.BallDiameter)));
            g.FillEllipse(brObj2, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[3], Settings.Default.BallDiameter)));

            brCue1.Dispose();
            brCue2.Dispose();
            brObj1.Dispose();
            brObj2.Dispose();

            g.DrawEllipse(Pens.Black, this.pbxTable.RealToDrawRect(this.VectorToRect(this.balls[4], Settings.Default.BallDiameter)));
        }

        private void DrawRoutes(Graphics g) {
            List<PointF> drawRoutes = new List<PointF>();
            drawRoutes.Add(this.pbxTable.RealToDraw(new PointF(this.balls[0].X, this.balls[0].Y)));
            foreach (var route in this.routes) {
                var rect = VectorToRect(route, Settings.Default.BallDiameter);
                g.DrawEllipse(Pens.Yellow, pbxTable.RealToDrawRect(rect));
                drawRoutes.Add(this.pbxTable.RealToDraw(new PointF(route.X, route.Y)));
            }
            
            g.DrawLines(Pens.Yellow, drawRoutes.ToArray());
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
            this.DrawTable(g);
            this.DrawPoints(g);
            this.DrawBalls(g);
            this.DrawRoutes(g);
        }

        private void btnZoomFit_Click(object sender, EventArgs e) {
            this.ZoomToTable();
        }

        private void grdProp_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        int pickBallIdx = -1;
        Vector2 pickOffset;
        Vector2 pickPt;
        private void pbxTable_MouseDown(object sender, MouseEventArgs e) {
            var ptReal = this.pbxTable.DrawToReal(e.Location);
            var vReal = new Vector2(ptReal.X, ptReal.Y);
            int closestIdx = -1;
            float closestDistSq = float.MaxValue;
            for (int i=0; i<this.balls.Length; i++) {
                float distSq = Vector2.DistanceSquared(this.balls[i], vReal);
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
            var vNew = new Vector2(ptNew.X, ptNew.Y) + this.pickOffset;

            if (this.pickBallIdx != 4) {
                vNew.X = vNew.X.Range(-Settings.Default.AreaWidth/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaWidth/2 - Settings.Default.BallDiameter/2);
                vNew.Y = vNew.Y.Range(-Settings.Default.AreaHeight/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaHeight/2 - Settings.Default.BallDiameter/2);
                // 나머지 3개의 볼에 대해 충돌점을 찾는다.
                // 충돌점들 중 가장 가까운 충돌점으로 이동
                var collisions = this.balls
                    .Where((ball, idx) => idx != pickBallIdx && idx != 4)
                    .Select(ball => Glb.FindCirclePointCollision(ball, Settings.Default.BallDiameter, vNew))
                    .Where(ball => ball != null)
                    .OrderBy(collision => Vector2.Distance((Vector2)collision, vOld));
                if (collisions.Count() > 0) {
                    vNew = (Vector2)collisions.ElementAt(0);
                }
            }
            this.balls[this.pickBallIdx] = vNew;
            this.CalcRoute();
            this.Refresh();
        }

        private void btnInitBalls_Click(object sender, EventArgs e) {
            this.InitBalls();
        }

        private void AddCuePower(float val) {
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

        private void AddCueAngle(float val) {
            var p1 = this.balls[0];
            var p2 = this.balls[4];
            float theta = (float)(Math.PI / 180 * val);
            Matrix3x2 m = Matrix3x2.CreateRotation(theta, p1);
            this.balls[4] = Vector2.Transform(p2, m);

            this.CalcRoute();
            this.pbxTable.Refresh();
        }

        private void btnRotAdd_Click(object sender, EventArgs e) {
            this.AddCueAngle(0.1f);
        }

        private void btnRotSub_Click(object sender, EventArgs e) {
            this.AddCueAngle(-0.1f);
        }
    }
}
