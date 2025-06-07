using System.Diagnostics;
namespace UserAction.Utilites;
public class RequestTimer : IDisposable
{
    private readonly string _label;
    private readonly Stopwatch _sw;

    public RequestTimer(string label)
    {
        _label = label;
        _sw = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _sw.Stop();
        Console.WriteLine($"⏱️\t{_label}: {_sw.ElapsedMilliseconds} ms");
    }
}
