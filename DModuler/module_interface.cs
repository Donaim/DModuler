using SharerSpace;

namespace DModulerSpace
{
    public interface ILoadable
    {
        void OnAssemblyLoad(DModuler m);
        void AfterAssemblyLoad(Sharer sh);
    }
}