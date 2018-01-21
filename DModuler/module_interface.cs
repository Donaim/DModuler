using SharerSpace;
using vutils;

namespace DModulerSpace
{
    public interface IAssemblyStarter
    {
        void Start(OutputEx o);
    }
    public interface ILoadable
    {
        void OnAssemblyLoad(DModuler m);
        void AfterAssemblyLoad(Sharer sh);
    }
}