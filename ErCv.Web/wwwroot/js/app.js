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

    getItem: (key) => {
        try { return localStorage.getItem(key); } catch { return null; }
    },

    setItem: (key, value) => {
        try { localStorage.setItem(key, value); } catch { }
    },

    removeItem: (key) => {
        try { localStorage.removeItem(key); } catch { }
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
    },

    unregisterShortcuts: () => {
        if (window.cvApp._shortcutHandler)
            window.removeEventListener('keydown', window.cvApp._shortcutHandler);
        window.cvApp._shortcutHandler = null;
    }
};
