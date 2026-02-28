/**
 * LeafMap Insights – статичен HTML/JS клиент към API
 * API адресите: от /api-config при сервиране от Node, иначе същият хост като страницата (hostname:5202/:5203).
 */
const API_URL_KEY = 'leafmap_api_base_url';
const AUTH_API_URL_KEY = 'leafmap_auth_api_base_url';
const TOKEN_KEY = 'leafmap_jwt';

const DATA_API_PORT = 5202;
const AUTH_API_PORT = 5203;

/** Начална точка на картата (България). */
const DEFAULT_MAP_CENTER = [42.65690447620973, 24.746039419748605];

/** Икона за маркер „дърво” на картата (вместо синята стрелка). */
function getTreeMarkerIcon() {
  if (typeof L === 'undefined') return {};
  return L.divIcon({
    className: 'tree-marker-icon',
    html: '<span class="tree-marker-emoji" aria-hidden="true">🌳</span>',
    iconSize: [32, 32],
    iconAnchor: [16, 32],
    popupAnchor: [0, -32]
  });
}

function getDefaultApiBaseUrl() {
  if (typeof window === 'undefined' || window.location.protocol === 'file:') return '';
  return 'http://' + window.location.hostname + ':' + DATA_API_PORT;
}

function getApiBaseUrl() {
  return localStorage.getItem(API_URL_KEY) || getDefaultApiBaseUrl();
}

function getAuthApiBaseUrl() {
  const stored = localStorage.getItem(AUTH_API_URL_KEY);
  if (stored) return stored;
  const base = getApiBaseUrl();
  return base ? base.replace(/:5202$/, ':5203') : ('http://' + (typeof window !== 'undefined' ? window.location.hostname : '') + ':' + AUTH_API_PORT);
}

let API_BASE_URL = getApiBaseUrl();

/** При зареждане: опит за /api-config; при неуспех – hostname:5202 и :5203. */
async function bootstrapApiConfig() {
  if (window.location.protocol === 'file:') return;
  const host = window.location.hostname;
  const fallbackData = 'http://' + host + ':' + DATA_API_PORT;
  const fallbackAuth = 'http://' + host + ':' + AUTH_API_PORT;
  try {
    const res = await fetch(window.location.origin + '/api-config', { method: 'GET', headers: { Accept: 'application/json' } });
    if (res.ok) {
      const data = await res.json();
      if (data.apiBaseUrl) {
        localStorage.setItem(API_URL_KEY, data.apiBaseUrl);
        API_BASE_URL = data.apiBaseUrl;
      }
      if (data.authApiBaseUrl) localStorage.setItem(AUTH_API_URL_KEY, data.authApiBaseUrl);
      return;
    }
  } catch (_) { /* няма /api-config (различен сървър) */ }
  if (!localStorage.getItem(API_URL_KEY)) {
    localStorage.setItem(API_URL_KEY, fallbackData);
    localStorage.setItem(AUTH_API_URL_KEY, fallbackAuth);
    API_BASE_URL = fallbackData;
  }
}

const API_URLS = [];

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

/** Дали текущият потребител е с роля Admin (от JWT). */
function isAdmin() {
  const token = getToken();
  if (!token) return false;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (Array.isArray(role)) return role.some(r => r === 'Admin');
    return role === 'Admin';
  } catch (_) { return false; }
}

/** Нормализира отговор от API до масив (поддържа { data: [] } или директно масив). */
function asArray(data) {
  if (Array.isArray(data)) return data;
  if (data && Array.isArray(data.data)) return data.data;
  if (data && Array.isArray(data.items)) return data.items;
  return [];
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
      ? `Не може да се свърже с API (${url}). Задайте правилния адрес на началната страница.`
      : (e.message || 'Грешка при заявка');
    const err = new Error(msg);
    err.cause = e;
    throw err;
  }
}

/** Заявки към Auth API (login/register, api/users за админ). Използва отделен URL; изпраща JWT ако има. */
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
      let msg = data?.message ?? data?.title ?? e.response.statusText;
      if (typeof data === 'string') msg = data;
      else if (Array.isArray(data)) msg = data.join(' ');
      if (!msg) msg = `HTTP ${e.response.status}`;
      const err = new Error(msg);
      err.status = e.response.status;
      err.data = data;
      throw err;
    }
    const msg = (e.message === 'Network Error' || e.code === 'ERR_NETWORK')
      ? `Не може да се свърже с Auth API (${url}). Проверете адреса и дали Auth API работи.`
      : (e.message || 'Грешка при заявка');
    const err = new Error(msg);
    err.cause = e;
    throw err;
  }
}

// ---------- Проверка на API ----------
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

  if (ok) {
    el.classList.add('ok');
    el.querySelector('span:last-child').textContent = 'Връзка с API: OK (' + getApiBaseUrl() + ')';
  } else {
    el.classList.add('error');
    el.querySelector('span:last-child').textContent = 'Не може да се свърже с API. Задайте адреса по-долу.';
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
  const authInput = document.getElementById('authApiUrlInput');
  if (authInput) {
    const authUrl = authInput.value.trim().replace(/\/+$/, '');
    if (authUrl) {
      localStorage.setItem(AUTH_API_URL_KEY, authUrl);
    } else {
      localStorage.setItem(AUTH_API_URL_KEY, url);
    }
  }
  checkApiStatus();
}

function setApiUrl(url) {
  API_BASE_URL = url.replace(/\/$/, '');
  checkApiStatus();
}

