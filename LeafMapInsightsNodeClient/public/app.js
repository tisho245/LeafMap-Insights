/**
 * LeafMap Insights – клиент (Node.js сървър подава API URL чрез /api-config)
 */
const API_URL_KEY = 'leafmap_api_base_url';
const AUTH_API_URL_KEY = 'leafmap_auth_api_base_url';
const TOKEN_KEY = 'leafmap_jwt';
const DEFAULT_API_URL = 'https://localhost:7234';
const DEFAULT_AUTH_API_URL = 'https://localhost:7240';

function getApiBaseUrl() {
  return localStorage.getItem(API_URL_KEY) || DEFAULT_API_URL;
}

function getAuthApiBaseUrl() {
  const stored = localStorage.getItem(AUTH_API_URL_KEY);
  if (stored) return stored;
  return DEFAULT_AUTH_API_URL;
}

let API_BASE_URL = getApiBaseUrl();

const API_URLS = [
  { label: 'HTTPS 7234', url: 'https://localhost:7234' },
  { label: 'HTTP 5202', url: 'http://localhost:5202' }
];

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

async function api(endpoint, options = {}) {
  const base = getApiBaseUrl();
  const url = `${base.replace(/\/$/, '')}/${endpoint.replace(/^\//, '')}`;
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers
  };
  const token = getToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;

  try {
    const res = await axios({
      url,
      method: options.method || 'GET',
      headers,
      data: options.body !== undefined ? (typeof options.body === 'string' ? options.body : JSON.stringify(options.body)) : undefined
    });
    return res.data;
  } catch (e) {
    if (e.response) {
      const data = e.response.data;
      const err = new Error(data?.message || data?.title || e.response.statusText || `HTTP ${e.response.status}`);
      err.status = e.response.status;
      err.data = data;
      throw err;
    }
    const msg = (e.message === 'Network Error' || e.code === 'ERR_NETWORK')
      ? `Не може да се свърже с API (${url}). Проверете API_BASE_URL в .env`
      : (e.message || 'Грешка при заявка');
    const err = new Error(msg);
    err.cause = e;
    throw err;
  }
}

/** Заявки към Auth API (login/register, api/users за Admin). При наличие на токен изпраща Bearer за защитени endpoints. */
async function authApi(endpoint, options = {}) {
  const base = getAuthApiBaseUrl();
  const url = `${base.replace(/\/$/, '')}/${endpoint.replace(/^\//, '')}`;
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers
  };
  const token = getToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;
  try {
    const res = await axios({
      url,
      method: options.method || 'GET',
      headers,
      data: options.body !== undefined ? (typeof options.body === 'string' ? options.body : JSON.stringify(options.body)) : undefined
    });
    return res.data;
  } catch (e) {
    if (e.response) {
      const data = e.response.data;
      const err = new Error(data?.message || data?.title || e.response.statusText || `HTTP ${e.response.status}`);
      err.status = e.response.status;
      err.data = data;
      throw err;
    }
    const msg = (e.message === 'Network Error' || e.code === 'ERR_NETWORK')
      ? `Не може да се свърже с Auth API (${url}). Проверете AUTH_API_BASE_URL в .env.`
      : (e.message || 'Грешка при заявка');
    const err = new Error(msg);
    err.cause = e;
    throw err;
  }
}

async function tryFetch(baseUrl, endpoint) {
  const b = baseUrl || getApiBaseUrl();
  const url = `${b.replace(/\/$/, '')}/${endpoint.replace(/^\//, '')}`;
  const res = await axios.get(url, { headers: { 'Accept': 'application/json' }, validateStatus: () => true });
  return res;
}

async function checkApiStatus() {
  const el = document.getElementById('apiStatus');
  const helpEl = document.getElementById('apiStatusHelp');
  if (!el) return;
  const base = getApiBaseUrl();
  el.classList.remove('ok', 'error');
  el.querySelector('span:last-child').textContent = 'Проверка на връзка с API...';
  if (helpEl) helpEl.hidden = true;

  let ok = false;
  try {
    const res = await tryFetch(base, 'api/divisions');
    ok = res.status === 200;
  } catch (_) {}

  if (!ok) {
    for (const { url } of API_URLS) {
      if (url === base) continue;
      try {
        const res = await tryFetch(url, 'api/divisions');
        if (res.status === 200) {
          const u = url.replace(/\/$/, '');
          localStorage.setItem(API_URL_KEY, u);
          API_BASE_URL = u;
          ok = true;
          break;
        }
      } catch (_) {}
    }
  }

  if (ok) {
    el.classList.add('ok');
    el.querySelector('span:last-child').textContent = 'Връзка с API: OK (' + getApiBaseUrl() + ')';
  } else {
    el.classList.add('error');
    el.querySelector('span:last-child').textContent = 'Не може да се свърже с API. Задайте API_BASE_URL в .env на сървъра.';
    if (helpEl) {
      helpEl.hidden = false;
      const input = document.getElementById('apiUrlInput');
      if (input) input.value = getApiBaseUrl();
    }
  }
}

