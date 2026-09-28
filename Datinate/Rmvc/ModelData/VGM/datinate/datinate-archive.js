(function () {
    const archiveBase =
        new URL("../libarchive/", window.location.href);

    const archiveExtensions = [
        ".zip",
        ".zipx",
        ".7z",
        ".rar",
        ".tar",
        ".tgz",
        ".tar.gz",
        ".gz",
        ".bz2",
        ".tbz",
        ".tbz2",
        ".xz",
        ".txz",
        ".lha",
        ".lzh",
        ".cab",
        ".cpio",
        ".ar",
        ".xar",
        ".iso"
    ];

    const config =
        window.datinateArchiveConfig ||
        (window.datinateArchiveConfig = {});

    if (typeof config.recompressZstdForVgm !== "boolean")
        config.recompressZstdForVgm = true;

    let archiveClassPromise = null;

    function withTimeout(promise, milliseconds, message) {
        let timer = null;

        const timeout = new Promise((_, reject) => {
            timer = setTimeout(
                () => reject(new Error(message)),
                milliseconds);
        });

        return Promise.race([promise, timeout])
            .finally(() => clearTimeout(timer));
    }

    function getSourceName(url) {
        try {
            const parsed = new URL(url, window.location.href);
            const name = parsed.pathname.split("/").pop() || "archive";
            return decodeURIComponent(name);
        }
        catch {
            return "archive";
        }
    }

    function getCleanPath(value) {
        return String(value || "")
            .split("?")[0]
            .split("#")[0]
            .toLowerCase();
    }

    function isZipPath(value) {
        const path = getCleanPath(value);
        return path.endsWith(".zip") ||
            path.endsWith(".zipx");
    }

    function isArchivePath(value) {
        const path = getCleanPath(value);
        return archiveExtensions.some(extension =>
            path.endsWith(extension));
    }

    async function getArchiveClass() {
        if (!archiveClassPromise) {
            archiveClassPromise = (async () => {
                const moduleUrl =
                    new URL("libarchive.js", archiveBase).href;

                const workerUrl =
                    new URL("worker-bundle.js", archiveBase).href;

                const module =
                    await import(moduleUrl);

                const Archive =
                    module.Archive;

                if (!Archive ||
                    typeof Archive.open !== "function") {
                    throw new Error(
                        "libarchive.js did not expose Archive.open().");
                }

                Archive.init({
                    workerUrl
                });

                return Archive;
            })();
        }

        return archiveClassPromise;
    }

    async function fetchSourceFile(url) {
        const response =
            await fetch(
                url,
                {
                    cache: "no-store"
                });

        if (!response.ok) {
            throw new Error(
                `Archive fetch failed. HTTP ${response.status}.`);
        }

        const blob =
            await response.blob();

        return new File(
            [blob],
            getSourceName(url),
            {
                type: blob.type || "application/octet-stream"
            });
    }

    function getLibArchiveEntryPath(rawEntry) {
        const parent =
            rawEntry?.path || "";

        const name =
            rawEntry?.file?.name || "";

        return `${parent}${name}`
            .replaceAll("\\", "/")
            .replace(/^\/+/, "");
    }

    function normaliseLibArchiveEntries(rawEntries) {
        return (rawEntries || [])
            .map(rawEntry => ({
                path: getLibArchiveEntryPath(rawEntry),
                size: Number(rawEntry?.file?.size) || 0,
                rawEntry
            }))
            .filter(entry => !!entry.path);
    }

    async function openLibArchive(file, url) {
        let archive = null;

        try {
            const Archive =
                await getArchiveClass();

            archive =
                await withTimeout(
                    Archive.open(file),
                    15000,
                    "Timed out opening archive.");

            const rawEntries =
                await withTimeout(
                    archive.getFilesArray(),
                    15000,
                    "Timed out listing archive contents.");

            const entries =
                normaliseLibArchiveEntries(rawEntries);

            async function extract(path) {
                const entry =
                    entries.find(item =>
                        item.path === path);

                if (!entry ||
                    !entry.rawEntry?.file ||
                    typeof entry.rawEntry.file.extract !== "function") {
                    throw new Error(
                        `Archive entry not found: ${path}`);
                }

                return withTimeout(
                    entry.rawEntry.file.extract(),
                    30000,
                    `Timed out extracting archive entry: ${path}`);
            }

            async function close() {
                if (!archive)
                    return;

                try {
                    await archive.close();
                }
                catch {
                }

                archive = null;
            }

            return {
                engine: "libarchive.js",
                url,
                file,
                entries,
                isZip: false,
                hasZstd: false,
                extract,
                close
            };
        }
        catch (error) {
            if (archive) {
                try {
                    await archive.close();
                }
                catch {
                }
            }

            throw error;
        }
    }

    async function open(url) {
        const file =
            await fetchSourceFile(url);

        if (isZipPath(url)) {
            if (!window.datinateZip ||
                typeof window.datinateZip.open !== "function") {
                throw new Error(
                    "Datinate ZIP support is unavailable.");
            }

            const session =
                await window.datinateZip.open(file);

            return {
                ...session,
                url,
                isZip: true
            };
        }

        return openLibArchive(file, url);
    }

    async function prepareVgmArchive(session) {
        if (!session)
            throw new Error("An archive session is required.");

        if (!session.isZip ||
            !session.hasZstd ||
            !config.recompressZstdForVgm) {
            return session.file;
        }

        if (typeof session.repackAsDeflate !== "function") {
            throw new Error(
                "The ZIP session cannot normalise Zstandard entries.");
        }

        console.log(
            "[DATINATE ARCHIVE] Recompressing Zstandard ZIP as Deflate for VGMPlay:",
            session.file.name);

        const normalised =
            await session.repackAsDeflate();

        console.log(
            "[DATINATE ARCHIVE] Zstandard ZIP normalisation complete:",
            session.file.name);

        return normalised;
    }

    async function list(url) {
        const session =
            await open(url);

        try {
            return session.entries.map(entry => ({
                path: entry.path,
                size: entry.size
            }));
        }
        finally {
            await session.close();
        }
    }

    window.datinateArchive = {
        isArchivePath,
        isZipPath,
        open,
        list,
        prepareVgmArchive,

        get recompressZstdForVgm() {
            return config.recompressZstdForVgm;
        },

        set recompressZstdForVgm(value) {
            config.recompressZstdForVgm = !!value;
        }
    };
})();