// ---------- Auth ----------
function updateAuthUI() {
  const token = getToken();
  const navAuth = document.querySelector('.nav-auth') || document.getElementById('navAuth');
  const emailEl = document.getElementById('userEmail');
  const authBadge = document.getElementById('authBadge');
  const authStatusText = document.querySelector('.auth-status-text');
  const btnLogin = document.getElementById('btnLogin');
  const btnRegister = document.getElementById('btnRegister');
  const btnLogout = document.getElementById('btnLogout');
  const formAddTree = document.getElementById('formAddTree');
  const addTreeLoginRequired = document.getElementById('addTreeLoginRequired');

  if (navAuth) {
    navAuth.classList.remove('auth-logged-in', 'auth-guest');
    navAuth.classList.add(token ? 'auth-logged-in' : 'auth-guest');
  }
  if (authBadge) authBadge.hidden = !token;
  if (authStatusText) authStatusText.hidden = !!token;

  if (token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const username = payload.name ?? payload.UserName ?? payload.preferred_username ?? payload.unique_name ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? payload.email ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? payload.Email ?? 'Потребител';
      if (emailEl) {
        emailEl.textContent = username;
        emailEl.setAttribute('title', 'Влезли сте като: ' + username);
      }
    } catch (_) {
      if (emailEl) emailEl.textContent = 'Потребител';
    }
    if (btnLogin) btnLogin.hidden = true;
    if (btnRegister) btnRegister.hidden = true;
    if (btnLogout) btnLogout.hidden = false;
    if (formAddTree) {
      formAddTree.hidden = false;
      if (document.getElementById('mapPicker') && typeof L !== 'undefined') {
        if (!mapPickerMap) initMapPicker();
        else if (typeof mapPickerMap.invalidateSize === 'function') mapPickerMap.invalidateSize();
      }
    }
    if (addTreeLoginRequired) addTreeLoginRequired.hidden = true;
  } else {
    if (emailEl) emailEl.textContent = '';
    if (btnLogin) btnLogin.hidden = false;
    if (btnRegister) btnRegister.hidden = false;
    if (btnLogout) btnLogout.hidden = true;
    if (formAddTree) formAddTree.hidden = true;
    if (addTreeLoginRequired) addTreeLoginRequired.hidden = false;
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
  const base = getAuthApiBaseUrl();
  if (!base || !base.startsWith('http')) throw new Error('Задайте Auth API адрес на началната страница.');
  const data = await authApi('api/auth/login', {
    method: 'POST',
    body: { Email: (email || '').trim(), Password: password || '' }
  });
  const token = getTokenFromResponse(data);
  if (token) setToken(token);
  const modal = document.getElementById('modalAuth');
  if (modal) modal.hidden = true;
  updateAuthUI();
  if (document.body.dataset.page === 'login') window.location.href = getRedirectUrl();
}

async function register(email, password, userName) {
  const body = { Email: (email || '').trim(), Password: password || '' };
  if (userName) body.UserName = userName.trim();
  const data = await authApi('api/auth/register', {
    method: 'POST',
    body
  });
  const token = getTokenFromResponse(data);
  if (token) setToken(token);
  const modal = document.getElementById('modalAuth');
  if (modal) modal.hidden = true;
  updateAuthUI();
  if (document.body.dataset.page === 'register') window.location.href = getRedirectUrl();
}

// ---------- Дървета (пагинирано от API – без замръзване) ----------
const TREES_PAGE_SIZE = 24;
let treesTotal = 0;
let treesLoadedCount = 0;

function renderTreeCard(tree, showAdminActions = false) {
  const name = tree.name || `Дърво #${tree.id}`;
  const speciesName = tree.species?.name || tree.speciesId;
  const lat = tree.latitude != null ? tree.latitude.toFixed(5) : '';
  const lng = tree.longitude != null ? tree.longitude.toFixed(5) : '';
  const adminBtns = showAdminActions ? `
    <button type="button" class="btn btn-sm btn-ghost btn-edit-tree" data-id="${tree.id}" title="Редактирай">✎</button>
    <button type="button" class="btn btn-sm btn-ghost btn-delete-tree" data-id="${tree.id}" data-name="${escapeHtml(name)}" title="Изтрий">✕</button>
  ` : '';
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
        ${adminBtns}
      </div>
    </article>
  `;
}

function updateTreesLoadMoreUI() {
  const wrap = document.getElementById('treesLoadMoreWrap');
  const btn = document.getElementById('treesLoadMore');
  const countEl = document.getElementById('treesCount');
  if (!wrap) return;
  if (treesTotal === 0) { wrap.hidden = true; return; }
  wrap.hidden = false;
  if (countEl) countEl.textContent = `Показани ${treesLoadedCount} от ${treesTotal}`;
  if (btn) {
    btn.hidden = treesLoadedCount >= treesTotal;
    btn.textContent = 'Покажи още';
  }
  if (treesLoadedCount >= treesTotal && countEl) countEl.textContent = `Всички ${treesTotal} дървета`;
}

async function loadTrees(append = false) {
  const listEl = document.getElementById('treesList');
  const loadingEl = document.getElementById('treesLoading');
  const errorEl = document.getElementById('treesError');
  const loadMoreWrap = document.getElementById('treesLoadMoreWrap');
  if (!listEl) return;
  if (!append) {
    listEl.innerHTML = '';
    treesTotal = 0;
    treesLoadedCount = 0;
    if (loadMoreWrap) loadMoreWrap.hidden = true;
    if (loadingEl) loadingEl.hidden = false;
    if (errorEl) errorEl.hidden = true;
  }
  try {
    let items = [];
    let total = 0;
    try {
      const data = await api(`api/trees/Paged?skip=${treesLoadedCount}&take=${TREES_PAGE_SIZE}&includeLookups=true`);
      items = data.items || data.Items || [];
      total = data.total ?? data.Total ?? 0;
    } catch (pagedErr) {
      if (pagedErr.status === 404 && !append) {
        const all = asArray(await api('api/trees?includeLookups=true'));
        total = all.length;
        items = all.slice(treesLoadedCount, treesLoadedCount + TREES_PAGE_SIZE);
      } else throw pagedErr;
    }
    if (!append) {
      if (loadingEl) loadingEl.hidden = true;
      if (errorEl) errorEl.hidden = true;
    }
    treesTotal = total;
    const showAdminActions = document.body.dataset.page === 'admin-trees' && isAdmin();
    items.forEach(tree => listEl.insertAdjacentHTML('beforeend', renderTreeCard(tree, showAdminActions)));
    treesLoadedCount += items.length;
    if (!append && items.length === 0 && total === 0) listEl.innerHTML = '<p class="empty">Няма регистрирани дървета.</p>';
    updateTreesLoadMoreUI();
    if (append) bindTreesCardActions();
    else setTimeout(() => bindTreesCardActions(), 0);
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) {
      errorEl.textContent = e.message || 'Грешка при зареждане.';
      errorEl.innerHTML = errorEl.textContent + ' <a href="index.html">Задайте адрес на API на началната страница</a>.';
      errorEl.hidden = false;
    }
  }
}

function bindTreesCardActions() {
  document.querySelectorAll('.btn-delete-tree').forEach(btn => {
    btn.onclick = async () => {
      const id = btn.getAttribute('data-id');
      const name = btn.getAttribute('data-name') || id;
      if (!confirm(`Да изтриете ли дърво „${name}"?`)) return;
      try {
        await api(`api/trees/${id}`, { method: 'DELETE' });
        btn.closest('.card')?.remove();
        treesLoadedCount--;
        treesTotal--;
        updateTreesLoadMoreUI();
      } catch (err) {
        alert(err.message || 'Грешка при изтриване.');
      }
    };
  });
  document.querySelectorAll('.btn-edit-tree').forEach(btn => {
    btn.onclick = () => { window.location.href = `edit-tree.html?id=${btn.getAttribute('data-id')}`; };
  });
}

