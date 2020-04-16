using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace ShimLib {
    public class ZoomPictureBox : Control {
        public static string ReleaseNote =
  @"
v1.0.0.4
- 20190321
ZoomPictureBox 이미지 픽셀 옵셋 교정
  - 이미지 첫 픽셀의 중심이 (0.0f, 0.0f)  에 표시 되도록 0.5 픽셀 라이즈 만큼 이동
  - 픽셀값은 픽셀의 센터에 표시하지 않고 죄상 모서리에 표시
  - 커서 위치 이미지 좌표는 반올림으로 처리

v1.0.0.3
- 20190314
1. PixelValue 폰트 default 폰트 고정사이즈로 변경

v1.0.0.2
- 20190313
1. PictureBox 상속에서 Control 상속으로 바꾸고 DoubleBuffered = true로 변경

v1.0.0.1
- 20190215
1. event처리에서 override로 변경

v1.0.0.0
- 20190213
1. VS2008 호환
2. ReleaseNote 추가
";
        private Func<int, int, StringBrush> FuncGetPixelValueDisp = null;

        // 이미지
        public Bitmap DrawingImage { get; set; }
        public float Zoom { get; set; }
        public SizeF Pan { get; set; }
        public float ZoomStep { get; set; }
        public float ZoomMin { get; set; }
        public float ZoomMax { get; set; }
        public bool EnableWheelZoom { get; set; }
        public bool EnableMousePan { get; set; }
        public bool AutoDrawCursorPixelInfo { get; set; }
        public bool UseDrawPixelValue { get; set; }
        public float DrawPixelValueZoom { get; set; }
        public bool AutoDrawCenterLine { get; set; }
        public Color CenterLineColor { get; set; }
        public DashStyle CenterLineDotStyle { get; set; }

        public bool AxisXInvert { get; set; }
        public bool AxisYInvert { get; set; }
        public bool AxisXYFlip { get; set; }
        public Point AxisOrigin = new Point(0, 0);

        // 생성자
        public ZoomPictureBox() {
            this.DoubleBuffered = true;
            this.DrawingImage = null;
            this.Zoom = 1;
            this.Pan = new SizeF(0, 0);
            this.ZoomStep = 1.2f;
            this.ZoomMin = 0.1f;
            this.ZoomMax = 100f;
            this.EnableWheelZoom = true;
            this.EnableMousePan = true;
            this.AutoDrawCursorPixelInfo = true;
            this.UseDrawPixelValue = true;
            this.DrawPixelValueZoom = 20f;
            this.AutoDrawCenterLine = true;
            this.CenterLineColor = Color.Yellow;
            this.CenterLineDotStyle = DashStyle.Dot;
            this.AxisXInvert = false;
            this.AxisYInvert = false;
            this.AxisXYFlip = false;
        }

        // 줌인
        public void ZoomIn() {
            this.Zoom = ValueClamp(this.Zoom * this.ZoomStep, this.ZoomMin, this.ZoomMax);
        }

        // 줌아웃
        public void ZoomOut() {
            this.Zoom = ValueClamp(this.Zoom / this.ZoomStep, this.ZoomMin, this.ZoomMax);
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
            this.Zoom = ValueClamp(Math.Min(scale1, scale2), this.ZoomMin, this.ZoomMax);
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
            return new PointF(wndX, wndY);
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
        public void SetFuncGetPixelValueDisp(Func<int, int, StringBrush> FuncGetPixelValueDisp) {
            this.FuncGetPixelValueDisp = FuncGetPixelValueDisp;
        }

        // 의사 컬러
        int pseudoDiv = 32;
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
        private StringBrush GetBuiltinDispPixelValue(int x, int y) {
            if (this.DrawingImage == null)
                return StringBrush.Create("0", Brushes.Black);
            if (x < 0 || x >= this.DrawingImage.Width || y < 0 || y >= this.DrawingImage.Height) {
                if (this.DrawingImage.PixelFormat == PixelFormat.Format8bppIndexed)
                    return StringBrush.Create("0", Brushes.Black);
                else
                    return StringBrush.Create("0,0,0", Brushes.Black);
            }

            Color col = this.DrawingImage.GetPixel(x, y);
            string text =
               (this.DrawingImage.PixelFormat == PixelFormat.Format8bppIndexed)
               ? string.Format("{0}", (col.R + col.G + col.B) / 3)
               : string.Format("{0},{1},{2}", col.R, col.G, col.B);
            var br = pseudo[(col.R + col.G + col.B) / 3 / pseudoDiv];
            return StringBrush.Create(text, br);
        }

        // 이미지 표시
        private void DrawImage(Graphics g) {
            // 첫 픽셀의 중심이 (0.0f, 0.0f)  에 표시 되도록 0.5 픽셀 라이즈 만큼 이동
            g.DrawImage(this.DrawingImage, this.Pan.Width - this.Zoom * 0.5f, this.Pan.Height - this.Zoom * 0.5f, this.DrawingImage.Width * this.Zoom, this.DrawingImage.Height * this.Zoom);

            // 이미지 개별 픽셀 값 표시
            if (this.UseDrawPixelValue && this.Zoom >= this.DrawPixelValueZoom) {
                PointF ptMin = this.DrawToReal(new Point(0, 0));
                PointF ptMax = this.DrawToReal(new Point(this.ClientSize.Width, this.ClientSize.Height));
                int x1 = ValueClamp((int)Math.Floor(ptMin.X), 0, this.DrawingImage.Width - 1);
                int x2 = ValueClamp((int)Math.Floor(ptMax.X), 0, this.DrawingImage.Width - 1);
                int y1 = ValueClamp((int)Math.Floor(ptMin.Y), 0, this.DrawingImage.Height - 1);
                int y2 = ValueClamp((int)Math.Floor(ptMax.Y), 0, this.DrawingImage.Height - 1);
                var font = SystemFonts.DefaultFont;
                for (int y = y1; y <= y2; y++) {
                    for (int x = x1; x <= x2; x++) {
                        StringBrush dispPixel;
                        if (this.FuncGetPixelValueDisp != null) {
                            dispPixel = this.FuncGetPixelValueDisp(x, y);
                        } else {
                            dispPixel = GetBuiltinDispPixelValue(x, y);
                        }
                        // 픽셀의 센터에 표시하지 않고 죄상 모서리에 표시
                        var pt = this.RealToDraw(new PointF(x-0.5f, y-0.5f));
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
                pen.DashStyle = this.CenterLineDotStyle;
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
            Point ptRealInt = new Point((int)Math.Round(ptReal.X), (int)Math.Round(ptReal.Y));
            Color col = Color.Black;
            if (this.DrawingImage != null) {
                if (ptRealInt.X >= 0 && ptRealInt.X < this.DrawingImage.Width && ptRealInt.Y >= 0 && ptRealInt.Y < this.DrawingImage.Height) {
                    col = this.DrawingImage.GetPixel(ptRealInt.X, ptRealInt.Y);
                }
            }
            StringBrush dispPixel;
            if (this.FuncGetPixelValueDisp != null) {
                dispPixel = this.FuncGetPixelValueDisp(ptRealInt.X, ptRealInt.Y);
            } else {
                dispPixel = GetBuiltinDispPixelValue(ptRealInt.X, ptRealInt.Y);
            }
            string pixelInfo = string.Format("{0,0:0.0}X ({1},{2}) [{3}]", this.Zoom, ptRealInt.X, ptRealInt.Y, dispPixel.Item1);
            var drawSize = g.MeasureString(pixelInfo, SystemFonts.DefaultFont);
            g.FillRectangle(Brushes.White, 0, 0, drawSize.Width, drawSize.Height);
            g.DrawString(pixelInfo, SystemFonts.DefaultFont, Brushes.Black, 0, 0);
        }

        // Range 함수
        public static T ValueClamp<T>(T value, T min, T max) where T : IComparable {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }

        // 페인트
        protected override void OnPaint(PaintEventArgs pe) {
            var g = pe.Graphics;

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

            // 마우스 커서 위치 픽셀 정보 표시 전에 사용자 Paint이벤트를 처리 한다.
            base.OnPaint(pe);

            // 마우스 커서 위치의 픽셀 정보 표시
            if (this.AutoDrawCursorPixelInfo) {
                this.DrawCursorPixelInfo(g);
            }
        }

        // 마우스 줌
        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);

            if (this.EnableWheelZoom == false)
                return;
            float factor = ((e.Delta > 0) ? this.ZoomStep : (1 / this.ZoomStep));
            var zoomTemp = ValueClamp(this.Zoom * factor, this.ZoomMin, this.ZoomMax);
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
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (this.EnableMousePan == false)
                return;

            if (e.Button != MouseButtons.Left)
                return;
            this.mousePan = true;
            this.ptOld = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            this.mousePan = false;

            if (this.EnableMousePan == false)
                return;

            if (e.Button != MouseButtons.Left)
                return;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (this.EnableMousePan && this.mousePan) {
                this.Pan += (Size)e.Location - (Size)this.ptOld;
                this.ptOld = e.Location;
            }
            this.Invalidate();
        }
    }

    public class StringBrush {
        public string Item1 { get; set; }
        public Brush Item2 { get; set; }
        private StringBrush(string Item1, Brush Item2) {
            this.Item1 = Item1;
            this.Item2 = Item2;
        }
        public static StringBrush Create(string Item1, Brush Item2) {
            return new StringBrush(Item1, Item2);
        }
    }
}
