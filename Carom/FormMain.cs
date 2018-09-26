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

        private void CalcRoute() {
            // 1. Cue1, Dir 볼로 Line생성
            // do {
            // 2. cushion 1,2,3,4 충돌 체크, ball 1,2,3 충돌 체크
            // 3. 충돌이 있다 가장 가까운 충돌점, 반사벡터로 Line생성
            // 4. ball 이 충돌이라면 충돌된 ball 은 충돌체크 아이템에서 제거
            // } while (has collision)
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
        }

        private void btnZoomFit_Click(object sender, EventArgs e) {
            this.ZoomToTable();
        }

        private void grdProp_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            this.pbxTable.Refresh();
        }

        int pickBallIdx = -1;
        Vector2 pickOffset;
        Vector2 pickPt;
        private void pbxTable_MouseDown(object sender, MouseEventArgs e) {
            var ptReal = Glb.PointFToVector2(this.pbxTable.DrawToReal(e.Location));
            int closestIdx = -1;
            float closestDistSq = float.MaxValue;
            for (int i=0; i<this.balls.Length; i++) {
                float distSq = Vector2.DistanceSquared(this.balls[i], ptReal);
                if (distSq < closestDistSq) {
                    closestIdx = i;
                    closestDistSq = distSq;
                }
            }

            if (closestDistSq <= Settings.Default.BallDiameter*Settings.Default.BallDiameter/4) {
                this.pickBallIdx = closestIdx;
                this.pickPt = ptReal;
                this.pickOffset = this.balls[closestIdx] - ptReal;
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
            var oldPt = this.balls[this.pickBallIdx];
            var newPt = Glb.PointFToVector2(this.pbxTable.DrawToReal(e.Location)) + this.pickOffset;

            if (this.pickBallIdx != 4) {
                newPt.X = newPt.X.Range(-Settings.Default.AreaWidth/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaWidth/2 - Settings.Default.BallDiameter/2);
                newPt.Y = newPt.Y.Range(-Settings.Default.AreaHeight/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaHeight/2 - Settings.Default.BallDiameter/2);
                // 나머지 3개의 볼에 대해 충돌점을 찾는다.
                // 충돌점들 중 가장 가까운 충돌점으로 이동
                var collisions = this.balls
                    .Where((ball, idx) => idx != pickBallIdx && idx != 4)
                    .Select(ball => Glb.FindPointCircleIntersections(ball, Settings.Default.BallDiameter, newPt))
                    .Where(ball => ball != null)
                    .OrderBy(collision => Vector2.Distance((Vector2)collision, oldPt));
                if (collisions.Count() > 0) {
                    newPt = (Vector2)collisions.ElementAt(0);
                }
            }
            this.balls[this.pickBallIdx] = newPt;
            this.CalcRoute();
            this.Refresh();
        }

        private void btnInitBalls_Click(object sender, EventArgs e) {
            this.InitBalls();
        }
    }
}