function onTreesLoadMore() {
  loadTrees(true);
}

// ---------- Карта (Leaflet) ----------
async function initMap() {
  const mapEl = document.getElementById('map');
  const loadingEl = document.getElementById('mapLoading');
  const errorEl = document.getElementById('mapError');
  if (!mapEl || typeof L === 'undefined') return;

  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;

  try {
    const data = await api('api/trees?includeLookups=true');
    const trees = asArray(data);
    const valid = trees.filter(t => t.latitude != null && t.longitude != null);
    if (loadingEl) loadingEl.hidden = true;
    mapEl.innerHTML = '';

    const map = L.map(mapEl).setView(DEFAULT_MAP_CENTER, 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    const treeIcon = getTreeMarkerIcon();
    if (valid.length) {
      const bounds = L.latLngBounds(valid.map(t => [t.latitude, t.longitude]));
      valid.forEach(tree => {
        L.marker([tree.latitude, tree.longitude], { icon: treeIcon })
          .addTo(map)
          .bindPopup(
            '<strong>' + escapeHtml(tree.name || 'Дърво #' + tree.id) + '</strong><br>' +
            (tree.species?.name ? escapeHtml(tree.species.name) : '') +
            ' <a href="tree.html?id=' + tree.id + '">Детайли</a>'
          );
      });
      map.fitBounds(bounds, { padding: [20, 20], maxZoom: 15 });
    } else if (!trees.length) {
      mapEl.insertAdjacentHTML('beforebegin', '<p class="empty">Няма дървета за показване на картата.</p>');
    } else {
      mapEl.insertAdjacentHTML('beforebegin', '<p class="empty">Няма дървета с координати.</p>');
    }
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) {
      errorEl.textContent = e.message || 'Грешка при зареждане на картата.';
      errorEl.innerHTML = errorEl.textContent + ' <a href="index.html">Задайте адрес на API на началната страница</a>.';
      errorEl.hidden = false;
    }
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
    if (!tree || typeof tree !== 'object') throw new Error('Невалидни данни за дърво.');
    loadingEl.hidden = true;
    const speciesName = (tree.species && tree.species.name) || tree.speciesId || '—';
    const genusName = (tree.genus && tree.genus.name) || tree.genusId || '—';
    const familyName = (tree.family && tree.family.name) || tree.familyId || '—';
    contentEl.innerHTML = `
      <div class="tree-detail-card">
        <h2>${escapeHtml(tree.name || `Дърво #${tree.id}`)}</h2>
        <dl class="detail-list">
          <dt>Вид</dt><dd>${escapeHtml(String(speciesName))}</dd>
          <dt>Род</dt><dd>${escapeHtml(String(genusName))}</dd>
          <dt>Семейство</dt><dd>${escapeHtml(String(familyName))}</dd>
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

// ---------- Таксономия (с CRUD за Admin) ----------
const TAXONOMY_SECTIONS = [
  { id: 'taxDivisions', endpoint: 'api/divisions', title: 'Отдел' },
  { id: 'taxClasses', endpoint: 'api/taxonomyclasses', title: 'Клас' },
  { id: 'taxFamilies', endpoint: 'api/families', title: 'Семейство' },
  { id: 'taxGenera', endpoint: 'api/genera', title: 'Род' },
  { id: 'taxSpecies', endpoint: 'api/species', title: 'Вид' }
];

function renderTaxonomyList(containerId, items, endpoint, key = 'name', showCrud = false) {
  if (!Array.isArray(items) || !items.length) {
    return '<p class="empty">Няма данни.</p>' + (showCrud ? `<button type="button" class="btn btn-sm btn-ghost tax-add" data-endpoint="${endpoint}">Добави</button>` : '');
  }
  const list = items.map(i => {
    const name = escapeHtml(i[key] || i.id || '');
    const id = i.id;
    const actions = showCrud ? ` <button type="button" class="btn btn-sm btn-ghost tax-edit" data-endpoint="${endpoint}" data-id="${id}" data-name="${name}">✎</button> <button type="button" class="btn btn-sm btn-ghost tax-delete" data-endpoint="${endpoint}" data-id="${id}" data-name="${name}">✕</button>` : '';
    return `<li>${name}${actions}</li>`;
  }).join('');
  const addBtn = showCrud ? `<button type="button" class="btn btn-sm btn-secondary tax-add" data-endpoint="${endpoint}">Добави</button>` : '';
  return addBtn + '<ul class="taxonomy-list-ul">' + list + '</ul>';
}

async function loadTaxonomy() {
  const loadingEl = document.getElementById('taxonomyLoading');
  const errorEl = document.getElementById('taxonomyError');
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;

  try {
    const showTaxCrud = document.body.dataset.page === 'admin-taxonomy' && isAdmin();
    for (const section of TAXONOMY_SECTIONS) {
      const el = document.getElementById(section.id);
      if (!el) continue;
      const data = await api(section.endpoint);
      const items = asArray(data);
      el.innerHTML = renderTaxonomyList(section.id, items, section.endpoint, 'name', showTaxCrud);
    }
    if (loadingEl) loadingEl.hidden = true;
    bindTaxonomyActions();
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) {
      errorEl.textContent = e.message || 'Грешка при зареждане на таксономия.';
      errorEl.hidden = false;
    }
  }
}

function openTaxModal(endpoint, editId, editName) {
  const modal = document.getElementById('taxModal');
  const title = document.getElementById('taxModalTitle');
  const input = document.getElementById('taxModalName');
  const idInput = document.getElementById('taxModalId');
  const endpointInput = document.getElementById('taxModalEndpoint');
  const errEl = document.getElementById('taxModalError');
  if (!modal || !title) return;
  errEl.hidden = true;
  modal.hidden = false;
  modal.classList.add('modal-open');
  const section = TAXONOMY_SECTIONS.find(s => s.endpoint === endpoint);
  endpointInput.value = endpoint;
  if (editId != null) {
    title.textContent = `Редактиране: ${section ? section.title : ''}`;
    idInput.value = editId;
    input.value = editName || '';
  } else {
    title.textContent = `Добави: ${section ? section.title : ''}`;
    idInput.value = '';
    input.value = '';
  }
  input.focus();
}

async function submitTaxModal(e) {
  e.preventDefault();
  const endpoint = document.getElementById('taxModalEndpoint')?.value;
  const id = document.getElementById('taxModalId')?.value;
  const name = document.getElementById('taxModalName')?.value?.trim();
  const errEl = document.getElementById('taxModalError');
  const modal = document.getElementById('taxModal');
  if (!endpoint || !name) return;
  errEl.hidden = true;
  try {
    if (id) {
      await api(`${endpoint}/${id}`, { method: 'PUT', body: JSON.stringify({ id: parseInt(id, 10), name }) });
    } else {
      await api(endpoint, { method: 'POST', body: JSON.stringify({ name }) });
    }
    if (modal) { modal.classList.remove('modal-open'); modal.hidden = true; }
    loadTaxonomy();
  } catch (err) {
    errEl.textContent = err.message || 'Грешка';
    errEl.hidden = false;
  }
}

function closeTaxModal() {
  const modal = document.getElementById('taxModal');
  if (modal) { modal.classList.remove('modal-open'); modal.hidden = true; }
}

function bindTaxonomyActions() {
  document.querySelectorAll('.tax-add').forEach(btn => {
    btn.onclick = () => openTaxModal(btn.getAttribute('data-endpoint'), null, '');
  });
  document.querySelectorAll('.tax-edit').forEach(btn => {
    btn.onclick = () => openTaxModal(btn.getAttribute('data-endpoint'), btn.getAttribute('data-id'), btn.getAttribute('data-name'));
  });
  document.querySelectorAll('.tax-delete').forEach(btn => {
    btn.onclick = async () => {
      const endpoint = btn.getAttribute('data-endpoint');
      const id = btn.getAttribute('data-id');
      const name = btn.getAttribute('data-name') || id;
      if (!confirm(`Да изтриете ли „${name}"?`)) return;
      try {
        await api(`${endpoint}/${id}`, { method: 'DELETE' });
        loadTaxonomy();
      } catch (err) {
        alert(err.message || 'Грешка при изтриване.');
      }
    };
  });
}

// ---------- Админ: списък потребители с пагинация (изисква вход като Admin) ----------
const ADMIN_PAGE_SIZE = 20;
let allAdminUsersCache = [];
let adminUsersVisibleCount = 0;

function renderAdminUserRow(u) {
  const id = (u.id || u.Id || '').toString();
  const email = escapeHtml(u.email || u.Email || '');
  const roles = Array.isArray(u.roles) ? u.roles : (u.roles ? [u.roles] : []);
  const rolesStr = escapeHtml(roles.join(', '));
  const actions = `
    <button type="button" class="btn btn-sm btn-ghost admin-edit-user" data-id="${escapeHtml(id)}" data-email="${email}" data-roles="${escapeHtml(roles.join(','))}" title="Редактирай">✎</button>
    <button type="button" class="btn btn-sm btn-ghost admin-delete-user" data-id="${escapeHtml(id)}" data-email="${email}" title="Изтрий">✕</button>
  `;
  return `<tr><td>${email}</td><td>${rolesStr}</td><td><code class="small">${escapeHtml(id.slice(0, 8))}…</code></td><td class="admin-actions">${actions}</td></tr>`;
}

function updateAdminLoadMore() {
  const wrap = document.getElementById('adminLoadMoreWrap');
  const btn = document.getElementById('adminLoadMore');
  const countEl = document.getElementById('adminUsersCount');
  const total = allAdminUsersCache.length;
  if (!wrap || total === 0) return;
  wrap.hidden = false;
  if (countEl) countEl.textContent = total <= ADMIN_PAGE_SIZE ? '' : `Показани ${Math.min(adminUsersVisibleCount, total)} от ${total}`;
  if (adminUsersVisibleCount >= total) {
    if (btn) btn.hidden = true;
    if (countEl) countEl.textContent = `Всички ${total} потребителя`;
    return;
  }
  if (btn) { btn.hidden = false; btn.textContent = 'Покажи още'; }
}

async function loadAdminUsers() {
  const listEl = document.getElementById('adminUsersList');
  const tbody = document.getElementById('adminUsersTableBody');
  const loadingEl = document.getElementById('adminUsersLoading');
  const errorEl = document.getElementById('adminUsersError');
  const tableWrap = document.getElementById('adminTableWrap');
  if (!listEl) return;

  listEl.innerHTML = '';
  allAdminUsersCache = [];
  adminUsersVisibleCount = 0;
  const wrap = document.getElementById('adminLoadMoreWrap');
  if (wrap) wrap.hidden = true;
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;

  try {
    const users = asArray(await authApi('api/users'));
    allAdminUsersCache = users;
    if (loadingEl) loadingEl.hidden = true;
    if (users.length) {
      listEl.innerHTML = `
        <div class="admin-toolbar">
          <button type="button" id="adminAddUser" class="btn btn-primary">Добави потребител</button>
        </div>
        <div class="admin-table-wrap" id="adminTableWrap">
          <table class="admin-table">
            <thead><tr><th>Имейл</th><th>Роли</th><th>Id</th><th>Действия</th></tr></thead>
            <tbody id="adminUsersTableBody"></tbody>
          </table>
        </div>
        <div id="adminLoadMoreWrap" class="load-more-wrap" hidden>
          <button type="button" id="adminLoadMore" class="btn btn-secondary">Покажи още</button>
          <span id="adminUsersCount" class="load-more-count"></span>
        </div>
      `;
      const body = document.getElementById('adminUsersTableBody');
      const firstEnd = Math.min(ADMIN_PAGE_SIZE, users.length);
      for (let i = 0; i < firstEnd; i++) body.insertAdjacentHTML('beforeend', renderAdminUserRow(users[i]));
      adminUsersVisibleCount = firstEnd;
      updateAdminLoadMore();
      const loadMoreBtn = document.getElementById('adminLoadMore');
      if (loadMoreBtn) loadMoreBtn.addEventListener('click', onAdminLoadMore);
      const addBtn = document.getElementById('adminAddUser');
      if (addBtn) addBtn.addEventListener('click', () => openUserModal());
      bindAdminUserActions();
    } else {
      listEl.innerHTML = '<p class="empty">Няма потребители или нямате права за преглед.</p>';
    }
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) {
      errorEl.textContent = e.status === 403 ? 'Само администратори имат достъп. Влезте с администраторски акаунт.' : (e.message || 'Грешка при зареждане.');
      errorEl.hidden = false;
    }
  }
}

