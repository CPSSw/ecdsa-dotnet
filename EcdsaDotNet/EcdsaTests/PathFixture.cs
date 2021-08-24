using System;
using System.IO;

public abstract class PathFixture : IDisposable
{
    protected PathFixture()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
        Directory.SetCurrentDirectory(path);
    }

    void IDisposable.Dispose()
    {
    }
}