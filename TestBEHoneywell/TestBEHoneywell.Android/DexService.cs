using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Java.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Accounts;
using Xamarin.Forms;
using Environment = System.Environment;
using Exception = System.Exception;
using OperationCanceledException = Android.OS.OperationCanceledException;
using Thread = System.Threading.Thread;

[assembly: Dependency(typeof(TestBEHoneywell.Droid.DexService))]

namespace TestBEHoneywell.Droid
{
    public class DexService : IDexService
    {
        internal static UUID DEX_SERVICE_SPP = UUID.FromString("F000C0E0-0451-4000-B000-000000000000");
        internal static UUID DEX_CHARACTERISTIC_DATAWRITE = UUID.FromString("F000C0E1-0451-4000-B000-000000000000");
        internal static UUID DEX_CHARACTERISTIC_STATUS = UUID.FromString("F000C0E2-0451-4000-B000-000000000000");
        internal static UUID CLIENT_CHARACTERISTIC_CONFIG = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        internal static  UUID DEX_SERVICE = UUID.FromString("F000FFD0-0451-4000-B000-000000000000");
        internal static  UUID DEVICE_SETTINGS = UUID.FromString("F000FFD3-0451-4000-B000-000000000000");

        internal static readonly string TAG = "DEX: ";
        internal static readonly long SCAN_TIMEOUT_MS = 10000;
        internal static readonly int REQUEST_ENABLE_BT = 1;
        internal static bool Scanning;
        internal static BluetoothLeScanner Scanner;
        internal BluetoothManager BluetoothManager;
        internal BluetoothAdapter BluetoothAdapter;
        internal static BluetoothDevice BluetoothDevice;
        internal static BluetoothScanCallback ScanCallback;
        internal static BluetoothGatt ConnectedGatt;
        internal static LEGattCallback GattCallback;
        internal static bool GattConnected;
        internal static Context Context;
        internal static bool deviceFound;
        internal static AssetManager asm;

        internal static StringBuffer DataReadOperationSync = new StringBuffer();
        internal static System.Text.StringBuilder mReceiveBuffer = new System.Text.StringBuilder();
        internal static byte[] DataBuffer = new byte[1024];
        internal static int LastReturnedPosition = 0;
        internal static int NextInsertPosition = 0;
        internal NfcAdapter NfcAdapter;
        public int REQUEST_LOCATION { get; private set; }

        //additions

     

        public bool Stacking { get; set; }

        public void ResetDex()
        {
            lock (ConnectedGatt)
            {
                var charac = ConnectedGatt.GetService(DEX_SERVICE).GetCharacteristic(DEVICE_SETTINGS);
                charac.SetValue(new byte[] { 0x06, 0xd3, 0xd3, 0x00, 0x00, 0x00 });
                ConnectedGatt.WriteCharacteristic(charac);
            }

        }

        public void StartStacking()
        {
            Stacking = true;
            StackList = new List<byte[]>();
        }

        public string ShowStack()
        {
            var result = string.Empty;

            if (StackList == null) return result;

            foreach (var bytese in StackList)
            {
                result += bytese.ToListOfByte() + Environment.NewLine;
            }

            Log.Info(TAG + Operation, result);

            return result;
        }

        public void EndStacking()
        {
            Stacking = false;
        }

        public List<byte[]> StackList { get; set; }

        //end

        public void StartCommunication(string operation, bool prepareForScan)
        {
            //Init(prepareForScan);
            Operation = operation;
            Fini = false;
        }

        public enum NextStep
        {
            TxBase = 1,
            RxBase = 2,
            TxAdj = 3
        }

        //public bool ReadyForNext { get; set; }

        public void EndCommunication(bool endAll = false)
        {
            Fini = true;
            //ReadyForNext = true;
            Operation = "Waiting for next";
            //if (endAll)
            //{
            //    BluetoothManager.Dispose();
            //    BluetoothManager = null;
            //    BluetoothAdapter.Dispose();
            //    BluetoothAdapter = null;
            //    ScanCallback.Dispose();
            //    ScanCallback = null;
            //    MainActivity.Receiver.Dispose();
            //    MainActivity.Receiver = null;
            //    GattCallback.Dispose();
            //    GattCallback = null;

            //}
        }

        public static bool Fini { get; set; }
        public MainActivity MainActivity { get; set; }

