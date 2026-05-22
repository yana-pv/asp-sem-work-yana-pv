const notifications = {
    connection: null,
    pollIntervalId: null,
    outsideClickHandler: null,

    async load() {
        if (!auth.isLoggedIn()) {
            this.updateBadge(0);
            return [];
        }

        try {
            const list = (await api.get('/Notifications')).map(n => this.normalize(n));
            const unread = list.filter(n => !n.isRead).length;
            this.updateBadge(unread);
            if (app.currentPage === 'matches') {
                chat.updateUnreadIndicators();
            }
            return list;
        } catch {
            return [];
        }
    },

    updateBadge(count) {
        const badge = document.getElementById('notificationBadge');
        if (!badge) return;

        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'block';
        } else {
            badge.style.display = 'none';
        }
    },

    async showPanel() {
        this.closePanel();

        const list = await this.load();
        const unreadList = list.filter(n => !n.isRead);
        const html = unreadList.length === 0
            ? '<div class="notification-empty">Нет новых уведомлений</div>'
            : unreadList.map(n => `
                <div class="notification-item ${n.isRead ? '' : 'unread'}" data-id="${escapeAttribute(n.id)}" data-link="${escapeAttribute(n.link || '')}">
                    <div>${this.getIcon(n.type)} ${escapeHtml(n.title)}</div>
                    <div class="meta">${new Date(n.createdAt).toLocaleString('ru-RU')}</div>
                </div>
            `).join('');

        const panel = document.createElement('div');
        panel.id = 'notificationPanel';
        panel.className = 'notification-panel';
        panel.innerHTML = `
            <div class="notification-panel-header">
                <strong>Уведомления</strong>
                <button class="btn btn-sm btn-secondary" id="markAllRead">Прочитано</button>
            </div>
            <div class="notification-panel-body">${html}</div>
        `;
        document.body.appendChild(panel);

        panel.querySelectorAll('.notification-item').forEach(item => {
            item.addEventListener('click', async () => {
                const id = item.dataset.id;
                await api.post('/Notifications/read', { notificationId: id });
                this.closePanel();
                await this.load();
                await this.followLink(item.dataset.link);
            });
        });

        document.getElementById('markAllRead')?.addEventListener('click', async (e) => {
            e.stopPropagation();
            const button = e.currentTarget;
            const body = panel.querySelector('.notification-panel-body');
            button.disabled = true;
            try {
                await api.post('/Notifications/read', { notificationId: null });
                await this.load();
                if (body) {
                    body.innerHTML = '<div class="notification-empty">Нет новых уведомлений</div>';
                }
            } finally {
                button.disabled = false;
            }
        });

        setTimeout(() => {
            this.outsideClickHandler = (e) => this.closePanelOnOutsideClick(e);
            document.addEventListener('click', this.outsideClickHandler);
        }, 0);
    },

    closePanelOnOutsideClick(e) {
        const panel = document.getElementById('notificationPanel');
        const button = document.getElementById('notificationsBtn');
        if (!panel) return;

        if (!panel.contains(e.target) && !button?.contains(e.target)) {
            this.closePanel();
        }
    },

    closePanel() {
        document.getElementById('notificationPanel')?.remove();

        if (this.outsideClickHandler) {
            document.removeEventListener('click', this.outsideClickHandler);
            this.outsideClickHandler = null;
        }
    },

    getUnreadMessageMatchIds(list) {
        return new Set(list
            .map(n => this.normalize(n))
            .filter(n => !n.isRead && n.type === 'message')
            .map(n => this.getMatchIdFromLink(n.link))
            .filter(Boolean));
    },

    getMatchIdFromLink(link) {
        const match = /^\/matches\/([^/]+)$/.exec(link || '');
        return match ? match[1].toLowerCase() : null;
    },

    async markMessagesReadForMatch(matchId) {
        const normalizedMatchId = matchId?.toLowerCase();
        if (!normalizedMatchId) return;

        const list = await this.load();
        const targetNotifications = list.filter(n =>
            !n.isRead &&
            n.type === 'message' &&
            this.getMatchIdFromLink(n.link) === normalizedMatchId
        );

        await Promise.all(targetNotifications.map(n =>
            api.post('/Notifications/read', { notificationId: n.id })
        ));

        await this.load();
    },

    async followLink(link) {
        if (!link) return;

        const parts = link.split('/').filter(Boolean);
        const page = parts[0];
        const matchId = parts[1];

        if (page === 'matches' && matchId) {
            try {
                app.currentPage = 'matches';
                const matches = await api.get('/Matches');
                const match = matches.find(m => m.matchId?.toLowerCase() === matchId.toLowerCase());

                if (match) {
                    await chat.openChat(match.matchId, match.matchedUserName, match.matchedUserId);
                    return;
                }
            } catch {
            }
        }

        if (app.pages[page]) {
            app.navigate(page);
        }
    },

    getIcon(type) {
        return {
            match: '💜',
            message: '💬',
            badge: '🏆',
            rating: '⭐',
            question: '❔'
        }[type] || '📌';
    },

    startPolling() {
        if (!auth.isLoggedIn()) return;

        this.load();
        this.startSignalR();

        if (!this.pollIntervalId) {
            this.pollIntervalId = setInterval(() => {
                this.startSignalR();
                if (app.currentPage === 'matches' && !chat.currentMatchId) {
                    app.renderPage();
                } else {
                    this.load();
                }
            }, 30000);
        }
    },

    stopPolling() {
        if (this.pollIntervalId) {
            clearInterval(this.pollIntervalId);
            this.pollIntervalId = null;
        }

        this.updateBadge(0);
        this.closePanel();
        this.connection?.stop().catch(() => {});
        this.connection = null;
    },

    async startSignalR() {
        if (typeof signalR === 'undefined') return;

        if (this.connection &&
            this.connection.state !== signalR.HubConnectionState.Disconnected) {
            return;
        }

        if (!this.connection) {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/chatHub', { accessTokenFactory: () => api.getToken() })
                .withAutomaticReconnect()
                .build();

            this.connection.onreconnected(() => this.load());
            this.connection.onclose(() => {
                this.connection = null;
            });

            this.connection.on('NewNotification', async (notification) => {
                await this.handleRealtimeNotification(notification);
            });
        }

        try {
            await this.connection.start();
        } catch {
            this.connection = null;
        }
    },

    async handleRealtimeNotification(notification) {
        const normalized = this.normalize(notification);
        const messageMatchId = this.getMatchIdFromLink(normalized.link);

        if (normalized.type === 'message' &&
            chat.currentMatchId &&
            messageMatchId === chat.currentMatchId.toLowerCase()) {
            await this.markMessagesReadForMatch(chat.currentMatchId);
            return;
        }

        if (normalized.title) {
            app.notify(normalized.title, normalized.type === 'message' ? 'info' : 'success');
        }

        if (normalized.type === 'message' && app.currentPage === 'matches' && !chat.currentMatchId) {
            await app.renderPage();
            return;
        }

        await this.load();
    },

    normalize(notification) {
        return {
            id: notification.id ?? notification.Id,
            type: notification.type ?? notification.Type,
            title: notification.title ?? notification.Title,
            link: notification.link ?? notification.Link,
            isRead: notification.isRead ?? notification.IsRead,
            createdAt: notification.createdAt ?? notification.CreatedAt
        };
    }
};
