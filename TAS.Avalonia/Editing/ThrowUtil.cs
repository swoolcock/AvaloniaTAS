using System.Globalization;

namespace TAS.Avalonia.Editing;

internal static class ThrowUtil {
    public static T CheckNotNull<T>(T val, string parameterName) where T : class => val ?? throw new ArgumentNullException(parameterName);

    public static int CheckNotNegative(int val, string parameterName) => val >= 0 ? val : throw new ArgumentOutOfRangeException(parameterName, (object) val, "value must not be negative");

    public static int CheckInRangeInclusive(int val, string parameterName, int lower, int upper) {
        if (val < lower || val > upper) {
            throw new ArgumentOutOfRangeException(parameterName, val, "Expected: " + lower.ToString(CultureInfo.InvariantCulture) + " <= " + parameterName + " <= " + upper.ToString(CultureInfo.InvariantCulture));
        }
        return val;
    }

    public static InvalidOperationException NoDocumentAssigned() => new InvalidOperationException("Document is null");

    public static InvalidOperationException NoValidCaretPosition() => new InvalidOperationException("Could not find a valid caret position in the line");
}
