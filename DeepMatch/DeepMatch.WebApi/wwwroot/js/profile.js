const profile = {
    async render() {
        if (!auth.isLoggedIn()) {
            app.navigate('login');
            return '<div class="loading">Перенаправление...</div>';
        }

        try {
            const data = await api.get('/Profile');
            const userName = data.userName || 'DeepMatch';
            const email = data.email || '';

            const avatarUrl = data.avatarUrl
                ? `<img src="/api/Profile/avatar/${data.id}" class="avatar-large" alt="Аватар" onerror="this.src='${app.defaultAvatarDataUri()}'">`
                : `<img src="${app.defaultAvatarDataUri()}" class="avatar-large" alt="Аватар по умолчанию">`;

            // Заработанные бейджи
            const allBadges = [
                { icon: '🧠', name: 'Логик', desc: '10 ответов с тегом «логика»' },
                { icon: '💙', name: 'Эмпат', desc: '10 ответов с тегом «эмпатия»' },
                { icon: '📚', name: 'Эрудит', desc: '5 разных категорий' },
                { icon: '⚔️', name: 'Мастер дебатов', desc: '20 лайков на ответах' },
                { icon: '🔥', name: 'Активный участник', desc: '7 дней подряд' },
                { icon: '💜', name: 'Создатель связей', desc: '5 мэтчей' }
            ];

            let badgesHtml = allBadges.map(b => {
                const earned = data.badges && data.badges.some(ub => ub.name === b.name);
                return `<span class="badge-item${earned ? '' : ' locked'}" title="${b.desc}">
                    <span class="badge-icon">${b.icon}</span> ${b.name}
                </span>`;
            }).join('');

            const photosHtml = data.photos && data.photos.length > 0
                ? data.photos.map(p => `
                    <button class="profile-photo-tile" type="button" data-photo-src="${escapeAttribute(p.url)}">
                        <img src="${escapeAttribute(p.url)}" alt="Фото профиля">
                    </button>
                `).join('')
                : '<p class="meta">Дополнительных фото пока нет.</p>';

            // Ответы с тегами
            let answersHtml = '';
            if (data.answers && data.answers.length > 0) {
                answersHtml = data.answers.map(a => {
                    const tagsHtml = a.tags && a.tags.length > 0
                        ? `<div style="margin-top:6px;">${a.tags.map(t => `<span class="badge">${escapeHtml(t)}</span>`).join(' ')}</div>`
                        : '';
                    return `
                        <div class="answer-item">
                            <div class="question-text">${escapeHtml(a.questionText)}</div>
                            <div class="answer-text">${escapeHtml(a.text)}</div>
                            <div class="meta">
                                ${new Date(a.createdAt).toLocaleDateString('ru-RU')}
                                ${tagsHtml}
                            </div>
                        </div>
                    `;
                }).join('');
            } else {
                answersHtml = '<p style="color:#6f7785;">Вы ещё не ответили ни на один вопрос.</p>';
            }

            return `
                <div class="card profile-card">
                    <div class="profile-header">
                        <div class="profile-avatar-stack">
                            ${avatarUrl}
                            <label class="btn btn-sm avatar-upload-button" for="avatarInput">Загрузить</label>
                            <input type="file" id="avatarInput" accept="image/*">
                            <div id="uploadStatus" class="avatar-upload-status" style="display:none;"></div>
                        </div>
                        <div class="profile-main-info">
                            <span class="section-kicker">Профиль</span>
                            <h1>${escapeHtml(userName)}</h1>
                            <p>${escapeHtml(email)}</p>
                            <div class="profile-stats-row">
                                <span>Рейтинг: ${data.rating || 0}</span>
                            </div>
                        </div>
                        <div class="badges-section">${badgesHtml}</div>
                    </div>
                </div>

                <div class="card bio-card">
                    <div class="section-title-row">
                        <h2>О себе</h2>
                        <span>общая информация</span>
                    </div>
                    <form id="bioForm">
                        <div class="form-group">
                            <textarea name="bio" maxlength="1000" placeholder="Расскажите немного о себе, интересах и том, что для вас важно...">${escapeHtml(data.bio || '')}</textarea>
                        </div>
                        <button class="btn" type="submit">Сохранить</button>
                    </form>
                </div>

                <div class="card photos-card">
                    <div class="section-title-row">
                        <h2>Фото</h2>
                        <label class="btn btn-sm profile-photo-upload" for="profilePhotoInput">Загрузить фото</label>
                        <input type="file" id="profilePhotoInput" accept="image/*">
                    </div>
                    <div class="profile-photo-grid">${photosHtml}</div>
                </div>

                <div class="card answers-card">
                    <div class="section-title-row">
                        <h2>Мои ответы</h2>
                        <span>портфолио мыслей</span>
                    </div>
                    <div class="answers-list">${answersHtml}</div>
                </div>
            `;
        } catch (e) {
            return `<div class="error-box">Ошибка загрузки профиля: ${e.message}</div>`;
        }
    },

    async afterRender() {
        const fileInput = document.getElementById('avatarInput');
        const status = document.getElementById('uploadStatus');
        const uploadButton = document.querySelector('.avatar-upload-button');
        const bioForm = document.getElementById('bioForm');
        const profilePhotoInput = document.getElementById('profilePhotoInput');
        const profilePhotoButton = document.querySelector('.profile-photo-upload');
        const photoTiles = Array.from(document.querySelectorAll('.profile-photo-tile[data-photo-src]'));

        if (photoTiles.length > 0) {
            const photos = photoTiles.map(tile => tile.dataset.photoSrc);
            photoTiles.forEach((tile, index) => {
                tile.addEventListener('click', () => app.openPhotoViewer(photos, index));
            });
        }

        if (fileInput && status) {
            fileInput.addEventListener('change', async () => {
                if (!fileInput.files || !fileInput.files[0]) return;

                const file = fileInput.files[0];
                const formData = new FormData();
                formData.append('file', file);

                try {
                    status.textContent = `Загружаем ${file.name}...`;
                    status.style.display = 'block';
                    status.className = 'meta';
                    uploadButton?.classList.add('is-loading');

                    const res = await fetch('/api/Profile/avatar', {
                        method: 'POST',
                        headers: {
                            'Authorization': `Bearer ${api.getToken()}`
                        },
                        body: formData
                    });

                    const data = await res.json();
                    if (res.ok) {
                        status.textContent = 'Аватар обновлён!';
                        status.className = 'success';
                        app.notify('Аватар обновлён', 'success');
                        setTimeout(() => app.navigate('profile'), 500);
                    } else {
                        status.textContent = data.message || 'Ошибка загрузки';
                        status.className = 'error';
                        app.notify(status.textContent, 'error');
                    }
                    status.style.display = 'block';
                } catch (e) {
                    status.textContent = 'Ошибка загрузки';
                    status.className = 'error';
                    status.style.display = 'block';
                    app.notify('Ошибка загрузки аватара', 'error');
                } finally {
                    uploadButton?.classList.remove('is-loading');
                    fileInput.value = '';
                }
            });
        }

        if (bioForm) {
            bioForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                try {
                    await api.put('/Profile', { bio: bioForm.bio.value });
                    app.notify('Информация о себе сохранена', 'success');
                } catch (error) {
                    app.notify(error.message || 'Не удалось сохранить профиль', 'error');
                }
            });
        }

        if (profilePhotoInput) {
            profilePhotoInput.addEventListener('change', async () => {
                if (!profilePhotoInput.files || !profilePhotoInput.files[0]) return;

                const file = profilePhotoInput.files[0];
                const formData = new FormData();
                formData.append('file', file);

                try {
                    profilePhotoButton?.classList.add('is-loading');
                    const res = await fetch('/api/Profile/photos', {
                        method: 'POST',
                        headers: {
                            'Authorization': `Bearer ${api.getToken()}`
                        },
                        body: formData
                    });

                    const data = await res.json().catch(() => null);
                    if (!res.ok) {
                        throw new Error(data?.message || data?.detail || 'Ошибка загрузки фото');
                    }

                    app.notify('Фото добавлено', 'success');
                    setTimeout(() => app.navigate('profile'), 400);
                } catch (error) {
                    app.notify(error.message || 'Ошибка загрузки фото', 'error');
                } finally {
                    profilePhotoButton?.classList.remove('is-loading');
                    profilePhotoInput.value = '';
                }
            });
        }
    }
};
