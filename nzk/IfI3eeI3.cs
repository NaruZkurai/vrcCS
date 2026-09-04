public static partial class nzk
{ public static bool IfI3eeI3<T>
( T x,    T y,    T z,
  T xVal, T yVal, T zVal)
  { if (  nzk.SNllE(xVal) == nzk.SNllE(x)
       && nzk.SNllE(yVal) == nzk.SNllE(y)
       && nzk.SNllE(zVal) == nzk.SNllE(z))
    { return true;}
    else
    { return false;} }
}