class SessionManager {
  constructor(storageKey = 'session') {
    this.storageKey = storageKey;
  }

  set(sessionData) {
    localStorage.setItem(this.storageKey, JSON.stringify(sessionData));
  }

  get() {
    const data = localStorage.getItem(this.storageKey);
    return data ? JSON.parse(data) : null;
  }

  exists() {
    return !!localStorage.getItem(this.storageKey);
  }

  clear() {
    localStorage.removeItem(this.storageKey);
  }

  getUserId() {
    const session = this.get();
    return session ? session.userId : null;
  }

  getUsername() {
    const session = this.get();
    return session ? session.username : null;
  }
}

export default SessionManager;
