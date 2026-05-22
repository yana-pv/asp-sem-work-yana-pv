const admin = {
    categories: ['Philosophy', 'Ethics', 'Life', 'Relationships', 'Society', 'SelfDiscovery'],

    async render() {
        const user = auth.getUserFromToken();
        if (user?.role !== 'Admin') {
            return '<div class="error-box">Доступ только для администратора</div>';
        }

        const [stats, questions, users, reports] = await Promise.all([
            api.get('/admin/AdminStats'),
            api.get('/admin/AdminQuestions'),
            api.get('/admin/AdminUsers'),
            api.get('/admin/AdminReports')
        ]);

        return `
            <div class="admin-page">
                <div class="card admin-hero">
                    <div>
                        <p class="meta">Панель администратора</p>
                        <h2>Управление DeepMatch</h2>
                    </div>
                    <div class="admin-tabs" role="tablist">
                        <button class="admin-tab active" data-admin-tab="stats">Статистика</button>
                        <button class="admin-tab" data-admin-tab="questions">Вопросы</button>
                        <button class="admin-tab" data-admin-tab="users">Пользователи</button>
                        <button class="admin-tab" data-admin-tab="reports">Жалобы</button>
                    </div>
                </div>

                <section class="admin-section active" data-admin-section="stats">
                    ${this.renderStats(stats)}
                </section>
                <section class="admin-section" data-admin-section="questions">
                    ${this.renderQuestions(questions)}
                </section>
                <section class="admin-section" data-admin-section="users">
                    ${this.renderUsers(users)}
                </section>
                <section class="admin-section" data-admin-section="reports">
                    ${this.renderReports(reports)}
                </section>
            </div>
        `;
    },

    renderStats(stats) {
        const cards = [
            ['Пользователи', stats.usersCount],
            ['Активные', stats.activeUsersCount],
            ['Заблокированы', stats.blockedUsersCount],
            ['Вопросы', stats.questionsCount],
            ['Ответы', stats.answersCount],
            ['Мэтчи', stats.matchesCount],
            ['Жалобы', stats.reportsCount]
        ];

        return `
            <div class="admin-grid">
                ${cards.map(([label, value]) => `
                    <div class="admin-stat">
                        <span>${escapeHtml(label)}</span>
                        <strong>${value}</strong>
                    </div>
                `).join('')}
            </div>
        `;
    },

    renderQuestions(questions) {
        return `
            <div class="card">
                <h2>Добавить вопрос</h2>
                <form id="adminQuestionForm" class="admin-form">
                    <div class="form-group">
                        <label>Текст вопроса</label>
                        <textarea name="text" required minlength="10" maxlength="2000" placeholder="Например: что делает разговор настоящим?"></textarea>
                    </div>
                    <div class="form-group">
                        <label>Категория</label>
                        <select name="category" required>
                            ${this.categories.map(c => `<option value="${c}">${c}</option>`).join('')}
                        </select>
                    </div>
                    <button class="btn" type="submit">Добавить вопрос</button>
                </form>
            </div>

            <div class="card">
                <h2>Вопросы</h2>
                <div class="admin-list">
                    ${questions.map(q => `
                        <article class="admin-list-item ${q.isActive ? '' : 'is-muted'}">
                            <div>
                                <strong>${escapeHtml(q.text)}</strong>
                                <p class="meta">
                                    ${escapeHtml(q.category)} · ответов: ${q.answersCount}
                                    ${q.dateOfDay ? ` · вопрос дня: ${escapeHtml(q.dateOfDay)}` : ''}
                                    ${q.isActive ? '' : ' · удалён'}
                                </p>
                            </div>
                            ${q.isActive ? `<button class="btn btn-secondary btn-sm" data-delete-question="${q.id}">Удалить</button>` : ''}
                        </article>
                    `).join('') || '<p class="meta">Вопросов пока нет</p>'}
                </div>
            </div>
        `;
    },

    renderUsers(users) {
        return `
            <div class="card">
                <h2>Пользователи</h2>
                <div class="admin-list">
                    ${users.map(u => `
                        <article class="admin-list-item ${u.isBlocked ? 'is-muted' : ''}">
                            <div>
                                <strong>${escapeHtml(u.userName)}</strong>
                                <p class="meta">
                                    ${escapeHtml(u.email)} · ${escapeHtml(u.role)} · рейтинг: ${u.rating}
                                    · жалоб: ${u.reportsCount}
                                    ${u.isBlocked ? ` · заблокирован: ${escapeHtml(u.blockReason || 'без причины')}` : ''}
                                </p>
                            </div>
                            ${u.role !== 'Admin' && !u.isBlocked ? `<button class="btn btn-secondary btn-sm" data-block-user="${u.id}">Заблокировать</button>` : ''}
                        </article>
                    `).join('') || '<p class="meta">Пользователей пока нет</p>'}
                </div>
            </div>
        `;
    },

    renderReports(reports) {
        return `
            <div class="card">
                <h2>Жалобы</h2>
                <div class="admin-list">
                    ${reports.map(r => `
                        <article class="admin-list-item ${r.isBlocked ? 'is-muted' : ''}">
                            <div>
                                <strong>${escapeHtml(r.reportedName)}</strong>
                                <p class="meta">Жалоба от ${escapeHtml(r.reporterName)} · ${new Date(r.createdAt).toLocaleDateString('ru-RU')}</p>
                                <p>${escapeHtml(r.reason)}</p>
                                <p class="meta">Всего жалоб: ${r.reportsCount} · ${r.isBlocked ? 'заблокирован' : 'активен'}</p>
                            </div>
                            ${!r.isBlocked ? `<button class="btn btn-secondary btn-sm" data-block-user="${r.reportedUserId}">Заблокировать</button>` : ''}
                        </article>
                    `).join('') || '<p class="meta">Жалоб пока нет</p>'}
                </div>
            </div>
        `;
    },

    afterRender() {
        document.querySelectorAll('.admin-tab').forEach(tab => {
            tab.addEventListener('click', () => this.switchTab(tab.dataset.adminTab));
        });

        const questionForm = document.getElementById('adminQuestionForm');
        if (questionForm) {
            questionForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                await this.createQuestion(questionForm);
            });
        }

        document.querySelectorAll('[data-delete-question]').forEach(button => {
            button.addEventListener('click', () => this.deleteQuestion(button.dataset.deleteQuestion));
        });

        document.querySelectorAll('[data-block-user]').forEach(button => {
            button.addEventListener('click', () => this.blockUser(button.dataset.blockUser));
        });
    },

    switchTab(name) {
        document.querySelectorAll('.admin-tab').forEach(tab => {
            tab.classList.toggle('active', tab.dataset.adminTab === name);
        });
        document.querySelectorAll('.admin-section').forEach(section => {
            section.classList.toggle('active', section.dataset.adminSection === name);
        });
    },

    async createQuestion(form) {
        try {
            await api.post('/admin/AdminQuestions', {
                text: form.text.value,
                category: form.category.value
            });
            app.notify('Вопрос добавлен', 'success');
            await app.renderPage();
            this.switchTab('questions');
        } catch (e) {
            this.showError(e);
        }
    },

    async deleteQuestion(questionId) {
        try {
            await api.post(`/admin/AdminQuestions/${questionId}/delete`, {});
            app.notify('Вопрос удалён', 'success');
            await app.renderPage();
            this.switchTab('questions');
        } catch (e) {
            this.showError(e);
        }
    },

    async blockUser(userId) {
        try {
            await api.post(`/admin/AdminUsers/${userId}/block`, {
                reason: 'Блокировка администратором'
            });
            app.notify('Пользователь заблокирован', 'success');
            await app.renderPage();
        } catch (e) {
            this.showError(e);
        }
    },

    showError(e) {
        if (e.data?.errors) {
            app.notify(e.data.errors.map(err => err.message).join(' '), 'error');
            return;
        }
        app.notify(e.message || 'Ошибка выполнения действия', 'error');
    }
};
