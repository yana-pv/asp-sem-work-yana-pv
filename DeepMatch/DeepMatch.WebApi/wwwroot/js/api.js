const API_BASE = '/api';

function escapeHtml(value) {
    if (value === null || value === undefined) return '';

    return String(value).replace(/[&<>"']/g, (char) => ({
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;'
    }[char]));
}

function escapeAttribute(value) {
    return escapeHtml(value);
}

const api = {
    getToken() {
        return localStorage.getItem('token');
    },

    headers(includeAuth = true) {
        const headers = {
            'Content-Type': 'application/json'
        };
        if (includeAuth && this.getToken()) {
            headers['Authorization'] = `Bearer ${this.getToken()}`;
        }
        return headers;
    },

    async get(endpoint, includeAuth = true) {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            headers: this.headers(includeAuth)
        });
        return this.handleResponse(res);
    },

    async post(endpoint, body, includeAuth = true) {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'POST',
            headers: this.headers(includeAuth),
            body: JSON.stringify(body)
        });
        return this.handleResponse(res);
    },

    async put(endpoint, body, includeAuth = true) {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'PUT',
            headers: this.headers(includeAuth),
            body: JSON.stringify(body)
        });
        return this.handleResponse(res);
    },

    async handleResponse(res) {
        const data = await res.json().catch(() => null);
        if (!res.ok) {
            const error = new Error(data?.detail || 'Ошибка сервера');
            error.status = res.status;
            error.data = data;
            throw error;
        }
        return data;
    }
};