function saveApiUrlAndCheck() {
  const input = document.getElementById('apiUrlInput');
  if (!input) return;
  const url = input.value.trim().replace(/\/+$/, '');
  if (!url) return;
  API_BASE_URL = url;
  localStorage.setItem(API_URL_KEY, url);
  checkApiStatus();
}

function setApiUrl(url) {
  API_BASE_URL = url.replace(/\/$/, '');
  checkApiStatus();
}

/** Връща ролите от JWT payload (claim role). */
function getRolesFromToken() {
  const token = getToken();
  if (!token) return [];
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (Array.isArray(role)) return role;
    if (typeof role === 'string') return [role];
    return [];
  } catch (_) { return []; }
}

function isAdmin() { return getRolesFromToken().includes('Admin'); }

function updateAuthUI() {
  const token = getToken();
  const emailEl = document.getElementById('userEmail');
  const btnLogin = document.getElementById('btnLogin');
  const btnLogout = document.getElementById('btnLogout');
  const formAddTree = document.getElementById('formAddTree');
  const addTreeLoginRequired = document.getElementById('addTreeLoginRequired');
  const adminLink = document.getElementById('adminLink');

  if (token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const email = payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload.Email || 'Потребител';
      if (emailEl) emailEl.textContent = email;
    } catch (_) {
      if (emailEl) emailEl.textContent = 'Потребител';
    }
    if (btnLogin) btnLogin.hidden = true;
    if (btnLogout) btnLogout.hidden = false;
    if (formAddTree) formAddTree.hidden = false;
    if (addTreeLoginRequired) addTreeLoginRequired.hidden = true;
    if (adminLink) adminLink.hidden = !isAdmin();
  } else {
    if (emailEl) emailEl.textContent = '';
    if (btnLogin) btnLogin.hidden = false;
    if (btnLogout) btnLogout.hidden = true;
    if (formAddTree) formAddTree.hidden = true;
    if (addTreeLoginRequired) addTreeLoginRequired.hidden = false;
    if (adminLink) adminLink.hidden = true;
  }
}

function getTokenFromResponse(data) {
  if (!data) return null;
  return data.token ?? data.Token ?? null;
}

function getRedirectUrl() {
  const params = new URLSearchParams(window.location.search);
  return params.get('next') || 'index.html';
}

async function login(email, password) {
  const data = await authApi('api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) });
  const token = getTokenFromResponse(data);
  if (token) setToken(token);
  updateAuthUI();
  if (document.body.dataset.page === 'login') window.location.href = getRedirectUrl();
}

async function register(email, password, userName) {
  const body = { email, password };
  if (userName) body.userName = userName;
  const data = await authApi('api/auth/register', { method: 'POST', body: JSON.stringify(body) });
  const token = getTokenFromResponse(data);
  if (token) setToken(token);
  updateAuthUI();
  if (document.body.dataset.page === 'register') window.location.href = getRedirectUrl();
}

function renderTreeCard(tree) {
  const name = tree.name || `Дърво #${tree.id}`;
  const speciesName = tree.species?.name || tree.speciesId;
  const lat = tree.latitude != null ? tree.latitude.toFixed(5) : '';
  const lng = tree.longitude != null ? tree.longitude.toFixed(5) : '';
  return `
    <article class="card" data-id="${tree.id}">
      <div class="card-body">
        <h3 class="card-title">${escapeHtml(name)}</h3>
        <p class="card-meta">Вид: ${escapeHtml(String(speciesName))}</p>
        <p class="card-meta">Координати: ${lat}, ${lng}</p>
        ${tree.description ? `<p class="card-desc">${escapeHtml(tree.description)}</p>` : ''}
      </div>
      <div class="card-actions">
        <a href="tree.html?id=${tree.id}" class="btn btn-sm btn-primary">Детайли</a>
      </div>
    </article>
  `;
}

async function loadTrees() {
  const listEl = document.getElementById('treesList');
  const loadingEl = document.getElementById('treesLoading');
  const errorEl = document.getElementById('treesError');
  if (!listEl) return;
  listEl.innerHTML = '';
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;
  try {
    const data = await api('api/trees?includeLookups=true');
    const trees = Array.isArray(data) ? data : (data && data.data ? data.data : null);
    if (loadingEl) loadingEl.hidden = true;
    if (Array.isArray(trees) && trees.length) listEl.innerHTML = trees.map(renderTreeCard).join('');
    else listEl.innerHTML = '<p class="empty">Няма регистрирани дървета.</p>';
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.textContent = e.message || 'Грешка при зареждане.'; errorEl.hidden = false; }
  }
}

