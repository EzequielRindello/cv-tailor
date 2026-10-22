// Alt+Up/Down reorders a bullet (handled in BulletEditor); stop the browser from also scrolling
document.addEventListener('keydown', (event) => {
    if (event.altKey && (event.key === 'ArrowUp' || event.key === 'ArrowDown') && event.target.closest?.('.bullet-row'))
        event.preventDefault();
});

window.cvApp = {
    downloadFile: async (fileName, streamRef) => {
        const buffer = await streamRef.arrayBuffer();
        const url = URL.createObjectURL(new Blob([buffer]));
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    },

    // False when the browser refuses (no permission, insecure context), so the caller can say so
    copyText: async (text) => {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            return false;
        }
    },

    // The live preview (EDIT-02) reloads its srcdoc on every edit; this puts the reader back where they were.
    // The page sets this up from outside: the sandboxed CV document itself runs no script.
    keepScroll: (frame) => {
        if (!frame || frame.__keepScroll) return;
        frame.__keepScroll = true;
        let top = 0;
        const track = () => frame.contentWindow?.addEventListener('scroll', () => { top = frame.contentWindow.scrollY; }, { passive: true });
        frame.addEventListener('load', () => {
            frame.contentWindow?.scrollTo(0, top);
            track();
        });
        // The first document may have loaded before this ran
        track();
    },

    getItem: (key) => {
        try { return localStorage.getItem(key); } catch { return null; }
    },

    setItem: (key, value) => {
        try { localStorage.setItem(key, value); } catch { }
    },

    removeItem: (key) => {
        try { localStorage.removeItem(key); } catch { }
    },

    getTheme: () => document.documentElement.getAttribute('data-theme') || 'light',

    setTheme: (theme) => {
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('theme', theme); } catch { }
    },

    // The target can still be mid-render when Blazor calls this, so retry for a few frames before giving up
    focusElement: (id, attempts = 15) => {
        const el = document.getElementById(id);
        if (el) {
            el.focus();
            return;
        }

        if (attempts > 0)
            requestAnimationFrame(() => window.cvApp.focusElement(id, attempts - 1));
    },

    // Scrolls a section to the top of the screen and moves focus to it, retrying while Blazor renders it
    revealElement: (id, attempts = 15) => {
        const el = document.getElementById(id);
        if (el) {
            const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            el.scrollIntoView({ behavior: reduceMotion ? 'auto' : 'smooth', block: 'start' });
            el.focus({ preventScroll: true });
            return;
        }

        if (attempts > 0)
            requestAnimationFrame(() => window.cvApp.revealElement(id, attempts - 1));
    },

    scrollToTop: () => {
        document.getElementById('app-header')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    },

    registerShortcuts: (dotNetRef) => {
        if (window.cvApp._shortcutHandler)
            window.removeEventListener('keydown', window.cvApp._shortcutHandler);

        window.cvApp._shortcutHandler = async (event) => {
            if (event.repeat) return;
            const mod = event.ctrlKey || event.metaKey;

            if (mod && event.shiftKey && event.key.toLowerCase() === 'v') {
                event.preventDefault();
                try {
                    const text = await navigator.clipboard.readText();
                    if (text) dotNetRef.invokeMethodAsync('OnPasteShortcut', text);
                } catch { }
                return;
            }

            if (mod && !event.shiftKey && event.key === 'Enter') {
                event.preventDefault();
                const textarea = document.getElementById('job-description');
                dotNetRef.invokeMethodAsync('OnGenerateShortcut', textarea ? textarea.value : '');
            }
        };

        window.addEventListener('keydown', window.cvApp._shortcutHandler);
        // Lets tests (and anything else) know the shortcuts will respond
        document.documentElement.setAttribute('data-shortcuts', 'ready');
    },

    unregisterShortcuts: () => {
        if (window.cvApp._shortcutHandler)
            window.removeEventListener('keydown', window.cvApp._shortcutHandler);
        window.cvApp._shortcutHandler = null;
        document.documentElement.removeAttribute('data-shortcuts');
    }
};
