﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication;
using HslCommunication.Instrument.DLT;
using System.Threading;
using System.IO.Ports;
using System.Xml.Linq;
using HslCommunicationDemo.Instrument;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormDLT645OverTcp : HslFormContent
	{
		public FormDLT645OverTcp( )
		{
			InitializeComponent( );
		}

		private DLT645OverTcp dLT645 = null;
		private DLT645Control control;
		private AddressExampleControl addressExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;
			Language( Program.Language );
			control = new DLT645Control( );
			this.userControlReadWriteDevice1.AddSpecialFunctionTab( control );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.Instrument.DLTHelper.GetDlt645Address( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "DLT645 Read Demo";

				label1.Text = "Com:";
				label3.Text = "baudRate:";
				label_address.Text = "station";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				textBox_password.Text = "Pwd:";
				textBox_op_code.Text = "Op Code:";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		

		#region Connect And Close



		private void button1_Click( object sender, EventArgs e )
		{
			if(!int.TryParse(textBox_port.Text,out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			dLT645?.ConnectClose( );
			dLT645 = new DLT645OverTcp( textBox_ip.Text, port, textBox_station.Text, textBox_password.Text, textBox_op_code.Text );
			dLT645.LogNet = LogNet;
			dLT645.EnableCodeFE = checkBox_enable_Fe.Checked;

			try
			{
				OperateResult connect = dLT645.ConnectServer( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					//userControlReadWriteOp1.SetReadWriteNet( dLT645, "00-00-00-00", true );

					userControlReadWriteDevice1.SetReadWriteNet( dLT645, "00-00-00-00", false );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( dLT645, "00-00-00-00", string.Empty );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => dLT645.ReadFromCoreServer( m, true, false ), string.Empty, "68 00 00 00 00 00 01 68 11 04 00 00 00 00 10 16" );

					control.SetDevice( dLT645, "00-00-00-00" );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message + Environment.NewLine +
						"Error: " + connect.ErrorCode );
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			dLT645.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}
		
		#endregion

		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlBaudRate, textBox_port.Text );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox_station.Text );


			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox_port.Text = element.Attribute( DemoDeviceList.XmlBaudRate ).Value;
			textBox_station.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;


			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}
