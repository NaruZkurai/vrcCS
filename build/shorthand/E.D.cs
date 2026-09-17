/*strings: Error Dialogue
 * usage NZK.S.E1
 *  NZK.S.E2
 *  NZK.S.E1
*/
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
/*add error code to output class*/
namespace NZK
{ public static partial class E

 {
    public static partial class D
    { public static void OK(string a, string b){ NZK.E.Dd(a ,b, "ok");}
      /* numeric code pair: resolves via BarCodeKiller, same as NerrOK.
         The object u supplies the value interpolated into rrNN<T>(T u) codes. */
      public static void OK<T>(T u, System.Int64 a, System.Int64 b)
      { string title; string message;
        NZK.E.BarCodeKiller(a, b, u, out title, out message);
        NZK.E.Dd(title, message, "ok"); }
      public static bool NerrOK<T>(T c, int a, int b, T u) where T : class
      { return NZK.E.BarCodePair(c == null, a, b, u); }
}}}