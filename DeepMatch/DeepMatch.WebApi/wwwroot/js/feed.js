const feed = {
    currentCard: null,

    async render() {
        const [card, remaining] = await Promise.all([
            this.loadCard(),
            this.loadRemaining()
        ]);

        let remainingHtml = '';
        if (remaining) {
            const color = remaining.remaining <= 5 ? 'var(--primary-dark)' : 'var(--ink)';
            remainingHtml = `
                <div style="margin-bottom:15px; color:${color}; font-size:0.9rem;">
                    Свайпов сегодня: ${remaining.used} / ${remaining.limit}
                    (осталось: ${remaining.remaining})
                </div>
            `;
        }

        if (!card) {
            return `
                <div class="card feed-shell">
                    <div class="section-title-row">
                        <h2>Лента</h2>
                    </div>
                    ${remainingHtml}
                    <p style="color:#6f7785; text-align:center;">Ответов для просмотра пока нет. Зайдите позже!</p>
                </div>
            `;
        }

        this.currentCard = card;

        return `
            <div class="card feed-shell">
                <div class="section-title-row">
                    <h2>Лента</h2>
                    <span>карточка мысли</span>
                </div>
                ${remainingHtml}
                <div class="card swipe-card" id="swipeCard">
                    ${this.renderCardHTML(card)}
                </div>
            </div>
        `;
    },

    async loadRemaining() {
        try {
            return await api.get('/Swipes/remaining');
        } catch {
            return null;
        }
    },

    async loadCard() {
        try {
            return await api.get('/Feed');
        } catch (e) {
            if (e.status === 404) return null;
            throw e;
        }
    },

    async swipe(direction) {
        if (!this.currentCard) return;

        try {
            const result = await api.post('/Swipes', {
                answerId: this.currentCard.answerId,
                direction: direction
            });

            if (result.isMatch) {
                const matchedUserName = escapeHtml(result.matchedUserName || 'собеседник');
                const matchHtml = `
                    <div id="matchOverlay" style="position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(0,0,0,0.8); display:flex; align-items:center; justify-content:center; z-index:1000;" onclick="document.getElementById('matchOverlay').remove()">
                        <div style="background:var(--surface); padding:32px; border-radius:18px; text-align:center; border:1px solid rgba(203,161,212,0.35); color:var(--ink); box-shadow:0 18px 46px rgba(39,48,68,0.14);">
                            <div style="font-size:3rem;">💜</div>
                            <h1 style="color:var(--primary-dark); font-size:1.8rem; margin:10px 0;">Это мэтч!</h1>
                            <p style="margin:10px 0;">Вы и <strong>${matchedUserName}</strong> понравились друг другу!</p>
                            <button class="btn" onclick="document.getElementById('matchOverlay').remove()">Продолжить</button>
                        </div>
                    </div>
                `;
                document.body.insertAdjacentHTML('beforeend', matchHtml);
            }

            app.navigate('feed');
        } catch (e) {
            if (e.status === 429) {
                app.notify('Дневной лимит свайпов исчерпан', 'warning');
            } else if (e.data?.errors) {
                app.notify(e.data.errors.map(err => err.message).join(' '), 'error');
            } else {
                app.notify(e.message, 'error');
            }
        }
    },

    renderCardHTML(card) {
        const tagsHtml = card.tags && card.tags.length > 0
            ? `<div style="margin-top:10px;">${card.tags.map(t => `<span class="badge">${escapeHtml(t)}</span>`).join(' ')}</div>`
            : '';

        return `
            <div class="meta">Вопрос: ${escapeHtml(card.questionText)}</div>
            <h2 style="color:var(--primary-dark); margin-bottom:10px;">Категория: ${escapeHtml(card.category)}</h2>
            <p style="font-size:1.1rem; line-height:1.6; min-height:150px;">${escapeHtml(card.answerText)}</p>
            ${tagsHtml}
            <div class="meta">${new Date(card.createdAt).toLocaleDateString('ru-RU')}</div>

            <div class="swipe-buttons">
                <button class="btn btn-swipe pass" id="btnPass">👈 Пропустить</button>
                <button class="btn btn-swipe like" id="btnLike">❤️ Нравится</button>
            </div>

            <div class="report-panel">
                <button class="btn btn-secondary btn-sm" type="button" id="btnReport">Пожаловаться</button>
                <form id="reportForm" class="report-form" style="display:none;">
                    <textarea name="reason" maxlength="500" minlength="5" required placeholder="Коротко опишите причину жалобы"></textarea>
                    <div class="report-actions">
                        <button class="btn btn-sm" type="submit">Отправить</button>
                        <button class="btn btn-secondary btn-sm" type="button" id="btnCancelReport">Отмена</button>
                    </div>
                </form>
            </div>
        `;
    },

    bindSwipeButtons() {
        const btnLike = document.getElementById('btnLike');
        const btnPass = document.getElementById('btnPass');
        if (btnLike) btnLike.addEventListener('click', () => this.swipe('like'));
        if (btnPass) btnPass.addEventListener('click', () => this.swipe('pass'));

        const btnReport = document.getElementById('btnReport');
        const reportForm = document.getElementById('reportForm');
        const btnCancelReport = document.getElementById('btnCancelReport');

        if (btnReport && reportForm) {
            btnReport.addEventListener('click', () => {
                reportForm.style.display = 'grid';
                btnReport.style.display = 'none';
            });
        }

        if (btnCancelReport && reportForm && btnReport) {
            btnCancelReport.addEventListener('click', () => {
                reportForm.reset();
                reportForm.style.display = 'none';
                btnReport.style.display = 'inline-flex';
            });
        }

        if (reportForm) {
            reportForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                if (!this.currentCard?.authorUserId) return;

                try {
                    await api.post('/Reports', {
                        reportedUserId: this.currentCard.authorUserId,
                        reason: reportForm.reason.value
                    });
                    app.notify('Жалоба отправлена', 'success');
                    reportForm.reset();
                    reportForm.style.display = 'none';
                    if (btnReport) btnReport.style.display = 'inline-flex';
                } catch (error) {
                    if (error.data?.errors) {
                        app.notify(error.data.errors.map(err => err.message).join(' '), 'error');
                    } else {
                        app.notify(error.message, 'error');
                    }
                }
            });
        }
    }
};
