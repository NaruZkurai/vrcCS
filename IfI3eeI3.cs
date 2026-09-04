public static partial class nzk
{ public static bool IfI3eeI3<T>
  ( T x,    T y,    T z,
    T xVal, T yVal, T zVal)
  { if (   nzk.ec(xVal, x)
        && nzk.ec(yVal, y)
        && nzk.ec(zVal, z))
    { return true; }
    else
    { return false; } }
}