        public DexService()
        {
        }

        private void InitializeStatus()
        {
            Log.Info(TAG, "Tap to Dex Adapter to pair by NFC");
        }

        private void sendFile()
        {
            if (ConnectedGatt != null)
            {
                SendDexFile sendDex = new SendDexFile(MainActivity, this);
                Thread sendFileThread = new Thread(sendDex.senddexfile);
                sendFileThread.Start();
            }
        }

        public DexService(MainActivity mainActivity)
        {
            MainActivity = mainActivity;
            //BluetoothManager = (BluetoothManager)MainActivity.GetSystemService(Context.BluetoothService);
            //BluetoothAdapter = BluetoothManager.Adapter;
            //ScanCallback = new BluetoothScanCallback(mainActivity, this);
            //MainActivity.Receiver = new BLEBroadcastReceiver(mainActivity, this);
            //GattCallback = new LEGattCallback(mainActivity, this);

            //NfcManager nfcManager = (NfcManager)mainActivity.GetSystemService(Context.NfcService);
            //NfcAdapter = NfcAdapter.GetDefaultAdapter(mainActivity);

            //Context = mainActivity;

            //StatusList = new ListView(mainActivity);

            //InitializeStatus();
        }

        internal static void StopLeScan()
        {
            if (Scanning)
            {
                Scanning = false;
                Scanner.StopScan(ScanCallback);
            }
        }

        private void startLeScan2()
        {
            Scanning = true;
            Log.Info(TAG, "Scan Started");

            ScanSettings settings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower).Build();

            List<ScanFilter> filters = new List<ScanFilter>
            {
                new ScanFilter.Builder().SetDeviceName("DEXADAPTER").Build()
            };

            Scanner.StartScan(filters, settings, ScanCallback);

            Log.Info(TAG, "Start Scanning");
        }

        public bool Connected { get; set; }

        internal Action<string, bool> LogMessage2;

