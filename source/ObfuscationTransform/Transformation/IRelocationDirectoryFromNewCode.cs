using ObfuscationTransform.Core;

namespace ObfuscationTransform.Transformation
{
    /// <summary>
    /// Creates a new relocation directory that correspond new modified code.
    /// The old relocation table addresses are transformes to new code addresses
    /// </summary>
    public interface IRelocationDirectoryFromNewCode
    {
        IRelocationDirectoryInfo CreateNewRelocationDirectoryInfo(ICode code);
    }
}