function onAdminLoadMore() {
  const tbody = document.getElementById('adminUsersTableBody');
  if (!tbody || allAdminUsersCache.length === 0) return;
  const nextEnd = Math.min(adminUsersVisibleCount + ADMIN_PAGE_SIZE, allAdminUsersCache.length);
  for (let i = adminUsersVisibleCount; i < nextEnd; i++) {
    tbody.insertAdjacentHTML('beforeend', renderAdminUserRow(allAdminUsersCache[i]));
  }
  adminUsersVisibleCount = nextEnd;
  updateAdminLoadMore();
  bindAdminUserActions();
}

function openUserModal(editUser) {
  const modal = document.getElementById('userModal');
  const title = document.getElementById('userModalTitle');
  const userId = document.getElementById('userId');
  const emailInput = document.getElementById('userEmailInput');
  const passwordRow = document.getElementById('userPasswordRow');
  const passwordInput = document.getElementById('userPassword');
  const rolesInput = document.getElementById('userRoles');
  const errEl = document.getElementById('userModalError');
  if (!modal || !title) return;
  errEl.hidden = true;
  if (editUser) {
    title.textContent = 'Редактиране на потребител';
    userId.value = editUser.id || editUser.Id || '';
    emailInput.value = editUser.email || editUser.Email || '';
    rolesInput.value = Array.isArray(editUser.roles) ? editUser.roles.join(', ') : (editUser.roles || '');
    passwordRow.style.display = 'none';
  } else {
    title.textContent = 'Добави потребител';
    userId.value = '';
    emailInput.value = '';
    rolesInput.value = 'User';
    passwordInput.value = '';
    passwordRow.style.display = '';
  }
  modal.classList.add('modal-open');
  modal.hidden = false;
}

