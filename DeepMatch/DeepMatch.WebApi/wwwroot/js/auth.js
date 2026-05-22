const auth = {
    isLoggedIn() {
        return !!localStorage.getItem('token');
    },

    getUserFromToken() {
        const token = localStorage.getItem('token');
        if (!token) return null;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const getClaim = (...names) => {
                for (const name of names) {
                    if (payload[name] !== undefined && payload[name] !== null) {
                        return payload[name];
                    }
                }
                return null;
            };

            const normalize = (value) => {
                if (value === undefined || value === null) return null;
                return value.toString().toLowerCase();
            };

            const userId = normalize(getClaim(
                'nameid',
                'nameidentifier',
                'sub',
                'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
                'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameid'
            ));
            const userName = getClaim(
                'unique_name',
                'name',
                'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
            );
            const email = getClaim(
                'email',
                'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
            );
            const role = getClaim(
                'role',
                'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
            );

            return {
                id: userId,
                email: email,
                name: userName,
                role: role
            };
        } catch {
            return null;
        }
    },

    logout() {
        localStorage.removeItem('token');
        notifications.stopPolling();
        app.updateNav();
        app.navigate('login');
    },

    renderRegister() {
        return `
            <div class="form-container">
                <h1>Регистрация</h1>
                <form id="registerForm">
                    <div class="form-group">
                        <label>Email</label>
                        <input type="email" name="email" required>
                    </div>
                    <div class="form-group">
                        <label>Имя</label>
                        <input type="text" name="userName" required minlength="3">
                    </div>
                    <div class="form-group">
                        <label>Возраст</label>
                        <input type="number" name="age" required min="16" max="120">
                    </div>
                    <div class="form-group">
                        <label>Пароль</label>
                        <input type="password" name="password" required minlength="6">
                    </div>
                    <div id="formError" class="error-box" style="display:none;"></div>
                    <button type="submit" class="btn">Зарегистрироваться</button>
                </form>
                <p style="margin-top: 18px; text-align: center; color: #6f7785;">
                    Уже есть аккаунт? <a href="#" data-page="login" style="color:var(--primary-dark);">Войти</a>
                </p>
            </div>
        `;
    },

    renderLogin() {
        return `
            <div class="form-container">
                <h1>Вход</h1>
                <form id="loginForm">
                    <div class="form-group">
                        <label>Email</label>
                        <input type="email" name="email" required>
                    </div>
                    <div class="form-group">
                        <label>Пароль</label>
                        <input type="password" name="password" required>
                    </div>
                    <div id="formError" class="error-box" style="display:none;"></div>
                    <button type="submit" class="btn">Войти</button>
                </form>
                <p style="margin-top: 18px; text-align: center; color: #6f7785;">
                    Нет аккаунта? <a href="#" data-page="register" style="color:var(--primary-dark);">Зарегистрироваться</a>
                </p>
            </div>
        `;
    },

    async handleRegister(form) {
        const errorBox = document.getElementById('formError');
        try {
            errorBox.style.display = 'none';
            const data = await api.post('/Auth/register', {
                email: form.email.value,
                userName: form.userName.value,
                age: parseInt(form.age.value, 10),
                password: form.password.value
            }, false);

            localStorage.setItem('token', data.token);
            app.updateNav();
            notifications.startPolling();
            app.navigate('daily');
        } catch (e) {
            errorBox.style.display = 'block';
            if (e.data?.errors) {
                errorBox.innerHTML = e.data.errors.map(err => `<div>${escapeHtml(err.message)}</div>`).join('');
            } else {
                errorBox.textContent = e.message;
            }
        }
    },

    async handleLogin(form) {
        const errorBox = document.getElementById('formError');
        try {
            errorBox.style.display = 'none';
            const data = await api.post('/Auth/login', {
                email: form.email.value,
                password: form.password.value
            }, false);

            localStorage.setItem('token', data.token);
            app.updateNav();
            notifications.startPolling();
            app.navigate('daily');
        } catch (e) {
            errorBox.style.display = 'block';
            if (e.data?.errors) {
                errorBox.innerHTML = e.data.errors.map(err => `<div>${escapeHtml(err.message)}</div>`).join('');
            } else {
                errorBox.textContent = e.message;
            }
        }
    }
};
