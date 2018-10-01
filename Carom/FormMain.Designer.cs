namespace Carom {
    partial class FormMain {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent() {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCueDistSub = new System.Windows.Forms.Button();
            this.btnCueDisAdd = new System.Windows.Forms.Button();
            this.btnRotSub = new System.Windows.Forms.Button();
            this.btnRotAdd = new System.Windows.Forms.Button();
            this.btnInitBalls = new System.Windows.Forms.Button();
            this.btnZoomFit = new System.Windows.Forms.Button();
            this.grdProp = new System.Windows.Forms.PropertyGrid();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.pbxTable = new ShimLib.ZoomPictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxTable)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.grdProp);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(621, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(258, 456);
            this.panel1.TabIndex = 0;
            // 
            // btnCueDistSub
            // 
            this.btnCueDistSub.Location = new System.Drawing.Point(108, 64);
            this.btnCueDistSub.Name = "btnCueDistSub";
            this.btnCueDistSub.Size = new System.Drawing.Size(96, 23);
            this.btnCueDistSub.TabIndex = 6;
            this.btnCueDistSub.Text = "Cue Power -";
            this.btnCueDistSub.UseVisualStyleBackColor = true;
            this.btnCueDistSub.Click += new System.EventHandler(this.btnCueDistSub_Click);
            // 
            // btnCueDisAdd
            // 
            this.btnCueDisAdd.Location = new System.Drawing.Point(108, 35);
            this.btnCueDisAdd.Name = "btnCueDisAdd";
            this.btnCueDisAdd.Size = new System.Drawing.Size(96, 23);
            this.btnCueDisAdd.TabIndex = 5;
            this.btnCueDisAdd.Text = "Cue Power +";
            this.btnCueDisAdd.UseVisualStyleBackColor = true;
            this.btnCueDisAdd.Click += new System.EventHandler(this.btnCueDisAdd_Click);
            // 
            // btnRotSub
            // 
            this.btnRotSub.Location = new System.Drawing.Point(6, 64);
            this.btnRotSub.Name = "btnRotSub";
            this.btnRotSub.Size = new System.Drawing.Size(96, 23);
            this.btnRotSub.TabIndex = 4;
            this.btnRotSub.Text = "Cue Rotate -";
            this.btnRotSub.UseVisualStyleBackColor = true;
            this.btnRotSub.Click += new System.EventHandler(this.btnRotSub_Click);
            // 
            // btnRotAdd
            // 
            this.btnRotAdd.Location = new System.Drawing.Point(6, 35);
            this.btnRotAdd.Name = "btnRotAdd";
            this.btnRotAdd.Size = new System.Drawing.Size(96, 23);
            this.btnRotAdd.TabIndex = 3;
            this.btnRotAdd.Text = "Cue Rotate +";
            this.btnRotAdd.UseVisualStyleBackColor = true;
            this.btnRotAdd.Click += new System.EventHandler(this.btnRotAdd_Click);
            // 
            // btnInitBalls
            // 
            this.btnInitBalls.Location = new System.Drawing.Point(108, 6);
            this.btnInitBalls.Name = "btnInitBalls";
            this.btnInitBalls.Size = new System.Drawing.Size(96, 23);
            this.btnInitBalls.TabIndex = 2;
            this.btnInitBalls.Text = "Init Balls";
            this.btnInitBalls.UseVisualStyleBackColor = true;
            this.btnInitBalls.Click += new System.EventHandler(this.btnInitBalls_Click);
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.Location = new System.Drawing.Point(6, 6);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new System.Drawing.Size(96, 23);
            this.btnZoomFit.TabIndex = 1;
            this.btnZoomFit.Text = "Zoom Fit";
            this.btnZoomFit.UseVisualStyleBackColor = true;
            this.btnZoomFit.Click += new System.EventHandler(this.btnZoomFit_Click);
            // 
            // grdProp
            // 
            this.grdProp.Dock = System.Windows.Forms.DockStyle.Top;
            this.grdProp.HelpVisible = false;
            this.grdProp.Location = new System.Drawing.Point(0, 0);
            this.grdProp.Name = "grdProp";
            this.grdProp.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.grdProp.Size = new System.Drawing.Size(258, 294);
            this.grdProp.TabIndex = 0;
            this.grdProp.ToolbarVisible = false;
            this.grdProp.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.grdProp_PropertyValueChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 456);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(879, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // pbxTable
            // 
            this.pbxTable.AutoDrawCenterLine = false;
            this.pbxTable.AutoDrawCursorPixelInfo = false;
            this.pbxTable.AxisXInvert = false;
            this.pbxTable.AxisXYFlip = false;
            this.pbxTable.AxisYInvert = false;
            this.pbxTable.BackColor = System.Drawing.Color.Gray;
            this.pbxTable.CenterLineColor = System.Drawing.Color.Yellow;
            this.pbxTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxTable.DrawingImage = null;
            this.pbxTable.DrawPixelValueZoom = 20F;
            this.pbxTable.EnableMousePan = true;
            this.pbxTable.EnableWheelZoom = true;
            this.pbxTable.Location = new System.Drawing.Point(0, 0);
            this.pbxTable.Name = "pbxTable";
            this.pbxTable.Pan = new System.Drawing.SizeF(0F, 0F);
            this.pbxTable.Size = new System.Drawing.Size(621, 456);
            this.pbxTable.TabIndex = 2;
            this.pbxTable.TabStop = false;
            this.pbxTable.UseDrawPixelValue = true;
            this.pbxTable.Zoom = 1F;
            this.pbxTable.ZoomMax = 100F;
            this.pbxTable.ZoomMin = 0.1F;
            this.pbxTable.ZoomStep = 1.2F;
            this.pbxTable.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxTable_Paint);
            this.pbxTable.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxTable_MouseDown);
            this.pbxTable.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbxTable_MouseMove);
            this.pbxTable.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbxTable_MouseUp);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnZoomFit);
            this.panel2.Controls.Add(this.btnCueDistSub);
            this.panel2.Controls.Add(this.btnInitBalls);
            this.panel2.Controls.Add(this.btnCueDisAdd);
            this.panel2.Controls.Add(this.btnRotAdd);
            this.panel2.Controls.Add(this.btnRotSub);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 294);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(258, 162);
            this.panel2.TabIndex = 7;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 478);
            this.Controls.Add(this.pbxTable);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "FormMain";
            this.Text = "Carom";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxTable)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private ShimLib.ZoomPictureBox pbxTable;
        private System.Windows.Forms.PropertyGrid grdProp;
        private System.Windows.Forms.Button btnZoomFit;
        private System.Windows.Forms.Button btnInitBalls;
        private System.Windows.Forms.Button btnCueDistSub;
        private System.Windows.Forms.Button btnCueDisAdd;
        private System.Windows.Forms.Button btnRotSub;
        private System.Windows.Forms.Button btnRotAdd;
        private System.Windows.Forms.Panel panel2;
    }
}

