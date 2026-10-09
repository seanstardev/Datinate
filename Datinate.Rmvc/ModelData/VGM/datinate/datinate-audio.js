(function () {
    const post =
        window.datinateWebView?.post ||
        (() => { });

    let sourceUrl = null;
    let sourceTitle = "";
    let playlist = [];
    let currentTrackIndex = -1;
    let loadedTrackIndex = -1;
    let currentBackend = null;
    let archiveSession = null;
    let operationVersion = 0;
    let volume = 1;
    let repeatPlaylist = false;
    let seekSession = null;
    let previewMode = false;

    function getSourceName(url) {
        try {
            const parsed =
                new URL(url, window.location.href);

            return decodeURIComponent(
                parsed.pathname.split("/").pop() ||
                "Audio");
        }
        catch {
            return "Audio";
        }
    }

    function cleanTitle(path) {
        const name =
            String(path || "")
                .replaceAll("\\", "/")
                .split("/")
                .pop() || "";

        const dot =
            name.lastIndexOf(".");

        return dot > 0
            ? name.substring(0, dot)
            : name;
    }

    function publicTrack(track, index) {
        return {
            index,
            title: track.title,
            durationSeconds:
                track.durationSeconds ?? null
        };
    }

    function getPlaylist() {
        return playlist.map(publicTrack);
    }

    function setPlaylist(tracks) {
        playlist = tracks || [];
        currentTrackIndex =
            playlist.length > 0
                ? 0
                : -1;
        loadedTrackIndex = -1;
    }

    function getBrowserArchiveCandidates(entries) {
        return entries.filter(entry =>
            window.datinateBrowserAudio
                .canProbablyPlayPath(entry.path));
    }

    async function classifyArchiveEntries(entries) {
        const browserCandidates =
            getBrowserArchiveCandidates(entries);

        if (browserCandidates.length > 0) {
            return {
                canPlay: true,
                backend: "browser",
                browserCandidates
            };
        }

        const paths =
            entries.map(entry => entry.path);

        const vgmCanPlay =
            await window.datinateVgm
                .canPlayAny(paths);

        return {
            canPlay: vgmCanPlay,
            backend: vgmCanPlay
                ? "vgm"
                : null,
            browserCandidates: []
        };
    }

    async function probeArchive(url) {
        const session =
            await window.datinateArchive.open(url);

        try {
            const classification =
                await classifyArchiveEntries(
                    session.entries);

            return {
                canPlay: classification.canPlay,
                sourceType: "archive",
                backend: classification.backend,
                entryCount: session.entries.length,
                trackCount:
                    classification.backend === "browser"
                        ? classification.browserCandidates.length
                        : null
            };
        }
        finally {
            await session.close();
        }
    }

    async function probeDirect(url) {
        const browserCandidate =
            window.datinateBrowserAudio
                .canProbablyPlayPath(url);

        const browserCanPlay =
            browserCandidate &&
            await window.datinateBrowserAudio
                .probeUrl(url);

        if (browserCanPlay) {
            return {
                canPlay: true,
                sourceType: "file",
                backend: "browser",
                trackCount: 1
            };
        }

        const vgmCanPlay =
            await window.datinateVgm
                .canPlayPath(url);

        return {
            canPlay: vgmCanPlay,
            sourceType: "file",
            backend: vgmCanPlay
                ? "vgm"
                : null,
            trackCount: vgmCanPlay
                ? 1
                : 0
        };
    }

    async function probe(url) {
        try {
            if (!url)
                return {
                    canPlay: false
                };

            const result =
                window.datinateArchive
                    .isArchivePath(url)
                    ? await probeArchive(url)
                    : await probeDirect(url);

            return {
                ...result,
                source: url
            };
        }
        catch (error) {
            console.warn(
                "[DATINATE AUDIO] Probe failed:",
                error);

            return {
                canPlay: false,
                source: url,
                reason: "probe-failed",
                error:
                    error?.message ||
                    String(error)
            };
        }
    }

    async function probeAndPost(url) {
        const result =
            await probe(url);

        post("probe-result", result);
        return result;
    }

    async function closeArchiveSession() {
        if (!archiveSession)
            return;

        try {
            await archiveSession.close();
        }
        catch {
        }

        archiveSession = null;
    }

    async function stopCurrentBackend() {
        if (currentBackend === "browser") {
            window.datinateBrowserAudio.stop();
        }
        else if (currentBackend === "vgm") {
            try {
                await window.datinateVgm.stop();
            }
            catch {
            }
        }
    }

    async function reset() {
        operationVersion++;
        seekSession = null;

        await stopCurrentBackend();
        window.datinateBrowserAudio.unload();
        await closeArchiveSession();

        sourceUrl = null;
        sourceTitle = "";
        playlist = [];
        currentTrackIndex = -1;
        loadedTrackIndex = -1;
        currentBackend = null;
        previewMode = false;
    }

    function buildBrowserArchivePlaylist(entries) {
        return entries.map(entry => ({
            title: cleanTitle(entry.path),
            durationSeconds: null,
            backend: "browser",
            archivePath: entry.path
        }));
    }

    function buildVgmPlaylist(tracks) {
        return tracks.map(track => ({
            title: track.title,
            durationSeconds:
                track.durationSeconds ?? null,
            seekable: !!track.seekable,
            backend: "vgm",
            vgmIndex: track.index
        }));
    }

    function buildPreviewPlaylist(paths) {
        return (paths || []).map(path => ({
            title:
                String(path || "")
                    .replaceAll("\\", "/")
                    .split("/")
                    .pop() || String(path || ""),
            durationSeconds: null,
            backend: "preview"
        }));
    }

    async function loadArchive(url) {
        archiveSession =
            await window.datinateArchive.open(url);

        const classification =
            await classifyArchiveEntries(
                archiveSession.entries);

        if (!classification.canPlay)
            return false;

        if (classification.backend === "browser") {
            currentBackend = "browser";

            setPlaylist(
                buildBrowserArchivePlaylist(
                    classification.browserCandidates));

            return playlist.length > 0;
        }

        const vgmArchiveFile =
            await window.datinateArchive
                .prepareVgmArchive(
                    archiveSession);

        const vgmTracks =
            await window.datinateVgm
                .loadArchiveFile(
                    vgmArchiveFile);

        currentBackend = "vgm";
        setPlaylist(
            buildVgmPlaylist(vgmTracks));

        await closeArchiveSession();
        return playlist.length > 0;
    }

    async function loadDirect(url) {
        const browserCandidate =
            window.datinateBrowserAudio
                .canProbablyPlayPath(url);

        const browserCanPlay =
            browserCandidate &&
            await window.datinateBrowserAudio
                .probeUrl(url);

        if (browserCanPlay) {
            currentBackend = "browser";

            window.datinateBrowserAudio
                .loadUrl(url);

            setPlaylist([{
                title: cleanTitle(getSourceName(url)),
                durationSeconds: null,
                backend: "browser",
                url
            }]);

            loadedTrackIndex = 0;
            return true;
        }

        const vgmCanPlay =
            await window.datinateVgm
                .canPlayPath(url);

        if (!vgmCanPlay)
            return false;

        const vgmTracks =
            await window.datinateVgm
                .loadDirectUrl(url);

        currentBackend = "vgm";
        setPlaylist(
            buildVgmPlaylist(vgmTracks));

        return playlist.length > 0;
    }

    async function load(url) {
        try {
            await reset();

            sourceUrl = url;
            sourceTitle =
                window.datinateArchive
                    .isArchivePath(url)
                    ? cleanTitle(
                        getSourceName(url))
                    : "";

            const loaded =
                window.datinateArchive
                    .isArchivePath(url)
                    ? await loadArchive(url)
                    : await loadDirect(url);

            if (!loaded) {

                return false;
            }

            await setVolume(volume);
            return true;
        }
        catch (error) {
            console.error(
                "[DATINATE AUDIO] Load failed:",
                error);

            return false;
        }
    }

    async function loadPreviewArchive(url) {
        const session =
            await window.datinateArchive.open(url);

        try {
            const browserCandidates =
                getBrowserArchiveCandidates(
                    session.entries);

            if (browserCandidates.length > 0) {
                setPlaylist(
                    buildPreviewPlaylist(
                        browserCandidates.map(entry =>
                            entry.path)));

                return playlist.length > 0;
            }

            const playablePaths =
                await window.datinateVgm
                    .getPlayablePaths(
                        session.entries.map(entry =>
                            entry.path));

            setPlaylist(
                buildPreviewPlaylist(
                    playablePaths));

            return playlist.length > 0;
        }
        finally {
            await session.close();
        }
    }

    async function loadPreviewDirect(url) {
        if (window.datinateBrowserAudio
            .canProbablyPlayPath(url)) {
            setPlaylist(
                buildPreviewPlaylist([
                    getSourceName(url)
                ]));

            return true;
        }

        const vgmCanPlay =
            await window.datinateVgm
                .canPlayPath(url);

        if (!vgmCanPlay)
            return false;

        setPlaylist(
            buildPreviewPlaylist([
                getSourceName(url)
            ]));

        return true;
    }

    async function loadPreview(url) {
        try {
            await reset();

            previewMode = true;
            sourceUrl = url;
            sourceTitle =
                window.datinateArchive
                    .isArchivePath(url)
                    ? cleanTitle(
                        getSourceName(url))
                    : "";

            const loaded =
                window.datinateArchive
                    .isArchivePath(url)
                    ? await loadPreviewArchive(url)
                    : await loadPreviewDirect(url);

            if (!loaded)
                return false;

            currentBackend = null;
            loadedTrackIndex = -1;
            return true;
        }
        catch (error) {
            console.error(
                "[DATINATE AUDIO] Preview load failed:",
                error);

            return false;
        }
    }

    async function prepareBrowserTrack(track, version) {
        if (track.archivePath) {
            if (!archiveSession)
                throw new Error(
                    "Archive session is no longer available.");

            const file =
                await archiveSession.extract(
                    track.archivePath);

            if (version !== operationVersion)
                return false;

            window.datinateBrowserAudio
                .loadFile(file);
        }
        else if (track.url) {
            window.datinateBrowserAudio
                .loadUrl(track.url);
        }

        return true;
    }

    async function playTrack(index) {
        if (index < 0 ||
            index >= playlist.length) {
            return false;
        }

        if (previewMode) {
            currentTrackIndex = index;
            loadedTrackIndex = -1;
            seekSession = null;
            return true;
        }

        const version =
            ++operationVersion;

        seekSession = null;

        await stopCurrentBackend();

        if (version !== operationVersion)
            return false;

        currentTrackIndex = index;
        loadedTrackIndex = -1;

        const track =
            playlist[index];

        if (track.backend === "browser") {
            currentBackend = "browser";

            const prepared =
                await prepareBrowserTrack(
                    track,
                    version);

            if (!prepared ||
                version !== operationVersion) {
                return false;
            }

            loadedTrackIndex = index;

            await window.datinateBrowserAudio
                .play();
        }
        else if (track.backend === "vgm") {
            currentBackend = "vgm";

            const played =
                await window.datinateVgm
                    .playTrack(
                        track.vgmIndex);

            if (!played ||
                version !== operationVersion) {
                return false;
            }

            loadedTrackIndex = index;
        }
        else {
            return false;
        }
        return true;
    }

    async function play() {
        if (previewMode)
            return false;

        if (currentTrackIndex < 0 &&
            playlist.length > 0) {
            currentTrackIndex = 0;
        }

        if (currentTrackIndex < 0)
            return false;

        const track =
            playlist[currentTrackIndex];

        if (loadedTrackIndex !== currentTrackIndex) {
            return playTrack(
                currentTrackIndex);
        }

        if (track.backend === "browser") {
            currentBackend = "browser";
            await window.datinateBrowserAudio.play();
        }
        else if (track.backend === "vgm") {
            currentBackend = "vgm";
            await window.datinateVgm.play();
        }
        else {
            return false;
        }
        return true;
    }

    async function pauseCurrentTrackBackend() {
        const track =
            playlist[currentTrackIndex];

        if (!track)
            return false;

        if (track.backend === "browser") {
            window.datinateBrowserAudio.pause();
            return true;
        }

        if (track.backend === "vgm") {
            await window.datinateVgm.pause();
            return true;
        }

        return false;
    }

    async function resumeCurrentTrackBackend() {
        const track =
            playlist[currentTrackIndex];

        if (!track ||
            loadedTrackIndex !== currentTrackIndex) {
            return false;
        }

        if (track.backend === "browser") {
            await window.datinateBrowserAudio.play();
            return true;
        }

        if (track.backend === "vgm") {
            return await window.datinateVgm.play();
        }

        return false;
    }

    async function pause() {
        if (previewMode)
            return;

        if (seekSession)
            seekSession.wasPlaying = false;

        await pauseCurrentTrackBackend();
    }

    async function stop() {
        seekSession = null;

        if (previewMode)
            return;

        await stopCurrentBackend();
        loadedTrackIndex = -1;
    }

    async function next() {
        if (playlist.length === 0)
            return false;

        const nextIndex =
            currentTrackIndex < 0
                ? 0
                : (currentTrackIndex + 1) % playlist.length;

        return playTrack(nextIndex);
    }

    async function previous() {
        if (playlist.length === 0)
            return false;

        const previousIndex =
            currentTrackIndex <= 0
                ? playlist.length - 1
                : currentTrackIndex - 1;

        return playTrack(previousIndex);
    }

    async function handleEnded() {
        if (playlist.length === 0)
            return;

        seekSession = null;

        /*
         * Natural completion only advances while Repeat Playlist
         * is enabled. When disabled, leave the completed track
         * selected and stop there.
         */
        if (!repeatPlaylist) {
            await pauseCurrentTrackBackend();
            return;
        }

        const nextIndex =
            currentTrackIndex < playlist.length - 1
                ? currentTrackIndex + 1
                : 0;

        await playTrack(nextIndex);
    }

    async function seek(seconds) {
        if (previewMode)
            return false;

        const track =
            playlist[currentTrackIndex];

        if (!track)
            return false;

        let result = false;

        if (track.backend === "browser") {
            result =
                window.datinateBrowserAudio
                    .seek(seconds);
        }
        else if (track.backend === "vgm") {
            result =
                await window.datinateVgm
                    .seek(seconds);
        }
        return result;
    }

    async function seekRatio(ratio) {
        const state =
            getState();

        if (!state.canSeek ||
            state.durationSeconds == null) {
            return false;
        }

        ratio =
            Math.max(
                0,
                Math.min(
                    1,
                    Number(ratio) || 0));

        return seek(
            state.durationSeconds * ratio);
    }

    async function seekRatioAndPlay(ratio) {
        const state =
            getState();

        if (!state.canSeek ||
            state.durationSeconds == null) {
            return false;
        }

        ratio =
            Math.max(
                0,
                Math.min(
                    1,
                    Number(ratio) || 0));

        const targetSeconds =
            state.durationSeconds * ratio;

        let startedForSeek = false;

        if (!state.loaded) {
            const started =
                await playTrack(
                    currentTrackIndex);

            if (!started)
                return false;

            startedForSeek = true;
        }

        const seeked =
            await seek(targetSeconds);

        if (!seeked) {
            if (startedForSeek)
                await stop();

            return false;
        }

        return await resumeCurrentTrackBackend();
    }

    async function beginSeek() {
        const state =
            getState();

        if (!state.canSeek)
            return false;

        if (seekSession)
            return true;

        seekSession = {
            wasPlaying: state.playing
        };

        if (seekSession.wasPlaying)
            await pauseCurrentTrackBackend();
        return true;
    }

    async function endSeekRatio(ratio) {
        const seeked =
            await seekRatioAndPlay(ratio);

        seekSession = null;
        return seeked;
    }

    async function setRepeatPlaylist(enabled) {
        repeatPlaylist = !!enabled;

        /*
         * Enabling Repeat Playlist is also an explicit request to
         * start/resume playback from the current track position.
         */
        if (repeatPlaylist &&
            !previewMode) {
            await play();
        }
        return repeatPlaylist;
    }

    async function toggleRepeatPlaylist() {
        return await setRepeatPlaylist(
            !repeatPlaylist);
    }

    async function setVolume(value) {
        volume =
            Math.max(
                0,
                Math.min(
                    1,
                    Number(value) || 0));

        if (previewMode)
            return volume;

        window.datinateBrowserAudio
            .setVolume(volume);

        if (currentBackend === "vgm") {
            try {
                await window.datinateVgm
                    .setVolume(volume);
            }
            catch {
            }
        }
        return volume;
    }

    function getBackendState(track) {
        if (!track) {
            return {
                loaded: false,
                playing: false,
                paused: false,
                positionSeconds: 0,
                durationSeconds: null,
                canSeek: false,
                volume
            };
        }

        if (track.backend === "browser") {
            return window.datinateBrowserAudio
                .getState();
        }

        if (track.backend === "vgm") {
            return window.datinateVgm
                .getState();
        }

        return {
            loaded: false,
            playing: false,
            paused: false,
            positionSeconds: 0,
            durationSeconds: null,
            canSeek: false,
            volume
        };
    }

    function rememberCurrentTrackDuration(backendState) {
        const track =
            playlist[currentTrackIndex];

        if (!track)
            return;

        const duration =
            Number(backendState?.durationSeconds);

        if (!Number.isFinite(duration) ||
            duration <= 0) {
            return;
        }

        track.durationSeconds =
            duration;
    }

    function getState() {
        const track =
            playlist[currentTrackIndex] || null;

        const backendState =
            getBackendState(track);

        rememberCurrentTrackDuration(
            backendState);

        const durationSeconds =
            backendState.durationSeconds ??
            track?.durationSeconds ??
            null;

        const canSeek =
            !!backendState.canSeek ||
            (
                track?.backend === "vgm" &&
                !!track.seekable &&
                Number(durationSeconds) > 0
            );

        return {
            source: sourceUrl,
            sourceTitle,
            trackIndex: currentTrackIndex,
            trackCount: playlist.length,
            title: track?.title || "",
            loaded:
                loadedTrackIndex === currentTrackIndex &&
                !!backendState.loaded,
            playing:
                seekSession?.wasPlaying
                    ? true
                    : !!backendState.playing,
            paused:
                seekSession?.wasPlaying
                    ? false
                    : !!backendState.paused,
            seeking: !!seekSession,
            preview: previewMode,
            repeatPlaylist,
            positionSeconds:
                Number(backendState.positionSeconds) || 0,
            durationSeconds,
            canSeek,
            volume
        };
    }

    window.datinateBrowserAudio
        .setEndedHandler(handleEnded);

    window.datinateVgm
        .setEndedHandler(handleEnded);

    window.datinateAudio = {
        probe,
        probeAndPost,
        load,
        loadPreview,
        getPlaylist,
        getState,
        play,
        pause,
        stop,
        next,
        previous,
        playTrack,
        seek,
        seekRatio,
        seekRatioAndPlay,
        beginSeek,
        endSeekRatio,
        setRepeatPlaylist,
        toggleRepeatPlaylist,
        setVolume,
        reset
    };

    post("facade-ready");
})();