function bindAdminUserActions() {
  document.querySelectorAll('.admin-edit-user').forEach(btn => {
    btn.onclick = () => {
      const id = btn.getAttribute('data-id');
      const u = allAdminUsersCache.find(x => (x.id || x.Id) === id);
      if (u) openUserModal(u);
    };
  });
  document.querySelectorAll('.admin-delete-user').forEach(btn => {
    btn.onclick = async () => {
      const id = btn.getAttribute('data-id');
      const email = btn.getAttribute('data-email') || id;
      if (!confirm(`Да изтриете ли потребител „${email}"?`)) return;
      try {
        await authApi(`api/users/${id}`, { method: 'DELETE' });
        loadAdminUsers();
      } catch (err) {
        alert(err.message || 'Грешка при изтриване.');
      }
    };
  });
}

async function submitUserForm(e) {
  e.preventDefault();
  const userId = document.getElementById('userId')?.value;
  const email = document.getElementById('userEmailInput')?.value?.trim();
  const password = document.getElementById('userPassword')?.value;
  const rolesStr = document.getElementById('userRoles')?.value?.trim() || 'User';
  const errEl = document.getElementById('userModalError');
  const modal = document.getElementById('userModal');
  if (!email) return;
  errEl.hidden = true;
  const roles = rolesStr.split(',').map(r => r.trim()).filter(Boolean);
  try {
    if (userId) {
      await authApi(`api/users/${userId}`, { method: 'PUT', body: JSON.stringify({ email, roles }) });
    } else {
      if (!password) { errEl.textContent = 'Въведете парола за нов потребител.'; errEl.hidden = false; return; }
      await authApi('api/users', { method: 'POST', body: JSON.stringify({ email, password, roles }) });
    }
    if (modal) { modal.classList.remove('modal-open'); modal.hidden = true; }
    loadAdminUsers();
  } catch (err) {
    errEl.textContent = err.message || (Array.isArray(err.data) ? err.data.join(' ') : 'Грешка');
    errEl.hidden = false;
  }
}

