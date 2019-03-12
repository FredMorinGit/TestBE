using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestBEHoneywell
{
    public interface IDexService
    {
        void Init(bool b );
        void StartLeScan();


        //additions

        void RebootDex();

        void ResetDex();


        void StartStacking();

        void EndStacking();

        string ShowStack();

        List<byte[]> StackList { get; set; }


        //end
        bool Stacking { get; set; }

        
        
        //byte[] WaitForBytes { get; set; }
        //byte[] ReceiveBytes { get; set; }

        int CopyPosition { get; set; }

        void StartCommunication(string operation, bool b);
        void EndCommunication(bool b);

        bool Connected { get; set; }

        string Operation { get; set; }

        void PrepareForScan(Action<string, bool> message);

        Task<bool> Send(byte[] sentBytes, byte[] waitForBytes, CancellationToken token);

     Task< bool> WaitFor(CancellationToken token);


    }

    public interface IBluetoothScanCallback
    {
    }

    public interface IBLEBroadcastReceiver
    {

    }

    public interface ILEGattCallback
    {

    }

    public interface ISendDexFile
    {

        //bool Waiting { get; set; }

    void SendDexBytes(byte[] bytes, CancellationToken token);

    }

}