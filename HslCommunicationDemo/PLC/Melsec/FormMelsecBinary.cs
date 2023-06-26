﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication.Profinet.Melsec;
using HslCommunication;
using System.Xml.Linq;
using HslCommunicationDemo.Control;
using HslCommunication.MQTT;
using HslCommunicationDemo.DemoControl;
using HslCommunicationDemo.PLC.Melsec;

namespace HslCommunicationDemo
{
	public partial class FormMelsecBinary : HslFormContent
	{
		public FormMelsecBinary()
		{
			InitializeComponent( );
			melsec_net = new MelsecMcNet( );
		}


		private MelsecMcNet melsec_net = null;
		private MqttClient mqttClient;
		private string readTopic;
		private string writeTopic;


		private AddressExampleControl addressExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;
			Language( Program.Language );

			checkBox1.CheckedChanged += CheckBox1_CheckedChanged;

			control = new McQna3EControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( control );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( Helper.GetMcAddress( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) ) ;
		}

		private void CheckBox1_CheckedChanged( object sender, EventArgs e )
		{
			if (checkBox1.Checked)
			{
				// 如果选中则，弹出MQTT信息输入
				using(FormMqttInput input = new FormMqttInput( ))
				{
					if (input.ShowDialog() == DialogResult.OK)
					{
						mqttClient = new MqttClient( input.MqttConnectionOptions );
						readTopic  = input.ReadTopic;
						writeTopic = input.WriteTopic;
						textBox_ip.Enabled = false;
						textBox_port.Enabled = false;
					}
					else
					{
						checkBox1.Checked = false;
					}
				}
			}
			else
			{
				textBox_ip.Enabled = true;
				textBox_port.Enabled = true;
			}
		}

		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Melsec Read PLC Demo";

