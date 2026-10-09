(function () {
    const ZSTD_COMPRESSION_METHOD = 93;
    const DEFLATE_COMPRESSION_METHOD = 8;
    const STORE_COMPRESSION_METHOD = 0;

    const zstdModuleUrl =
        new URL(
            "../zstddec/zstddec-stream.mjs",
            window.location.href).href;

    let codecRegistered = false;
    let decoderPromise = null;

    function requireZipJs() {
        const zipApi = window.zip;

        if (!zipApi ||
            typeof zipApi.ZipReader !== "function" ||
            typeof zipApi.ZipWriter !== "function" ||
            typeof zipApi.registerCodec !== "function") {
            throw new Error(
                "zip.js is not available or the deployed build is missing read/write support.");
        }

        return zipApi;
    }

    async function getZstdDecoder() {
        if (!decoderPromise) {
            decoderPromise = (async () => {
                const module =
                    await import(zstdModuleUrl);

                if (typeof module.ZSTDDecoder !== "function") {
                    throw new Error(
                        "zstddec did not expose ZSTDDecoder.");
                }

                const decoder =
                    new module.ZSTDDecoder();

                await decoder.init();
                return decoder;
            })();
        }

        return decoderPromise;
    }

    function combineChunks(chunks, totalLength) {
        const combined =
            new Uint8Array(totalLength);

        let offset = 0;

        for (const chunk of chunks) {
            combined.set(chunk, offset);
            offset += chunk.byteLength;
        }

        return combined;
    }

    class ZstdDecompressionStream extends TransformStream {
        constructor(format, options = {}) {
            if (format !== "zstd") {
                throw new Error(
                    `Unsupported Zstandard stream format: ${format}`);
            }

            const chunks = [];
            let totalLength = 0;

            super({
                transform(chunk) {
                    const bytes =
                        chunk instanceof Uint8Array
                            ? chunk
                            : new Uint8Array(
                                chunk.buffer || chunk,
                                chunk.byteOffset || 0,
                                chunk.byteLength || chunk.length);

                    const copy = bytes.slice();
                    chunks.push(copy);
                    totalLength += copy.byteLength;
                },

                async flush(controller) {
                    const decoder =
                        await getZstdDecoder();

                    const compressed =
                        combineChunks(
                            chunks,
                            totalLength);

                    const expectedSize =
                        Number(options.uncompressedSize) || 0;

                    const decompressed =
                        decoder.decode(
                            compressed,
                            expectedSize);

                    controller.enqueue(decompressed);
                }
            });
        }
    }

    function ensureCodecRegistered() {
        const zipApi = requireZipJs();

        if (codecRegistered)
            return zipApi;

        zipApi.registerCodec({
            compressionMethod: ZSTD_COMPRESSION_METHOD,
            format: "zstd",
            DecompressionStream: ZstdDecompressionStream,
            versionNeeded: 63
        });

        codecRegistered = true;
        return zipApi;
    }

    function normalisePath(path) {
        return String(path || "")
            .replaceAll("\\", "/")
            .replace(/^\/+/, "");
    }

    function getFileName(path) {
        const normalised =
            normalisePath(path);

        return normalised
            .split("/")
            .pop() || "archive-entry";
    }

    function normaliseEntries(rawEntries) {
        return (rawEntries || [])
            .map(rawEntry => ({
                path: normalisePath(rawEntry.filename),
                size: Number(rawEntry.uncompressedSize) || 0,
                compressedSize: Number(rawEntry.compressedSize) || 0,
                compressionMethod: Number(rawEntry.compressionMethod),
                directory: !!rawEntry.directory,
                encrypted: !!rawEntry.encrypted,
                rawEntry
            }))
            .filter(entry => !!entry.path);
    }

    async function open(file) {
        const zipApi =
            ensureCodecRegistered();

        const reader =
            new zipApi.ZipReader(
                new zipApi.BlobReader(file));

        let closed = false;

        try {
            const rawEntries =
                await reader.getEntries();

            const entries =
                normaliseEntries(rawEntries);

            const hasZstd =
                entries.some(entry =>
                    !entry.directory &&
                    entry.compressionMethod ===
                        ZSTD_COMPRESSION_METHOD);

            async function extract(path) {
                const entry =
                    entries.find(item =>
                        item.path === path);

                if (!entry || entry.directory)
                    throw new Error(
                        `ZIP entry not found: ${path}`);

                if (entry.encrypted)
                    throw new Error(
                        `Encrypted ZIP entries are not supported: ${path}`);

                const blob =
                    await entry.rawEntry.getData(
                        new zipApi.BlobWriter(
                            "application/octet-stream"));

                return new File(
                    [blob],
                    getFileName(entry.path),
                    {
                        type: "application/octet-stream"
                    });
            }

            async function repackAsDeflate() {
                const writer =
                    new zipApi.ZipWriter(
                        new zipApi.BlobWriter(
                            "application/zip"),
                        {
                            compressionMethod:
                                DEFLATE_COMPRESSION_METHOD,
                            level: 6
                        });

                for (const entry of entries) {
                    const raw =
                        entry.rawEntry;

                    if (entry.encrypted) {
                        throw new Error(
                            `Encrypted ZIP entries cannot be normalised: ${entry.path}`);
                    }

                    if (entry.directory) {
                        await writer.add(
                            entry.path,
                            undefined,
                            {
                                directory: true,
                                compressionMethod:
                                    STORE_COMPRESSION_METHOD,
                                lastModDate: raw.lastModDate
                            });

                        continue;
                    }

                    const blob =
                        await raw.getData(
                            new zipApi.BlobWriter(
                                "application/octet-stream"));

                    await writer.add(
                        entry.path,
                        new zipApi.BlobReader(blob),
                        {
                            compressionMethod:
                                DEFLATE_COMPRESSION_METHOD,
                            level: 6,
                            lastModDate: raw.lastModDate
                        });
                }

                const output =
                    await writer.close();

                return new File(
                    [output],
                    file.name,
                    {
                        type: "application/zip",
                        lastModified:
                            file.lastModified || Date.now()
                    });
            }

            async function close() {
                if (closed)
                    return;

                closed = true;
                await reader.close();
            }

            return {
                engine: "zip.js",
                file,
                entries,
                hasZstd,
                extract,
                repackAsDeflate,
                close
            };
        }
        catch (error) {
            try {
                await reader.close();
            }
            catch {
            }

            throw error;
        }
    }

    window.datinateZip = {
        ZSTD_COMPRESSION_METHOD,
        open
    };
})();