// ---------- Карта за избор на координати (add-tree) ----------
let mapPickerMap = null;
let mapPickerMarker = null;

function initMapPicker() {
  const mapEl = document.getElementById('mapPicker');
  const latInput = document.getElementById('treeLat');
  const lngInput = document.getElementById('treeLng');
  if (!mapEl || !latInput || !lngInput || typeof L === 'undefined') return;

  const params = new URLSearchParams(window.location.search);
  let lat = parseFloat(params.get('lat'));
  let lng = parseFloat(params.get('lng'));
  if (Number.isFinite(lat) && Number.isFinite(lng)) {
    latInput.value = lat;
    lngInput.value = lng;
  } else {
    lat = parseFloat(latInput.value) || DEFAULT_MAP_CENTER[0];
    lng = parseFloat(lngInput.value) || DEFAULT_MAP_CENTER[1];
  }

  const center = [lat, lng];
  mapPickerMap = L.map(mapEl).setView(center, 13);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
  }).addTo(mapPickerMap);

  const treeIcon = getTreeMarkerIcon();
  function setMarker(latLng) {
    const la = typeof latLng.lat === 'function' ? latLng.lat() : latLng.lat;
    const ln = typeof latLng.lng === 'function' ? latLng.lng() : latLng.lng;
    latInput.value = Math.round(la * 1e6) / 1e6;
    lngInput.value = Math.round(ln * 1e6) / 1e6;
    if (mapPickerMarker) mapPickerMarker.setLatLng(latLng);
    else {
      mapPickerMarker = L.marker(latLng, { draggable: true, icon: treeIcon })
        .addTo(mapPickerMap)
        .on('dragend', function () { setMarker(this.getLatLng()); });
    }
  }

  mapPickerMap.on('click', function (e) { setMarker(e.latlng); });
  if (Number.isFinite(lat) && Number.isFinite(lng)) setMarker({ lat, lng });
}

// ---------- Редактиране на дърво (само Admin) ----------
let editMapPickerMap = null;
let editMapPickerMarker = null;

function initEditMapPicker(lat, lng) {
  const mapEl = document.getElementById('editMapPicker');
  const latInput = document.getElementById('editTreeLat');
  const lngInput = document.getElementById('editTreeLng');
  if (!mapEl || !latInput || !lngInput || typeof L === 'undefined') return;
  const la = Number.isFinite(lat) ? lat : DEFAULT_MAP_CENTER[0];
  const ln = Number.isFinite(lng) ? lng : DEFAULT_MAP_CENTER[1];
  latInput.value = la;
  lngInput.value = ln;
  editMapPickerMap = L.map(mapEl).setView([la, ln], 13);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap' }).addTo(editMapPickerMap);
  const treeIcon = getTreeMarkerIcon();
  function setMarker(latLng) {
    const a = typeof latLng.lat === 'function' ? latLng.lat() : latLng.lat;
    const b = typeof latLng.lng === 'function' ? latLng.lng() : latLng.lng;
    latInput.value = Math.round(a * 1e6) / 1e6;
    lngInput.value = Math.round(b * 1e6) / 1e6;
    if (editMapPickerMarker) editMapPickerMarker.setLatLng(latLng);
    else editMapPickerMarker = L.marker(latLng, { draggable: true, icon: treeIcon }).addTo(editMapPickerMap).on('dragend', function () { setMarker(this.getLatLng()); });
  }
  editMapPickerMap.on('click', e => setMarker(e.latlng));
  setMarker({ lat: la, lng: ln });
}

async function loadEditTreePage() {
  const params = new URLSearchParams(window.location.search);
  const id = params.get('id');
  const form = document.getElementById('formEditTree');
  const loadingEl = document.getElementById('editTreeLoading');
  const requiredEl = document.getElementById('editTreeLoginRequired');
  const errorEl = document.getElementById('editTreeError');
  if (!id || !form) return;
  if (!isAdmin()) {
    if (requiredEl) requiredEl.hidden = false;
    if (loadingEl) loadingEl.hidden = true;
    return;
  }
  if (requiredEl) requiredEl.hidden = true;
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;
  try {
    await loadTaxonomySelectsForEdit();
    const tree = await api(`api/trees/${id}?includeLookups=true`);
    if (loadingEl) loadingEl.hidden = true;
    document.getElementById('editTreeId').value = tree.id;
    document.getElementById('editTreeName').value = tree.name || '';
    document.getElementById('editTreeLat').value = tree.latitude ?? DEFAULT_MAP_CENTER[0];
    document.getElementById('editTreeLng').value = tree.longitude ?? DEFAULT_MAP_CENTER[1];
    document.getElementById('editTreeDescription').value = tree.description || '';
    document.getElementById('editTreeDivisionId').value = tree.divisionId || '';
    document.getElementById('editTreeTaxonomyClassId').value = tree.taxonomyClassId || '';
    document.getElementById('editTreeFamilyId').value = tree.familyId || '';
    document.getElementById('editTreeGenusId').value = tree.genusId || '';
    document.getElementById('editTreeSpeciesId').value = tree.speciesId || '';
    form.hidden = false;
    initEditMapPicker(tree.latitude, tree.longitude);
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.textContent = e.message || 'Грешка при зареждане.'; errorEl.hidden = false; }
  }
}

