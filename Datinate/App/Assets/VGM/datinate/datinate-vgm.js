(function () {
    const vgmBase =
        new URL(
            "../vgmplay/extension/",
            window.location.href);

    let instancePromise = null;
    let playbackPromise = null;
    let playlist = [];
    let selectedTrackIndex = -1;
    let endedHandler = null;

    function getSourceName(url) {
        try {
            const parsed =
                new URL(url, window.location.href);

            return decodeURIComponent(
                parsed.pathname.split("/").pop() ||
                "audio");
        }
        catch {
            return "audio";
        }
    }

    function loadScript(url) {
        return new Promise((resolve, reject) => {
            const script =
                document.createElement("script");

            script.src = url;
            script.async = true;

            script.onload = () => resolve();
            script.onerror = () => reject(
                new Error(
                    `Unable to load VGMPlay script: ${url}`));

            document.head.appendChild(script);
        });
    }

    async function waitForInstance() {
        for (let i = 0; i < 600; i++) {
            const vgm =
                window.vgmPlayInstance;

            if (vgm &&
                typeof vgm.isPlayable === "function") {
                await configureEphemeralSession(vgm);
                installHooks(vgm);
                return vgm;
            }

            await new Promise(resolve =>
                setTimeout(resolve, 50));
        }

        throw new Error(
            "Timed out waiting for VGMPlay instance.");
    }

    async function configureEphemeralSession(vgm) {
        if (vgm.__datinateEphemeralSessionConfigured)
            return;

        vgm.__datinateEphemeralSessionConfigured = true;

        /*
         * VGMPlay's normal web player maintains a persistent
         * library/cache in IndexedDB/IDBFS. Datinate owns the
         * media lifecycle instead, so restored games must never
         * leak into a later Datinate player session.
         *
         * Disable persistence before checkEverythingReady() can
         * start VGMPlay's background cache restore.
         */
        const existingCacheInit =
            vgm._cacheInitPromise || null;

        vgm._initCache = null;
        vgm._saveCache = null;
        vgm._isCached = null;
        vgm._markCached = null;
        vgm._hasCachedSourceMetadata = null;

        if (existingCacheInit) {
            try {
                await existingCacheInit;
            }
            catch {
            }
        }

        vgm._cacheReady = false;
        vgm._cacheInitPromise = null;

        resetLibraryState(vgm);
    }

    function resetLibraryState(vgm) {
        if (!vgm)
            return;

        try {
            if (vgm.isVGMLoaded ||
                vgm.isVGMPlaying) {
                vgm.stop();
            }
        }
        catch {
        }

        removeTransientGameFiles();

        vgm.games = [];
        vgm.activeGame = null;
        vgm.currentFileKey = -1;
        vgm.amountOfGamesLoaded = 0;
        vgm.zipURLLoaded = [];
        vgm.zipURLPending = [];
        vgm._processedURLs = new Set();
        vgm._cacheFingerprints = new Set();
        vgm._cacheArchiveNames = new Set();

        playlist = [];
        selectedTrackIndex = -1;
    }

    function removeTransientGameFiles() {
        if (typeof FS === "undefined")
            return;

        let rootEntries;

        try {
            rootEntries = FS.readdir("/");
        }
        catch {
            return;
        }

        for (const entry of rootEntries) {
            if (!entry.startsWith("game_"))
                continue;

            removeFsPathRecursively(
                "/" + entry);
        }
    }

    function removeFsPathRecursively(path) {
        try {
            const stat =
                FS.stat(path);

            if (!FS.isDir(stat.mode)) {
                FS.unlink(path);
                return;
            }

            const entries =
                FS.readdir(path);

            for (const entry of entries) {
                if (entry === "." ||
                    entry === "..") {
                    continue;
                }

                removeFsPathRecursively(
                    path + "/" + entry);
            }

            FS.rmdir(path);
        }
        catch {
        }
    }

    function installHooks(vgm) {
        if (vgm.__datinateHooksInstalled)
            return;

        vgm.__datinateHooksInstalled = true;
        vgm.__datinateVolume = 1;
        vgm.loopMode = 0;
        vgm.isRandomEnabled = false;

        vgm._masterGainTarget = function () {
            return Math.max(
                0,
                Math.min(
                    1,
                    Number(this.__datinateVolume) || 0));
        };

        const originalChangeTrack =
            vgm.changeTrack.bind(vgm);

        vgm.__datinateOriginalChangeTrack =
            originalChangeTrack;

        vgm.changeTrack = async function (action) {
            if (action === "next" &&
                this.__datinateOwnsPlaylist !== false) {
                if (typeof endedHandler === "function")
                    endedHandler();

                return;
            }

            return originalChangeTrack(action);
        };

        vgm.__datinateOwnsPlaylist = true;
    }

    async function ensureInstance() {
        if (!instancePromise) {
            instancePromise = (async () => {
                if (!window.vgmPlayInstance) {
                    window.__VGM_DEBUG__ = false;

                    try {
                        localStorage.removeItem(
                            "vgm_debug_mode");
                    }
                    catch {
                    }

                    window.VGMPLAY_EXTENSION_OPTIONS = {
                        useAsLibrary: true,
                        autoScanDist: false,
                        moduleSet: "web",
                        baseURL: vgmBase.href
                    };

                    await loadScript(
                        new URL(
                            "vgmplay-js-glue.js",
                            vgmBase).href);
                }

                return waitForInstance();
            })();
        }

        return instancePromise;
    }

    async function ensurePlaybackReady() {
        if (!playbackPromise) {
            playbackPromise = (async () => {
                const vgm =
                    await ensureInstance();

                if (typeof vgm.checkEverythingReady !== "function") {
                    throw new Error(
                        "VGMPlay is missing checkEverythingReady().");
                }

                await vgm.checkEverythingReady();
                installHooks(vgm);
                return vgm;
            })();
        }

        return playbackPromise;
    }

    async function resumeAudioContext(vgm) {
        if (vgm.context?.state === "suspended") {
            try {
                await vgm.context.resume();
            }
            catch {
            }
        }
    }

    async function canPlayPath(path) {
        try {
            const vgm =
                await ensureInstance();

            return !!vgm.isPlayable(path);
        }
        catch (error) {
            console.warn(
                "[DATINATE VGM] Capability check failed:",
                error);

            return false;
        }
    }

    async function canPlayAny(paths) {
        try {
            const vgm =
                await ensureInstance();

            return (paths || []).some(path =>
                vgm.isPlayable(path));
        }
        catch (error) {
            console.warn(
                "[DATINATE VGM] Archive capability check failed:",
                error);

            return false;
        }
    }

    async function getPlayablePaths(paths) {
        try {
            const vgm =
                await ensureInstance();

            return (paths || [])
                .filter(path =>
                    vgm.isPlayable(path));
        }
        catch (error) {
            console.warn(
                "[DATINATE VGM] Playable-path query failed:",
                error);

            return [];
        }
    }

    function getPlayableList(vgm, game) {
        if (!game)
            return [];

        if (game.playableList?.length)
            return game.playableList;

        return (game.files || [])
            .filter(file =>
                file?.filepath &&
                vgm.isPlayable(file.filepath))
            .map(file => ({
                filepath: file.filepath,
                title: file.title || null,
                lengthSec: file.lengthSec || 0
            }));
    }

    function cleanTitle(value) {
        let path =
            String(value || "");

        const trackMarker =
            path.indexOf("|track=");

        if (trackMarker >= 0)
            path = path.substring(0, trackMarker);

        return path
            .replaceAll("\\", "/")
            .split("/")
            .pop() || path;
    }

    function buildPlaylist(vgm, games) {
        const result = [];

        for (const game of games) {
            const gameIndex =
                vgm.games.indexOf(game);

            const playable =
                getPlayableList(vgm, game);

            playable.forEach((entry, trackIndex) => {
                result.push({
                    title:
                        entry.title ||
                        cleanTitle(entry.filepath),

                    gameName:
                        game.name || "",

                    durationSeconds:
                        Number(entry.lengthSec) > 0
                            ? Number(entry.lengthSec)
                            : null,

                    seekable:
                        Number(entry.lengthSec) > 0 &&
                        typeof vgm.SeekVGM === "function" &&
                        !(
                            typeof vgm._isUsfFile === "function" &&
                            vgm._isUsfFile(
                                String(entry.filepath).toLowerCase())
                        ),

                    gameIndex,
                    trackIndex,
                    filepath: entry.filepath
                });
            });
        }

        return result;
    }

    async function loadArchiveFile(file) {
        const vgm =
            await ensurePlaybackReady();

        resetLibraryState(vgm);

        const gamesBefore =
            vgm.games?.length || 0;

        const bytes =
            new Uint8Array(
                await file.arrayBuffer());

        await vgm.processZipBuffer(
            bytes,
            file.name);

        const addedGames =
            (vgm.games || []).slice(gamesBefore);

        playlist =
            buildPlaylist(
                vgm,
                addedGames);

        selectedTrackIndex =
            playlist.length > 0
                ? 0
                : -1;

        return getPlaylist();
    }

    async function loadDirectUrl(url) {
        const vgm =
            await ensurePlaybackReady();

        resetLibraryState(vgm);

        const response =
            await fetch(
                url,
                {
                    cache: "no-store"
                });

        if (!response.ok) {
            throw new Error(
                `VGM source fetch failed. HTTP ${response.status}.`);
        }

        const gamesBefore =
            vgm.games?.length || 0;

        const bytes =
            new Uint8Array(
                await response.arrayBuffer());

        await vgm.processSingleBuffer(
            bytes,
            getSourceName(url));

        const addedGames =
            (vgm.games || []).slice(gamesBefore);

        playlist =
            buildPlaylist(
                vgm,
                addedGames);

        selectedTrackIndex =
            playlist.length > 0
                ? 0
                : -1;

        return getPlaylist();
    }

    async function playTrack(index) {
        const track =
            playlist[index];

        if (!track)
            return false;

        const vgm =
            await ensurePlaybackReady();

        await resumeAudioContext(vgm);

        if (vgm.isVGMLoaded ||
            vgm.isVGMPlaying) {
            try {
                vgm.stop();
            }
            catch {
            }
        }

        selectedTrackIndex = index;
        vgm.activeGame =
            vgm.games[track.gameIndex];
        vgm.currentFileKey =
            track.trackIndex;

        await vgm.playFileFromFS(
            false,
            track.filepath,
            track.gameIndex + 1,
            track.trackIndex);
        return true;
    }

    async function play() {
        const vgm =
            await ensurePlaybackReady();

        await resumeAudioContext(vgm);

        if (vgm.isVGMLoaded &&
            vgm.isPlaybackPaused) {
            vgm.play();
            return true;
        }

        if (!vgm.isVGMLoaded &&
            selectedTrackIndex >= 0) {
            return playTrack(
                selectedTrackIndex);
        }

        return !!vgm.isVGMLoaded;
    }

    async function pause() {
        const vgm =
            await ensurePlaybackReady();

        if (vgm.isVGMLoaded &&
            !vgm.isPlaybackPaused) {
            vgm.pause();
        }
    }

    async function stop() {
        const vgm =
            await ensurePlaybackReady();

        if (vgm.isVGMLoaded ||
            vgm.isVGMPlaying) {
            vgm.stop();
        }
    }

    function getCurrentTrack(vgm) {
        if (selectedTrackIndex < 0)
            return null;

        return playlist[selectedTrackIndex] || null;
    }

    function getDurationSeconds(vgm) {
        const duration =
            Number(vgm.trackLengthSeconds);

        if (Number.isFinite(duration) &&
            duration > 0) {
            return duration;
        }

        const track =
            getCurrentTrack(vgm);

        return Number(track?.durationSeconds) > 0
            ? Number(track.durationSeconds)
            : null;
    }

    function getPositionSeconds(vgm) {
        const sampleRate =
            Number(vgm.sampleRate);

        if (!vgm.isVGMLoaded ||
            !Number.isFinite(sampleRate) ||
            sampleRate <= 0) {
            return 0;
        }

        let sample =
            Number(vgm.visualSamplePosition) || 0;

        if (vgm.isVGMPlaying &&
            !vgm.isPlaybackPaused &&
            vgm.context) {
            const elapsed =
                Math.max(
                    0,
                    vgm.context.currentTime -
                    (Number(vgm.playbackStartTime) || 0));

            sample =
                (Number(vgm.startSample) || 0) +
                elapsed * sampleRate;
        }

        const duration =
            getDurationSeconds(vgm);

        let seconds =
            Math.max(
                0,
                sample / sampleRate);

        if (duration != null)
            seconds = Math.min(seconds, duration);

        return seconds;
    }

    function canSeek(vgm) {
        const track =
            getCurrentTrack(vgm);

        if (!track ||
            !vgm.isVGMLoaded ||
            typeof vgm.SeekVGM !== "function" ||
            getDurationSeconds(vgm) == null) {
            return false;
        }

        if (typeof vgm._isUsfFile === "function" &&
            vgm._isUsfFile(
                String(track.filepath).toLowerCase())) {
            return false;
        }

        return true;
    }

    async function seek(seconds) {
        const vgm =
            await ensurePlaybackReady();

        if (!canSeek(vgm))
            return false;

        const duration =
            getDurationSeconds(vgm);

        let target =
            Number(seconds);

        if (!Number.isFinite(target))
            return false;

        target =
            Math.max(
                0,
                Math.min(
                    duration,
                    target));

        const sampleRate =
            Number(vgm.sampleRate);

        const targetSample =
            Math.floor(
                target * sampleRate);

        const seekSecond =
            Math.floor(
                targetSample / sampleRate);

        const seekMilliseconds =
            Math.round(
                (
                    targetSample / sampleRate -
                    seekSecond
                ) * 1000);

        vgm._lastSeekAt =
            performance.now();
        vgm._lastSeekWasMUS =
            false;

        vgm.SeekVGM(
            seekSecond,
            seekMilliseconds);

        vgm.samplesGenerated =
            targetSample;
        vgm.visualSamplePosition =
            targetSample;
        vgm.startSample =
            targetSample;
        vgm.emulatorFinished =
            false;
        vgm.isFadingOut =
            false;

        if (vgm.context &&
            !vgm.isPlaybackPaused) {
            vgm.playbackStartTime =
                vgm.context.currentTime;
        }

        if (vgm.workletNode) {
            vgm.workletNode.port.postMessage({
                type: "stop"
            });

            vgm.workletNode.port.postMessage({
                type: "start"
            });

            vgm._pumpBuffers();
        }
        return true;
    }

    async function setVolume(value) {
        const vgm =
            await ensurePlaybackReady();

        value =
            Math.max(
                0,
                Math.min(
                    1,
                    Number(value) || 0));

        vgm.__datinateVolume = value;

        if (vgm.masterGain &&
            vgm.context &&
            !vgm.isFadingOut) {
            const now =
                vgm.context.currentTime;

            vgm.masterGain.gain
                .cancelScheduledValues(now);

            vgm.masterGain.gain
                .setTargetAtTime(
                    value,
                    now,
                    0.01);
        }
        return value;
    }

    function getState() {
        const vgm =
            window.vgmPlayInstance;

        if (!vgm) {
            return {
                loaded: false,
                playing: false,
                paused: false,
                positionSeconds: 0,
                durationSeconds: null,
                canSeek: false,
                volume: 1
            };
        }

        return {
            loaded: !!vgm.isVGMLoaded,
            playing:
                !!vgm.isVGMPlaying &&
                !vgm.isPlaybackPaused,
            paused: !!vgm.isPlaybackPaused,
            positionSeconds:
                getPositionSeconds(vgm),
            durationSeconds:
                getDurationSeconds(vgm),
            canSeek:
                canSeek(vgm),
            volume:
                Number(vgm.__datinateVolume) || 0
        };
    }

    function getPlaylist() {
        return playlist.map((track, index) => ({
            index,
            title: track.title,
            gameName: track.gameName,
            durationSeconds: track.durationSeconds,
            seekable: !!track.seekable
        }));
    }

    function setEndedHandler(handler) {
        endedHandler = handler;
    }

    window.datinateVgm = {
        canPlayPath,
        canPlayAny,
        getPlayablePaths,
        loadArchiveFile,
        loadDirectUrl,
        playTrack,
        play,
        pause,
        stop,
        seek,
        setVolume,
        getState,
        getPlaylist,
        setEndedHandler
    };
})();