				label1.Text = "Ip:";
				label3.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
			}
			else
			{
				checkBox_EnableWriteBitToWordRegister.Text = "支持使用位写入字寄存器(实际读字，修改位，写字)";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}

		#region Connect And Close

		private async void button1_Click( object sender, EventArgs e )
		{
			if (checkBox1.Checked)
			{
				// 使用MQTT中转
				melsec_net.LogNet = LogNet;
				OperateResult connect = this.mqttClient.ConnectServer( );
				if (!connect.IsSuccess)
				{
					MessageBox.Show( connect.Message + Environment.NewLine + "ErrorCode: " + connect.ErrorCode );
					button1.Enabled = true;
					return;
				}

				melsec_net.EnableWriteBitToWordRegister = checkBox_EnableWriteBitToWordRegister.Checked;
				connect = melsec_net.ConnectServer( this.mqttClient, readTopic, writeTopic );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					// 设置子控件的读取能力
					userControlReadWriteDevice1.SetReadWriteNet( melsec_net, "D100", true );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( melsec_net, "D100", string.Empty );
					userControlReadWriteDevice1.BatchRead.SetReadRandom( melsec_net.ReadRandom, "D100;W100;D500" );
					userControlReadWriteDevice1.BatchRead.SetReadWordRandom( melsec_net.ReadRandom, "D100;W100;D500" );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => melsec_net.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );

					control.SetDevice( melsec_net, "D100" );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message );
					button1.Enabled = true;
				}
			}
			else
			{
				melsec_net.IpAddress = textBox_ip.Text;
				if (!int.TryParse( textBox_port.Text, out int port ))
				{
					MessageBox.Show( DemoUtils.PortInputWrong );
					return;
				}

				melsec_net.Port = port;

				melsec_net.ConnectClose( );
				melsec_net.LogNet = LogNet;
				melsec_net.ConnectTimeOut = 3000; // 连接3秒超时

				melsec_net.EnableWriteBitToWordRegister = checkBox_EnableWriteBitToWordRegister.Checked;
				button1.Enabled = false;
				OperateResult connect = await melsec_net.ConnectServerAsync( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					// 设置子控件的读取能力
					userControlReadWriteDevice1.SetReadWriteNet( melsec_net, "D100", true );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( melsec_net, "D100", string.Empty );
					userControlReadWriteDevice1.BatchRead.SetReadRandom( melsec_net.ReadRandom, "D100;W100;D500" );
					userControlReadWriteDevice1.BatchRead.SetReadWordRandom( melsec_net.ReadRandom, "D100;W100;D500" );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => melsec_net.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );

					control.SetDevice( melsec_net, "D100" );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message );
					button1.Enabled = true;
				}
			}
		}

		private McQna3EControl control;

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			melsec_net.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}

		#endregion

		#region 测试使用

		private void test1()
		{
			OperateResult<bool[]> read = melsec_net.ReadBool( "M100", 10 );
			if(read.IsSuccess)
			{
				bool m100 = read.Content[0];
				// and so on
				bool m109 = read.Content[9];
			}
			else
			{
				// failed
			}
		}

		private void test2( )
		{
			bool[] values = new bool[] { true, false, true, true, false, true, false, true, true, false };
			OperateResult read = melsec_net.Write( "M100", values );
			if (read.IsSuccess)
			{
				// success
			}
			else
			{
				// failed
			}
		}


		private void test3( )
		{
			short d100_short = melsec_net.ReadInt16( "D100" ).Content;
			ushort d100_ushort = melsec_net.ReadUInt16( "D100" ).Content;
			int d100_int = melsec_net.ReadInt32( "D100" ).Content;
			uint d100_uint = melsec_net.ReadUInt32( "D100" ).Content;
			long d100_long = melsec_net.ReadInt64( "D100" ).Content;
			ulong d100_ulong = melsec_net.ReadUInt64( "D100" ).Content;
			float d100_float = melsec_net.ReadFloat( "D100" ).Content;
			double d100_double = melsec_net.ReadDouble( "D100" ).Content;
			// need to specify the text length
			string d100_string = melsec_net.ReadString( "D100", 10 ).Content;
		}
		private void test4( )
		{
			melsec_net.Write( "D100", (short)5 );
			melsec_net.Write( "D100", (ushort)5 );
			melsec_net.Write( "D100", 5 );
			melsec_net.Write( "D100", (uint)5 );
			melsec_net.Write( "D100", (long)5 );
			melsec_net.Write( "D100", (ulong)5 );
			melsec_net.Write( "D100", 5f );
			melsec_net.Write( "D100", 5d );
			// length should Multiples of 2 
			melsec_net.Write( "D100", "12345678" );
		}


		private void test5( )
		{
			OperateResult<byte[]> read = melsec_net.Read( "D100", 10 );
			if(read.IsSuccess)
			{
				int count = melsec_net.ByteTransform.TransInt32( read.Content, 0 );
				float temp = melsec_net.ByteTransform.TransSingle( read.Content, 4 );
				short name1 = melsec_net.ByteTransform.TransInt16( read.Content, 8 );
				string barcode = Encoding.ASCII.GetString( read.Content, 10, 10 );
			}
		}

		private void test6( )
		{
			OperateResult<UserType> read = melsec_net.ReadCustomer<UserType>( "D100" );
			if (read.IsSuccess)
			{
				UserType value = read.Content;
			}
			// write value
			melsec_net.WriteCustomer( "D100", new UserType( ) );

			melsec_net.LogNet = new HslCommunication.LogNet.LogNetSingle( Application.StartupPath + "\\Logs.txt" );

		}

		// private MelsecMcAsciiNet melsec_ascii_net = null;

		#endregion


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox_ip.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox_port.Text );
			element.SetAttributeValue( "EnableWriteBitToWordRegister", checkBox_EnableWriteBitToWordRegister.Text );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox_ip.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox_port.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			checkBox_EnableWriteBitToWordRegister.Checked = GetXmlValue( element, "EnableWriteBitToWordRegister", false, bool.Parse );

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

	}

	public class UserType : HslCommunication.IDataTransfer
	{
		#region IDataTransfer

		private HslCommunication.Core.IByteTransform ByteTransform = new HslCommunication.Core.RegularByteTransform();


		public ushort ReadCount => 10;

		public void ParseSource( byte[] Content )
		{
			int count = ByteTransform.TransInt32( Content, 0 );
			float temp = ByteTransform.TransSingle( Content, 4 );
			short name1 = ByteTransform.TransInt16( Content, 8 );
			string barcode = Encoding.ASCII.GetString( Content, 10, 10 );
		}

		public byte[] ToSource( )
		{
			byte[] buffer = new byte[20];
			ByteTransform.TransByte( count ).CopyTo( buffer, 0 );
			ByteTransform.TransByte( temp ).CopyTo( buffer, 4 );
			ByteTransform.TransByte( name1 ).CopyTo( buffer, 8 );
			Encoding.ASCII.GetBytes( barcode ).CopyTo( buffer, 10 );
			return buffer;
		}


		#endregion


		#region Public Data

		public int count { get; set; }

		public float temp { get; set; }

		public short name1 { get; set; }

		public string barcode { get; set; }

		#endregion
	}
}
