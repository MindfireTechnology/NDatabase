namespace NDatabase2.Odb.Core.Transaction
{
    internal sealed class IdInfo
    {
        internal IdInfo(long position, byte status)
        {
            Position = position;
            Status = status;
        }

        internal long Position { get; private set; }
        internal byte Status { get; set; }
    }
}
