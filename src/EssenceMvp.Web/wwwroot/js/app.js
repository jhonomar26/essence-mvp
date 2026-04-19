window.storage = {
    get: (key) => localStorage.getItem(key),
    set: (key, value) => localStorage.setItem(key, value),
    remove: (key) => localStorage.removeItem(key)
};

window.devLog = (level, msg) => {
    if (level === 'error') console.error('[Essence]', msg);
    else console.log('[Essence]', msg);
};
