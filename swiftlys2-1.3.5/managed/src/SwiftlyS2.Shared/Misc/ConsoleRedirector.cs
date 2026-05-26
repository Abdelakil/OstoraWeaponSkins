using System.Text;
using SwiftlyS2.Core.Natives;

namespace SwiftlyS2.Shared.Misc;

internal class ConsoleRedirector : TextWriter
{
    private readonly TextWriter originalOut;
    private readonly Lock lockObject = new();
    private bool isRedirecting = false;

    public ConsoleRedirector()
    {
        originalOut = Console.Out;
    }

    public override Encoding Encoding => originalOut.Encoding;

    public override void WriteLine( string? value )
    {
        lock (lockObject)
        {
            if (isRedirecting)
            {
                return;
            }

            try
            {
                isRedirecting = true;
                var v = value ?? "(null)";
                if (!v.EndsWith('\n'))
                {
                    v += "\n";
                }

                if (v.Length >= 512) // maximum console output length per message
                {
                    var chunks = v.Chunk(512);
                    foreach (var chunk in chunks)
                    {
                        var chunkStr = new string(chunk);
                        NativeEngineHelpers.SendMessageToConsole(chunkStr);
                    }
                }
                else
                {
                    NativeEngineHelpers.SendMessageToConsole(v);
                }
            }
            finally
            {
                isRedirecting = false;
            }
        }
    }

    public override void Write( string? value )
    {
        lock (lockObject)
        {
            if (isRedirecting)
            {
                return;
            }

            try
            {
                isRedirecting = true;

                var v = value ?? "(null)";
                if (v.Length >= 512) // maximum console output length per message
                {
                    var chunks = v.Chunk(512);
                    foreach (var chunk in chunks)
                    {
                        var chunkStr = new string(chunk);
                        NativeEngineHelpers.SendMessageToConsole(chunkStr);
                    }
                }
                else
                {
                    NativeEngineHelpers.SendMessageToConsole(v);
                }
            }
            finally
            {
                isRedirecting = false;
            }
        }
    }
}