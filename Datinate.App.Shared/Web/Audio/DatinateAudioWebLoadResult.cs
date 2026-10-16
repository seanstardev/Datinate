using System;

namespace Datinate.App.Shared.Web.Audio
{
    public enum DatinateAudioWebLoadStatus
    {
        Loaded,
        NotPlayable,
        Unavailable,
        Cancelled
    }

    // NOTE: A plain immutable struct avoids the record/init support types missing from .NET Standard 2.0.
    public readonly struct DatinateAudioWebLoadResult
    {
        public DatinateAudioWebLoadStatus Status { get; }
        public Uri Source { get; }
        public string? Reason { get; }
        public string? Error { get; }

        public bool Loaded => Status == DatinateAudioWebLoadStatus.Loaded;

        public DatinateAudioWebLoadResult(
            DatinateAudioWebLoadStatus status,
            Uri source,
            string? reason = null,
            string? error = null)
        {
            Status = status;
            Source = source;
            Reason = reason;
            Error = error;
        }
    }
}
