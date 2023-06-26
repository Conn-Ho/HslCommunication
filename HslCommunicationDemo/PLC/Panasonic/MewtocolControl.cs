﻿using HslCommunication;
using HslCommunication.Profinet.Panasonic;
using HslCommunicationDemo.PLC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HslCommunicationDemo.PLC.Panasonic
{
	public class MewtocolControl : UserControl
	{
		public MewtocolControl( )
		{
			InitializeComponent( );
		}

		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button5;

		private void InitializeComponent( )
		{
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button5 = new System.Windows.Forms.Button();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.textBox1);
			this.groupBox2.Controls.Add(this.button5);
			this.groupBox2.Location = new System.Drawing.Point(3, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(807, 256);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Mewtocol Function";
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(6, 59);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(795, 191);
			this.textBox1.TabIndex = 29;
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(6, 25);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(108, 28);
			this.button5.TabIndex = 28;
			this.button5.Text = "Plc-Type";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// MewtocolControl
			// 
			this.Controls.Add(this.groupBox2);
			this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.Name = "MewtocolControl";
			this.Size = new System.Drawing.Size(813, 259);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		private void button5_Click( object sender, EventArgs e )
		{
			OperateResult<string> read = mewtocol == null ? mewtocolOverTcp.ReadPlcType( ) : mewtocol.ReadPlcType( );
			if (read.IsSuccess)
			{
				textBox1.Text = DateTime.Now.ToString( ) + Environment.NewLine + read.Content;
			}
			else
			{
				MessageBox.Show( "Read failed: " + read.Message );
			}
		}

		public void SetDevice( PanasonicMewtocol mewtocol, string address )
		{
			this.mewtocol = mewtocol;
		}


		public void SetDevice( PanasonicMewtocolOverTcp mewtocolOverTcp, string address )
		{
			this.mewtocolOverTcp = mewtocolOverTcp;
		}

		private PanasonicMewtocol mewtocol;
		private PanasonicMewtocolOverTcp mewtocolOverTcp;
	}
}
