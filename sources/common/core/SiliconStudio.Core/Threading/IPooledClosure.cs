namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Interface implemented by pooled closure types through the AssemblyProcessor.
    /// Enables <see cref="PooledDelegateHelper"/> to keep closures and delegates alive.
    /// </summary>
    public interface IPooledClosure
    {
        void AddReference();

        void Release();
    }
}