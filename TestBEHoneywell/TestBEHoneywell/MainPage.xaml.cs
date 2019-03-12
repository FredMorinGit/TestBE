using MvvmHelpers;
//using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace TestBEHoneywell
{
    public partial class MainPage
    {
        public MainViewModel MainViewModel => this.BindingContext as MainViewModel;

        public MainPage()
        {
            InitializeComponent();

            //MyListView.ItemsSource = this.History;

            this.BindingContext = new MainViewModel();
        }

        private readonly byte[] b05 = { 0x05 };

        private readonly byte[] b04 = { 0x04 };
        private readonly byte[] b0405 = { 0x04, 0x05 };
        private readonly byte[] bNAK = { 0x15 };

        private readonly byte[] b1030 = { 0x10, 0x30 };

        private readonly byte[] b1031 = { 0x10, 0x31 };

        private readonly byte[] bPart1 = {
            0x10, 0x02, 0x44, 0x58, 0x53, 0x2a, 0x30, 0x35, 0x39, 0x35, 0x34, 0x38, 0x39, 0x38, 0x39, 0x38,
            0x2a, 0x44, 0x58, 0x2a, 0x30, 0x30, 0x34, 0x30, 0x31, 0x30, 0x55, 0x43, 0x53, 0x2a, 0x31, 0x2a,
            0x30, 0x35, 0x39, 0x35, 0x34, 0x38, 0x39, 0x38, 0x39, 0x38, 0x0D, 0x0A, 0x53, 0x54, 0x2A, 0x38,
            0x39, 0x34, 0x2A, 0x30, 0x30, 0x30, 0x31, 0x0D, 0x0A, 0x47, 0x38, 0x32, 0x2A, 0x44, 0x2A, 0x39,
            0x39, 0x39, 0x30, 0x30, 0x30, 0x34, 0x31, 0x2A, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x32, 0x31,
            0x33, 0x2A, 0x30, 0x30, 0x31, 0x32, 0x33, 0x34, 0x2A, 0x32, 0x35, 0x33, 0x35, 0x37, 0x31, 0x39,
            0x34, 0x37, 0x2A, 0x34, 0x37, 0x38, 0x2A, 0x32, 0x30, 0x31, 0x39, 0x30, 0x31, 0x32, 0x33, 0x0D,
            0x0A, 0x4C, 0x53, 0x2A, 0x30, 0x31, 0x30, 0x30, 0x0D, 0x0A, 0x47, 0x38, 0x33, 0x2A, 0x31, 0x2A,
            0x32, 0x34, 0x30, 0x30, 0x2A, 0x55, 0x4E, 0x2A, 0x30, 0x30, 0x37, 0x32, 0x36, 0x32, 0x38, 0x34,
            0x30, 0x30, 0x30, 0x31, 0x2A, 0x2A, 0x2A, 0x2A, 0x31, 0x2E, 0x30, 0x36, 0x30, 0x34, 0x2A, 0x2A,
            0x4E, 0x41, 0x4E, 0x20, 0x31, 0x36, 0x20, 0x32, 0x34, 0x50, 0x4B, 0x20, 0x43, 0x52, 0x41, 0x4E,
            0x42, 0x52, 0x59, 0x0D, 0x0A, 0x47, 0x37, 0x32, 0x2A, 0x35, 0x32, 0x35, 0x2A, 0x30, 0x32, 0x2A,
            0x2A, 0x2A, 0x30, 0x2E, 0x30, 0x35, 0x0D, 0x0A, 0x47, 0x38, 0x33, 0x2A, 0x32, 0x2A, 0x32, 0x34,
            0x30, 0x2A, 0x55, 0x4E, 0x2A, 0x30, 0x30, 0x37, 0x32, 0x36, 0x32, 0x38, 0x34, 0x30, 0x30, 0x30,
            0x31, 0x2A, 0x2A, 0x2A, 0x2A, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x2A, 0x2A, 0x4E, 0x41, 0x4E,
            0x20, 0x31, 0x36, 0x20, 0x32, 0x34, 0x50, 0x10, 0x17, 0xD3, 0xFB};

        private readonly byte[] bPart2 = {
            0x10, 0x02, 0x4B, 0x20, 0x43, 0x52, 0x41, 0x4E, 0x42, 0x52, 0x59, 0x0D, 0x0A, 0x47, 0x37, 0x32,
            0x2A, 0x35, 0x32, 0x35, 0x2A, 0x30, 0x32, 0x2A, 0x2A, 0x2A, 0x30, 0x2E, 0x30, 0x35, 0x0D, 0x0A,
            0x4C, 0x45, 0x2A, 0x30, 0x31, 0x30, 0x30, 0x0D, 0x0A, 0x47, 0x38, 0x34, 0x2A, 0x32, 0x36, 0x34,
            0x30, 0x2A, 0x36, 0x31, 0x32, 0x31, 0x32, 0x30, 0x30, 0x2A, 0x31, 0x33, 0x32, 0x30, 0x30, 0x0D,
            0x0A, 0x47, 0x38, 0x36, 0x2A, 0x39, 0x39, 0x39, 0x30, 0x30, 0x30, 0x0D, 0x0A, 0x47, 0x38, 0x35,
            0x2A, 0x32, 0x41, 0x31, 0x37, 0x0D, 0x0A, 0x53, 0x45, 0x2A, 0x31, 0x32, 0x2A, 0x30, 0x30, 0x30,
            0x31, 0x0D, 0x0A, 0x44, 0x58, 0x45, 0x2A, 0x31, 0x2A, 0x31, 0x0D, 0x0A, 0x10, 0x03, 0xAF, 0xCE
        };

        private readonly byte[] adj1 = {
            0x10,0x02,0x44,0x58,0x53,0x2A,0x30,0x35,0x39,0x35,0x34,0x38,0x39,0x38,0x39,0x38
            ,0x2A,0x44,0x58,0x2A,0x30,0x30,0x34,0x30,0x31,0x30,0x55,0x43,0x53,0x2A,0x31,0x0D
            ,0x0A,0x53,0x54,0x2A,0x38,0x39,0x35,0x2A,0x30,0x30,0x30,0x31,0x0D,0x0A,0x47,0x38
            ,0x37,0x2A,0x53,0x2A,0x44,0x2A,0x39,0x39,0x39,0x30,0x30,0x30,0x34,0x31,0x2A,0x36
            ,0x35,0x37,0x43,0x2A,0x32,0x0D,0x0A,0x4C,0x53,0x2A,0x30,0x31,0x30,0x30,0x0D,0x0A
            ,0x47,0x38,0x39,0x2A,0x31,0x2A,0x34,0x38,0x30,0x30,0x0D,0x0A,0x4C,0x45,0x2A,0x30
            ,0x31,0x30,0x30,0x0D,0x0A,0x47,0x38,0x34,0x2A,0x35,0x2A,0x30,0x0D,0x0A,0x47,0x38
            ,0x36,0x2A,0x39,0x39,0x39,0x30,0x30,0x30,0x0D,0x0A,0x47,0x38,0x35,0x2A,0x45,0x10
            ,0x17,0x33,0xB3 };

        private readonly byte[] adj2 = {
            0x10,0x02,0x33,0x42,0x32,0x0D,0x0A,0x53,0x45,0x2A,0x39,0x2A,0x30,0x30,0x30,0x31,
            0x0D,0x0A,0x44,0x58,0x45,0x2A,0x31,0x2A,0x31,0x0D,0x0A,0x10,0x03,0x80,0x62,0x04
        };

        private readonly byte[] b1076 = {
            0x10, 0x01, 0x30, 0x35, 0x39, 0x35, 0x34, 0x38, 0x39, 0x38, 0x39, 0x38, 0x53, 0x52, 0x30, 0x31,
            0x4c, 0x30, 0x31, 0x52, 0x30, 0x31, 0x4c, 0x30, 0x31, 0x10, 0x03, 0x79, 0x76};

        private readonly byte[] b1099 =
        {
            0x10, 0x01, 0x30, 0x30, 0x30, 0x35, 0x39, 0x35, 0x34, 0x38, 0x39, 0x38, 0x39, 0x38, 0x52, 0x30, 0x31, 0x4C,
            0x30, 0x31, 0x10, 0x03, 0x85, 0x1A
        };

        private readonly byte[] b1005 = {
            0x10, 0x01, 0x30, 0x30, 0x30, 0x35, 0x39, 0x35, 0x34, 0x38, 0x39, 0x38, 0x39, 0x38, 0x52, 0x30,
            0x31, 0x4C, 0x30, 0x31, 0x10, 0x03, 0x85, 0x1A, 0x05};

        private readonly byte[] b10B7 = {
            0x10, 0x01, 0x30, 0x35, 0x39, 0x35, 0x34, 0x38, 0x39, 0x38, 0x39, 0x38, 0x52, 0x52, 0x30, 0x31,
            0x4c, 0x30, 0x31, 0x52, 0x30, 0x31, 0x4c, 0x30, 0x31, 0x10, 0x03, 0x79, 0xB7};

        //public string LabelText { get; set; }

        public IDevice Device { get; set; }

        //public IGattService DexService { get; set; }
        //public IGattService DexSession { get; set; }

        //public IGattCharacteristic DataCharacteristic { get; set; }
        //public IGattCharacteristic StatusCharacteristic { get; set; }

        //private readonly Guid DexServiceGuid = Guid.Parse("F000C0E0-0451-4000-B000-000000000000");
        //private readonly Guid DexDataCharacterticGuid = Guid.Parse("F000C0E1-0451-4000-B000-000000000000");
        //private readonly Guid DexStatusCharacterticGuid = Guid.Parse("F000C0E2-0451-4000-B000-000000000000");
        //private readonly Guid DexSessionGuid = Guid.Parse("F000FFD2-0451-4000-B000-000000000000");

        public List<IGattService> Services { get; set; }
        public List<IGattCharacteristic> Characteristics { get; set; }

        public ObservableCollection<string> History => MainViewModel.History;

        private async void RxBase(object sender, EventArgs e)
        {

            tokenSource = new CancellationTokenSource();

            await Task.Run(async () => {
                await Task.Delay(1);
                if (!App.DexService.Connected)
                {
                    UpdateLog("Not Connected");
                    return;
                }

                App.DexService.StartCommunication("Receiving Rx", false);
                UpdateLog("Receiving Rx", true);
                App.DexService.Operation = " Receiving Rx ";

                var result = false;
                int tries = 0;
                step = 1;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b05, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                    tries += 1;
                }

                tries = 0;
                await Task.Delay(500);
                step++;
                result = false;

                while (!result && tries < 10)
                {
                    await App.DexService.Send(b10B7, b1031, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                tries = 0;
                step++;
                result = false;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b04, b05, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1030, b1005, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                step++;
                tries = 0;
                result = false;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1031, b0405, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);

                tries = 0;
                step++;
                result = false;
                while (!result && tries < 10)
                {
                    App.DexService.StartStacking();
                    await App.DexService.Send(b1030, null, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    await Task.Delay(1000);
                    result = true;
                }

                App.DexService.EndStacking();
                await Task.Delay(2000);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1031, b04, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }
                App.DexService.EndCommunication(false);

                await Task.Delay(500);
            }, tokenSource.Token);

            
            UpdateLog(App.DexService.ShowStack().AddBreaks(), true);


        }

        private async void TxAdj(object sender, EventArgs e)
        {

            tokenSource = new CancellationTokenSource();


            await Task.Run(async () =>
            {
                await Task.Delay(1);
                UpdateLog("Sending Adj", true);
                App.DexService.StartCommunication(" Sending Adj ", false);
                var result = false;
                int tries = 0;
                step = 1;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b05, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1076, b1031, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b04, b05, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1030, b1005, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1031, b04, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b05, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(adj1, b1031, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(2000);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(adj2, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(1000);
                result = false;
                step++;
                tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b04, null, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                App.DexService.EndCommunication(true);
                await Task.Delay(10);
            }, tokenSource.Token);

        }

        private void CancelThis(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

         public CancellationTokenSource tokenSource { get; set; }


         void ResetDex(object sender, EventArgs e)
         {
             App.DexService.ResetDex();
         }

         void RebootDex(object sender, EventArgs e)
         {
             App.DexService.RebootDex();
         }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            MainViewModel.History.Clear();

            await Task.Delay(1);

            App.DexService.PrepareForScan(UpdateLog);
        }

        public int stepNumber { get; set; }

        public void UpdateLog(string message, bool clear = false)
        {
            if (clear)
            {
                MainViewModel.History.Clear();
            }
            MainViewModel.History.Insert(0, message);
            
        }

        private int step = 0;

        private bool onedone = false;

        private async void TxBase(object sender, EventArgs e)
        {

            tokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                if (!App.DexService.Connected)
                {
                    UpdateLog("Not Connected");
                    return;
                }

                App.DexService.StartCommunication(" Sending Tx ", onedone);
                UpdateLog("Sending Tx", true);
                onedone = true;
                step = 1;
                var result = false;
                int tries = 0;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b05, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1076, b1031, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b04, b05, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1030, b1099, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b1031, b04, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(b05, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(bPart1, b1031, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(500);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                    await App.DexService.Send(bPart2, b1030, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++}");
                    result = await App.DexService.WaitFor(tokenSource.Token);
                }

                await Task.Delay(2000);
                result = false;
                tries = 0;
                step++;
                while (!result && tries < 10)
                {
                   await App.DexService.Send(b04, null, tokenSource.Token);
                    UpdateLog($"Step {step} Try {tries++} WAIT FOR NULL");
                    result = true;
                    break;
                    //result = await App.DexService.WaitFor();
                }

                await Task.Delay(2000);
                App.DexService.EndCommunication(false);
            }, tokenSource.Token);

           

        }
    }

    public class MainViewModel : BaseViewModel
    {
        private ObservableCollection<string> _history;

        public void Refresh(string s = "", bool clearOne = false)
        {
            if (string.IsNullOrEmpty(s)) return;
            if (clearOne && History.Any()) History.RemoveAt(0);
            History.Insert(0, s);
        }

        public MainViewModel()
        {
            History = new ObservableCollection<string>();
            History.Insert(0, "Started");
        }

        public ObservableCollection<string> History
        {
            get => _history;
            set
            {
                _history = value;
                OnPropertyChanged(nameof(History));
            }
        }
    }

    public static class CharacteristicHelper
    {
        public static async Task<CharacteristicGattResult> WriteBuffered(this IGattCharacteristic characteristic,
            byte[] bytes, int maxLength = 20)
        {
            CharacteristicGattResult result;

            if (!characteristic.CanWriteWithoutResponse()) return null;

            var len = bytes.Length;
            int pass = 0;

            while (len > maxLength)
            {
                var temp = bytes.Skip(pass++ * maxLength).Take(maxLength).ToArray();
                result = await characteristic.WriteWithoutResponse(temp);
                await Task.Delay(10);
                len -= maxLength;
            }

            return await characteristic.WriteWithoutResponse(bytes.Skip(pass * maxLength).Take(len).ToArray());
        }
    }

    public static class ByteHelper
    {
        public static string ToListOfByte(this byte[] bytes, bool excludeZeros = false)
        {
            var result = string.Empty;

            if (bytes == null) return result;

            if (bytes.Length == 0) return result;

            foreach (var b in bytes)
            {
                if (!excludeZeros || b != 0x00)
                {
                    result += $"0x{b:X2} ";
                }
            }

            result = result.Substring(0, result.Length - 1);

            return result;
        }

        public static byte[] ToByteArray(this string strAscii)
        {
            return System.Text.Encoding.ASCII.GetBytes(strAscii);
        }

        public static string MakeWithColumns(this string s, int columns = 8, bool excludedEndingZeros = true)
        {
            var sTemp = string.Empty;

            var iCol = 0;
            var iRow = 0;

            sTemp += Environment.NewLine + "         -------------------------------------------------";

            if (excludedEndingZeros)
            {
                while (s.Length > 4)
                {
                    if (s.Substring(s.Length - 5, 5) == " 0x00")
                    {
                        s = s.Substring(0, s.Length - 5);
                    }
                    else { break; }
                }
            }

            var lines = s.Split(' ');

            foreach (var l in lines)
            {
                if (iCol % columns == 0)
                {
                    sTemp += Environment.NewLine + $"{iRow:X2} =>";
                    iRow++;
                }
                iCol++;
                sTemp += $"{l} ";
            }

            sTemp += Environment.NewLine + "         -------------------------------------------------";

            return string.IsNullOrEmpty(sTemp) ? "Empty" : sTemp;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[(first?.Length ?? 0) + (second?.Length ?? 0)];
            if (first != null) Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            if (second != null) Buffer.BlockCopy(second, 0, ret, first?.Length ?? 0, second.Length);
            return ret;
        }

        public static int Search(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                if (Match(haystack, needle, i))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool Match(byte[] haystack, byte[] needle, int start)
        {
            if (needle.Length + start > haystack.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < needle.Length; i++)
                {
                    if (needle[i] != haystack[i + start])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static string AddBreaks(this string line, int iBreak = 80)
        {
            if (line.Length <= iBreak)
            {
                return line;
            }

            var sTemp = Environment.NewLine;

            var run = line.Length / iBreak;

            for (var i = 0; i <= run; i++)
            {
                string p;

                if (line.Length >= iBreak)
                {
                    p = line.Substring(0, iBreak);
                    line = line.Substring(iBreak, line.Length - iBreak);
                }
                else
                {
                    p = line;
                }

                sTemp += p + Environment.NewLine;
            }

            return sTemp;
        }
    }
}