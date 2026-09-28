(function () {
    const parameters =
        new URLSearchParams(
            window.location.search);

    const sessionId =
        parameters.get("session") || "";

    function post(type, data = {}) {
        try {
            chrome.webview.postMessage(JSON.stringify({
                channel: "datinate-audio",
                sessionId,
                type,
                ...data
            }));
        }
        catch {
        }
    }

    function stringify(value) {
        if (typeof value === "string")
            return value;

        try {
            return JSON.stringify(value);
        }
        catch {
            return String(value);
        }
    }

    function installConsoleForwarding() {
        if (window.__datinateConsoleForwardingInstalled)
            return;

        window.__datinateConsoleForwardingInstalled = true;

        const originalLog = console.log.bind(console);
        const originalWarn = console.warn.bind(console);
        const originalError = console.error.bind(console);

        function forward(level, args) {
            post("console", {
                level,
                message: args.map(stringify).join(" ")
            });
        }

        console.log = (...args) => {
            originalLog(...args);
            forward("log", args);
        };

        console.warn = (...args) => {
            originalWarn(...args);
            forward("warn", args);
        };

        console.error = (...args) => {
            originalError(...args);
            forward("error", args);
        };
    }

    installConsoleForwarding();

    window.addEventListener("error", event => {
        console.error(
            "[WINDOW ERROR]",
            event.message,
            event.filename,
            event.lineno,
            event.colno);
    });

    window.addEventListener("unhandledrejection", event => {
        console.error(
            "[UNHANDLED PROMISE]",
            event.reason?.stack ||
            event.reason?.message ||
            String(event.reason));
    });

    window.datinateWebView = {
        sessionId,
        post
    };
})();
