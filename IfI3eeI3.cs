public static partial class NZK
{ public static bool IfI3eeI3<T>
  ( T x,    T y,    T z,
    T xVal, T yVal, T zVal)
  { if (   NZK.ec(xVal, x)
        && NZK.ec(yVal, y)
        && NZK.ec(zVal, z))
    { return true; }
    else
    { return false; } }
}