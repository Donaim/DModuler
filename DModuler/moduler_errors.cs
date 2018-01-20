using System;
namespace DModulerSpace {
    public class ModulerError : Exception {
        public bool OK => InnerException == null;

        public ModulerError(string mess, Exception ex) : base(mess, ex) {

        }
        public static implicit operator bool (ModulerError me) { return me.OK; }
    }
}