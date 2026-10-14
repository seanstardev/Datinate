namespace Datinate.Rmvc.Proxy
{
    /// <summary>Reports a progress update and completes when its handler has finished.</summary>
    public delegate Task ProgressCallback(string message, int part, int total);

    /// <summary>Reports file-operation progress and completes when its handler has finished.</summary>
    public delegate Task FileProgressCallback(int current, int total, string message);
}
