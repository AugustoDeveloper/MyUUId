using System.Net.NetworkInformation;

var myId = MyUUId.NewUUId();
Console.WriteLine(myId);
Console.WriteLine(Guid.Parse(myId.ToString()));
Console.WriteLine(Guid.NewGuid());

public readonly struct MyUUId
{
    private readonly uint timeLow;
    private readonly ushort timeMid;
    private readonly ushort timeHiAndVersion;
    private readonly byte clockSeqHiAndReserved;
    private readonly byte clockSeqLow;
    private readonly byte[] node;

    public static readonly DateTime StartTimestamp = new DateTime(1582, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc);

    public MyUUId(
            uint timeLow, 
            ushort timeMid, 
            ushort timeHiAndVersion,
            byte clockSeqHiAndReserved,
            byte clockSeqLow,
            byte[] node)
    {
        this.timeLow = timeLow;
        this.timeMid = timeMid;
        this.timeHiAndVersion = timeHiAndVersion;
        this.clockSeqHiAndReserved = clockSeqHiAndReserved;
        this.clockSeqLow = clockSeqLow;
        this.node = node;
    }

    public static MyUUId NewUUId()
    {
        var clock = DateTime.UtcNow.Ticks;
        var currentTimestamp = (long)(clock - MyUUId.StartTimestamp.Ticks) * 1000 + 1024;

        var timeLow = (uint)(currentTimestamp & 0xFFFFFFFF);
        var timeMid = (ushort)((currentTimestamp & 0xFFFF00000000) >> 32);
        var timeHiAndVersion = (ushort)(currentTimestamp >> 48);
        ushort version = 1 << 12;
        timeHiAndVersion = (ushort) (version | timeHiAndVersion);
        var clockSeqLow = (byte) (clock & 0xFF);
        var clockSeqHiAndReserved = (byte) (((clock & 0x3F00) >> 8) | 0x80);
        
        var x = NetworkInterface.GetAllNetworkInterfaces();
        var address = x[0].GetPhysicalAddress();

        return new(timeLow, timeMid, timeHiAndVersion, clockSeqLow, clockSeqHiAndReserved, address.GetAddressBytes());
    }

    public override string ToString()
        => timeLow.ToString("x8") + "-" +
            timeMid.ToString("x4") + "-" +
            timeHiAndVersion.ToString("x4") + "-" +
            clockSeqLow.ToString("x2") +
            clockSeqHiAndReserved.ToString("x2") + "-" +
            Convert.ToHexString(node).ToLower();
}

/*
  uuid->clock_seq_low = clock_seq & 0xFF;
  uuid->clock_seq_hi_and_reserved = (clock_seq & 0x3F00) >> 8;
  uuid->clock_seq_hi_and_reserved |= 0x80;

 o  Set the clock_seq_low field to the eight least significant bits
      (bits zero through 7) of the clock sequence in the same order of
      significance.

   o  Set the 6 least significant bits (bits zero through 5) of the
      clock_seq_hi_and_reserved field to the 6 most significant bits
      (bits 8 through 13) of the clock sequence in the same order of
      significance.

   o  Set the two most significant bits (bits 6 and 7) of the
      clock_seq_hi_and_reserved to zero and one, respectively.

*/
