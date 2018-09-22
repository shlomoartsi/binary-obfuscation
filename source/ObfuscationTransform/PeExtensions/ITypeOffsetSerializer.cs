namespace ObfuscationTransform.PeExtensions
{
    public interface ITypeOffsetSerializer
    {
        void Serialize(byte[] buffer, ref ulong bufferOffset, RelocationTypeOffset relocatiomOffset);
    }
}