using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace ShimLib {
    public class ZoomPictureBox : PictureBox {
        private Func<int, int, Tuple<string, Brush>> FuncGetPixelValueDisp = null;

        // 이미지
        public Bitmap DrawingImage { get; set; } = null;
        public float Zoom { get; set; } = 1;
        public SizeF Pan { get; set; } = new SizeF(0, 0);
        public float ZoomStep { get; set; } = 1.2f;
        public float ZoomMin { get; set; } = 0.1f;
        public float ZoomMax { get; set; } = 100f;
        public bool EnableWheelZoom { get; set; } = true;
        public bool EnableMousePan { get; set; } = true;
        public bool AutoDrawCursorPixelInfo { get; set; } = true;
        public bool UseDrawPixelValue { get; set; } = true;
        public float DrawPixelValueZoom { get; set; } = 20f;
        public bool AutoDrawCenterLine { get; set; } = true;
        public Color CenterLineColor { get; set; } = Color.Yellow;

        public bool AxisXInvert { get; set; } = false;
        public bool AxisYInvert { get; set; } = false;
        public bool AxisXYFlip { get; set; } = false;
        public Point AxisOrigin = new Point(0, 0);

        // 생성자
        public ZoomPictureBox() {
            this.MouseWheel += new MouseEventHandler(this.ZoomPictureBox_MouseWheel);
            this.MouseDown += new MouseEventHandler(this.ZoomPictureBox_MouseDown);
            this.MouseUp += new MouseEventHandler(this.ZoomPictureBox_MouseUp);
            this.MouseMove += new MouseEventHandler(this.ZoomPictureBox_MouseMove);
            this.Paint += new PaintEventHandler(this.ZoomPictureBox_Paint);
        }

        // 줌인
        public void ZoomIn() {
            this.Zoom = (this.Zoom * this.ZoomStep).Range(this.ZoomMin, this.ZoomMax);
        }

        // 줌아웃
        public void ZoomOut() {
            this.Zoom = (this.Zoom / this.ZoomStep).Range(this.ZoomMin, this.ZoomMax);
        }

        // 줌리셋
        public void ZoomReset() {
            this.Pan = new SizeF(0, 0);
            this.Zoom = 1f;
            this.Invalidate();
        }

        // 윈도우 크기에 이미지 줌 맞춤
        public void ZoomToImage() {
            if (this.DrawingImage == null) {
                this.ZoomReset();
                return;
            }
            
            this.ZoomToRect(0, 0, this.DrawingImage.Width, this.DrawingImage.Height);
        }

        // 윈도우 크기에 영역 줌 맞춤
        public void ZoomToRect(RectangleF rect) {
            this.ZoomToRect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        // 윈도우 크기에 영역 줌 맞춤
        public void ZoomToRect(float x, float y, float width, float height) {
            float scale1 = (float)this.ClientRectangle.Width / width;
            float scale2 = (float)this.ClientRectangle.Height / height;
            this.Zoom = Math.Min(scale1, scale2).Range(this.ZoomMin, this.ZoomMax);
            float panX = (this.ClientRectangle.Width - width * this.Zoom) / 2 - x * this.Zoom;
            float panY = (this.ClientRectangle.Height - height * this.Zoom) / 2 - y * this.Zoom;
            this.Pan = new SizeF(panX, panY);

            this.Invalidate();
        }

        // 좌표 변환 함수 들
        public PointF DrawToReal(PointF ptWnd) {
            float realX = (ptWnd.X - this.Pan.Width) / this.Zoom;
            float realY = (ptWnd.Y - this.Pan.Height) / this.Zoom;
            return new PointF(realX, realY);
        }

        public PointF RealToDraw(PointF ptReal) {
            float wndX = ptReal.X * this.Zoom + this.Pan.Width;
            float wndY = ptReal.Y * this.Zoom + this.Pan.Height;
            return new PointF((int)wndX, (int)wndY);
        }

        public float RealToDrawX(float x) {
            return x * Zoom + Pan.Width;
        }

        public float RealToDrawY(float y) {
            return y * Zoom + Pan.Height;
        }

        public RectangleF RealToDrawRect(RectangleF rect) {
            float x = rect.X * Zoom + Pan.Width;
            float y = rect.Y * Zoom + Pan.Height;
            float width = rect.Width * Zoom;
            float height = rect.Height * Zoom;
            return new RectangleF(x, y, width, height);
        }

        // 픽셀 값 표시 함수 지정
        public void SetFuncGetPixelValueDisp(Func<int, int, Tuple<string, Brush>> FuncGetPixelValueDisp) {
            this.FuncGetPixelValueDisp = FuncGetPixelValueDisp;
        }

        // 의사 컬러
        int pseudo_div = 32;
        Brush[] pseudo = {
            Brushes.White,      // 0~31
            Brushes.LightCyan,  // 32~63           // blue
            Brushes.DodgerBlue, // 63~95
            Brushes.Yellow,     // 96~127
            Brushes.Brown,      // 128~159         // green
            Brushes.Magenta,    // 160~191
            Brushes.Red    ,    // 192~223         // red
            Brushes.Black,      // 224~255
        };

        // 픽셀 값 표시 내장 함수
        private Tuple<string, Brush> GetBuiltinDispPixelValue(int x, int y) {
            if (this.DrawingImage == null)
                return Tuple.Create("0", Brushes.Black);
            if (x < 0 || x >= this.DrawingImage.Width || y < 0 || y >= this.DrawingImage.Height) {
                if (this.DrawingImage.PixelFormat == PixelFormat.Format8bppIndexed)
                    return Tuple.Create("0", Brushes.Black);
                else
                    return Tuple.Create("0,0,0", Brushes.Black);
            }

            Color col = this.DrawingImage.GetPixel(x, y);
            string text =
               (this.DrawingImage.PixelFormat == PixelFormat.Format8bppIndexed)
               ? string.Format("{0}", (col.R + col.G + col.B) / 3)
               : string.Format("{0},{1},{2}", col.R, col.G, col.B);
            var br = pseudo[(col.R + col.G + col.B) / 3 / pseudo_div];
            return Tuple.Create(text, br);
        }

        // 이미지 표시
        private void DrawImage(Graphics g) {
            g.DrawImage(this.DrawingImage, this.Pan.Width, this.Pan.Height, this.DrawingImage.Width * this.Zoom, this.DrawingImage.Height * this.Zoom);

            // 이미지 개별 픽셀 값 표시
            if (this.UseDrawPixelValue && this.Zoom >= this.DrawPixelValueZoom) {
                FontFamily ff = SystemFonts.MessageBoxFont.FontFamily;
                float fs = SystemFonts.MessageBoxFont.Size * this.Zoom / 70  * ((this.DrawingImage.PixelFormat == PixelFormat.Format8bppIndexed) ? 3f : 1);
                Font font = new Font(ff, fs);
                PointF ptMin = this.DrawToReal(new Point(0, 0));
                PointF ptMax = this.DrawToReal(new Point(this.ClientSize.Width, this.ClientSize.Height));
                int x1 = ((int)Math.Floor(ptMin.X)).Range(0, this.DrawingImage.Width - 1);
                int x2 = ((int)Math.Floor(ptMax.X)).Range(0, this.DrawingImage.Width - 1);
                int y1 = ((int)Math.Floor(ptMin.Y)).Range(0, this.DrawingImage.Height - 1);
                int y2 = ((int)Math.Floor(ptMax.Y)).Range(0, this.DrawingImage.Height - 1);
                for (int y = y1; y <= y2; y++) {
                    for (int x = x1; x <= x2; x++) {
                        Tuple<string, Brush> dispPixel;
                        if (this.FuncGetPixelValueDisp != null) {
                            dispPixel = this.FuncGetPixelValueDisp(x, y);
                        } else {
                            dispPixel = GetBuiltinDispPixelValue(x, y);
                        }
                        var pt = this.RealToDraw(new Point(x, y));
                        g.DrawString(dispPixel.Item1, font, dispPixel.Item2, pt);
                    }
                }
                font.Dispose();
            }
        }

        // 센터 라인 표시
        public void DrawCenterLine(Graphics g) {
            if (this.DrawingImage != null) {
                Pen pen = new Pen(this.CenterLineColor);
                pen.DashStyle = DashStyle.Dash;
                Point ptH1 = new Point(0, this.DrawingImage.Height / 2);
                Point ptH2 = new Point(this.DrawingImage.Width, this.DrawingImage.Height / 2);
                Point ptV1 = new Point(this.DrawingImage.Width / 2, 0);
                Point ptV2 = new Point(this.DrawingImage.Width / 2, this.DrawingImage.Height);
                g.DrawLine(pen, RealToDraw(ptH1), RealToDraw(ptH2));
                g.DrawLine(pen, RealToDraw(ptV1), RealToDraw(ptV2));
                pen.Dispose();
            }
        }

        // 마우스 커서 위치의 픽셀 정보 표시
        public void DrawCursorPixelInfo(Graphics g) {
            Point ptMouse = this.PointToClient(System.Windows.Forms.Cursor.Position);
            PointF ptReal = this.DrawToReal(ptMouse);
            Point ptRealInt = new Point((int)Math.Floor(ptReal.X), (int)Math.Floor(ptReal.Y));
            Color col = Color.Black;
            if (this.DrawingImage != null) {
                if (ptRealInt.X >= 0 && ptRealInt.X < this.DrawingImage.Width && ptRealInt.Y >= 0 && ptRealInt.Y < this.DrawingImage.Height) {
                    col = this.DrawingImage.GetPixel(ptRealInt.X, ptRealInt.Y);
                }
            }
            Tuple<string, Brush> dispPixel;
            if (this.FuncGetPixelValueDisp != null) {
                dispPixel = this.FuncGetPixelValueDisp(ptRealInt.X, ptRealInt.Y);
            } else {
                dispPixel = GetBuiltinDispPixelValue(ptRealInt.X, ptRealInt.Y);
            }
            string pixelInfo = $"{this.Zoom,0:0.0}X ({ptRealInt.X},{ptRealInt.Y}) [{dispPixel.Item1}]";
            var drawSize = g.MeasureString(pixelInfo, SystemFonts.DefaultFont);
            g.FillRectangle(Brushes.White, 0, 0, drawSize.Width, drawSize.Height);
            g.DrawString(pixelInfo, SystemFonts.DefaultFont, Brushes.Black, 0, 0);
        }

        // 페인트
        private void ZoomPictureBox_Paint(object sender, PaintEventArgs e) {
            var g = e.Graphics;
            
            g.InterpolationMode = InterpolationMode.NearestNeighbor;    // 이미지 필터링
            g.PixelOffsetMode = PixelOffsetMode.Half;                   // 이미지 픽셀 옵셋
            g.SmoothingMode = SmoothingMode.HighSpeed;                  // 드로잉 안티알리아싱
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;   // 폰트 안티알리아싱

            // 이미지 표시
            if (this.DrawingImage != null) {
                this.DrawImage(g);
            }

            // 센터 라인 표시
            if (this.AutoDrawCenterLine) {
                this.DrawCenterLine(g);
            }

            // 마우스 커서 위치의 픽셀 정보 표시
            if (this.AutoDrawCursorPixelInfo) {
                this.DrawCursorPixelInfo(g);
            }
        }

        // 마우스 줌
        private void ZoomPictureBox_MouseWheel(object sender, MouseEventArgs e) {
            if (this.EnableWheelZoom == false)
                return;
            float factor = ((e.Delta > 0) ? this.ZoomStep : (1 / this.ZoomStep));
            var zoomTemp = (this.Zoom * factor).Range(this.ZoomMin, this.ZoomMax);
            factor = zoomTemp / this.Zoom;
            SizeF vM = new SizeF(e.Location);
            SizeF vI = this.Pan;
            SizeF vI2 = (vI - vM);
            vI2.Width *= factor;
            vI2.Height *= factor;
            vI2 += vM;
            this.Pan = vI2;
            this.Zoom *= factor;
            this.Invalidate();
        }

        // 마우스로 이미지 이동
        private bool mousePan = false;
        private Point ptOld = new Point();
        private void ZoomPictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (this.EnableMousePan == false)
                return;

            if (e.Button != MouseButtons.Left)
                return;
            this.mousePan = true;
            this.ptOld = e.Location;
        }

        private void ZoomPictureBox_MouseUp(object sender, MouseEventArgs e) {
            this.mousePan = false;

            if (this.EnableMousePan == false)
                return;

            if (e.Button != MouseButtons.Left)
                return;
        }

        private void ZoomPictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (this.EnableMousePan && this.mousePan) {
                this.Pan += (Size)e.Location - (Size)this.ptOld;
                this.ptOld = e.Location;
            }
            this.Invalidate();
        }
    }
}
