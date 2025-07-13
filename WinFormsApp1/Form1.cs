using HidLibrary;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System;
using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _cts;
        private Task _hidReadTask;
        private bool _isReading = false;
        private HidDevice _device;

        public struct ButtonInfo
        {
            public int ByteIndex;
            public byte Mask;
            public string Name;

            public ButtonInfo(int byteIndex, byte mask, string name)
            {
                ByteIndex = byteIndex;
                Mask = mask;
                Name = name;
            }
        }

        ButtonInfo[] buttons = new ButtonInfo[]
        {
            new ButtonInfo(3, 0x01, "B"),
            new ButtonInfo(3, 0x02, "A"),
            new ButtonInfo(3, 0x04, "Y"),
            new ButtonInfo(3, 0x08, "X"),
            new ButtonInfo(3, 0x10, "R"),
            new ButtonInfo(3, 0x20, "Z"),
            new ButtonInfo(3, 0x40, "Start/Pause"),
            new ButtonInfo(4, 0x01, "Dpad Down"),
            new ButtonInfo(4, 0x02, "Dpad Right"),
            new ButtonInfo(4, 0x04, "Dpad Left"),
            new ButtonInfo(4, 0x08, "Dpad Up"),
            new ButtonInfo(4, 0x10, "L"),
            new ButtonInfo(4, 0x20, "ZL"),
            new ButtonInfo(5, 0x01, "Home"),
            new ButtonInfo(5, 0x02, "Capture"),
            new ButtonInfo(5, 0x04, "GR"),
            new ButtonInfo(5, 0x08, "GL"),
            new ButtonInfo(5, 0x10, "Chat"),
        };

        public Form1()
        {
            InitializeComponent();
        }
        private void InitHIDDevice()
        {
            int vendorId = 0x057e;
            int productId = 0x2073;
            label1.Text = "Connecting via HID now.";
            _device = HidDevices.Enumerate(vendorId, productId).FirstOrDefault();
            if (_device == null)
            {
                label1.Text = "Couldn't connect via HID.";
                return;
            }
            label1.Text = "Connected via HID.";
        }
        public int InitializeViaUSB()
        {
            // GC controller data
            const int VID = 0x057e;
            const int PID = 0x2073;

            UsbDevice usbDevice = null;
            UsbEndpointWriter writer = null;
            try
            {
                UsbDeviceFinder usbFinder = new UsbDeviceFinder(VID, PID);
                usbDevice = UsbDevice.OpenUsbDevice(usbFinder);
                if (usbDevice == null)
                {
                    label1.Text = "Device not found.";
                    return -1;
                }

                label1.Text = "Device found.";
                progressBar1.Value = 1;

                IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
                if (wholeUsbDevice != null)
                {
                    wholeUsbDevice.SetConfiguration(1);
                    wholeUsbDevice.ClaimInterface(1);
                    //usbDevice.Close();
                    //usbDevice = null;
                }
                foreach (UsbConfigInfo config in usbDevice.Configs)
                {
                    foreach (UsbInterfaceInfo iface in config.InterfaceInfoList)
                    {
                        foreach (UsbEndpointInfo ep in iface.EndpointInfoList)
                        {
                            byte epId = ep.Descriptor.EndpointID;
                            string direction = (epId & 0x80) != 0 ? "IN" : "OUT";
                            System.Diagnostics.Debug.WriteLine($"Endpoint: 0x{epId:X2}, Direction: {direction}");
                        }
                    }
                }
                writer = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);

                byte[] DEFAULT_REPORT_DATA = new byte[] { 0x03, 0x91, 0x00, 0x0d, 0x00, 0x08,
                                                0x00, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

                byte[] SET_LED_DATA = new byte[]{ 0x09, 0x91, 0x00, 0x07, 0x00, 0x08,
                                                  0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                int bytesWritten;
                label1.Text = "Sending default report data.";
                ErrorCode ec_default = writer.Write(DEFAULT_REPORT_DATA, 2000, out bytesWritten);
                label1.Text = "Sent default report data.";
                progressBar1.Value = 2;

                label1.Text = "Sending LED data.";
                ErrorCode ec_led = writer.Write(SET_LED_DATA, 2000, out bytesWritten);
                label1.Text = "Sent LED data.";
                progressBar1.Value = 3;
                writer.Dispose();
                return 0;
            }
            catch (Exception ex)
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                label1.Text = "Error: " + ex.Message;
                return -1;
            }
            finally
            {
                if (usbDevice != null)
                {
                    if (usbDevice.IsOpen)
                    {
                        if (writer != null)
                        {
                            writer.Dispose();
                        }
                        IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
                        if (wholeUsbDevice != null)
                            wholeUsbDevice.ReleaseInterface(0);

                        usbDevice.Close();
                    }
                    usbDevice = null;
                    label1.Text = "Preparation done.";
                }
                UsbDevice.Exit();
            }
            label1.Text = "Connected.";
            progressBar1.Value = 4;
        }

        private void emptyLabels()
        {
            Invoke((Delegate)(() => B.Text = ""));
            Invoke((Delegate)(() => A.Text = ""));
            Invoke((Delegate)(() => Y.Text = ""));
            Invoke((Delegate)(() => X.Text = ""));
            Invoke((Delegate)(() => R.Text = ""));
            Invoke((Delegate)(() => Z.Text = ""));
            Invoke((Delegate)(() => Start.Text = ""));
            Invoke((Delegate)(() => Ddown.Text = ""));
            Invoke((Delegate)(() => Dright.Text = ""));
            Invoke((Delegate)(() => Dleft.Text = ""));
            Invoke((Delegate)(() => Dup.Text = ""));
            Invoke((Delegate)(() => L.Text = ""));
            Invoke((Delegate)(() => ZL.Text = ""));
            Invoke((Delegate)(() => Home.Text = ""));
            Invoke((Delegate)(() => Capture.Text = ""));
            //Invoke((Delegate)(() => GR.Text = ""));
            //Invoke((Delegate)(() => GL.Text = ""));
            Invoke((Delegate)(() => Chat.Text = ""));
        }
        private void ReadHidLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_device.IsConnected)
                    {
                        Invoke(() => label1.Text = "Controller disconnected");
                        _device.Dispose();
                        break;
                    }

                    HidDeviceData data = _device.Read();

                    if (data.Status == HidDeviceData.ReadStatus.Success)
                    {
                        emptyLabels();
                        byte[] hexData = data.Data;

                        int leftStickX = hexData[6] | ((hexData[7] & 0x0F) << 8);
                        int leftStickY = ((hexData[7] >> 4) | (hexData[8] << 4));
                        int rightStickX = hexData[9] | ((hexData[10] & 0x0F) << 8);
                        int rightStickY = ((hexData[10] >> 4) | (hexData[11] << 4));

                        // Norm around -1 to +1
                        float normX_left = (leftStickX - 2048) / 2048f; 
                        float normY_left = (leftStickY - 2048) / 2048f;

                        float normX_right = (rightStickX - 2048) / 2048f;
                        float normY_right = (rightStickY - 2048) / 2048f;

                        Invoke((Delegate)(() => stick_left.Location = new Point(216 + (int)(normX_left*20), (97 - (int)(normY_left * 20)))));
                        Invoke((Delegate)(() => stick_right.Location = new Point(330 + (int)(normX_right * 15), (150 - (int)(normY_right * 15)))));


                        if (hexData.Length > 14)
                        {
                            byte left_trigger = (byte)hexData[13];
                            byte right_trigger = (byte)hexData[14];

                            // Trick to make it update instant and not slowly flow in
                            Invoke((Delegate)(() => progressBarLeft.Value = (byte)hexData[13] + 1));
                            Invoke((Delegate)(() => progressBarLeft.Value = (byte)hexData[13]));
                            Invoke((Delegate)(() => progressBarRight.Value = (byte)hexData[14] + 1));
                            Invoke((Delegate)(() => progressBarRight.Value = (byte)hexData[14]));

                            for (int i = 0; i < 18; i++)
                            {
                                // Sticks



                                // Buttons
                                if ((hexData[buttons[i].ByteIndex] & buttons[i].Mask) != 0)
                                {
                                    switch (buttons[i].Name)
                                    {
                                        case "B":
                                            Invoke((Delegate)(() => B.Text = "X"));
                                            break;
                                        case "A":
                                            Invoke((Delegate)(() => A.Text = "X"));
                                            break;
                                        case "Y":
                                            Invoke((Delegate)(() => Y.Text = "X"));
                                            break;
                                        case "X":
                                            Invoke((Delegate)(() => X.Text = "X"));
                                            break;
                                        case "R":
                                            Invoke((Delegate)(() => R.Text = "X"));
                                            break;
                                        case "Z":
                                            Invoke((Delegate)(() => Z.Text = "X"));
                                            break;
                                        case "Start / Pause":
                                            Invoke((Delegate)(() => Start.Text = "X"));
                                            break;
                                        case "Dpad Down":
                                            Invoke((Delegate)(() => Ddown.Text = "X"));
                                            break;
                                        case "Dpad Right":
                                            Invoke((Delegate)(() => Dright.Text = "X"));
                                            break;
                                        case "Dpad Left":
                                            Invoke((Delegate)(() => Dleft.Text = "X"));
                                            break;
                                        case "Dpad Up":
                                            Invoke((Delegate)(() => Dup.Text = "X"));
                                            break;
                                        case "L":
                                            Invoke((Delegate)(() => L.Text = "X"));
                                            break;
                                        case "ZL":
                                            Invoke((Delegate)(() => ZL.Text = "X"));
                                            break;
                                        case "Home":
                                            Invoke((Delegate)(() => Home.Text = "X"));
                                            break;
                                        case "Capture":
                                            Invoke((Delegate)(() => Capture.Text = "X"));
                                            break;
                                        //case "GR":
                                        //    Invoke((Delegate)(() => GR.Text = "X"));
                                        //    break;
                                        //case "GL":
                                        //    Invoke((Delegate)(() => GL.Text = "X"));
                                        //    break;
                                        case "Chat":
                                            Invoke((Delegate)(() => Chat.Text = "X"));
                                            break;
                                        default:
                                            break;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke(() => label1.Text = "Error in ReadLoop: " + ex.Message);
            }
            finally
            {
                _isReading = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int res = InitializeViaUSB();
            if (res != -1)
            {
                button1.Enabled = false;
                if (_isReading) return;
                InitHIDDevice();
                if (_device == null) return;
                progressBar1.Value = 5;

                _cts = new CancellationTokenSource();
                _isReading = true;
                _hidReadTask = Task.Run(() => ReadHidLoop(_cts.Token));
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 5;

            progressBarLeft.Minimum = 0;
            progressBarLeft.Maximum = 255;

            progressBarRight.Minimum = 0;
            progressBarRight.Maximum = 255;

            // Set progress
            progressBar1.Value = 0;
            progressBarLeft.Value = 0;
            progressBarRight.Value = 0;


        }
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();

                try
                {
                    if (_hidReadTask != null)
                        await _hidReadTask;
                }
                catch (OperationCanceledException)
                {
                    // Task ended normally
                }
                catch (Exception ex)
                {
                    label1.Text = "Fehler beim Beenden des HID-Tasks: " + ex.Message;
                }

                _cts.Dispose();
                _cts = null;
                _isReading = false;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
