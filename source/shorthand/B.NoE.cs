namespace NZK{  public static partial class B{
  /* ALIAS. B.NoE predates the N.<operator>.<condition> naming and is kept so
     existing call sites keep compiling. It answers exactly the same question
     as B.N.ll.e ("null or empty"); new code should call that, because the name
     states the operator the test uses.

     Do NOT give this its own body - two implementations of one predicate is
     how the two Sanitize copies drifted apart earlier in this toolkit. */
  public static bool NoE(string a){return NZK.B.N.ll.e(a);}
  }
}
