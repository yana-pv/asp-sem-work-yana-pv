const chat = {
    connection: null,
    currentMatchId: null,
    messageIds: new Set(),
    matchedUserId: null,
    matchInfo: null,

    async renderList() {
        try {
            this.currentMatchId = null;
            this.matchedUserId = null;
            this.matchedUserName = null;
            this.matchInfo = null;

            const matches = await api.get('/Matches');

            if (!matches || matches.length === 0) {
                return `
                    <div class="tg-shell">
                        <aside class="tg-sidebar">
                            <div class="tg-sidebar-header">
                                <h2>Мэтчи</h2>
                            </div>
                            <div class="tg-empty">У вас пока нет мэтчей.</div>
                        </aside>
                        <section class="tg-chat-empty">
                            <h2>Продолжайте свайпать</h2>
                            <p>Когда появится мэтч, чат откроется здесь.</p>
                        </section>
                    </div>
                `;
            }

            return this.renderShell(matches, null, `
                <section class="tg-chat-empty">
                    <h2>Выберите чат</h2>
                    <p>Откройте мэтч слева, чтобы продолжить разговор.</p>
                </section>
            `);
        } catch (e) {
            return `<div class="card"><div class="error-box">Не удалось загрузить мэтчи: ${e.message}</div></div>`;
        }
    },

    bindListEvents() {
        document.querySelectorAll('.match-item[data-match-id]').forEach(item => {
            item.addEventListener('click', () => {
                const matchId = item.dataset.matchId;
                const name = item.dataset.matchName;
                const userId = item.dataset.matchUserId;
                if (matchId) {
                    this.openChat(matchId, name, userId);
                }
            });
        });
    },

    avatarHtml(userId) {
        return `<img src="/api/Profile/avatar/${userId}" 
                     class="avatar-xs" 
                     onerror="this.src='${app.defaultAvatarDataUri()}'"
                     style="margin-right:6px; vertical-align:middle;">`;
    },

    async openChat(matchId, userName, matchedUserId) {
        this.currentMatchId = matchId;
        this.matchedUserId = matchedUserId
        this.matchedUserName = userName;
        this.matchInfo = null;
        await notifications.markMessagesReadForMatch(matchId);
        await api.post(`/Chat/${matchId}/read`, {});

        const matches = await api.get('/Matches');

        document.getElementById('app').innerHTML = `
            ${this.renderShell(matches, matchId, `
                <section class="chat-container">
                    <div class="chat-header">
                        <div class="tg-chat-title">
                            ${this.avatarHtml(matchedUserId)}
                            <div>
                                <h2>Чат с ${escapeHtml(userName)}</h2>
                                <p class="meta">Мэтч по ответам</p>
                            </div>
                        </div>
                        <div class="chat-header-actions">
                            <button class="btn btn-secondary btn-sm" id="openProfileBtn">Профиль</button>
                            <button class="btn btn-secondary btn-sm icebreaker-btn" id="icebreakerBtn">Ледокол</button>
                        </div>
                    </div>
                    <div id="chatMessages" class="chat-messages">Загрузка...</div>
                    <div class="chat-input">
                        <input type="text" id="messageInput" placeholder="Сообщение" autocomplete="off" />
                        <button class="btn" id="sendBtn">Отправить</button>
                    </div>
                </section>
            `)}
        `;
        this.bindListEvents();

        const icebreakerBtn = document.getElementById('icebreakerBtn');
        if (icebreakerBtn) {
            icebreakerBtn.addEventListener('click', async () => {
                icebreakerBtn.disabled = true;
                icebreakerBtn.textContent = '⏳ Думаю...';
                try {
                    await api.post(`/Chat/${matchId}/icebreaker`, {});
                    const messages = await api.get(`/Chat/${matchId}/messages`);
                    this.messageIds.clear();
                    this.renderMessages(messages);
                } catch (e) {
                    console.error('Ошибка ледокола:', e);
                } finally {
                    icebreakerBtn.disabled = false;
                    icebreakerBtn.textContent = 'Ледокол';
                }
            });
        }

        const openProfileBtn = document.getElementById('openProfileBtn');
        if (openProfileBtn) {
            openProfileBtn.addEventListener('click', () => this.openPublicProfile(matchedUserId));
        }

        await this.initChat(matchId);
    },

    renderShell(matches, activeMatchId, chatHtml) {
        return `
            <div class="tg-shell">
                <aside class="tg-sidebar">
                    <div class="tg-sidebar-header">
                        <h2>Мэтчи</h2>
                        <span>${matches.length}</span>
                    </div>
                    <div class="tg-search">Поиск по мыслям</div>
                    <div class="tg-match-list">
                        ${matches.map(match => this.renderMatchItem(match, activeMatchId)).join('')}
                    </div>
                </aside>
                ${chatHtml}
            </div>
        `;
    },

    renderMatchItem(match, activeMatchId) {
        const isUnread = match.hasUnreadMessages === true;
        const isActive = activeMatchId?.toLowerCase() === match.matchId.toLowerCase();
        return `
            <div class="match-item ${isUnread ? 'match-unread' : ''} ${isActive ? 'active' : ''}"
                 data-match-id="${match.matchId}"
                 data-match-name="${escapeAttribute(match.matchedUserName)}"
                 data-match-user-id="${match.matchedUserId}">
                <img src="/api/Profile/avatar/${match.matchedUserId}" 
                     class="avatar" 
                     onerror="this.src='${app.defaultAvatarDataUri()}'">
                <div class="match-text">
                    <strong>${escapeHtml(match.matchedUserName)}</strong>
                    <span>Открыть диалог</span>
                </div>
                <div class="match-actions">
                    <span class="match-date">${new Date(match.createdAt).toLocaleDateString('ru-RU')}</span>
                    <span class="match-unread-dot" title="Новое сообщение"></span>
                </div>
            </div>
        `;
    },

    async initChat(matchId) {
        try {
            const [info, messages] = await Promise.all([
                api.get(`/Chat/${matchId}/info`),
                api.get(`/Chat/${matchId}/messages`)
            ]);
            this.matchInfo = info;
            this.messageIds.clear();
            this.renderMessages(messages);
        } catch (e) {
            document.getElementById('app').innerHTML =
                `<div class="card"><div class="error-box">Ошибка загрузки чата: ${e.message}</div></div>`;
            return;
        }

        if (this.connection) {
            await this.connection.stop().catch(() => {});
            this.connection = null;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/chatHub', { accessTokenFactory: () => api.getToken() })
            .withAutomaticReconnect()
            .build();

        this.connection.on('ReceiveMessage', (message) => {
            const senderId = (message.senderUserId || message.senderId)?.toString().toLowerCase();
            const currentUserId = auth.getUserFromToken()?.id?.toString().toLowerCase();
            if (senderId !== currentUserId) {
                this.addMessage(message);
            }
        });

        await this.connection.start();
        await this.connection.invoke('JoinMatchChat', matchId);

        const sendBtn = document.getElementById('sendBtn');
        const messageInput = document.getElementById('messageInput');

        if (sendBtn && messageInput) {
            const sendMessage = async () => {
                const content = messageInput.value.trim();
                if (!content) return;

                try {
                    const result = await api.post('/Chat/send', {
                        matchId: this.currentMatchId,
                        content
                    });
                    this.addMessage(result);
                    messageInput.value = '';
                } catch (e) {
                    app.notify(e.message, 'error');
                }
            };

            sendBtn.addEventListener('click', sendMessage);
            messageInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    sendMessage();
                }
            });
        }
    },

    renderMessages(messages) {
        const container = document.getElementById('chatMessages');
        if (!container) return;

        const matchContextHtml = this.renderMatchContext();
        const messagesHtml = messages.map(message => {
            if (message.id && this.messageIds.has(message.id)) return '';
            if (message.id) this.messageIds.add(message.id);

            const senderId = (message.senderUserId || message.senderId)?.toString().toLowerCase();
            const currentUserId = auth.getUserFromToken()?.id?.toString().toLowerCase();
            const isOwn = senderId === currentUserId;
            const isSystem = message.isIcebreaker;
            const otherUserId = isOwn ? currentUserId : this.matchedUserId;
            const senderName = isSystem ? '❄️ DeepMatch' : (isOwn ? 'Вы' : (this.matchedUserName || 'Собеседник'));

            return `
                <div class="message ${isOwn ? 'message-own' : ''} ${isSystem ? 'message-system' : ''}">
                    <div class="message-sender" style="display:flex; align-items:center; gap:6px;">
                        ${!isOwn && !isSystem ? this.avatarHtml(otherUserId) : ''}
                        <span>${escapeHtml(senderName)}</span>
                    </div>
                    <div class="message-content">${escapeHtml(message.content)}</div>
                    <div class="message-time">${new Date(message.timestamp).toLocaleTimeString('ru-RU')}</div>
                </div>
            `;
        }).join('');

        container.innerHTML = matchContextHtml + messagesHtml;

        container.scrollTop = container.scrollHeight;
    },

    renderMatchContext() {
        if (!this.matchInfo) return '';

        return `
            <div class="match-context-intro">Мэтч начался с этих ответов</div>
            <div class="message message-own match-context-message">
                <div class="message-sender">Вы</div>
                <div class="match-context-question">${escapeHtml(this.matchInfo.currentQuestionText)}</div>
                <div class="message-content">${escapeHtml(this.matchInfo.currentAnswerText)}</div>
            </div>
            <div class="message match-context-message">
                <div class="message-sender" style="display:flex; align-items:center; gap:6px;">
                    ${this.avatarHtml(this.matchedUserId)}
                    <span>${escapeHtml(this.matchInfo.matchedUserName || this.matchedUserName || 'Собеседник')}</span>
                </div>
                <div class="match-context-question">${escapeHtml(this.matchInfo.matchedQuestionText)}</div>
                <div class="message-content">${escapeHtml(this.matchInfo.matchedAnswerText)}</div>
            </div>
        `;
    },

    addMessage(message) {
        if (message.id && this.messageIds.has(message.id)) return;
        if (message.id) this.messageIds.add(message.id);

        const container = document.getElementById('chatMessages');
        if (!container) return;

        const senderId = (message.senderUserId || message.senderId)?.toString().toLowerCase();
        const currentUserId = auth.getUserFromToken()?.id?.toString().toLowerCase();
        const isOwn = senderId === currentUserId;
        const isSystem = message.isIcebreaker;
        const otherUserId = isOwn ? currentUserId : this.matchedUserId;
        const senderName = isSystem ? '❄️ DeepMatch' : (isOwn ? 'Вы' : (this.matchedUserName || 'Собеседник'));

        container.innerHTML += `
            <div class="message ${isOwn ? 'message-own' : ''} ${isSystem ? 'message-system' : ''}">
                <div class="message-sender" style="display:flex; align-items:center; gap:6px;">
                    ${!isOwn && !isSystem ? this.avatarHtml(otherUserId) : ''}
                    <span>${escapeHtml(senderName)}</span>
                </div>
                <div class="message-content">${escapeHtml(message.content)}</div>
                <div class="message-time">${new Date(message.timestamp).toLocaleTimeString('ru-RU')}</div>
            </div>
        `;
        container.scrollTop = container.scrollHeight;
    },

    async updateUnreadIndicators() {
        if (app.currentPage !== 'matches') return;

        const matches = await api.get('/Matches').catch(() => []);
        const unreadMessageMatchIds = new Set(matches
            .filter(match => match.hasUnreadMessages)
            .map(match => match.matchId.toLowerCase()));

        document.querySelectorAll('.match-item[data-match-id]').forEach(item => {
            const matchId = item.dataset.matchId?.toLowerCase();
            item.classList.toggle('match-unread', unreadMessageMatchIds.has(matchId));
        });
    },

    async openPublicProfile(userId) {
        try {
            const data = await api.get(`/Profile/public/${userId}`);
            const badgeIcons = {
                'Логик': '🧠',
                'Эмпат': '💙',
                'Эрудит': '📚',
                'Мастер дебатов': '⚔️',
                'Активный участник': '🔥',
                'Создатель связей': '💜'
            };
            const badgesHtml = data.badges && data.badges.length > 0
                ? data.badges.map(b => `
                    <span class="badge-item">
                        <span class="badge-icon">${badgeIcons[b.name] || '✨'}</span>${escapeHtml(b.name)}
                    </span>
                `).join('')
                : '<p class="meta">Бейджей пока нет.</p>';
            const photosHtml = data.photos && data.photos.length > 0
                ? data.photos.map(p => `
                    <button class="profile-photo-tile public-photo-tile" type="button" data-photo-src="${escapeAttribute(p.url)}">
                        <img src="${escapeAttribute(p.url)}" alt="Фото ${escapeAttribute(data.userName)}">
                    </button>
                `).join('')
                : '<p class="meta">Фото пока нет.</p>';

            document.getElementById('app').innerHTML = `
                <div class="public-profile-page">
                    <button class="btn btn-secondary back-chat-btn" id="backToChatBtn" type="button">
                        <span aria-hidden="true">←</span>
                        Назад в чат
                    </button>

                    <div class="card profile-card public-profile-card">
                        <div class="profile-header">
                            <div class="profile-avatar-stack">
                                <img src="/api/Profile/avatar/${data.id}" class="avatar-large" onerror="this.src='${app.defaultAvatarDataUri()}'" alt="Аватар">
                            </div>
                            <div class="profile-main-info">
                                <span class="section-kicker">Профиль</span>
                                <h1>${escapeHtml(data.userName)}</h1>
                                <div class="profile-stats-row"><span>Рейтинг: ${data.rating || 0}</span></div>
                            </div>
                            <div class="badges-section">${badgesHtml}</div>
                        </div>
                    </div>

                    <div class="card bio-card">
                        <div class="section-title-row">
                            <h2>О себе</h2>
                            <span>общая информация</span>
                        </div>
                        <p>${data.bio ? escapeHtml(data.bio) : 'Пользователь пока не заполнил информацию о себе.'}</p>
                    </div>

                    <div class="card photos-card">
                        <div class="section-title-row">
                            <h2>Фото</h2>
                            <span>галерея</span>
                        </div>
                        <div class="profile-photo-grid public-photo-grid">${photosHtml}</div>
                    </div>

                    <div class="public-report-card">
                        <button class="btn btn-secondary btn-sm" type="button" id="btnReport">Пожаловаться</button>
                        <form class="report-form public-report-form" style="display:none;">
                            <textarea name="reason" required minlength="5" maxlength="500" placeholder="Коротко опишите причину жалобы"></textarea>
                            <div class="report-actions">
                                <button class="btn btn-sm" type="submit">Отправить</button>
                                <button class="btn btn-secondary btn-sm" type="button" id="btnCancelPublicReport">Отмена</button>
                            </div>
                        </form>
                    </div>
                </div>
            `;

            document.getElementById('backToChatBtn')?.addEventListener('click', () => {
                this.openChat(this.currentMatchId, this.matchedUserName, this.matchedUserId);
            });

            const photoTiles = Array.from(document.querySelectorAll('.public-photo-tile[data-photo-src]'));
            if (photoTiles.length > 0) {
                const photos = photoTiles.map(tile => tile.dataset.photoSrc);
                photoTiles.forEach((tile, index) => {
                    tile.addEventListener('click', () => app.openPhotoViewer(photos, index));
                });
            }

            const reportButton = document.getElementById('btnReport');
            const reportForm = document.querySelector('.public-report-form');
            const cancelReportButton = document.getElementById('btnCancelPublicReport');

            reportButton?.addEventListener('click', () => {
                if (reportForm) reportForm.style.display = 'grid';
                reportButton.style.display = 'none';
            });

            cancelReportButton?.addEventListener('click', () => {
                reportForm?.reset();
                if (reportForm) reportForm.style.display = 'none';
                if (reportButton) reportButton.style.display = 'inline-flex';
            });

            reportForm?.addEventListener('submit', async (e) => {
                e.preventDefault();
                const form = e.currentTarget;
                try {
                    await api.post('/Reports', {
                        reportedUserId: data.id,
                        reason: form.reason.value
                    });
                    app.notify('Жалоба отправлена', 'success');
                    form.reset();
                    form.style.display = 'none';
                    if (reportButton) reportButton.style.display = 'inline-flex';
                } catch (error) {
                    app.notify(error.message || 'Не удалось отправить жалобу', 'error');
                }
            });
        } catch (error) {
            app.notify(error.message || 'Не удалось открыть профиль', 'error');
        }
    }
};