async function loadTaxonomySelectsForEdit() {
  const keys = ['editTreeDivisionId', 'editTreeTaxonomyClassId', 'editTreeFamilyId', 'editTreeGenusId', 'editTreeSpeciesId'];
  const endpoints = ['api/divisions', 'api/taxonomyclasses', 'api/families', 'api/genera', 'api/species'];
  for (let i = 0; i < keys.length; i++) {
    const sel = document.getElementById(keys[i]);
    if (!sel) continue;
    const prev = sel.value;
    sel.innerHTML = '<option value="">— изберете —</option>';
    const list = asArray(await api(endpoints[i]));
    list.forEach(item => { const opt = document.createElement('option'); opt.value = item.id ?? item.Id ?? ''; opt.textContent = item.name ?? item.Name ?? item.id ?? item.Id ?? ''; sel.appendChild(opt); });
    if (prev) sel.value = prev;
  }
}

async function submitEditTree(e) {
  e.preventDefault();
  const id = document.getElementById('editTreeId')?.value;
  const successEl = document.getElementById('editTreeSuccess');
  const errorEl = document.getElementById('editTreeError');
  successEl.hidden = true;
  errorEl.hidden = true;
  const payload = {
    id: parseInt(id, 10),
    name: document.getElementById('editTreeName').value.trim() || 'Без име',
    latitude: parseFloat(document.getElementById('editTreeLat').value),
    longitude: parseFloat(document.getElementById('editTreeLng').value),
    description: document.getElementById('editTreeDescription').value.trim() || null,
    divisionId: parseInt(document.getElementById('editTreeDivisionId').value, 10),
    taxonomyClassId: parseInt(document.getElementById('editTreeTaxonomyClassId').value, 10),
    familyId: parseInt(document.getElementById('editTreeFamilyId').value, 10),
    genusId: parseInt(document.getElementById('editTreeGenusId').value, 10),
    speciesId: parseInt(document.getElementById('editTreeSpeciesId').value, 10)
  };
  try {
    await api(`api/trees/${id}`, { method: 'PUT', body: JSON.stringify(payload) });
    successEl.textContent = 'Дървото е запазено.';
    successEl.hidden = false;
  } catch (err) {
    errorEl.textContent = err.message || 'Грешка при запазване.';
    errorEl.hidden = false;
  }
}

// ---------- Добавяне на дърво ----------
async function loadTaxonomySelects() {
  const keys = ['treeDivisionId', 'treeTaxonomyClassId', 'treeFamilyId', 'treeGenusId', 'treeSpeciesId'];
  const endpoints = ['api/divisions', 'api/taxonomyclasses', 'api/families', 'api/genera', 'api/species'];

  for (let i = 0; i < keys.length; i++) {
    const sel = document.getElementById(keys[i]);
    if (!sel) continue;
    const prev = sel.value;
    sel.innerHTML = '<option value="">— изберете —</option>';
    try {
      const list = asArray(await api(endpoints[i]));
      if (list.length) {
        list.forEach(item => {
          const opt = document.createElement('option');
          opt.value = item.id ?? item.Id ?? '';
          opt.textContent = item.name ?? item.Name ?? item.id ?? item.Id ?? '';
          sel.appendChild(opt);
        });
      }
      if (prev) sel.value = prev;
    } catch (_) {}
  }
}

function parseNum(val, def) {
  const n = Number(val);
  return Number.isFinite(n) ? n : (def ?? 0);
}

