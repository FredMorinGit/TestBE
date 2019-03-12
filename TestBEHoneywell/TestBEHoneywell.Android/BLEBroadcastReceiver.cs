using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.IO;
using Java.Lang;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(TestBEHoneywell.Droid.BLEBroadcastReceiver))]

namespace TestBEHoneywell.Droid
{
    [BroadcastReceiver]
    public class BLEBroadcastReceiver : BroadcastReceiver, IBLEBroadcastReceiver
    {
        private BluetoothDevice device;

        public MainActivity MainActivity { get; set; }

        public DexService DexService { get; set; }

        public BLEBroadcastReceiver()
        {
        }

        public BLEBroadcastReceiver(MainActivity mainActivity, DexService dexService)
        {
            MainActivity = mainActivity;
            DexService = dexService;
        }

        public override IBinder PeekService(Context myContext, Intent service)
        {
            Log.Info(DexService.TAG + DexService.Operation, $"PeekService => {service.Type}");
            return base.PeekService(myContext, service);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            if (BluetoothDevice.ActionPairingRequest.Equals(action))
            {
                if (MainActivity.pin == null)
                    return;
                device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                if (device.Address.Equals(DexService.BluetoothDevice.Address))
                {
                    try
                    {
                        if (!MainActivity.toggleoperation)
                        {
                            byte[] bArray = Encoding.UTF8.GetBytes(MainActivity.pin);
                            DexService.BluetoothDevice.SetPin(bArray);
                            DexService.BluetoothDevice.SetPairingConfirmation(true);
                            MainActivity.toggleoperation = true;
                        }
                        else
                        {
                            MainActivity.toggleoperation = false;
                        }
                    }
                    catch (UnsupportedEncodingException e)
                    {
                        e.PrintStackTrace();
                    }
                    catch (InterruptedException e)
                    {
                        e.PrintStackTrace();
                    }
                }
            }
        }
    }
}