const app = {
    pages: {
        login: {
            render: () => auth.renderLogin(),
            title: 'Вход'
        },
        register: {
            render: () => auth.renderRegister(),
            title: 'Регистрация'
        },
        daily: {
            render: () => app.renderDaily(),
            title: 'Вопрос дня',
            auth: true
        },
        feed: {
            render: () => feed.render(),
            title: 'Лента',
            auth: true,
            afterRender: () => feed.bindSwipeButtons()
        },
        profile: {
            render: () => profile.render(),
            title: 'Профиль',
            auth: true,
            afterRender: () => profile.afterRender()
        },
        matches: {
            render: () => chat.renderList(),
            title: 'Мэтчи',
            auth: true,
            afterRender: () => chat.bindListEvents()
        },
        rules: {
            render: () => app.renderRules(),
            title: 'Правила'
        },
        admin: {
            render: () => admin.render(),
            title: 'Админ',
            auth: true,
            admin: true,
            afterRender: () => admin.afterRender()
        }
    },

    currentPage: 'login',

    navigate(page) {
        if (this.pages[page]) {
            this.currentPage = page;
            this.renderPage();
        }
    },

    notify(message, type = 'info') {
        let container = document.getElementById('toastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toastContainer';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        container.appendChild(toast);

        setTimeout(() => toast.classList.add('show'), 10);
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 220);
        }, 3200);
    },

    openPhotoViewer(photos, startIndex = 0) {
        if (!photos || photos.length === 0) return;

        let currentIndex = startIndex;
        const overlay = document.createElement('div');
        overlay.className = 'photo-viewer-overlay';

        const render = () => {
            overlay.innerHTML = `
                <button class="photo-viewer-close" type="button" aria-label="Закрыть">×</button>
                <button class="photo-viewer-nav photo-viewer-prev" type="button" aria-label="Предыдущее фото">‹</button>
                <img src="${escapeAttribute(photos[currentIndex])}" alt="Фото профиля">
                <button class="photo-viewer-nav photo-viewer-next" type="button" aria-label="Следующее фото">›</button>
                <div class="photo-viewer-counter">${currentIndex + 1} / ${photos.length}</div>
            `;

            overlay.querySelector('.photo-viewer-close')?.addEventListener('click', () => overlay.remove());
            overlay.querySelector('.photo-viewer-prev')?.addEventListener('click', (e) => {
                e.stopPropagation();
                currentIndex = (currentIndex - 1 + photos.length) % photos.length;
                render();
            });
            overlay.querySelector('.photo-viewer-next')?.addEventListener('click', (e) => {
                e.stopPropagation();
                currentIndex = (currentIndex + 1) % photos.length;
                render();
            });
        };

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) overlay.remove();
        });
        document.addEventListener('keydown', function handleKey(e) {
            if (!document.body.contains(overlay)) {
                document.removeEventListener('keydown', handleKey);
                return;
            }
            if (e.key === 'Escape') overlay.remove();
            if (e.key === 'ArrowLeft') {
                currentIndex = (currentIndex - 1 + photos.length) % photos.length;
                render();
            }
            if (e.key === 'ArrowRight') {
                currentIndex = (currentIndex + 1) % photos.length;
                render();
            }
        });

        render();
        document.body.appendChild(overlay);
    },

    async renderPage() {
        const page = this.pages[this.currentPage];

        if (page.auth && !auth.isLoggedIn()) {
            this.currentPage = 'login';
            await this.renderPage();
            return;
        }

        const currentUser = auth.getUserFromToken();
        if (page.admin && currentUser?.role !== 'Admin') {
            this.currentPage = 'daily';
            app.notify('Доступ только для администратора', 'warning');
            await this.renderPage();
            return;
        }

        if ((this.currentPage === 'login' || this.currentPage === 'register') && auth.isLoggedIn()) {
            this.currentPage = 'daily';
            await this.renderPage();
            return;
        }

        document.title = `${page.title} — DeepMatch`;
        const appRoot = document.getElementById('app');
        if (!appRoot) return;

        const content = await page.render();
        appRoot.innerHTML = content;

        this.bindEvents();
        this.bindNavigation();

        if (page.afterRender) {
            page.afterRender();
        }
    },

    bindEvents() {
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', (e) => {
                e.preventDefault();
                auth.handleRegister(registerForm);
            });
        }

        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', (e) => {
                e.preventDefault();
                auth.handleLogin(loginForm);
            });
        }

        const answerForm = document.getElementById('answerForm');
        if (answerForm) {
            // Кнопка адвоката дьявола
            const btnDevilsAdvocate = document.getElementById('btnDevilsAdvocate');
            if (btnDevilsAdvocate) {
                btnDevilsAdvocate.addEventListener('click', async (e) => {
                    e.preventDefault();
                    const answerText = answerForm.answerText.value.trim();
                    if (!answerText) {
                        app.notify('Сначала напишите ответ', 'warning');
                        return;
                    }

                    const question = await api.get('/Questions/daily');
                    btnDevilsAdvocate.disabled = true;
                    btnDevilsAdvocate.textContent = '⏳ Генерирую...';

                    try {
                        const response = await api.post('/AiAssistant/devils-advocate', {
                            questionText: question.text,
                            userAnswer: answerText
                        });

                        const adviceBox = document.getElementById('devilsAdvice');
                        const adviceText = document.getElementById('devilsAdviceText');
                        if (adviceBox && adviceText) {
                            adviceText.textContent = response.alternativeView;
                            adviceBox.style.display = 'block';
                        }
                    } catch (e) {
                        app.notify('Не удалось вызвать адвоката дьявола: ' + e.message, 'error');
                    } finally {
                        btnDevilsAdvocate.disabled = false;
                        btnDevilsAdvocate.textContent = '👿 Адвокат дьявола';
                    }
                });
            }

            // Отправка ответа
            answerForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                const errorBox = document.getElementById('answerError');
                const successBox = document.getElementById('answerSuccess');
                if (errorBox) errorBox.style.display = 'none';
                if (successBox) successBox.style.display = 'none';

                try {
                    const questionId = answerForm.dataset.questionId;
                    await api.post('/Answers', {
                        questionId,
                        text: answerForm.answerText.value
                    });

                    if (successBox) {
                        successBox.textContent = 'Ответ сохранён!';
                        successBox.style.display = 'block';
                    }
                    answerForm.reset();
                } catch (e) {
                    if (errorBox) {
                        errorBox.style.display = 'block';
                        if (e.data?.errors) {
                            errorBox.innerHTML = e.data.errors.map(err => `<div>${escapeHtml(err.message)}</div>`).join('');
                        } else {
                            errorBox.textContent = e.message;
                        }
                    }
                }
            });
        }
    },

    bindNavigation() {
        document.querySelectorAll('[data-page]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = link.dataset.page;
                if (page) {
                    this.navigate(page);
                }
            });
        });

        const logoutBtn = document.getElementById('logoutBtn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();
                auth.logout();
            });
        }

        const notifBtn = document.getElementById('notificationsBtn');
        if (notifBtn) {
            notifBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                notifications.showPanel();
            });
        }
    },

    updateNav() {
        const navLinks = document.getElementById('navLinks');
        const navAuth = document.getElementById('navAuth');
        const user = auth.getUserFromToken();

        if (auth.isLoggedIn() && user) {
            if (navLinks) navLinks.style.display = 'flex';
            if (navAuth) {
                const adminLink = user.role === 'Admin'
                    ? '<a href="#" data-page="admin">Админ</a>'
                    : '';
                navAuth.innerHTML = `
                <div class="user-panel">
                    <img src="/api/Profile/avatar/${user.id}" class="avatar-xs" 
                         onerror="this.src='${app.defaultAvatarDataUri()}'">
                    <span class="user-panel-name">${escapeHtml(user.name)}</span>
                    <a href="#" id="notificationsBtn" class="notification-button" title="Уведомления">
                        🔔
                        <span id="notificationBadge" class="notification-badge" style="display:none;">0</span>
                    </a>
                </div>
                ${adminLink}
                <a href="#" id="logoutBtn">Выйти</a>
            `;
            }
        } else {
            if (navLinks) navLinks.style.display = 'none';
            if (navAuth) navAuth.innerHTML = `
                <a href="#" data-page="rules">Правила</a>
                <a href="#" data-page="login">Войти</a>
            `;
        }
    },

    async renderDaily() {
        try {
            const question = await api.get('/Questions/daily');
            return `
                <div class="daily-layout">
                    <section class="card daily-hero">
                        <div class="hero-copy">
                            <span class="section-kicker">Вопрос дня</span>
                            <h1>${escapeHtml(question.text)}</h1>
                            <p class="meta">Категория: ${escapeHtml(question.category)}</p>
                        </div>
                        <div class="hero-art" aria-hidden="true">
                            <span></span>
                            <span></span>
                            <span></span>
                        </div>
                    </section>
                    <section class="card answer-compose">
                    <h2>Ваш ответ</h2>
                    <form id="answerForm" data-question-id="${question.id}">
                        <div class="form-group">
                            <textarea name="answerText" placeholder="Поделитесь своими мыслями..." required minlength="10" maxlength="5000"></textarea>
                        </div>
                        <button type="button" class="btn btn-secondary" id="btnDevilsAdvocate" style="margin-bottom:10px; width:100%;">
                            👿 Адвокат дьявола
                        </button>
                        <div id="devilsAdvice" style="margin-bottom:10px; padding:12px; background:var(--accent-soft); border-radius:14px; display:none; border-left:4px solid var(--primary);">
                            <p style="color:#263449; font-style:italic; margin:0;" id="devilsAdviceText"></p>
                        </div>
                        <div id="answerError" class="error-box" style="display:none;"></div>
                        <div id="answerSuccess" class="success" style="display:none; margin-bottom: 12px;"></div>
                        <button type="submit" class="btn">Ответить</button>
                    </form>
                    </section>
                </div>
            `;
        } catch (e) {
            if (e.status === 404) {
                return `
                    <div class="card">
                        <h2>Вопрос дня</h2>
                        <p style="color:#6f7785;">Вопрос дня ещё не назначен. Зайдите позже.</p>
                    </div>
                `;
            }
            return `<div class="error-box">Ошибка загрузки: ${e.message}</div>`;
        }
    },

    renderRules() {
        return `
            <section class="rules-page">
                <div class="card rules-hero">
                    <span class="section-kicker">Правила DeepMatch</span>
                    <h1>Здесь ценятся мысли, уважение и честный интерес</h1>
                    <p>Рейтинг растёт за активность, которая помогает находить людей по взглядам, а не только по анкете.</p>
                </div>

                <div class="rules-grid">
                    <article class="rules-card rules-card-accent">
                        <h2>Как начисляется рейтинг</h2>
                        <ul class="rules-list">
                            <li><strong>+5</strong> за новый ответ на вопрос дня.</li>
                            <li><strong>+20</strong> каждому участнику за взаимный мэтч.</li>
                            <li><strong>+10</strong> за каждый новый бейдж.</li>
                        </ul>
                    </article>

                    <article class="rules-card">
                        <h2>Что дают баллы</h2>
                        <ul class="rules-list">
                            <li>Базовый дневной лимит: <strong>30 свайпов</strong>.</li>
                            <li>Каждые <strong>100 рейтинга</strong> дают ещё 1 свайп в день.</li>
                            <li>Каждый бейдж тоже добавляет <strong>+1 свайп</strong> к дневному лимиту.</li>
                        </ul>
                    </article>

                    <article class="rules-card">
                        <h2>Бейджи</h2>
                        <ul class="rules-list">
                            <li><strong>Логик</strong> и <strong>Эмпат</strong>: 10 ответов с соответствующими AI-тегами.</li>
                            <li><strong>Эрудит</strong>: ответы в 5 разных категориях.</li>
                            <li><strong>Мастер дебатов</strong>: 20 лайков на ответах.</li>
                            <li><strong>Активный участник</strong>: ответы 7 дней подряд.</li>
                            <li><strong>Создатель связей</strong>: 5 взаимных мэтчей.</li>
                        </ul>
                    </article>

                    <article class="rules-card">
                        <h2>Правила общения</h2>
                        <ul class="rules-list">
                            <li>Пишите свои мысли честно и по теме вопроса.</li>
                            <li>Не оскорбляйте других участников и не публикуйте чужие личные данные.</li>
                            <li>Жалобы помогают модерации: после 5 жалоб профиль может быть заблокирован автоматически.</li>
                        </ul>
                    </article>
                </div>
            </section>
        `;
    },

    init() {
        this.updateNav();
        this.navigate(auth.isLoggedIn() ? 'daily' : 'login');

        if (auth.isLoggedIn()) {
            notifications.startPolling();
        }
    },

    defaultAvatarDataUri() {
        return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Crect width='100' height='100' rx='50' fill='%23cba1d4'/%3E%3Ccircle cx='50' cy='38' r='16' fill='%23ffffcd'/%3E%3Cpath d='M22 84c5-20 19-30 28-30s23 10 28 30' fill='%23ffffcd'/%3E%3C/svg%3E";
    }
};

document.addEventListener('DOMContentLoaded', () => {
    app.init();
});
