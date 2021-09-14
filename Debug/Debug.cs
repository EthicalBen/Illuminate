namespace Illuminate {
    internal static class Debug {
        internal static void _(params object?[] objs) {
            if(objs.Length == 0) System.Console.WriteLine("<3");


            foreach(object? obj in objs) {
                System.Console.WriteLine(obj);
            }
        }
    }
}