        public void PrepareForScan(Action<string, bool> message)
        {
            LogMessage2 = message;

            Connected = false;

            if (MainActivity.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
            {
                Scanner = BluetoothAdapter.BluetoothLeScanner;
                if (BluetoothAdapter.IsEnabled)
                {
                    if (ContextCompat.CheckSelfPermission(MainActivity, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                    {
                        StartLeScan();
                    }
                    else
                    {
                        ActivityCompat.RequestPermissions(MainActivity, new[] { Manifest.Permission.AccessFineLocation }, REQUEST_LOCATION);
                    }
                }
                else
                {
                    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                    MainActivity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
                }
            }
            else
            {
                Toast.MakeText(MainActivity, "BLE is not supported", ToastLength.Long).Show();
                MainActivity.Finish();
            }
        }

        public static byte[] WaitForBytes { get; set; }
        public static bool Waiting { get; internal set; }
        public static byte[] ReceiveBytes { get; set; }

        private readonly byte[] bNAK = { 0x15 };

        public async Task<bool> WaitFor(CancellationToken token)
        {
            try
            {
                Log.Info(TAG, "1");
                if (WaitForBytes == null)
                {
                    Log.Info(TAG, "2");

                    await Task.Delay(1000);
                    Log.Info(TAG, "3");
                    ReceiveBytes = null;
                    Log.Info(TAG, "4");

                    return true;
                }

                Log.Info(TAG, "5");
                if (ReceiveBytes == null)
                {
                    Log.Info(TAG, "6");

                    Log.Error(TAG + Operation, "ReceiveBytes is NULL.. we fix it");
                    Log.Info(TAG, "7");
                }

                Log.Info(TAG, "8");

                //   int counter = 20;

                while (!Fini && ReceiveBytes != null && ByteHelper.Search(ReceiveBytes, WaitForBytes) == -1
                ) // !ReceiveBytes.(WaitForBytes))
                {

                    if (!App.DexService.Connected) return false;

                    if (token.IsCancellationRequested) return false;

                    Log.Info(TAG + Operation,
                        $"Waiting for {WaitForBytes.ToListOfByte()} but We got {ReceiveBytes.ToListOfByte()}");

                    Log.Info(TAG, "9");
                    await Task.Delay(200);
                    //counter++;

                    Log.Info(TAG, "10");

                    if (WaitForBytes == null) return true;

                    if (ReceiveBytes == null) continue;

                    if (ReceiveBytes != null && (ReceiveBytes.Length < 1 || !ReceiveBytes.Contains(bNAK[0]))) continue;
                    Log.Info(TAG, "11");

                    ReceiveBytes = null;
                    Log.Info(TAG, "12");

                    WaitForBytes = null;
                    Log.Info(TAG, "13");

                    return false;
                }

                Log.Info(TAG, "14");

                ReceiveBytes = null;
                Log.Info(TAG, "15");

                WaitForBytes = null;
                Log.Info(TAG, "16");

                return true;
            }
            catch (OperationCanceledException canceledException)
            {
                Log.Warn(TAG, $"OPeration Cancelled => {canceledException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public int CopyPosition { get; set; }

        public string Operation { get; set; }

        public async Task<bool> Send(byte[] sentBytes, byte[] waitBytes, CancellationToken token)
        {
            try
            {
                var sendDexFile = new SendDexFile(MainActivity, this);
                ReceiveBytes = new byte[0];
                WaitForBytes = new byte[0];
                //if(WaitForBytes?.Length > 0) Arrays.Fill(WaitForBytes, 0);
                WaitForBytes = waitBytes;

                Log.Info(TAG + Operation, $"Waiting for {waitBytes.ToListOfByte()}");

                LogMessage2($"Sending => {sentBytes.ToListOfByte()}", false);

                sendDexFile.SendDexBytes(sentBytes, token);

                await Task.Delay(200, token);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void StartLeScan()
        {
            Scanning = true;

            Log.Info(TAG, "ScanStarted");
            //
            ScanSettings settings = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower) // LowPower uses the least amount of power. It is enforced is the application is not in the foreground
                .Build();
            List<ScanFilter> filters = new List<ScanFilter>();
            filters.Add(new ScanFilter.Builder().SetDeviceName("DEXAdapter").Build()); // Filter allows the application to look only for the DEX adapter.

            Scanner.StartScan(filters, settings, ScanCallback);

            System.Diagnostics.Debug.Write("START SCANNING");
            //cTimer.Start();
        }

        public void RebootDex()
        {
            lock (ConnectedGatt)
            {
                var charac = ConnectedGatt.GetService(DEX_SERVICE).GetCharacteristic(DEVICE_SETTINGS);
                charac.SetValue(new byte[] {0x06, 0xd2, 0xd2, 0x00, 0x00, 0x00 });
                ConnectedGatt.WriteCharacteristic(charac);
            }
        }

        private static NdefMessage getTestMessage()
        {
            byte[] mimeBytes = Encoding.ASCII.GetBytes("application/com.android.cts.verifier.nfc");
            byte[] id = new byte[] { 1, 3, 3, 7 };
            byte[] payload = Encoding.ASCII.GetBytes("Cts Verifier NDEF Push Tag");
            return new NdefMessage(new[]
            {
                new NdefRecord(NdefRecord.TnfMimeMedia, mimeBytes, id, payload)
            });
        }

        public NdefMessage CreateMessage(NfcEvent e)
        {
            return getTestMessage();
        }

        internal NdefMessage[] GetNdefMessages(Intent intent)
        {
            try
            {
                IParcelable[] rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
                if (rawMessages != null)
                {
                    NdefMessage[] messages = new NdefMessage[rawMessages.Length];
                    for (int i = 0; i < messages.Length; i++)
                    {
                        messages[i] = (NdefMessage)rawMessages[i];
                    }
                    return messages;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        ~DexService()
        {
            MainActivity.UnregisterReceiver(MainActivity.Receiver);
        }

        internal static string displayByteArray(byte[] bytes)
        {
            string res = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                res += (char)bytes[i];
            }
            return res;
        }

        public void Init(bool prepareForScan = false)
        {
            if (MainActivity == null)
            {
                throw new Exception("MainActivity not set");
            }

            if (prepareForScan)
            {
                PrepareForScan(LogMessage2);
            }

            BluetoothManager = (BluetoothManager)MainActivity.GetSystemService(Context.BluetoothService);
            BluetoothAdapter = BluetoothManager.Adapter;
            ScanCallback = new BluetoothScanCallback(MainActivity, this);
            MainActivity.Receiver = new BLEBroadcastReceiver(MainActivity, this);
            GattCallback = new LEGattCallback(MainActivity, this);

            var nfcManager = (NfcManager)MainActivity.GetSystemService(Context.NfcService);
            Log.Info(TAG + Operation, $"nfcManager => {nfcManager.ToString()}");
            NfcAdapter = NfcAdapter.GetDefaultAdapter(MainActivity);

            Context = MainActivity;

            InitializeStatus();

            if (BluetoothAdapter != null)
            {
                IntentFilter mfFilter = new IntentFilter(BluetoothDevice.ActionAclConnected);
                mfFilter.AddAction(BluetoothDevice.ActionPairingRequest);
                mfFilter.AddAction(BluetoothDevice.ActionBondStateChanged);
                mfFilter.AddAction(BluetoothDevice.ActionFound);
                mfFilter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
                mfFilter.Priority = (int)IntentFilterPriority.HighPriority;
                MainActivity.RegisterReceiver(MainActivity.Receiver, mfFilter);
            }

            MainActivity.PendingIntent = PendingIntent.GetActivity(MainActivity, 0,
                new Intent(MainActivity, MainActivity.GetType()).AddFlags(ActivityFlags.SingleTop), 0);

            asm = MainActivity.Resources.Assets;
        }

        public class LEGattCallback : BluetoothGattCallback, ILEGattCallback
        {
            public MainActivity MainActivity { get; set; }
            public DexService DexService { get; set; }

            public BluetoothGattCharacteristic WriteCharacteristic { get; set; }
            public BluetoothGattCharacteristic StatusCharacteristic { get; set; }
            public BluetoothGattCharacteristic DeviceCharacteristic { get; set; }



            public NextStep NextStep { get; set; }

            public LEGattCallback(MainActivity mainActivity, DexService dexService)
            {
                MainActivity = mainActivity;
                NextStep = NextStep.TxBase;
                DexService = dexService;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                base.OnConnectionStateChange(gatt, status, newState);

                try
                {
                    if (status == GattStatus.Success && newState == ProfileState.Connected)
                    {
                        GattConnected = true;
                        MainActivity.RunOnUiThread(() => { Log.Info(TAG + DexService.Operation, "Connected"); });
                        DexService.LogMessage2("Gatt Connected", false);
                    }

                    if (newState == ProfileState.Disconnected)
                    {
                        GattConnected = false;
                        DexService.LogMessage2("Gatt Disconnected", false);
                        return;
                    }

                    gatt.DiscoverServices();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                base.OnServicesDiscovered(gatt, status);

                try
                {

                    foreach (var bluetoothGattService in gatt.Services)
                    {
                        Log.Info(TAG, $"Service Discovered => {bluetoothGattService.Uuid}");
                        foreach (var bluetoothGattCharacteristic in bluetoothGattService.Characteristics)
                        {
                            Log.Info(TAG, $"Characteristic Discovered => {bluetoothGattCharacteristic.Uuid}");
                        }
                    }



                    WriteCharacteristic = gatt.GetService(DEX_SERVICE_SPP)
                        .GetCharacteristic(DEX_CHARACTERISTIC_DATAWRITE);


                    Log.Info(TAG, $"WriteCharacteristic Properties => {WriteCharacteristic.Properties.ToString()}");


                    DeviceCharacteristic = gatt.GetService(DEX_SERVICE).GetCharacteristic(DEVICE_SETTINGS);


                    Log.Info(TAG, $"DeviceCharacteristic Properties => {DeviceCharacteristic.Properties.ToString()}");


                    StatusCharacteristic = gatt.GetService(DEX_SERVICE_SPP)
                        .GetCharacteristic(DEX_CHARACTERISTIC_STATUS);


                    Log.Info(TAG, $"StatusCharacteristic Properties => {StatusCharacteristic.Properties.ToString()}");


                    gatt.SetCharacteristicNotification(DeviceCharacteristic, true);

                    var descDevice = DeviceCharacteristic.GetDescriptor(CLIENT_CHARACTERISTIC_CONFIG);

                    Log.Info(TAG, $"descDevice Permissions => {descDevice.Permissions.ToString()}");


                    descDevice.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    gatt.WriteDescriptor(descDevice);
                    descDevice.SetValue(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
                    gatt.WriteDescriptor(descDevice);


                    if (gatt.SetCharacteristicNotification(WriteCharacteristic, true))
                    {
                        MainActivity.RunOnUiThread(() =>
                        {
                            Log.Info(TAG + DexService.Operation, "Set Write Characteristic succeeded");
                        });
                        DexService.LogMessage2("Set Write Characteristic succeeded", false);
                    }
                    else
                    {
                        MainActivity.RunOnUiThread(() => { Log.Info(TAG + DexService.Operation, "Set Write Characteristic failed"); });
                    }

                    BluetoothGattDescriptor descWrite = WriteCharacteristic.GetDescriptor(CLIENT_CHARACTERISTIC_CONFIG);

                    Log.Info(TAG, $"descWrite Permissions => {descWrite.Permissions.ToString()}");



                    descWrite.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    gatt.WriteDescriptor(descWrite);
                    descWrite.SetValue(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
                    gatt.WriteDescriptor(descWrite);
                    DexService.Connected = true;
                    DexService.LogMessage2("Device Connected", false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
                [GeneratedEnum] GattStatus status)
            {
                try
                {
                    base.OnCharacteristicRead(gatt, characteristic, status);
                    Log.Info(TAG + DexService.Operation,
                        $"Characteristic Read => {characteristic.GetValue().ToListOfByte()}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Log.Error(TAG, $"Error in OnCharacteristicRead => {e.Message}");
                }
            }

            public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
                [GeneratedEnum] GattStatus status)
            {
                try
                {
                    base.OnCharacteristicWrite(gatt, characteristic, status);
                    Log.Info(TAG + DexService.Operation,
                        $"Characteristic {characteristic.Uuid} Wrote => {characteristic.GetValue().ToListOfByte()}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Log.Error(TAG, $"Error in OnCharacteristicWrite => {e.Message}");
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                Log.Info(TAG + DexService.Operation,
                    $"Characteristic {characteristic.Uuid} Changed => {characteristic.GetValue()?.ToListOfByte()}");

                base.OnCharacteristicChanged(gatt, characteristic);

                try
                {
                    if (!characteristic.Uuid.Equals(DEX_CHARACTERISTIC_DATAWRITE)) return;

                    if (DEX_CHARACTERISTIC_DATAWRITE.Equals(characteristic.Uuid))
                    {
                        byte[] receive = characteristic.GetValue();
                        lock (DataReadOperationSync)
                        {
                            var characteristicValue = new string(Encoding.Default.GetChars(receive));
                           
                            mReceiveBuffer.Append(characteristicValue);
                           
                            Log.Info(TAG + DexService.Operation,
                                $"Data Received => {receive.ToListOfByte()}");

                            DexService.LogMessage2($"Data Received => {receive.ToListOfByte()}", false);

                            if (DexService.Stacking)
                            {
                                DexService.StackList.Add(receive);
                            }

                            ReceiveBytes = ByteHelper.Combine(ReceiveBytes, receive);

                            Log.Info(TAG + DexService.Operation,
                                $"ReceivedBytes is now  => {ReceiveBytes.ToListOfByte()}");
                        }
                    }
                }
                catch (Java.IO.IOException e)
                {
                    e.PrintStackTrace();
                }
                catch (InterruptedException e)
                {
                    e.PrintStackTrace();
                }
                catch (Exception exception)
                {
                    Log.Error(TAG, $"Error dans OnCharacteristicChanged {exception.Message}");
                }

                //Log.Info(DexService.TAG + DexService.Operation, $"Characteristic UUID IS => {characteristic.Uuid}");
                //Log.Info(DexService.TAG + DexService.Operation, $"DEX UUID IS            => {DexService.DEX_CHARACTERISTIC_DATAWRITE}");
            }
        }

        public class SendDexFile : ISendDexFile
        {
            public MainActivity MainActivity { get; set; }
            public DexService DexService { get; set; }

            public SendDexFile(MainActivity mainActivity, DexService dexService)
            {
                DexService = dexService;
                MainActivity = mainActivity;
            }

            //public bool Waiting { get; set; }

            public void SendDexBytes(byte[] bytes, CancellationToken token)
            {
                BluetoothGattCharacteristic charac = ConnectedGatt.GetService(DEX_SERVICE_SPP)
                    .GetCharacteristic(DEX_CHARACTERISTIC_DATAWRITE);
                lock (ConnectedGatt)
                {
                    try
                    {
                        int ctr = 0;
                        byte[] buffer = new byte[20];
                        while (bytes.Length > ctr * 20)
                        {
                            var len = bytes.Length;
                            var rendu = ctr * 20;

                            buffer = len > rendu + 20
                                ? bytes.Skip(20 * ctr).Take(20).ToArray()
                                : bytes.Skip(20 * ctr).Take(len - rendu).ToArray();
                            charac.SetValue(buffer);
                            ctr++;

                            Java.Lang.Thread.Sleep(20);
                           // charac.WriteType = GattWriteType.NoResponse;
                            bool status = ConnectedGatt.WriteCharacteristic(charac);
                            if (!status || ctr * 20 > len)
                            {
                                var read = ConnectedGatt.ReadCharacteristic(charac);
                                charac.GetValue();
                                if (read)
                                {
                                    MainActivity.RunOnUiThread(() =>
                                    {
                                        Log.Info(TAG + DexService.Operation, "Read completed!");
                                    });
                                }

                                break;
                            }

                            Arrays.Fill(buffer, 0);
                        }
                    }
                    catch (Java.IO.IOException e)
                    {
                        e.PrintStackTrace();
                    }
                    catch (InterruptedException e)
                    {
                        e.PrintStackTrace();
                    }
                    catch (System.OperationCanceledException)
                    {
                        Log.Warn(TAG + DexService.Operation, "Send Dex Bytes Cancelled");
                    }
                    catch (Java.Lang.Exception ex)
                    {
                        Log.Error(TAG, ex.Message);
                    }
                }
            }

            public void senddexfile()
            {
                try
                {
                    BluetoothGattCharacteristic charac = ConnectedGatt.GetService(DEX_SERVICE_SPP)
                        .GetCharacteristic(DEX_CHARACTERISTIC_DATAWRITE);
                    string filepath = "DEX_SAMPLE.txt";
                    byte[] buffer = new byte[20];

                    lock (ConnectedGatt)
                    {
                        try
                        {
                            Stream inputstream = asm.Open(filepath);
                            while (inputstream.Read(buffer, 0, buffer.Length) > 0)
                            {
                                charac.SetValue(buffer);
                                Java.Lang.Thread.Sleep(20);
                                bool status = ConnectedGatt.WriteCharacteristic(charac);
                                if (!status)
                                {
                                    MainActivity.RunOnUiThread(() =>
                                    {
                                        Log.Info(TAG + DexService.Operation, "send dexFile completed!");
                                    });
                                    break;
                                }

                                Arrays.Fill(buffer, 0);
                            }

                            inputstream.Close();
                        }
                        catch (Java.IO.IOException e)
                        {
                            e.PrintStackTrace();
                        }
                        catch (InterruptedException e)
                        {
                            e.PrintStackTrace();
                        }
                    }
                }
                catch (Java.Lang.Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public class BluetoothScanCallback : ScanCallback, IBluetoothScanCallback
        {
            public MainActivity MainActivity { get; set; }
            public DexService DexService { get; set; }

            public BluetoothScanCallback(MainActivity mainActivity, DexService dexService)
            {
                MainActivity = mainActivity;
                DexService = dexService;
            }

            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                if (!deviceFound)
                {
                    if (MainActivity.connectedHandleAddress == null ||
                        !MainActivity.connectedHandleAddress.Equals(result.Device.Address)) return;
                    Log.Info(TAG + DexService.Operation, "Device Found: " + result.Device.Name);
                    deviceFound = true;
                    BluetoothDevice = result.Device;
                    
                        ConnectedGatt = BluetoothDevice.ConnectGatt(Context, false, GattCallback);
                    
                    StopLeScan();
                    Log.Info(TAG + DexService.Operation + DexService.Operation, "onScanResult: " + result.Device.Name);
                }
            }

            public override void OnBatchScanResults(IList<ScanResult> results)
            {
                base.OnBatchScanResults(results);
                Log.Info(TAG + DexService.Operation, "onBatchScanResults: " + results);
            }

            public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
            {
                base.OnScanFailed(errorCode);
                Log.Warn(TAG, "Scan Failed: " + errorCode);
            }
        }
    }
}