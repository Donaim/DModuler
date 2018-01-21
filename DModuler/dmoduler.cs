using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using SharerSpace;
using vutils;

namespace DModulerSpace
{
    public class DModuler {
        private readonly Sharer sh;

        public DModuler(Sharer sh_) {
            sh = sh_;
        }

        public OutputEx TryLoadLibrary(string file) => LoadLibrary(file, out _);
        public OutputEx LoadLibrary(string file, out OutputEx re, bool ignoreConstructErrors = false, bool ignoreLoadingErrors = true) {
            re = new OutputEx();
            _LoadLibrary(file, re, ignoreConstructErrors, ignoreLoadingErrors);
            return re;
        }
        void _LoadLibrary(string file, OutputEx re, bool ignoreConstructErrors = false, bool ignoreLoadingErrors = true) {
            if(!File.Exists(file))
            {
                re.Throw($"Libary file \"{file}\" does not exist!");
                return;
            }

            Assembly ass;
            try {
                ass = Assembly.UnsafeLoadFrom(file);
            } catch (Exception lex) {
                re.Throw($"Error loading assembly \"{file}\"", lex);
                return;
            }

            if(!parseLibrary(ass, re, ignoreConstructErrors, out var list)) { return; }
            if(!loadIntefaces(list, re, ignoreLoadingErrors)) { return; }
        }

        static bool isLoadableType(Type t) => t.GetInterface(nameof(ILoadable)) != default(Type);
        static OutputEx createInstances(IEnumerable<Type> ldbTypes, out IEnumerable<ILoadable> list, OutputEx err, bool ignoreConstructErrors) {
            var re = new List<ILoadable>();
      
            foreach(var o in ldbTypes) {
                try {
                    var inst = Activator.CreateInstance(o);
                    re.Add((ILoadable)inst);
                }
                catch (Exception cex) {
                    err.Log($"Cannot create instance of type \"{o.Name}\" !", cex);
                    if(!ignoreConstructErrors) {
                        list = null;
                        return err.ThrowLast();
                    }
                }
            }

            list = re;
            return true;
        }
        private OutputEx parseLibrary(Assembly a, OutputEx err , bool ignoreConstructErrors, out IEnumerable<ILoadable> list) {
            var types = a.GetTypes();
            var ldbTypes = types.Where(isLoadableType);

            if(!createInstances(ldbTypes, out list, err, ignoreConstructErrors)) {
                return err.Throw($"Cannot parse library \"{a.GetName()}\"");
            }

            return err;
        }
        private OutputEx loadIntefaces(IEnumerable<ILoadable> list, OutputEx err, bool ignoreLoadingErrors) {
            foreach(var l in list) {
                loadInterface(l);
            }
            
            foreach(var l in list) {
                try {
                    l.OnAssemblyLoad(this);
                }
                catch (Exception lex) {
                    err.Log($"Interface \"{l}\" throwed error on load", lex);
                    if(!ignoreLoadingErrors) { return err.ThrowLast(); }
                }
            }
            foreach(var l in list) {
                try {
                    l.AfterAssemblyLoad(sh);
                }
                catch (Exception aex) {
                    err.Log($"Interface \"{l}\" throwed error after load", aex);
                    if(!ignoreLoadingErrors) { return err.ThrowLast(); }
                }
            }

            return err;
        }

        private List<ILoadable> loadedList = new List<ILoadable>();
        private void loadInterface(ILoadable ildb) {
            loadedList.Add(ildb);
            if(ildb is ISharable ish) {
                sh.AddType(ildb.GetType(), ish.Name);
                sh.AddInterface(ish);
            }
        }
    }
    
}