async function submitAddTree(e) {
  e.preventDefault();
  const successEl = document.getElementById('addTreeSuccess');
  const errorEl = document.getElementById('addTreeError');
  successEl.hidden = true;
  errorEl.hidden = true;
  const latEl = document.getElementById('treeLat');
  const lngEl = document.getElementById('treeLng');
  const lat = parseNum(latEl && latEl.value, 42.6977);
  const lng = parseNum(lngEl && lngEl.value, 23.3219);
  const divisionId = parseNum(document.getElementById('treeDivisionId') && document.getElementById('treeDivisionId').value);
  const taxonomyClassId = parseNum(document.getElementById('treeTaxonomyClassId') && document.getElementById('treeTaxonomyClassId').value);
  const familyId = parseNum(document.getElementById('treeFamilyId') && document.getElementById('treeFamilyId').value);
  const genusId = parseNum(document.getElementById('treeGenusId') && document.getElementById('treeGenusId').value);
  const speciesId = parseNum(document.getElementById('treeSpeciesId') && document.getElementById('treeSpeciesId').value);
  const payload = {
    name: (document.getElementById('treeName') && document.getElementById('treeName').value.trim()) || 'Без име',
    latitude: lat,
    longitude: lng,
    description: (document.getElementById('treeDescription') && document.getElementById('treeDescription').value.trim()) || null,
    divisionId: divisionId,
    taxonomyClassId: taxonomyClassId,
    familyId: familyId,
    genusId: genusId,
    speciesId: speciesId,
    division: { id: divisionId },
    taxonomyClass: { id: taxonomyClassId },
    genus: { id: genusId },
    family: { id: familyId },
    species: { id: speciesId }
  };

  try {
    await api('api/trees', { method: 'POST', body: JSON.stringify(payload) });
    successEl.textContent = 'Дървото е добавено успешно.';
    successEl.hidden = false;
    document.getElementById('formAddTree').reset();
    const latInp = document.getElementById('treeLat');
    const lngInp = document.getElementById('treeLng');
    if (latInp && lngInp) {
      latInp.value = DEFAULT_MAP_CENTER[0];
      lngInp.value = DEFAULT_MAP_CENTER[1];
      if (typeof mapPickerMarker !== 'undefined' && mapPickerMarker && typeof mapPickerMap !== 'undefined' && mapPickerMap) {
        mapPickerMarker.setLatLng(DEFAULT_MAP_CENTER);
        mapPickerMap.setView(DEFAULT_MAP_CENTER, mapPickerMap.getZoom());
      }
    }
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

// ---------- Инициализация ----------
document.addEventListener('DOMContentLoaded', async () => {
  if (typeof axios === 'undefined') {
    console.error('LeafMap: axios не е зареден. Проверете интернет връзката или блокиране на CDN и презаредете страницата.');
    const status = document.getElementById('apiStatus');
    if (status) {
      status.classList.add('error');
      status.querySelector('span:last-child').textContent = 'Грешка: axios не е зареден. Презаредете или отворете от HTTP сървър.';
    }
    return;
  }
  await bootstrapApiConfig();
  API_BASE_URL = getApiBaseUrl();
  updateAuthUI();
  highlightNav();
  closeTaxModal();
  const userModalEl = document.getElementById('userModal');
  if (userModalEl) { userModalEl.classList.remove('modal-open'); userModalEl.hidden = true; }

  if (document.getElementById('apiStatus')) checkApiStatus();
  if (document.getElementById('treesList')) {
    loadTrees();
    const treesLoadMore = document.getElementById('treesLoadMore');
    if (treesLoadMore) treesLoadMore.addEventListener('click', onTreesLoadMore);
  }
  if (document.getElementById('map')) initMap();
  if (document.getElementById('taxDivisions')) loadTaxonomy();
  const taxonomyAdminLinkWrap = document.getElementById('taxonomyAdminLinkWrap');
  if (taxonomyAdminLinkWrap && document.body.dataset.page === 'taxonomy') taxonomyAdminLinkWrap.hidden = !isAdmin();
  if (document.getElementById('treeDetailContent')) {
    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');
    if (id) loadTreeDetail(id);
  }
  if (document.getElementById('formAddTree')) {
    loadTaxonomySelects();
    document.getElementById('formAddTree').addEventListener('submit', submitAddTree);
    const formAddTreeEl = document.getElementById('formAddTree');
    if (document.getElementById('mapPicker') && !formAddTreeEl.hidden) initMapPicker();
  }
  if (document.getElementById('formEditTree')) {
    loadEditTreePage();
    document.getElementById('formEditTree').addEventListener('submit', submitEditTree);
  }
  if (document.getElementById('adminUsersList')) loadAdminUsers();
  const userForm = document.getElementById('userForm');
  if (userForm) userForm.addEventListener('submit', submitUserForm);
  const userModalClose = document.getElementById('userModalClose');
  if (userModalClose) userModalClose.addEventListener('click', () => { const m = document.getElementById('userModal'); if (m) { m.classList.remove('modal-open'); m.hidden = true; } });
  const taxModalForm = document.getElementById('taxModalForm');
  if (taxModalForm) taxModalForm.addEventListener('submit', submitTaxModal);
  const taxModalClose = document.getElementById('taxModalClose');
  if (taxModalClose) taxModalClose.addEventListener('click', closeTaxModal);

  const btnLogout = document.getElementById('btnLogout');
  if (btnLogout) btnLogout.addEventListener('click', () => { setToken(null); updateAuthUI(); if (document.body.dataset.page === 'login' || document.body.dataset.page === 'register') window.location.href = 'index.html'; });

  const formLogin = document.getElementById('formLogin');
  if (formLogin) formLogin.addEventListener('submit', async e => {
    e.preventDefault();
    const errEl = document.getElementById('loginError');
    if (errEl) errEl.hidden = true;
    try {
      await login(document.getElementById('loginEmail').value, document.getElementById('loginPassword').value);
    } catch (err) {
      if (errEl) {
        errEl.textContent = (err.status === 401 ? 'Невалиден email или парола.' : err.message) || 'Грешка при вход.';
        errEl.hidden = false;
      }
    }
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
    } catch (err) {
      if (errEl) { errEl.textContent = (err.data && Array.isArray(err.data) ? err.data.join(' ') : err.message) || 'Грешка при регистрация.'; errEl.hidden = false; }
    }
  });

  const apiUrlSave = document.getElementById('apiUrlSave');
  if (apiUrlSave) apiUrlSave.addEventListener('click', saveApiUrlAndCheck);
  const apiUrlInput = document.getElementById('apiUrlInput');
  if (apiUrlInput) {
    apiUrlInput.value = API_BASE_URL;
    apiUrlInput.placeholder = 'http://IP_НА_СЪРВЪРА:5202';
    apiUrlInput.addEventListener('keydown', e => { if (e.key === 'Enter') saveApiUrlAndCheck(); });
  }
  const authApiUrlInput = document.getElementById('authApiUrlInput');
  if (authApiUrlInput) {
    authApiUrlInput.value = getAuthApiBaseUrl();
    authApiUrlInput.placeholder = 'http://IP_НА_СЪРВЪРА:5203 или празно (същият хост)';
    authApiUrlInput.addEventListener('keydown', e => { if (e.key === 'Enter') saveApiUrlAndCheck(); });
  }
});
