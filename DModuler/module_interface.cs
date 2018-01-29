using System;
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

    [AttributeUsage(AttributeTargets.Class)]
    public class AutoShareTypeAttribute : Attribute {
        public readonly string Name;
        public AutoShareTypeAttribute(string name = null) {
            Name = name;
        }
    }
}