const STORAGE_KEY = 'cv-generator-settings';

export const defaultSettings = {
  template: 'harvard',
  selectedFormats: { pdf: true, docx: false },
  allowTwoPages: false,
};

export function loadSettings() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return { ...defaultSettings };

    const parsed = JSON.parse(raw);
    return {
      ...defaultSettings,
      ...parsed,
      selectedFormats: {
        ...defaultSettings.selectedFormats,
        ...(parsed?.selectedFormats || {}),
      },
    };
  } catch {
    return { ...defaultSettings };
  }
}

export function saveSettings(settings) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({
    template: settings.template,
    selectedFormats: settings.selectedFormats,
    allowTwoPages: settings.allowTwoPages,
  }));
}
