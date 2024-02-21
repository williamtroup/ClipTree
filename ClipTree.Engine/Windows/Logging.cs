using log4net;

namespace ClipTree.Engine.Windows;

public abstract class Logging
{
    protected ILog Log => LogManager.GetLogger(GetType());
}