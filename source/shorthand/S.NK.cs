namespace NZK{  public static partial class S{
  /// <summary>
  /// The literal log prefix used by every NaNimate log line.
  ///
  /// It recurs dozens of times across the toolkit and is load-bearing: the
  /// editor log is filtered BY THIS PREFIX when a run is diagnosed, so the
  /// literal must never be reworded or normalised. Hoisting it to one place
  /// makes that guarantee explicit instead of a copy/paste convention.
  /// </summary>
  public const string NK="[NZK NaNimate] ";

  /// <summary>Log prefix, concatenated ahead of a message.</summary>
  public static string NZKNaNimatePrefix(string message){return NK+message;}

  /// <summary>
  /// True when a string ends with a suffix, case-insensitively and without
  /// culture sensitivity.
  ///
  /// OrdinalIgnoreCase is deliberate: asset extensions arrive with either
  /// case from user-supplied paths, and a culture-aware comparison can
  /// behave differently per locale, which is unacceptable for an extension
  /// test that decides whether a path is a prefab.
  /// </summary>
  public static bool EndsWithOIC(string a,string b){
    return a!=null&&a.EndsWith(b,System.StringComparison.OrdinalIgnoreCase);
  }
  }
  }
