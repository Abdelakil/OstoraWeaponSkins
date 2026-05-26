using System.Reflection;
using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2;

internal class Program
{
    public static void Main()
    {
        PrintPadding<CMoveData>();
    }

    public static void PrintPadding<T>()
    {
        var fields = typeof(T).GetFields(BindingFlags.Instance |
                                         BindingFlags.Public |
                                         BindingFlags.NonPublic);

        Array.Sort(fields, (a, b) =>
            Marshal.OffsetOf<T>(a.Name).ToInt32()
            .CompareTo(Marshal.OffsetOf<T>(b.Name).ToInt32()));

        var current = 0;

        foreach (var field in fields)
        {
            var offset = Marshal.OffsetOf<T>(field.Name).ToInt32();

            if (offset > current)
                Console.WriteLine($"Padding: {current} - {offset - 1}");

            Console.WriteLine($"{field.Name}: {offset}");

            current = offset + Marshal.SizeOf(field.FieldType);
        }

        var total = Marshal.SizeOf<T>();

        if (current < total)
            Console.WriteLine($"Padding: {current} - {total - 1}");
    }

    private static int GetFieldSize(Type type)
    {
        if (type.IsPointer)
            return IntPtr.Size;
        else if (type.IsEnum)
            return GetFieldSize(Enum.GetUnderlyingType(type));

        try
        {
            return Marshal.SizeOf(type);
        }
        catch
        {
            return 0;
        }
    }
}