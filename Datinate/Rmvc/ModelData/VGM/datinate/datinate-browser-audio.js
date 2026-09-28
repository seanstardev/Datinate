(function () {
    const audio =
        document.createElement("audio");

    audio.preload = "metadata";
    audio.style.display = "none";
    document.body.appendChild(audio);

    let objectUrl = null;
    let endedHandler = null;

    const mimeByExtension = new Map([
        [".mp3", "audio/mpeg"],
        [".wav", "audio/wav"],
        [".wave", "audio/wav"],
        [".flac", "audio/flac"],
        [".ogg", "audio/ogg"],
        [".oga", "audio/ogg"],
        [".opus", "audio/ogg; codecs=opus"],
        [".aac", "audio/aac"],
        [".m4a", "audio/mp4"],
        [".mp4", "audio/mp4"],
        [".webm", "audio/webm"],
        [".weba", "audio/webm"]
    ]);

    function getExtension(value) {
        let path = String(value || "")
            .split("?")[0]
            .split("#")[0]
            .toLowerCase();

        const slash =
            Math.max(
                path.lastIndexOf("/"),
                path.lastIndexOf("\\"));

        if (slash >= 0)
            path = path.substring(slash + 1);

        const dot =
            path.lastIndexOf(".");

        return dot >= 0
            ? path.substring(dot)
            : "";
    }

    function canProbablyPlayPath(value) {
        const extension =
            getExtension(value);

        const mime =
            mimeByExtension.get(extension);

        if (!mime)
            return false;

        return audio.canPlayType(mime) !== "";
    }

    function waitForMetadata(testAudio, timeoutMs) {
        return new Promise(resolve => {
            let finished = false;

            function complete(result) {
                if (finished)
                    return;

                finished = true;
                cleanup();
                resolve(result);
            }

            function cleanup() {
                clearTimeout(timer);
                testAudio.removeEventListener(
                    "loadedmetadata",
                    onSuccess);
                testAudio.removeEventListener(
                    "canplay",
                    onSuccess);
                testAudio.removeEventListener(
                    "error",
                    onError);
            }

            function onSuccess() {
                complete(true);
            }

            function onError() {
                complete(false);
            }

            const timer =
                setTimeout(
                    () => complete(false),
                    timeoutMs);

            testAudio.addEventListener(
                "loadedmetadata",
                onSuccess,
                { once: true });

            testAudio.addEventListener(
                "canplay",
                onSuccess,
                { once: true });

            testAudio.addEventListener(
                "error",
                onError,
                { once: true });
        });
    }

    async function probeUrl(url) {
        const testAudio =
            document.createElement("audio");

        testAudio.preload = "metadata";

        try {
            const resultPromise =
                waitForMetadata(
                    testAudio,
                    5000);

            testAudio.src = url;
            testAudio.load();

            return await resultPromise;
        }
        catch {
            return false;
        }
        finally {
            testAudio.removeAttribute("src");
            testAudio.load();
        }
    }

    function revokeObjectUrl() {
        if (!objectUrl)
            return;

        URL.revokeObjectURL(objectUrl);
        objectUrl = null;
    }

    function unload() {
        audio.pause();
        audio.removeAttribute("src");
        audio.load();
        revokeObjectUrl();
    }

    function loadUrl(url) {
        unload();
        audio.src = url;
        audio.load();
    }

    function loadFile(file) {
        unload();

        objectUrl =
            URL.createObjectURL(file);

        audio.src = objectUrl;
        audio.load();
    }

    async function play() {
        await audio.play();
    }

    function pause() {
        audio.pause();
    }

    function stop() {
        audio.pause();

        try {
            audio.currentTime = 0;
        }
        catch {
        }
    }

    function seek(seconds) {
        const duration =
            Number.isFinite(audio.duration)
                ? audio.duration
                : null;

        let value =
            Number(seconds);

        if (!Number.isFinite(value))
            return false;

        value = Math.max(0, value);

        if (duration != null)
            value = Math.min(duration, value);

        try {
            audio.currentTime = value;
            return true;
        }
        catch {
            return false;
        }
    }

    function setVolume(value) {
        value =
            Math.max(
                0,
                Math.min(
                    1,
                    Number(value) || 0));

        audio.volume = value;
        return value;
    }

    function getState() {
        const duration =
            Number.isFinite(audio.duration)
                ? audio.duration
                : null;

        const position =
            Number.isFinite(audio.currentTime)
                ? audio.currentTime
                : 0;

        return {
            loaded: !!audio.src,
            playing: !audio.paused && !audio.ended,
            paused: audio.paused && !!audio.src,
            positionSeconds: position,
            durationSeconds: duration,
            canSeek: duration != null && duration > 0,
            volume: audio.volume
        };
    }

    function setEndedHandler(handler) {
        endedHandler = handler;
    }

    audio.addEventListener("ended", () => {
        if (typeof endedHandler === "function")
            endedHandler();
    });

    window.datinateBrowserAudio = {
        canProbablyPlayPath,
        probeUrl,
        loadUrl,
        loadFile,
        unload,
        play,
        pause,
        stop,
        seek,
        setVolume,
        getState,
        setEndedHandler
    };
})();
