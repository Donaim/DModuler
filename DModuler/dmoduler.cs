using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using SharerSpace;

namespace DModulerSpace
{
    public class DModuler {
        private readonly Sharer sh;

        public DModuler(Sharer sh_) {
            sh = sh_;
        }

        public bool TryLoadLibrary(string file) => LoadLibrary(file, out var _);
        public bool LoadLibrary(string file, out Exception ex, bool ignoreConstructErrors = false, bool ignoreLoadingErrors = true) {
            ex = null;

            if(!File.Exists(file))
            {
                ex = new Exception($"Libary file \"{file}\" does not exist!");
                return false;
            }

            Assembly ass;
            try {
                ass = Assembly.UnsafeLoadFrom(file);
            } catch (Exception lex) {
                ex = new Exception($"Error loading assembly \"{file}\"", lex);
                return false;
            }

            if(!parseLibrary(ass, out ex, ignoreConstructErrors, out var list)) { return false; }
            if(!loadIntefaces(list, out ex, ignoreLoadingErrors)) { return false; }

            return true;
        }

        static bool isLoadableType(Type t) => t.GetInterface(nameof(ILoadable)) != default(Type);
        static bool createInstances(IEnumerable<Type> ldbTypes, out IEnumerable<ILoadable> list, out Exception ex, bool ignoreConstructErrors) {
            ex = null;
            var re = new List<ILoadable>();
      
            foreach(var o in ldbTypes) {
                try {
                    var inst = Activator.CreateInstance(o);
                    re.Add((ILoadable)inst);
                }
                catch (Exception cex) {
                    ex = new Exception($"Cannot create instance of type \"{o.Name}\" !", cex);
                    if(!ignoreConstructErrors) {
                        list = null;
                        return false;
                    }
                }
            }

            list = re;
            return true;
        }
        private bool parseLibrary(Assembly a, out Exception ex, bool ignoreConstructErrors, out IEnumerable<ILoadable> list) {
            var types = a.GetTypes();
            var ldbTypes = types.Where(isLoadableType);

            if(!createInstances(ldbTypes, out list, out var cex, ignoreConstructErrors)) {
                ex = new Exception($"Cannot parse library \"{a.GetName()}\"", cex);
                list = null;
                return false;
            }

            ex = null;
            return true;
        }
        private bool loadIntefaces(IEnumerable<ILoadable> list, out Exception ex, bool ignoreLoadingErrors) {
            ex = null;
            foreach(var l in list) {
                loadInterface(l);
            }
            
            foreach(var l in list) {
                try {
                    l.OnAssemblyLoad(this);
                }
                catch (Exception lex) {
                    ex = new Exception($"Interface \"{l}\" throwed error on load", lex);
                    if(!ignoreLoadingErrors) { return false; }
                }
            }
            foreach(var l in list) {
                try {
                    l.AfterAssemblyLoad(sh);
                }
                catch (Exception aex) {
                    ex = new Exception($"Interface \"{l}\" throwed error after load", aex);
                    if(!ignoreLoadingErrors) { return false; }
                }
            }
            return true;
        }

        private List<ILoadable> loadedList = new List<ILoadable>();
        private void loadInterface(ILoadable ildb) {
            loadedList.Add(ildb);
            if(ildb is ISharable ish) {
                sh.AddInterface(ish);
            }
        }
    }
    
}