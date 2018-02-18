using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using SharerSpace;
using ResultSpace;

namespace DModulerSpace
{
    public class DModuler {
        private readonly Sharer sh;

        public DModuler(Sharer sh_) {
            sh = sh_;
        }

        public bool TryLoadLibrary(string file) => LoadLibrary(file, out _);
        public ResultV LoadLibrary(string file, out ResultV re, bool ignoreConstructErrors = false, bool ignoreLoadingErrors = true, bool runAssembly = false) {
            re = new ResultV();
            try { _LoadLibrary(file, re, ignoreConstructErrors, ignoreLoadingErrors, runAssembly); } catch { }
            return re;
        }
        void _LoadLibrary(string file, ResultV re, bool ignoreConstructErrors, bool ignoreLoadingErrors, bool runAssembly) {
            if(!File.Exists(file))
            {
                throw re.Throw($"Libary file \"{file}\" does not exist!");
            }

            Assembly ass;
            Type[] types;
            try {
                ass = Assembly.UnsafeLoadFrom(file);
                types = ass.GetTypes();
            } catch (Exception lex) {
                throw re.Throw($"Error loading assembly \"{file}\"", lex);
            }

            createInstances(types, out var ldb_list, re, ignoreConstructErrors);
            loadIntefaces(ldb_list, re, ignoreLoadingErrors);
            autoaddInterfaces(types, re);

            if(runAssembly) { runStartups(ldb_list, re); }
        }
        void autoaddInterfaces(IEnumerable<Type> types, ResultV re) {
            foreach(var t in types) {
                var autosharetype = t.GetCustomAttribute<AutoShareTypeAttribute>();
                if (autosharetype != null) {
                    string name = autosharetype.Name != null ? autosharetype.Name : t.Name; 
                    sh.AddType(t, name);
                }
            }
        }
        static void runStartups(IEnumerable<object> ldb_list, ResultV re) {
            foreach(var o in ldb_list) {
                if (o is IAssemblyStarter st) {
                    try { st.Start(re); } 
                    catch (Exception ex) { throw re.Throw(ex); }
                }
            }
        }
        static void createInstances(IEnumerable<Type> types, out IEnumerable<object> list, ResultV err, bool ignoreConstructErrors) {
            var re = new List<object>();
            list = re;
      
            foreach(var o in types.Where(isTypeToLoad)) {
                if(o.IsAbstract || o.IsInterface) { continue; } // do not create abstract classes

                try {
                    var inst = Activator.CreateInstance(o);
                    re.Add(inst);
                }
                catch (Exception cex) {
                    err.Log($"Cannot create instance of type \"{o.Name}\" !", cex);
                    if(!ignoreConstructErrors) {
                        list = null;
                        throw err.ThrowLast();
                    }
                }
            }
        }
        static bool isTypeToLoad(Type t) => t.GetInterface(nameof(ILoadable)) != default(Type) || t.GetInterface(nameof(IAssemblyStarter)) != default(Type);
        private void loadIntefaces(IEnumerable<object> list, ResultV err, bool ignoreLoadingErrors) {
            foreach(var o in list) {
                if(o is ILoadable l) {
                    try { l.OnAssemblyLoad(this); }
                    catch (Exception lex) {
                        err.Log($"Interface \"{l}\" throwed error on load", lex);
                        if(!ignoreLoadingErrors) { throw err.ThrowLast(); }
                    }
                }
            }
            foreach(var o in list) {
                if(o is ILoadable l) {
                    try { l.AfterAssemblyLoad(sh); }
                    catch (Exception aex) {
                        err.Log($"Interface \"{l}\" throwed error after load", aex);
                        if(!ignoreLoadingErrors) { throw err.ThrowLast(); }
                    }
                }
            }
        }
    }
}