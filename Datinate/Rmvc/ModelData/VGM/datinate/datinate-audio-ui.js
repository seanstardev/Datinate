(function () {
    const sourceTitleElement =
        document.getElementById("sourceTitle");

    const titleElement =
        document.getElementById("trackTitle");

    const statusElement =
        document.getElementById("status");

    const playlistElement =
        document.getElementById("playlist");

    const repeatButton =
        document.getElementById("repeatButton");

    const previousButton =
        document.getElementById("previousButton");

    const playPauseButton =
        document.getElementById("playPauseButton");

    const playPauseIcon =
        document.getElementById("playPauseIcon");

    const stopButton =
        document.getElementById("stopButton");

    const nextButton =
        document.getElementById("nextButton");

    const progress =
        document.getElementById("progress");

    const elapsedElement =
        document.getElementById("elapsed");

    const durationElement =
        document.getElementById("duration");

    const volume =
        document.getElementById("volume");

    let userSeeking = false;
    let seekPointerId = null;
    let lastRenderedTrackIndex = -2;

    function formatTime(seconds) {
        if (!Number.isFinite(seconds) ||
            seconds < 0) {
            return "--:--";
        }

        const whole =
            Math.floor(seconds);

        const minutes =
            Math.floor(whole / 60);

        const remaining =
            whole % 60;

        return `${minutes}:${remaining
            .toString()
            .padStart(2, "0")}`;
    }

    function getSource() {
        return new URLSearchParams(
            window.location.search)
            .get("source");
    }

    function isPreviewMode() {
        const value =
            new URLSearchParams(
                window.location.search)
                .get("preview");

        return value === "1" ||
            value === "true";
    }

    function renderPlaylist() {
        const tracks =
            window.datinateAudio
                .getPlaylist();

        playlistElement.replaceChildren();

        tracks.forEach(track => {
            const button =
                document.createElement("button");

            button.type = "button";
            button.className = "track";
            button.dataset.index =
                String(track.index);

            const number =
                document.createElement("span");

            number.className =
                "track-number";

            number.textContent =
                String(track.index + 1)
                    .padStart(2, "0");

            const title =
                document.createElement("span");

            title.className =
                "track-name";

            title.textContent =
                track.title ||
                `Track ${track.index + 1}`;

            const duration =
                document.createElement("span");

            duration.className =
                "track-duration";

            duration.textContent =
                track.durationSeconds != null
                    ? formatTime(track.durationSeconds)
                    : "";

            button.append(
                number,
                title,
                duration);

            button.addEventListener(
                "click",
                async () => {
                    await window.datinateAudio
                        .playTrack(track.index);
                });

            playlistElement.appendChild(button);
        });
    }

    function updateTrackHighlight(index) {
        playlistElement
            .querySelectorAll(".track")
            .forEach(element => {
                element.classList.toggle(
                    "active",
                    Number(element.dataset.index) === index);
            });
    }

    function updateTrackDuration(
        index,
        durationSeconds) {

        if (index < 0 ||
            !Number.isFinite(durationSeconds) ||
            durationSeconds <= 0) {
            return;
        }

        const duration =
            playlistElement.querySelector(
                `.track[data-index="${index}"] .track-duration`);

        if (!duration)
            return;

        duration.textContent =
            formatTime(durationSeconds);
    }

    function ensureTrackVisible(index) {
        if (index < 0)
            return;

        const track =
            playlistElement.querySelector(
                `.track[data-index="${index}"]`);

        if (!track)
            return;

        const trackBounds =
            track.getBoundingClientRect();

        const playlistBounds =
            playlistElement.getBoundingClientRect();

        const fullyVisible =
            trackBounds.top >= playlistBounds.top &&
            trackBounds.bottom <= playlistBounds.bottom;

        if (fullyVisible)
            return;

        track.scrollIntoView({
            block: "nearest",
            inline: "nearest"
        });
    }

    function renderRepeatState(repeatPlaylist) {
        repeatButton.classList.toggle(
            "active",
            !!repeatPlaylist);

        repeatButton.setAttribute(
            "aria-pressed",
            repeatPlaylist
                ? "true"
                : "false");

        repeatButton.title =
            repeatPlaylist
                ? "Repeat playlist: On"
                : "Repeat playlist: Off";
    }

    function updateSeekPreview() {
        const state =
            window.datinateAudio
                .getState();

        if (state.durationSeconds == null)
            return;

        const ratio =
            Number(progress.value) /
            1000;

        elapsedElement.textContent =
            formatTime(
                state.durationSeconds * ratio);
    }

    function renderPlayPauseState(isPlaying) {
        if (!playPauseIcon)
            return;

        playPauseIcon.replaceChildren();

        if (isPlaying) {
            const left =
                document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect");

            left.setAttribute("x", "7");
            left.setAttribute("y", "5");
            left.setAttribute("width", "3.5");
            left.setAttribute("height", "14");

            const right =
                document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "rect");

            right.setAttribute("x", "13.5");
            right.setAttribute("y", "5");
            right.setAttribute("width", "3.5");
            right.setAttribute("height", "14");

            playPauseIcon.append(left, right);
        }
        else {
            const play =
                document.createElementNS(
                    "http://www.w3.org/2000/svg",
                    "path");

            play.setAttribute(
                "d",
                "M8 5L19 12L8 19Z");

            playPauseIcon.appendChild(play);
        }

        playPauseButton.title =
            isPlaying
                ? "Pause"
                : "Play";
    }

    function renderState() {
        const state =
            window.datinateAudio
                .getState();

        sourceTitleElement.textContent =
            state.sourceTitle ||
            "";

        sourceTitleElement.hidden =
            !state.sourceTitle;

        titleElement.textContent =
            state.title ||
            "No track selected";

        updateTrackDuration(
            state.trackIndex,
            state.durationSeconds);

        renderPlayPauseState(
            state.playing);

        renderRepeatState(
            state.repeatPlaylist);

        if (!userSeeking) {
            elapsedElement.textContent =
                formatTime(
                    state.positionSeconds);
        }

        durationElement.textContent =
            state.durationSeconds != null
                ? formatTime(
                    state.durationSeconds)
                : "--:--";

        progress.disabled =
            !state.canSeek;

        if (!userSeeking) {
            const ratio =
                state.durationSeconds > 0
                    ? state.positionSeconds /
                        state.durationSeconds
                    : 0;

            progress.value =
                String(
                    Math.round(
                        Math.max(
                            0,
                            Math.min(1, ratio)) *
                        1000));
        }

        volume.value =
            String(
                Math.round(
                    state.volume * 100));

        if (state.trackIndex !==
            lastRenderedTrackIndex) {
            lastRenderedTrackIndex =
                state.trackIndex;

            updateTrackHighlight(
                state.trackIndex);

            ensureTrackVisible(
                state.trackIndex);
        }
    }

    async function initialise() {
        const source =
            getSource();

        if (!source) {
            statusElement.textContent =
                "No audio source was supplied.";

            window.datinateWebView?.post(
                "player-unavailable",
                {
                    reason: "no-source"
                });

            return;
        }

        statusElement.textContent =
            "Loading audio...";

        try {
            const preview =
                isPreviewMode();

            const loaded =
                preview
                    ? await window.datinateAudio
                        .loadPreview(source)
                    : await window.datinateAudio
                        .load(source);

            if (!loaded) {
                statusElement.textContent =
                    "This media type is not supported.";

                window.datinateWebView?.post(
                    "player-unavailable",
                    {
                        source,
                        reason: "load-returned-false"
                    });

                return;
            }

            renderPlaylist();
            renderState();

            statusElement.textContent = "";

            window.datinateWebView?.post(
                "player-ready",
                {
                    source,
                    trackCount:
                        window.datinateAudio
                            .getPlaylist()
                            .length
                });
        }
        catch (error) {
            const message =
                error?.message ||
                String(error);

            console.error(
                "[DATINATE AUDIO UI] Initialisation failed:",
                error);

            statusElement.textContent =
                "This media type is not supported.";

            window.datinateWebView?.post(
                "player-unavailable",
                {
                    source,
                    reason: "load-exception",
                    error: message
                });
        }
    }

    async function beginProgressSeek(event) {
        if (progress.disabled ||
            userSeeking) {
            return;
        }

        const started =
            await window.datinateAudio
                .beginSeek();

        if (!started)
            return;

        userSeeking = true;
        seekPointerId =
            event.pointerId;

        try {
            progress.setPointerCapture(
                event.pointerId);
        }
        catch {
        }

        updateSeekPreview();
    }

    async function finishProgressSeek(event) {
        if (!userSeeking)
            return;

        if (seekPointerId != null &&
            event.pointerId !== seekPointerId) {
            return;
        }

        const ratio =
            Number(progress.value) /
            1000;

        userSeeking = false;
        seekPointerId = null;

        await window.datinateAudio
            .endSeekRatio(ratio);

        renderState();
    }

    repeatButton.addEventListener(
        "click",
        async () => {
            await window.datinateAudio
                .toggleRepeatPlaylist();

            renderState();
        });

    previousButton.addEventListener(
        "click",
        async () => {
            await window.datinateAudio.previous();
        });

    playPauseButton.addEventListener(
        "click",
        async () => {
            const state =
                window.datinateAudio.getState();

            if (state.playing)
                await window.datinateAudio.pause();
            else
                await window.datinateAudio.play();
        });

    stopButton.addEventListener(
        "click",
        async () => {
            await window.datinateAudio.stop();
        });

    nextButton.addEventListener(
        "click",
        async () => {
            await window.datinateAudio.next();
        });

    progress.addEventListener(
        "pointerdown",
        beginProgressSeek);

    progress.addEventListener(
        "pointerup",
        finishProgressSeek);

    progress.addEventListener(
        "pointercancel",
        finishProgressSeek);

    progress.addEventListener(
        "input",
        () => {
            if (userSeeking)
                updateSeekPreview();
        });

    progress.addEventListener(
        "change",
        async () => {
            if (userSeeking)
                return;

            const ratio =
                Number(progress.value) /
                1000;

            await window.datinateAudio
                .seekRatioAndPlay(ratio);
        });

    volume.addEventListener(
        "input",
        async () => {
            await window.datinateAudio
                .setVolume(
                    Number(volume.value) / 100);
        });

    setInterval(
        renderState,
        200);

    window.addEventListener(
        "pagehide",
        () => {
            try {
                void window.datinateAudio
                    ?.reset?.();
            }
            catch {
            }
        },
        { once: true });

    initialise();
})();
