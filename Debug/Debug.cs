[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Illuminate")]
namespace Illuminate {
	internal static class Debug {
		internal static void _(params object?[] objs) {
			foreach(object? obj in objs) {
				System.Console.WriteLine(obj);
			}
		}
	}
}