async function loadTreeDetail(id) {
  const contentEl = document.getElementById('treeDetailContent');
  const loadingEl = document.getElementById('treeDetailLoading');
  const errorEl = document.getElementById('treeDetailError');
  if (!contentEl) return;
  contentEl.innerHTML = '';
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;
  try {
    const tree = await api(`api/trees/${id}?includeLookups=true`);
    if (loadingEl) loadingEl.hidden = true;
    contentEl.innerHTML = `
      <div class="tree-detail-card">
        <h2>${escapeHtml(tree.name || `Дърво #${tree.id}`)}</h2>
        <dl class="detail-list">
          <dt>Вид</dt><dd>${escapeHtml((tree.species && tree.species.name) || tree.speciesId)}</dd>
          <dt>Род</dt><dd>${escapeHtml((tree.genus && tree.genus.name) || tree.genusId)}</dd>
          <dt>Семейство</dt><dd>${escapeHtml((tree.family && tree.family.name) || tree.familyId)}</dd>
          <dt>Ширина</dt><dd>${tree.latitude}</dd>
          <dt>Дължина</dt><dd>${tree.longitude}</dd>
          ${tree.description ? `<dt>Описание</dt><dd>${escapeHtml(tree.description)}</dd>` : ''}
        </dl>
      </div>
    `;
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.textContent = e.message || 'Грешка при зареждане.'; errorEl.hidden = false; }
  }
}

function renderTaxonomyList(items, key = 'name') {
  if (!Array.isArray(items) || !items.length) return '<p class="empty">Няма данни.</p>';
  return '<ul>' + items.map(i => `<li>${escapeHtml(i[key] || i.id)}</li>`).join('') + '</ul>';
}

async function loadTaxonomy() {
  const ids = ['taxDivisions', 'taxClasses', 'taxFamilies', 'taxGenera', 'taxSpecies'];
  const endpoints = ['api/divisions', 'api/taxonomyclasses', 'api/families', 'api/genera', 'api/species'];
  const loadingEl = document.getElementById('taxonomyLoading');
  const errorEl = document.getElementById('taxonomyError');
  const first = document.getElementById(ids[0]);
  if (!first) return;
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;
  ids.forEach(id => { const el = document.getElementById(id); if (el) el.innerHTML = ''; });
  try {
    await Promise.all(endpoints.map((ep, i) =>
      api(ep).then(data => {
        const el = document.getElementById(ids[i]);
        if (el) el.innerHTML = renderTaxonomyList(Array.isArray(data) ? data : []);
      })
    ));
    if (loadingEl) loadingEl.hidden = true;
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.textContent = e.message || 'Грешка при зареждане на таксономия.'; errorEl.hidden = false; }
  }
}

async function loadTaxonomySelects() {
  const keys = ['treeDivisionId', 'treeTaxonomyClassId', 'treeFamilyId', 'treeGenusId', 'treeSpeciesId'];
  const endpoints = ['api/divisions', 'api/taxonomyclasses', 'api/families', 'api/genera', 'api/species'];
  for (let i = 0; i < keys.length; i++) {
    const sel = document.getElementById(keys[i]);
    if (!sel) continue;
    const prev = sel.value;
    sel.innerHTML = '<option value="">— изберете —</option>';
    try {
      const list = await api(endpoints[i]);
      if (Array.isArray(list)) list.forEach(item => { const opt = document.createElement('option'); opt.value = item.id; opt.textContent = item.name || item.id; sel.appendChild(opt); });
      if (prev) sel.value = prev;
    } catch (_) {}
  }
}

async function submitAddTree(e) {
  e.preventDefault();
  const successEl = document.getElementById('addTreeSuccess');
  const errorEl = document.getElementById('addTreeError');
  successEl.hidden = true;
  errorEl.hidden = true;
  const payload = {
    name: document.getElementById('treeName').value.trim() || 'Без име',
    latitude: parseFloat(document.getElementById('treeLat').value),
    longitude: parseFloat(document.getElementById('treeLng').value),
    description: document.getElementById('treeDescription').value.trim() || null,
    divisionId: parseInt(document.getElementById('treeDivisionId').value, 10),
    taxonomyClassId: parseInt(document.getElementById('treeTaxonomyClassId').value, 10),
    familyId: parseInt(document.getElementById('treeFamilyId').value, 10),
    genusId: parseInt(document.getElementById('treeGenusId').value, 10),
    speciesId: parseInt(document.getElementById('treeSpeciesId').value, 10)
  };
  try {
    await api('api/trees', { method: 'POST', body: JSON.stringify(payload) });
    successEl.textContent = 'Дървото е добавено успешно.';
    successEl.hidden = false;
    document.getElementById('formAddTree').reset();
  } catch (err) {
    errorEl.textContent = err.message || (err.data && (Array.isArray(err.data) ? err.data.join(' ') : err.data)) || 'Грешка при запазване.';
    errorEl.hidden = false;
  }
}

function highlightNav() {
  const current = window.location.pathname.replace(/^\//, '').split('/').pop() || 'index.html';
  document.querySelectorAll('.nav-link').forEach(a => {
    const href = (a.getAttribute('href') || '').replace(/^\.\//, '');
    a.classList.toggle('active', href === current || (current === '' && href === 'index.html'));
  });
}

function escapeHtml(s) {
  const div = document.createElement('div');
  div.textContent = s;
  return div.innerHTML;
}

function runInit() {
  API_BASE_URL = getApiBaseUrl();
  updateAuthUI();
  highlightNav();
  if (document.getElementById('apiStatus')) checkApiStatus();
  if (document.getElementById('treesList')) loadTrees();
  if (document.getElementById('taxDivisions')) loadTaxonomy();
  if (document.getElementById('treeDetailContent')) {
    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');
    if (id) loadTreeDetail(id);
  }
  if (document.getElementById('formAddTree')) {
    loadTaxonomySelects();
    document.getElementById('formAddTree').addEventListener('submit', submitAddTree);
  }
  const btnLogout = document.getElementById('btnLogout');
  if (btnLogout) btnLogout.addEventListener('click', () => { setToken(null); updateAuthUI(); if (document.body.dataset.page === 'login' || document.body.dataset.page === 'register') window.location.href = 'index.html'; });
  const formLogin = document.getElementById('formLogin');
  if (formLogin) formLogin.addEventListener('submit', async e => {
    e.preventDefault();
    const errEl = document.getElementById('loginError');
    if (errEl) errEl.hidden = true;
    try { await login(document.getElementById('loginEmail').value, document.getElementById('loginPassword').value); }
    catch (err) { if (errEl) { errEl.textContent = err.message || 'Грешка при вход.'; errEl.hidden = false; } }
  });
  const formRegister = document.getElementById('formRegister');
  if (formRegister) formRegister.addEventListener('submit', async e => {
    e.preventDefault();
    const errEl = document.getElementById('registerError');
    if (errEl) errEl.hidden = true;
    try {
      await register(
        document.getElementById('registerEmail').value,
        document.getElementById('registerPassword').value,
        document.getElementById('registerUserName') ? document.getElementById('registerUserName').value.trim() || null : null
      );
    } catch (err) { if (errEl) { errEl.textContent = (err.data && Array.isArray(err.data) ? err.data.join(' ') : err.message) || 'Грешка при регистрация.'; errEl.hidden = false; } }
  });
  const apiUrlSave = document.getElementById('apiUrlSave');
  if (apiUrlSave) apiUrlSave.addEventListener('click', saveApiUrlAndCheck);
  const apiUrlInput = document.getElementById('apiUrlInput');
  if (apiUrlInput) {
    apiUrlInput.value = getApiBaseUrl();
    apiUrlInput.addEventListener('keydown', e => { if (e.key === 'Enter') saveApiUrlAndCheck(); });
  }
}

document.addEventListener('DOMContentLoaded', () => {
  if (window.location.protocol !== 'file:') {
    axios.get('/api-config').then(r => {
      if (r.data) {
        if (r.data.apiBaseUrl) localStorage.setItem(API_URL_KEY, r.data.apiBaseUrl);
        if (r.data.authApiBaseUrl) localStorage.setItem(AUTH_API_URL_KEY, r.data.authApiBaseUrl);
      }
    }).catch(() => {}).finally(runInit);
  } else {
    runInit();
  }
});
