using System.Windows;

namespace SystemChecker.Tests;

public abstract class TestBase
{
    protected TestBase()
    {
        if (Application.Current == null)
        {
            new Application();
        }
    }
} 