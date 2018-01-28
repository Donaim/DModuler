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

        public bool TryLoadLibrary(string file) => LoadLibrary(file, out _);
        public OutputEx LoadLibrary(string file, out OutputEx re, bool ignoreConstructErrors = false, bool ignoreLoadingErrors = true, bool runAssembly = false) {
            re = new OutputEx();
            try { _LoadLibrary(file, re, ignoreConstructErrors, ignoreLoadingErrors, runAssembly); } catch { }
            return re;
        }
        void _LoadLibrary(string file, OutputEx re, bool ignoreConstructErrors, bool ignoreLoadingErrors, bool runAssembly) {
            if(!File.Exists(file))
            {
                throw re.Throw($"Libary file \"{file}\" does not exist!");
            }

            Assembly ass;
            try {
                ass = Assembly.UnsafeLoadFrom(file);
            } catch (Exception lex) {
                throw re.Throw($"Error loading assembly \"{file}\"", lex);
            }

            initLibrary(ass, re, ignoreConstructErrors, out var list);
            loadIntefaces(list, re, ignoreLoadingErrors);

            if(runAssembly) { runStartups(list, re); }
        }
        static bool runStartups(IEnumerable<object> list, OutputEx re) {
            foreach(var o in list) {
                if(o is IAssemblyStarter st) { st.Start(re); }
            }
            if (re) { return re; }
            else { throw re; }
        }
        static void createInstances(IEnumerable<Type> types, out IEnumerable<object> list, OutputEx err, bool ignoreConstructErrors) {
            var re = new List<object>();
            list = re;
      
            foreach(var o in types) {
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
        static bool isTypeToLoad(Type t) => t.GetInterface(nameof(ILoadable)) != default(Type) || t.GetInterface(nameof(IAssemblyStarter)) != default(Type)|| t.GetInterface(nameof(IAutoshare)) != default(Type);
        private void initLibrary(Assembly a, OutputEx err , bool ignoreConstructErrors, out IEnumerable<object> list) {
            var types = a.GetTypes();
            // Console.WriteLine($"Types = [{string.Join(" | ", types.ToList())}]");
            var ldbTypes = types.Where(isTypeToLoad);

            try { createInstances(ldbTypes, out list, err, ignoreConstructErrors); }
            catch { throw err.Throw($"Cannot init library \"{a.GetName()}\""); }
        }
        private void loadIntefaces(IEnumerable<object> list, OutputEx err, bool ignoreLoadingErrors) {
            foreach(var o in list) {
                if(o is ISharable ish) {
                    sh.AddType(ish.GetType(), ish.Name);
                    sh.AddInterface(ish);
                }
            }
            
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