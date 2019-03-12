using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Exception = Java.Lang.Exception;

namespace TestBEHoneywell.Droid
{
    [Activity(Label = "TestBEHoneywell", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
       

        internal static string connectedHandleAddress;
        internal static string pin;
        internal static string battery;
        internal static bool toggleoperation = true;
     
        internal static MainActivity currentActivity;

        public DexService DexService { get; set; }

     
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);


            //connectedHandleAddress = "00:10:20:8E:2D:7E";
            //connectedHandleAddress = "00:10:20:8E:2B:27";
            //connectedHandleAddress = "00:10:20:8E:28:9D";
            connectedHandleAddress = "00:10:20:8E:2B:ED";
            base.OnCreate(savedInstanceState);

            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            try
            {
                DexService = new DexService(this);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           

             DexService.Init();

            LoadApplication(new App());

            App.DexService = DexService;

            currentActivity = this;





          
            //if (DexService.BluetoothAdapter == null)
            //{
            //    Log.Info(DexService.TAG + DexService.Operation, "Initialize bluetooth adapter failed!");
            //}
            //else
            //{
            //    IntentFilter mfilter = new IntentFilter(BluetoothDevice.ActionAclConnected);
            //    mfilter.AddAction(BluetoothDevice.ActionPairingRequest);
            //    mfilter.AddAction(BluetoothDevice.ActionBondStateChanged);
            //    mfilter.AddAction(BluetoothDevice.ActionFound);
            //    mfilter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            //    mfilter.Priority = (int)IntentFilterPriority.HighPriority;
            //    RegisterReceiver(Receiver, mfilter);
            //}

            //PendingIntent = PendingIntent.GetActivity(this,
            //    0,
            //    new Intent(this,
            //        GetType()).AddFlags(ActivityFlags.SingleTop),
            //    0);

        }

        internal PendingIntent PendingIntent;

        internal BLEBroadcastReceiver Receiver;


        public override void OnCreate([GeneratedEnum] Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

       
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            try
            {
                NdefMessage[] messages = DexService.GetNdefMessages(intent);
                if (messages[0] != null)
                {
                    string temp = DexService.displayByteArray(messages[0].ToByteArray());
                    string mac = temp.Substring(11, 12);
                    // Since the Bluttooh address is reversed in the NDEF Record we need to turn it around to have the actual address
                    connectedHandleAddress = mac.Substring(10, 2) + ":" + mac.Substring(8, 2) + ":" + mac.Substring(6, 2) + ":" + mac.Substring(4, 2) + ":" + mac.Substring(2, 2) + ":" + mac.Substring(0, 2);
                    // Gets the Bluetooth pin number
                    int index = temp.LastIndexOf('$');
                    if (index >= 0)
                    {
                        pin = temp.Substring(index + 1, 6);
                    }
                    // Gets the battery level of the DEX adapter
                    battery = temp.Substring(temp.IndexOf(':') + 1, (temp.LastIndexOf('%') - temp.IndexOf(':')));
                    string firmware = temp.Substring(temp.LastIndexOf('%') + 1);
                    Log.Info(DexService.TAG + DexService.Operation, "Bluetooth Address: " + connectedHandleAddress);
                    Log.Info(DexService.TAG + DexService.Operation, "Pin Number: " + pin);
                    Log.Info(DexService.TAG + DexService.Operation, "Battery Level:" + battery);
                    Log.Info(DexService.TAG + DexService.Operation, "Firmware: " + firmware);
                    // Scards the scanning process for the BLE device.
                    
                }
            }
            catch (Exception e)
            {
                Log.Error(DexService.TAG, $"{e.Message} => NFC Error - Unable to read tag");
            }
        }

    }
}