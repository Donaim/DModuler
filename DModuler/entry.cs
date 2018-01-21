namespace DModulerSpace
{
    public static class Entry {
        public static void Main(string [] args) {
            if(args.Length > 0) { new DModuler(new SharerSpace.Sharer()).LoadLibrary(args[0], out _); }
            else { throw new System.Exception("Expected assembly name in arguments!"); }
        }
    }
}