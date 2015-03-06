using System;

namespace KeyValueLite
{
  /// <summary>
  /// </summary>
  public static class GuardExtensions
  {
    #region Derived from Caveman Tools
    /// Derived from CaveMan Tools (https://bitbucket.org/sapiensworks/caveman-tools/wiki/CTools)
    /// since CaveMan has too many other things we dont need, and doesnt have a PCL version

    public static void MustNotBeNull<T>(this T param, string paramName = null) where T : class
    {
      if (param == null)
        throw new ArgumentNullException(paramName ?? string.Empty);
    }

    public static void MustNotBeEmpty(this string arg, string paramName = null)
    {
      if (string.IsNullOrEmpty(arg))
        throw new FormatException(string.Format("Argument '{0}' must not be null or empty", paramName ?? ""));
    }
    #endregion
  }
}