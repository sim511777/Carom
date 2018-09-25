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
        PointF[] balls;

        public FormMain() {
            InitializeComponent();
            this.grdProp.SelectedObject = Settings.Default;
            this.InitBalls();
            this.ZoomToTable();
        }

        private void InitBalls() {
            this.balls = new PointF[] {
                new PointF(-Settings.Default.AreaWidth/4, Settings.Default.BallDiameter * 2),
                new PointF(Settings.Default.AreaWidth*3/8, 0),
                new PointF(Settings.Default.AreaWidth/4, 0),
                new PointF(-Settings.Default.AreaWidth/4, 0),
            };
            this.CalcRoute();
            this.Refresh();
        }

        private void CalcRoute() {

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
            
            List<PointF> pointPts = new List<PointF>();
            float xStep = Settings.Default.AreaWidth / 8;
            for (int i=-4; i<=4; i++) {
                pointPts.Add(new PointF(xStep*i, -Settings.Default.AreaHeight/2-Settings.Default.CushinThickness-Settings.Default.WoodThickness/2));
                pointPts.Add(new PointF(xStep*i, Settings.Default.AreaHeight/2+Settings.Default.CushinThickness+Settings.Default.WoodThickness/2));
            }
            float yStep = Settings.Default.AreaHeight / 4;
            for (int i=-2; i<=2; i++) {
                pointPts.Add(new PointF(-Settings.Default.AreaWidth/2-Settings.Default.CushinThickness-Settings.Default.WoodThickness/2, yStep*i));
                pointPts.Add(new PointF(Settings.Default.AreaWidth/2+Settings.Default.CushinThickness+Settings.Default.WoodThickness/2, yStep*i));
            }
            foreach (var pointPt in pointPts) {
                var realRect = this.PointToRect(pointPt, Settings.Default.PointDiameter);
                var drawRect = this.pbxTable.RealToDrawRect(realRect);
                g.FillEllipse(brPoint, drawRect);
            }

            brPoint.Dispose();
        }

        private RectangleF PointToRect(PointF pt, float size) {
            return new RectangleF(pt.X-size/2, pt.Y-size/2, size, size);
        }

        private void DrawBalls(Graphics g) {
            Brush brCue1 = new SolidBrush(Settings.Default.CueBall1Color);
            Brush brCue2 = new SolidBrush(Settings.Default.CueBall2Color);
            Brush brObj1 = new SolidBrush(Settings.Default.ObjBall1Color);
            Brush brObj2 = new SolidBrush(Settings.Default.ObjBall2Color);

            g.FillEllipse(brCue1, this.pbxTable.RealToDrawRect(this.PointToRect(this.balls[0], Settings.Default.BallDiameter)));
            g.FillEllipse(brCue2, this.pbxTable.RealToDrawRect(this.PointToRect(this.balls[1], Settings.Default.BallDiameter)));
            g.FillEllipse(brObj1, this.pbxTable.RealToDrawRect(this.PointToRect(this.balls[2], Settings.Default.BallDiameter)));
            g.FillEllipse(brObj2, this.pbxTable.RealToDrawRect(this.PointToRect(this.balls[3], Settings.Default.BallDiameter)));

            brCue1.Dispose();
            brCue2.Dispose();
            brObj1.Dispose();
            brObj2.Dispose();
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

        private float GetDistanceSq(PointF pt1, PointF pt2) {
            float dx = pt1.X - pt2.X;
            float dy = pt1.Y - pt2.Y;
            return dx*dx + dy*dy;
        }

        int pickBallIdx = -1;
        PointF pickOffset;
        PointF pickPt;
        private void pbxTable_MouseDown(object sender, MouseEventArgs e) {
            var ptReal = this.pbxTable.DrawToReal(e.Location);
            int closestIdx = -1;
            float closestDistSq = float.MaxValue;
            for (int i=0; i<this.balls.Length; i++) {
                float distSq = GetDistanceSq(this.balls[i], ptReal);
                if (distSq < closestDistSq) {
                    closestIdx = i;
                    closestDistSq = distSq;
                }
            }

            if (closestDistSq <= Settings.Default.BallDiameter*Settings.Default.BallDiameter/4) {
                this.pickBallIdx = closestIdx;
                this.pickPt = ptReal;
                this.pickOffset = this.balls[closestIdx] - new SizeF(ptReal);
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
            var newPt = this.pbxTable.DrawToReal(e.Location) + new SizeF(this.pickOffset);
            newPt.X = newPt.X.Range(-Settings.Default.AreaWidth/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaWidth/2 - Settings.Default.BallDiameter/2);
            newPt.Y = newPt.Y.Range(-Settings.Default.AreaHeight/2 + Settings.Default.BallDiameter/2, Settings.Default.AreaHeight/2 - Settings.Default.BallDiameter/2);
            
            // 거리가 가장 가까운 볼을 찾는다
            // 거리를 구한다
            // 거리가 볼사이즈 보다 크거나 같으면 패스
            // 작으면 이전 위치에서 새 위치의 방향으로 볼에 닫는 거리만큼만 이동 
            int closestIdx = -1;
            float closestDistSq = float.MaxValue;
            for (int i=0; i<this.balls.Length; i++) {
                if (i == this.pickBallIdx)
                    continue;
                float distSq = GetDistanceSq(this.balls[i], newPt);
                if (distSq < closestDistSq) {
                    closestIdx = i;
                    closestDistSq = distSq;
                }
            }
            if (closestDistSq < Settings.Default.BallDiameter*Settings.Default.BallDiameter) {
                Vector2 vOld = new Vector2(this.balls[this.pickBallIdx].X, this.balls[this.pickBallIdx].Y);
                Vector2 vNew = new Vector2(newPt.X, newPt.Y);
                Vector2 vColide = new Vector2(this.balls[closestIdx].X, this.balls[closestIdx].X);
                //Vector2.Normalize(vNew-vOld)*vColide-vN
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
