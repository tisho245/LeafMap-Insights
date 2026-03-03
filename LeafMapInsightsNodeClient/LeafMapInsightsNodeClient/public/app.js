/**
 * LeafMap Insights – клиент. API адресите използват същия хост като страницата (работи отвън и във вътрешната мрежа).
 */
const API_URL_KEY = 'leafmap_api_base_url';
const AUTH_API_URL_KEY = 'leafmap_auth_api_base_url';
const TOKEN_KEY = 'leafmap_jwt';

const DATA_API_PORT = 5202;
const AUTH_API_PORT = 5203;

/** Начална точка на картата (България). */
const DEFAULT_MAP_CENTER = [42.65690447620973, 24.746039419748605];

/** Икона за маркер „дърво” на картата. */
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
  return base ? base.replace(new RegExp(':' + DATA_API_PORT + '$'), ':' + AUTH_API_PORT) : ('http://' + (typeof window !== 'undefined' ? window.location.hostname : '') + ':' + AUTH_API_PORT);
}

let API_BASE_URL = getApiBaseUrl();

/** Нормализира отговор от API до масив. */
function asArray(data) {
  if (Array.isArray(data)) return data;
  if (data && typeof data === 'object') {
    const arr = data.data ?? data.items ?? data.Items ?? data.value ?? data.trees ?? data.Trees ?? data.result ?? data.Result;
    if (Array.isArray(arr)) return arr;
  }
  return [];
}

const API_URLS = [];

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else { localStorage.removeItem(TOKEN_KEY); localStorage.removeItem('leafmap_username'); }
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
      ? `Не може да се свърже с API (${url}). Проверете адреса на началната страница.`
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

/** Заявка към Data API без Authorization (за повторен опит при 401 при списък дървета). */
async function apiNoAuth(endpoint) {
  const base = getApiBaseUrl();
  const url = `${base.replace(/\/$/, '')}/${endpoint.replace(/^\//, '')}`;
  const res = await axios.get(url, { headers: { 'Accept': 'application/json' }, validateStatus: () => true });
  if (res.status !== 200) throw new Error(res.statusText || 'Грешка при заявка');
  return res.data;
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
    el.querySelector('span:last-child').textContent = 'Не може да се свърже с API. Проверете .env (API_HOST) на сървъра или задайте адреса по-долу.';
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
  const navAuth = document.querySelector('.nav-auth');
  const emailEl = document.getElementById('userEmail');
  const authBadge = document.getElementById('authBadge');
  const authStatusText = document.querySelector('.auth-status-text');
  const btnLogin = document.getElementById('btnLogin');
  const btnRegister = document.getElementById('btnRegister');
  const btnLogout = document.getElementById('btnLogout');
  const formAddTree = document.getElementById('formAddTree');
  const addTreeLoginRequired = document.getElementById('addTreeLoginRequired');
  const adminLink = document.getElementById('adminLink');
  const addTreeLink = document.getElementById('addTreeLink');

  if (navAuth) {
    navAuth.classList.remove('auth-logged-in', 'auth-guest');
    navAuth.classList.add(token ? 'auth-logged-in' : 'auth-guest');
  }
  if (authBadge) authBadge.hidden = !token;
  if (authStatusText) authStatusText.hidden = !!token;

  if (token) {
    try {
      const storedName = localStorage.getItem('leafmap_username');
      const payload = JSON.parse(atob(token.split('.')[1]));
      const username = storedName ?? payload.UserName ?? payload.name ?? payload.preferred_username ?? payload.unique_name ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? (payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload.Email ? String(payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload.Email).split('@')[0] : null) ?? payload.email ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? payload.Email ?? 'Потребител';
      if (emailEl) { emailEl.textContent = username; emailEl.setAttribute('title', 'Влезли сте като: ' + username); }
    } catch (_) {
      if (emailEl) emailEl.textContent = 'Потребител';
    }
    if (btnLogin) btnLogin.hidden = true;
    if (btnRegister) btnRegister.hidden = true;
    if (btnLogout) btnLogout.hidden = false;
    if (formAddTree) formAddTree.hidden = false;
    if (addTreeLoginRequired) addTreeLoginRequired.hidden = true;
    if (adminLink) adminLink.hidden = !isAdmin();
    if (addTreeLink) addTreeLink.hidden = false;
    if (document.getElementById('mapPicker') && typeof L !== 'undefined') {
      if (typeof initMapPicker === 'function' && !window.mapPickerMap) initMapPicker();
      else if (window.mapPickerMap && typeof window.mapPickerMap.invalidateSize === 'function') window.mapPickerMap.invalidateSize();
    }
  } else {
    if (emailEl) emailEl.textContent = '';
    if (btnLogin) btnLogin.hidden = false;
    if (btnRegister) btnRegister.hidden = false;
    if (btnLogout) btnLogout.hidden = true;
    if (formAddTree) formAddTree.hidden = true;
    if (addTreeLoginRequired) addTreeLoginRequired.hidden = false;
    if (adminLink) adminLink.hidden = true;
    if (addTreeLink) addTreeLink.hidden = true;
  }
}

function getTokenFromResponse(data) {
  if (!data) return null;
  return data.token ?? data.Token ?? data.accessToken ?? data.access_token ?? null;
}

function getRedirectUrl() {
  const params = new URLSearchParams(window.location.search);
  return params.get('next') || 'index.html';
}

async function login(username, password) {
  const base = getAuthApiBaseUrl();
  console.log('[LeafMap login] Auth API URL:', base);
  if (!base || !base.startsWith('http')) throw new Error('Задайте Auth API адрес на началната страница.');
  let data;
  try {
    data = await authApi('api/auth/login', { method: 'POST', body: { UserName: (username || '').trim(), Password: password || '' } });
    console.log('[LeafMap login] Отговор получен. Тип:', typeof data, 'Ключове:', data && typeof data === 'object' ? Object.keys(data) : '-');
  } catch (e) {
    console.log('[LeafMap login] Грешка от authApi:', e.status, e.message, e.data);
    throw e;
  }
  const token = getTokenFromResponse(data || {});
  console.log('[LeafMap login] Токен извлечен:', token ? 'да (' + token.length + ' символа)' : 'не');
  if (token) setToken(token);
  if (data && (data.userName ?? data.UserName)) localStorage.setItem('leafmap_username', data.userName ?? data.UserName);
  try { updateAuthUI(); } catch (_) {}
  const redirect = getRedirectUrl();
  console.log('[LeafMap login] Пренасочване към:', redirect);
  if (document.body.dataset.page === 'login') window.location.href = redirect;
}

async function register(email, password, userName) {
  const body = { Email: (email || '').trim(), Password: password || '' };
  if (userName) body.UserName = userName.trim();
  const data = await authApi('api/auth/register', { method: 'POST', body });
  const token = getTokenFromResponse(data || {});
  if (token) setToken(token);
  updateAuthUI();
  if (document.body.dataset.page === 'register') window.location.href = getRedirectUrl();
}

function renderTreeCard(tree, showAdminActions = false) {
  const id = tree.id ?? tree.Id;
  const name = tree.name ?? tree.Name ?? `Дърво #${id ?? '?'}`;
  const species = tree.species ?? tree.Species;
  const speciesName = species?.name ?? species?.Name ?? tree.speciesId ?? tree.SpeciesId;
  const latVal = tree.latitude ?? tree.Latitude;
  const lngVal = tree.longitude ?? tree.Longitude;
  const lat = latVal != null ? Number(latVal).toFixed(5) : '';
  const lng = lngVal != null ? Number(lngVal).toFixed(5) : '';
  const adminBtns = showAdminActions ? `<button type="button" class="btn btn-sm btn-ghost btn-edit-tree" data-id="${id}" title="Редактирай">✎</button><button type="button" class="btn btn-sm btn-ghost btn-delete-tree" data-id="${id}" data-name="${escapeHtml(name)}" title="Изтрий">✕</button>` : '';
  const desc = tree.description ?? tree.Description;
  const detailsHref = 'tree.html?id=' + encodeURIComponent(String(id ?? ''));
  return `
    <article class="card" data-id="${id}">
      <div class="card-body">
        <h3 class="card-title">${escapeHtml(name)}</h3>
        <p class="card-meta">Вид: ${escapeHtml(String(speciesName ?? ''))}</p>
        <p class="card-meta">Координати: ${lat}, ${lng}</p>
        ${desc ? `<p class="card-desc">${escapeHtml(desc)}</p>` : ''}
      </div>
      <div class="card-actions">
        <a href="${detailsHref}" class="btn btn-sm btn-primary card-details-link">Детайли</a>
        ${adminBtns}
      </div>
    </article>
  `;
}

const TREES_PAGE_SIZE = 24;
let treesTotal = 0;
let treesLoadedCount = 0;
let allTreesCache = [];

function updateTreesLoadMoreUI() {
  const wrap = document.getElementById('treesLoadMoreWrap');
  const btn = document.getElementById('treesLoadMore');
  const countEl = document.getElementById('treesCount');
  if (!wrap) return;
  if (treesTotal === 0) { wrap.hidden = true; return; }
  wrap.hidden = false;
  if (countEl) countEl.textContent = `Показани ${treesLoadedCount} от ${treesTotal}`;
  if (btn) { btn.hidden = treesLoadedCount >= treesTotal; btn.textContent = 'Покажи още'; }
  if (treesLoadedCount >= treesTotal && countEl) countEl.textContent = `Всички ${treesTotal}`;
}

async function loadTrees(append) {
  const listEl = document.getElementById('treesList');
  const loadingEl = document.getElementById('treesLoading');
  const errorEl = document.getElementById('treesError');
  const loadMoreWrap = document.getElementById('treesLoadMoreWrap');
  console.log('[LeafMap trees] loadTrees(append=' + append + ') listEl=', !!listEl, 'apiBase=', getApiBaseUrl());
  if (!listEl) {
    console.log('[LeafMap trees] Няма treesList – излизаме');
    return;
  }
  if (!append) {
    listEl.innerHTML = '';
    treesTotal = 0;
    treesLoadedCount = 0;
    if (loadMoreWrap) loadMoreWrap.hidden = true;
    if (loadingEl) loadingEl.hidden = false;
    if (errorEl) errorEl.hidden = true;
  }
  try {
    if (!append) {
      const url = getApiBaseUrl().replace(/\/$/, '') + '/api/trees?includeLookups=true';
      console.log('[LeafMap trees] Заявка към:', url);
      let raw;
      try {
        raw = await api('api/trees?includeLookups=true');
        console.log('[LeafMap trees] api() отговор: тип=', typeof raw, Array.isArray(raw) ? 'дължина=' + raw.length : 'ключове=' + (raw && typeof raw === 'object' ? Object.keys(raw).join(',') : '-'));
      } catch (e) {
        console.log('[LeafMap trees] api() хвърли:', e.status, e.message);
        if (e.status === 401 || e.status === 403) {
          raw = await apiNoAuth('api/trees?includeLookups=true');
          console.log('[LeafMap trees] apiNoAuth отговор: тип=', typeof raw);
        } else {
          try {
            raw = await apiNoAuth('api/trees');
            console.log('[LeafMap trees] fallback api/trees без includeLookups: дължина=', Array.isArray(raw) ? raw.length : (raw && typeof raw === 'object' && raw.value ? raw.value.length : '-'));
          } catch (_) { throw e; }
        }
      }
      if (typeof raw === 'string') {
        console.log('[LeafMap trees] API върна текст вместо JSON');
        throw new Error('Сървърът върна неочакван отговор. Проверете дали Data API (порт 5202) работи и връща JSON.');
      }
      let trees = asArray(raw || []);
      if (trees.length === 0 && raw && typeof raw === 'object' && !Array.isArray(raw)) {
        const anyArr = raw.value ?? raw.items ?? raw.Items ?? raw.data;
        if (Array.isArray(anyArr)) trees = anyArr;
      }
      console.log('[LeafMap trees] asArray върна дължина=', trees.length, 'първи елемент ключове=', trees[0] && typeof trees[0] === 'object' ? Object.keys(trees[0]).slice(0, 8).join(',') : '-');
      allTreesCache = trees.slice();
      treesTotal = trees.length;
      treesLoadedCount = Math.min(TREES_PAGE_SIZE, trees.length);
      const page = trees.slice(0, treesLoadedCount);
      console.log('[LeafMap trees] Показваме страница с', page.length, 'карти');
      const showAdminActions = isAdmin();
      page.forEach(tree => listEl.insertAdjacentHTML('beforeend', renderTreeCard(tree, showAdminActions)));
      if (errorEl) errorEl.hidden = true;
      if (trees.length === 0) listEl.innerHTML = '<p class="empty">Няма регистрирани дървета.</p>';
      updateTreesLoadMoreUI();
      try { window.dispatchEvent(new CustomEvent('treesLoaded')); } catch (_) {}
    } else {
      const nextEnd = Math.min(treesLoadedCount + TREES_PAGE_SIZE, allTreesCache.length);
      const page = allTreesCache.slice(treesLoadedCount, nextEnd);
      treesLoadedCount = nextEnd;
      const showAdminActions = isAdmin();
      page.forEach(tree => listEl.insertAdjacentHTML('beforeend', renderTreeCard(tree, showAdminActions)));
      updateTreesLoadMoreUI();
    }
  } catch (e) {
    console.log('[LeafMap trees] loadTrees catch:', e.message, 'status=', e.status);
    if (errorEl) {
      errorEl.textContent = e.message || 'Грешка при зареждане.';
      errorEl.hidden = false;
    }
  } finally {
    if (loadingEl) loadingEl.hidden = true;
  }
}

function bindTreesListClickDelegation() {
  const listEl = document.getElementById('treesList');
  if (!listEl) return;
  listEl.addEventListener('click', function (e) {
    if (e.target.closest('.card-details-link')) return;
    const delBtn = e.target.closest('.btn-delete-tree');
    if (delBtn) {
      e.preventDefault();
      const id = delBtn.getAttribute('data-id');
      const name = delBtn.getAttribute('data-name') || id;
      if (!confirm(`Да изтриете ли дърво „${name}"?`)) return;
      (async () => {
        try {
          await api(`api/trees/${id}`, { method: 'DELETE' });
          delBtn.closest('.card')?.remove();
          treesLoadedCount--;
          treesTotal--;
          updateTreesLoadMoreUI();
        } catch (err) { alert(err.message || 'Грешка при изтриване.'); }
      })();
      return;
    }
    const editBtn = e.target.closest('.btn-edit-tree');
    if (editBtn) {
      e.preventDefault();
      window.location.href = 'edit-tree.html?id=' + encodeURIComponent(String(editBtn.getAttribute('data-id') || ''));
    }
  });
}

function onTreesLoadMore() { loadTrees(true); }

// ---------- Карта (Leaflet) ----------
async function initMap() {
  const mapEl = document.getElementById('map');
  const loadingEl = document.getElementById('mapLoading');
  const errorEl = document.getElementById('mapError');
  if (!mapEl || typeof L === 'undefined') return;
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;
  const base = getApiBaseUrl();
  console.log('[LeafMap map] initMap apiBase=', base);
  if (!base || !base.startsWith('http')) {
    console.log('[LeafMap map] Няма валиден API адрес');
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.innerHTML = 'Няма зададен API адрес. <a href="index.html">Начало</a> – проверете връзката с API.'; errorEl.hidden = false; }
    return;
  }
  try {
    let data;
    try {
      data = await api('api/trees?includeLookups=true');
      console.log('[LeafMap map] api() отговор: тип=', typeof data, Array.isArray(data) ? 'дължина=' + data.length : 'ключове=' + (data && typeof data === 'object' ? Object.keys(data).join(',') : '-'));
    } catch (e) {
      console.log('[LeafMap map] api() хвърли:', e.status, e.message);
      if (e.status === 401 || e.status === 403) {
        data = await apiNoAuth('api/trees?includeLookups=true');
        console.log('[LeafMap map] apiNoAuth отговор: тип=', typeof data);
      } else throw e;
    }
    const trees = asArray(data || []);
    const valid = trees.filter(t => (t.latitude ?? t.Latitude) != null && (t.longitude ?? t.Longitude) != null);
    console.log('[LeafMap map] trees дължина=', trees.length, 'с координати=', valid.length);
    if (loadingEl) loadingEl.hidden = true;
    mapEl.innerHTML = '';
    const map = L.map(mapEl).setView(DEFAULT_MAP_CENTER, 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' }).addTo(map);
    const treeIcon = getTreeMarkerIcon();
    if (valid.length) {
      valid.forEach(tree => {
        const lat = tree.latitude ?? tree.Latitude;
        const lng = tree.longitude ?? tree.Longitude;
        const name = tree.name ?? tree.Name ?? 'Дърво #' + (tree.id ?? tree.Id);
        const species = tree.species ?? tree.Species;
        L.marker([lat, lng], { icon: treeIcon }).addTo(map).bindPopup(
          '<strong>' + escapeHtml(name) + '</strong><br>' + (species?.name ?? species?.Name ? escapeHtml(species.name ?? species.Name) : '') + ' <a href="tree.html?id=' + (tree.id ?? tree.Id) + '">Детайли</a>'
        );
      });
    }
    map.setView(DEFAULT_MAP_CENTER, 12);
    if (!trees.length) {
      mapEl.insertAdjacentHTML('beforebegin', '<p class="empty">Няма дървета за показване на картата.</p>');
    } else if (!valid.length) {
      mapEl.insertAdjacentHTML('beforebegin', '<p class="empty">Няма дървета с координати.</p>');
    }
  } catch (e) {
    console.log('[LeafMap map] initMap catch:', e.message, 'status=', e.status);
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) {
      errorEl.innerHTML = (e.message || 'Грешка при зареждане на картата.') + ' <a href="index.html">Проверете API на Начало</a>.';
      errorEl.hidden = false;
    }
  }
}

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
  if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
    lat = parseFloat(latInput.value) || DEFAULT_MAP_CENTER[0];
    lng = parseFloat(lngInput.value) || DEFAULT_MAP_CENTER[1];
  } else {
    latInput.value = lat;
    lngInput.value = lng;
  }
  mapPickerMap = L.map(mapEl).setView([lat, lng], 13);
  window.mapPickerMap = mapPickerMap;
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap' }).addTo(mapPickerMap);
  const treeIcon = getTreeMarkerIcon();
  function setMarker(latLng) {
    const la = typeof latLng.lat === 'function' ? latLng.lat() : latLng.lat;
    const ln = typeof latLng.lng === 'function' ? latLng.lng() : latLng.lng;
    latInput.value = Math.round(la * 1e6) / 1e6;
    lngInput.value = Math.round(ln * 1e6) / 1e6;
    if (mapPickerMarker) mapPickerMarker.setLatLng(latLng);
    else {
      mapPickerMarker = L.marker(latLng, { draggable: true, icon: treeIcon }).addTo(mapPickerMap).on('dragend', function () { setMarker(this.getLatLng()); });
    }
  }
  mapPickerMap.on('click', function (e) { setMarker(e.latlng); });
  setMarker({ lat, lng });
}

// ---------- Админ: пагинация ----------
const ADMIN_PAGE_SIZE = 20;
let allAdminUsersCache = [];
let adminUsersVisibleCount = 0;

function renderAdminUserRow(u) {
  const id = (u.id || u.Id || '').toString();
  const userName = (u.userName || u.UserName || '').toString().trim();
  const email = (u.email || u.Email || '').toString().trim();
  const roles = Array.isArray(u.roles) ? u.roles : (u.roles ? [].concat(u.roles) : []);
  const rolesStr = escapeHtml(roles.join(', ') || '—');
  const idShort = id.length > 12 ? id.slice(0, 8) + '…' : id;
  const emailConfirmed = u.emailConfirmed ?? u.EmailConfirmed;
  const phone = (u.phoneNumber || u.PhoneNumber || '').toString().trim();
  const phoneConfirmed = u.phoneNumberConfirmed ?? u.PhoneNumberConfirmed;
  const twoFactor = u.twoFactorEnabled ?? u.TwoFactorEnabled;
  const lockoutEnd = u.lockoutEnd ?? u.LockoutEnd;
  const lockoutEnabled = u.lockoutEnabled ?? u.LockoutEnabled;
  const accessFailed = u.accessFailedCount ?? u.AccessFailedCount ?? 0;
  const lockoutStr = lockoutEnd ? (typeof lockoutEnd === 'string' ? lockoutEnd.slice(0, 19) : '—') : '—';
  const actions = '<button type="button" class="btn btn-sm btn-ghost admin-edit-user" data-id="' + escapeHtml(id) + '" title="Редактирай">✎</button> <button type="button" class="btn btn-sm btn-ghost admin-delete-user" data-id="' + escapeHtml(id) + '" data-email="' + escapeHtml(email) + '" title="Изтрий">✕</button>';
  return '<tr><td>' + escapeHtml(userName || '—') + '</td><td>' + escapeHtml(email || '—') + '</td><td>' + (emailConfirmed ? '✓' : '—') + '</td><td>' + escapeHtml(phone || '—') + '</td><td>' + (phoneConfirmed ? '✓' : '—') + '</td><td>' + (twoFactor ? '✓' : '—') + '</td><td>' + escapeHtml(lockoutStr) + '</td><td>' + (lockoutEnabled ? '✓' : '—') + '</td><td>' + accessFailed + '</td><td>' + rolesStr + '</td><td><code class="small" title="' + escapeHtml(id) + '">' + escapeHtml(idShort) + '</code></td><td class="admin-actions">' + actions + '</td></tr>';
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
    if (countEl) countEl.textContent = `Всички ${total}`;
    return;
  }
  if (btn) { btn.hidden = false; btn.textContent = 'Покажи още'; }
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
  const userNameInput = document.getElementById('userNameInput');
  const emailInput = document.getElementById('userEmailInput');
  const passwordRow = document.getElementById('userPasswordRow');
  const phoneInput = document.getElementById('userPhoneInput');
  const emailConfirmedCb = document.getElementById('userEmailConfirmed');
  const phoneConfirmedCb = document.getElementById('userPhoneNumberConfirmed');
  const twoFactorCb = document.getElementById('userTwoFactorEnabled');
  const lockoutEnabledCb = document.getElementById('userLockoutEnabled');
  const lockoutEndInput = document.getElementById('userLockoutEnd');
  const rolesInput = document.getElementById('userRoles');
  const errEl = document.getElementById('userModalError');
  if (!modal || !title) return;
  if (errEl) errEl.hidden = true;
  if (editUser) {
    title.textContent = 'Редактиране на потребител';
    userId.value = editUser.id || editUser.Id || '';
    if (userNameInput) userNameInput.value = editUser.userName || editUser.UserName || '';
    if (emailInput) emailInput.value = editUser.email || editUser.Email || '';
    if (phoneInput) phoneInput.value = editUser.phoneNumber || editUser.PhoneNumber || '';
    if (emailConfirmedCb) emailConfirmedCb.checked = !!(editUser.emailConfirmed ?? editUser.EmailConfirmed);
    if (phoneConfirmedCb) phoneConfirmedCb.checked = !!(editUser.phoneNumberConfirmed ?? editUser.PhoneNumberConfirmed);
    if (twoFactorCb) twoFactorCb.checked = !!(editUser.twoFactorEnabled ?? editUser.TwoFactorEnabled);
    if (lockoutEnabledCb) lockoutEnabledCb.checked = !!(editUser.lockoutEnabled ?? editUser.LockoutEnabled);
    const le = editUser.lockoutEnd ?? editUser.LockoutEnd;
    if (lockoutEndInput) lockoutEndInput.value = le ? (typeof le === 'string' ? le.slice(0, 19) : '') : '';
    if (rolesInput) rolesInput.value = Array.isArray(editUser.roles) ? editUser.roles.join(', ') : (editUser.roles || '');
    if (passwordRow) passwordRow.style.display = 'none';
  } else {
    title.textContent = 'Добави потребител';
    userId.value = '';
    if (userNameInput) userNameInput.value = '';
    if (emailInput) emailInput.value = '';
    if (phoneInput) phoneInput.value = '';
    if (emailConfirmedCb) emailConfirmedCb.checked = false;
    if (phoneConfirmedCb) phoneConfirmedCb.checked = false;
    if (twoFactorCb) twoFactorCb.checked = false;
    if (lockoutEnabledCb) lockoutEnabledCb.checked = false;
    if (lockoutEndInput) lockoutEndInput.value = '';
    if (rolesInput) rolesInput.value = 'User';
    if (document.getElementById('userPassword')) document.getElementById('userPassword').value = '';
    if (passwordRow) passwordRow.style.display = '';
  }
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
      if (!confirm('Да изтриете ли потребител „' + email + '"?')) return;
      try {
        await authApi('api/users/' + id, { method: 'DELETE' });
        loadAdminUsers();
      } catch (err) { alert(err.message || 'Грешка при изтриване.'); }
    };
  });
}

async function submitUserForm(e) {
  e.preventDefault();
  const userId = document.getElementById('userId')?.value;
  const userName = document.getElementById('userNameInput')?.value?.trim();
  const email = document.getElementById('userEmailInput')?.value?.trim() || null;
  const password = document.getElementById('userPassword')?.value;
  const phone = document.getElementById('userPhoneInput')?.value?.trim() || null;
  const emailConfirmed = document.getElementById('userEmailConfirmed')?.checked ?? false;
  const phoneNumberConfirmed = document.getElementById('userPhoneNumberConfirmed')?.checked ?? false;
  const twoFactorEnabled = document.getElementById('userTwoFactorEnabled')?.checked ?? false;
  const lockoutEnabled = document.getElementById('userLockoutEnabled')?.checked ?? false;
  const lockoutEndRaw = document.getElementById('userLockoutEnd')?.value;
  const lockoutEnd = lockoutEndRaw ? new Date(lockoutEndRaw).toISOString() : null;
  const rolesStr = document.getElementById('userRoles')?.value?.trim() || 'User';
  const errEl = document.getElementById('userModalError');
  const modal = document.getElementById('userModal');
  if (!userName) { if (errEl) { errEl.textContent = 'Въведете потребителско име.'; errEl.hidden = false; } return; }
  if (errEl) errEl.hidden = true;
  const roles = rolesStr.split(',').map(r => r.trim()).filter(Boolean);
  try {
    if (userId) {
      await authApi('api/users/' + userId, { method: 'PUT', body: JSON.stringify({ userName, email, emailConfirmed, phoneNumber: phone, phoneNumberConfirmed, twoFactorEnabled, lockoutEnd, lockoutEnabled, roles }) });
    } else {
      if (!password) { if (errEl) { errEl.textContent = 'Въведете парола за нов потребител.'; errEl.hidden = false; } return; }
      await authApi('api/users', { method: 'POST', body: JSON.stringify({ userName, email, password, emailConfirmed, phoneNumber: phone, phoneNumberConfirmed, twoFactorEnabled, lockoutEnabled, roles }) });
    }
    if (modal) modal.hidden = true;
    loadAdminUsers();
  } catch (err) {
    if (errEl) { errEl.textContent = err.message || (Array.isArray(err.data) ? err.data.join(' ') : 'Грешка'); errEl.hidden = false; }
  }
}

async function loadAdminUsers() {
  const listEl = document.getElementById('usersList');
  const loadingEl = document.getElementById('usersLoading');
  const errorEl = document.getElementById('usersError');
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
      listEl.innerHTML = '<div class="admin-toolbar"><button type="button" id="adminAddUser" class="btn btn-primary">Добави потребител</button></div><div class="admin-table-wrap"><table class="admin-table admin-table-users"><thead><tr><th>Потребител</th><th>Имейл</th><th>Имейл ✓</th><th>Телефон</th><th>Тел. ✓</th><th>2FA</th><th>Блокиран до</th><th>Блок. вкл.</th><th>Грешки</th><th>Роли</th><th>Id</th><th>Действия</th></tr></thead><tbody id="adminUsersTableBody"></tbody></table></div><div id="adminLoadMoreWrap" class="load-more-wrap" hidden><button type="button" id="adminLoadMore" class="btn btn-secondary">Покажи още</button> <span id="adminUsersCount" class="load-more-count"></span></div>';
      const tbody = document.getElementById('adminUsersTableBody');
      const firstEnd = Math.min(ADMIN_PAGE_SIZE, users.length);
      for (let i = 0; i < firstEnd; i++) tbody.insertAdjacentHTML('beforeend', renderAdminUserRow(users[i]));
      adminUsersVisibleCount = firstEnd;
      updateAdminLoadMore();
      const loadMoreBtn = document.getElementById('adminLoadMore');
      if (loadMoreBtn) loadMoreBtn.addEventListener('click', onAdminLoadMore);
      const addBtn = document.getElementById('adminAddUser');
      if (addBtn) addBtn.addEventListener('click', () => openUserModal());
      bindAdminUserActions();
    } else {
      listEl.innerHTML = '<p class="empty">Няма потребители или нямате права.</p>';
    }
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) {
      errorEl.textContent = e.status === 403 ? 'Само администратори. Влезте с администраторски акаунт.' : (e.message || 'Грешка при зареждане.');
      errorEl.hidden = false;
    }
  }
}

// ---------- Редактиране на дърво (само Admin) ----------
var editMapPickerMap = null;
var editMapPickerMarker = null;

function initEditMapPicker(lat, lng) {
  var mapEl = document.getElementById('editMapPicker');
  var latInput = document.getElementById('editTreeLat');
  var lngInput = document.getElementById('editTreeLng');
  if (!mapEl || !latInput || !lngInput || typeof L === 'undefined') return;
  var la = Number.isFinite(lat) ? lat : DEFAULT_MAP_CENTER[0];
  var ln = Number.isFinite(lng) ? lng : DEFAULT_MAP_CENTER[1];
  latInput.value = la;
  lngInput.value = ln;
  editMapPickerMap = L.map(mapEl).setView([la, ln], 13);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap' }).addTo(editMapPickerMap);
  var treeIcon = getTreeMarkerIcon();
  function setMarker(latLng) {
    var a = typeof latLng.lat === 'function' ? latLng.lat() : latLng.lat;
    var b = typeof latLng.lng === 'function' ? latLng.lng() : latLng.lng;
    latInput.value = Math.round(a * 1e6) / 1e6;
    lngInput.value = Math.round(b * 1e6) / 1e6;
    if (editMapPickerMarker) editMapPickerMarker.setLatLng(latLng);
    else editMapPickerMarker = L.marker(latLng, { draggable: true, icon: treeIcon }).addTo(editMapPickerMap).on('dragend', function () { setMarker(this.getLatLng()); });
  }
  editMapPickerMap.on('click', function (e) { setMarker(e.latlng); });
  setMarker({ lat: la, lng: ln });
}

async function loadTaxonomySelectsForEdit() {
  var keys = ['editTreeDivisionId', 'editTreeTaxonomyClassId', 'editTreeFamilyId', 'editTreeGenusId', 'editTreeSpeciesId'];
  var endpoints = ['api/divisions', 'api/taxonomyclasses', 'api/families', 'api/genera', 'api/species'];
  for (var i = 0; i < keys.length; i++) {
    var sel = document.getElementById(keys[i]);
    if (!sel) continue;
    var prev = sel.value;
    sel.innerHTML = '<option value="">— изберете —</option>';
    var list = asArray(await api(endpoints[i]));
    list.forEach(function(item) { var opt = document.createElement('option'); opt.value = item.id != null ? item.id : item.Id; opt.textContent = item.name || item.Name || item.id || item.Id || ''; sel.appendChild(opt); });
    if (prev) sel.value = prev;
  }
}

async function loadEditTreePage() {
  var params = new URLSearchParams(window.location.search);
  var id = params.get('id');
  var form = document.getElementById('formEditTree');
  var loadingEl = document.getElementById('editTreeLoading');
  var requiredEl = document.getElementById('editTreeLoginRequired');
  var errorEl = document.getElementById('editTreeError');
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
    var tree = await api('api/trees/' + id + '?includeLookups=true');
    if (loadingEl) loadingEl.hidden = true;
    document.getElementById('editTreeId').value = tree.id;
    document.getElementById('editTreeName').value = tree.name || '';
    document.getElementById('editTreeLat').value = tree.latitude != null ? tree.latitude : DEFAULT_MAP_CENTER[0];
    document.getElementById('editTreeLng').value = tree.longitude != null ? tree.longitude : DEFAULT_MAP_CENTER[1];
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

async function submitEditTree(e) {
  e.preventDefault();
  var id = document.getElementById('editTreeId').value;
  var successEl = document.getElementById('editTreeSuccess');
  var errorEl = document.getElementById('editTreeError');
  successEl.hidden = true;
  errorEl.hidden = true;
  var payload = {
    id: parseInt(id, 10),
    name: document.getElementById('editTreeName').value.trim() || 'Без име',
    latitude: parseFloat(document.getElementById('editTreeLat').value),
    longitude: parseFloat(document.getElementById('editTreeLng').value),
    description: document.getElementById('editTreeDescription').value.trim() || null,
    divisionId: parseInt(document.getElementById('editTreeDivisionId').value, 10),
    taxonomyClassId: parseInt(document.getElementById('editTreeTaxonomyClassId').value, 10),
    familyId: parseInt(document.getElementById('editTreeFamilyId').value, 10),
    genusId: parseInt(document.getElementById('editTreeGenusId').value, 10),
    speciesId: parseInt(document.getElementById('editTreeSpeciesId').value, 10),
    // за да мине същата валидация като при MAUI клиента – навигационни обекти с Id
    division: { id: parseInt(document.getElementById('editTreeDivisionId').value, 10) },
    taxonomyClass: { id: parseInt(document.getElementById('editTreeTaxonomyClassId').value, 10) },
    genus: { id: parseInt(document.getElementById('editTreeGenusId').value, 10) },
    family: { id: parseInt(document.getElementById('editTreeFamilyId').value, 10) },
    species: { id: parseInt(document.getElementById('editTreeSpeciesId').value, 10) }
  };
  try {
    await api('api/trees/' + id, { method: 'PUT', body: JSON.stringify(payload) });
    successEl.textContent = 'Дървото е запазено.';
    successEl.hidden = false;
  } catch (err) {
    var msg = err && (err.message || (typeof err.data === 'string' ? err.data : '')) || 'Грешка при запазване.';
    if (err && err.data && typeof err.data === 'object' && !Array.isArray(err.data)) {
      // ASP.NET Core validation errors (ProblemDetails: errors[field] = [messages...])
      if (err.data.errors && typeof err.data.errors === 'object') {
        var parts = [];
        for (var key in err.data.errors) {
          if (!Object.prototype.hasOwnProperty.call(err.data.errors, key)) continue;
          var arr = err.data.errors[key];
          if (Array.isArray(arr)) {
            parts = parts.concat(arr.filter(Boolean));
          }
        }
        if (parts.length) msg = parts.join(' ');
      }
    } else if (err && Array.isArray(err.data)) {
      msg = err.data.join(' ');
    }
    errorEl.textContent = msg;
    errorEl.hidden = false;
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
        <div class="tree-detail-photo" id="treeDetailPhotoWrap"></div>
      </div>
    `;
    const photoUrl = tree.photoURL || tree.PhotoURL;
    if (photoUrl && (photoUrl.startsWith('data:') || photoUrl.startsWith('http:') || photoUrl.startsWith('https:'))) {
      const wrap = document.getElementById('treeDetailPhotoWrap');
      if (wrap) {
        const img = document.createElement('img');
        img.src = photoUrl;
        img.alt = 'Снимка на дървото';
        img.className = 'tree-detail-img';
        wrap.appendChild(img);
      }
    }
    if (isAdmin() && id) {
      const detailActions = document.querySelector('.detail-actions');
      if (detailActions) {
        const editLink = document.createElement('a');
        editLink.href = 'edit-tree.html?id=' + encodeURIComponent(String(id));
        editLink.className = 'btn btn-secondary btn-sm';
        editLink.textContent = 'Редактирай';
        detailActions.insertBefore(editLink, detailActions.firstChild);
      }
    }
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.textContent = e.message || 'Грешка при зареждане.'; errorEl.hidden = false; }
  }
}

var TAXONOMY_SECTIONS = [
  { id: 'taxDivisions', endpoint: 'api/divisions', title: 'Отдел' },
  { id: 'taxClasses', endpoint: 'api/taxonomyclasses', title: 'Клас' },
  { id: 'taxFamilies', endpoint: 'api/families', title: 'Семейство' },
  { id: 'taxGenera', endpoint: 'api/genera', title: 'Род' },
  { id: 'taxSpecies', endpoint: 'api/species', title: 'Вид' }
];

function renderTaxonomyList(containerId, items, endpoint, key) {
  key = key || 'name';
  var admin = isAdmin();
  if (!Array.isArray(items) || !items.length) {
    return '<p class="empty">Няма данни.</p>' + (admin ? '<button type="button" class="btn btn-sm btn-ghost tax-add" data-endpoint="' + endpoint + '">Добави</button>' : '');
  }
  var list = items.map(function(i) {
    var name = escapeHtml(i[key] || i.id || '');
    var id = i.id;
    var actions = admin ? ' <button type="button" class="btn btn-sm btn-ghost tax-edit" data-endpoint="' + endpoint + '" data-id="' + id + '" data-name="' + name + '">✎</button> <button type="button" class="btn btn-sm btn-ghost tax-delete" data-endpoint="' + endpoint + '" data-id="' + id + '" data-name="' + name + '">✕</button>' : '';
    return '<li>' + name + actions + '</li>';
  }).join('');
  var addBtn = admin ? '<button type="button" class="btn btn-sm btn-secondary tax-add" data-endpoint="' + endpoint + '">Добави</button>' : '';
  return addBtn + '<ul class="taxonomy-list-ul">' + list + '</ul>';
}

async function loadTaxonomy() {
  var loadingEl = document.getElementById('taxonomyLoading');
  var errorEl = document.getElementById('taxonomyError');
  if (loadingEl) loadingEl.hidden = false;
  if (errorEl) errorEl.hidden = true;
  try {
    for (var s = 0; s < TAXONOMY_SECTIONS.length; s++) {
      var section = TAXONOMY_SECTIONS[s];
      var el = document.getElementById(section.id);
      if (!el) continue;
      var data = await api(section.endpoint);
      var items = asArray(data);
      el.innerHTML = renderTaxonomyList(section.id, items, section.endpoint);
    }
    if (loadingEl) loadingEl.hidden = true;
    bindTaxonomyActions();
  } catch (e) {
    if (loadingEl) loadingEl.hidden = true;
    if (errorEl) { errorEl.textContent = e.message || 'Грешка при зареждане на таксономия.'; errorEl.hidden = false; }
  }
}

function openTaxModal(endpoint, editId, editName) {
  var modal = document.getElementById('taxModal');
  var title = document.getElementById('taxModalTitle');
  var input = document.getElementById('taxModalName');
  var idInput = document.getElementById('taxModalId');
  var endpointInput = document.getElementById('taxModalEndpoint');
  var errEl = document.getElementById('taxModalError');
  if (!modal || !title) return;
  if (errEl) errEl.hidden = true;
  var section = TAXONOMY_SECTIONS.find(function(s) { return s.endpoint === endpoint; });
  endpointInput.value = endpoint;
  if (editId != null) {
    title.textContent = 'Редактиране: ' + (section ? section.title : '');
    idInput.value = editId;
    input.value = editName || '';
  } else {
    title.textContent = 'Добави: ' + (section ? section.title : '');
    idInput.value = '';
    input.value = '';
  }
  modal.hidden = false;
  input.focus();
}

async function submitTaxModal(e) {
  e.preventDefault();
  var endpoint = document.getElementById('taxModalEndpoint').value;
  var id = document.getElementById('taxModalId').value;
  var name = document.getElementById('taxModalName').value.trim();
  var errEl = document.getElementById('taxModalError');
  var modal = document.getElementById('taxModal');
  if (!endpoint || !name) return;
  if (errEl) errEl.hidden = true;
  try {
    if (id) {
      await api(endpoint + '/' + id, { method: 'PUT', body: JSON.stringify({ id: parseInt(id, 10), name: name }) });
    } else {
      await api(endpoint, { method: 'POST', body: JSON.stringify({ name: name }) });
    }
    modal.hidden = true;
    loadTaxonomy();
  } catch (err) {
    errEl.textContent = err.message || 'Грешка';
    errEl.hidden = false;
  }
}

function bindTaxonomyActions() {
  document.querySelectorAll('.tax-add').forEach(function(btn) {
    btn.onclick = function() { openTaxModal(btn.getAttribute('data-endpoint'), null, ''); };
  });
  document.querySelectorAll('.tax-edit').forEach(function(btn) {
    btn.onclick = function() { openTaxModal(btn.getAttribute('data-endpoint'), btn.getAttribute('data-id'), btn.getAttribute('data-name')); };
  });
  document.querySelectorAll('.tax-delete').forEach(function(btn) {
    btn.onclick = async function() {
      var endpoint = btn.getAttribute('data-endpoint');
      var id = btn.getAttribute('data-id');
      var name = btn.getAttribute('data-name') || id;
      if (!confirm('Да изтриете ли „' + name + '"?')) return;
      try {
        await api(endpoint + '/' + id, { method: 'DELETE' });
        loadTaxonomy();
      } catch (err) { alert(err.message || 'Грешка при изтриване.'); }
    };
  });
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
      const data = await api(endpoints[i]);
      const list = asArray(data);
      list.forEach(item => {
        const id = item.id ?? item.Id ?? '';
        const name = item.name ?? item.Name ?? item.id ?? item.Id ?? '';
        if (id === '' && name === '') return;
        const opt = document.createElement('option');
        opt.value = id;
        opt.textContent = name;
        sel.appendChild(opt);
      });
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
      if (typeof mapPickerMarker !== 'undefined' && mapPickerMarker && mapPickerMap) {
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

function initMobileNav() {
  const burger = document.getElementById('btnBurger');
  const nav = document.getElementById('siteNav');
  const overlay = document.getElementById('navOverlay');
  const closeBtn = document.getElementById('btnNavClose');

  if (!burger || !nav || !overlay) return;

  const isMobile = () => window.matchMedia('(max-width: 820px)').matches;

  const open = () => {
    document.body.classList.add('nav-open');
    burger.setAttribute('aria-expanded', 'true');
    overlay.hidden = false;
    document.body.style.overflow = 'hidden';
  };

  const close = () => {
    document.body.classList.remove('nav-open');
    burger.setAttribute('aria-expanded', 'false');
    overlay.hidden = true;
    document.body.style.overflow = '';
  };

  burger.addEventListener('click', () => {
    if (document.body.classList.contains('nav-open')) close();
    else open();
  });

  overlay.addEventListener('click', close);
  if (closeBtn) closeBtn.addEventListener('click', close);

  nav.querySelectorAll('a,button').forEach(el => {
    el.addEventListener('click', () => {
      if (isMobile() && document.body.classList.contains('nav-open')) close();
    });
  });

  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') close();
  });

  window.addEventListener('resize', () => {
    if (!isMobile()) close();
  });

  overlay.hidden = true;
  burger.setAttribute('aria-expanded', 'false');
}

function escapeHtml(s) {
  const div = document.createElement('div');
  div.textContent = s;
  return div.innerHTML;
}

function runInit() {
  const dataPage = document.body.dataset.page;
  console.log('[LeafMap runInit] старт, data-page=', dataPage);
  API_BASE_URL = getApiBaseUrl();
  initMobileNav();
  updateAuthUI();
  highlightNav();
  if (document.getElementById('apiStatus')) checkApiStatus();
  const treesListEl = document.getElementById('treesList');
  console.log('[LeafMap runInit] treesList=', !!treesListEl, 'map=', !!document.getElementById('map'));
  if (treesListEl) {
    bindTreesListClickDelegation();
    loadTrees();
    const treesLoadMore = document.getElementById('treesLoadMore');
    if (treesLoadMore) treesLoadMore.addEventListener('click', onTreesLoadMore);
  }
  if (document.getElementById('map')) initMap();
  if (document.getElementById('taxDivisions')) loadTaxonomy();
  if (document.getElementById('usersList') && document.body.dataset.page === 'admin') loadAdminUsers();
  if (document.getElementById('treeDetailContent')) {
    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');
    if (id) loadTreeDetail(id);
  }
  if (document.getElementById('formAddTree')) {
    loadTaxonomySelects();
    document.getElementById('formAddTree').addEventListener('submit', submitAddTree);
    if (document.getElementById('mapPicker') && !document.getElementById('formAddTree').hidden && typeof L !== 'undefined') initMapPicker();
  }
  if (document.getElementById('formEditTree')) {
    loadEditTreePage();
    document.getElementById('formEditTree').addEventListener('submit', submitEditTree);
  }
  const btnLogout = document.getElementById('btnLogout');
  if (btnLogout) btnLogout.addEventListener('click', () => { setToken(null); updateAuthUI(); if (document.body.dataset.page === 'login' || document.body.dataset.page === 'register') window.location.href = 'index.html'; });
  const formLogin = document.getElementById('formLogin');
  if (formLogin) formLogin.addEventListener('submit', async e => {
    e.preventDefault();
    const errEl = document.getElementById('loginError');
    if (errEl) errEl.hidden = true;
    try { await login(document.getElementById('loginUsername').value, document.getElementById('loginPassword').value); }
    catch (err) {
      if (errEl) {
        let msg = err.message || 'Грешка при вход.';
        if (err.status === 401) msg = 'Невалидно потребителско име или парола. Проверете данните.';
        if (err.status === 500) msg = 'Сървърна грешка при вход (500). Опитайте по-късно или свържете се с администратор.';
        errEl.textContent = msg;
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
    } catch (err) { if (errEl) { errEl.textContent = (err.data && Array.isArray(err.data) ? err.data.join(' ') : err.message) || 'Грешка при регистрация.'; errEl.hidden = false; } }
  });
  const apiUrlSave = document.getElementById('apiUrlSave');
  if (apiUrlSave) apiUrlSave.addEventListener('click', saveApiUrlAndCheck);
  const apiUrlInput = document.getElementById('apiUrlInput');
  if (apiUrlInput) {
    apiUrlInput.value = getApiBaseUrl();
    apiUrlInput.addEventListener('keydown', e => { if (e.key === 'Enter') saveApiUrlAndCheck(); });
  }
  const userForm = document.getElementById('userForm');
  if (userForm) userForm.addEventListener('submit', submitUserForm);
  const userModalClose = document.getElementById('userModalClose');
  if (userModalClose) userModalClose.addEventListener('click', () => { const m = document.getElementById('userModal'); if (m) m.hidden = true; });
  const userModal = document.getElementById('userModal');
  if (userModal) userModal.addEventListener('click', (e) => { if (e.target === userModal) userModal.hidden = true; });
  const taxModalForm = document.getElementById('taxModalForm');
  if (taxModalForm) taxModalForm.addEventListener('submit', submitTaxModal);
  const taxModalClose = document.getElementById('taxModalClose');
  if (taxModalClose) taxModalClose.addEventListener('click', () => { const m = document.getElementById('taxModal'); if (m) m.hidden = true; });
  const taxModal = document.getElementById('taxModal');
  if (taxModal) taxModal.addEventListener('click', (e) => { if (e.target === taxModal) taxModal.hidden = true; });
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      const um = document.getElementById('userModal');
      const tm = document.getElementById('taxModal');
      if (um && !um.hidden) um.hidden = true;
      if (tm && !tm.hidden) tm.hidden = true;
    }
  });
}

/** При зареждане: използваме същия хост като страницата за API (hostname:5202 / :5203). Ако /api-config отговори – може да го ползваме, иначе винаги имаме fallback по hostname. */
async function bootstrapApiConfig() {
  if (window.location.protocol === 'file:') return;
  const host = window.location.hostname;
  const fallbackData = 'http://' + host + ':' + DATA_API_PORT;
  const fallbackAuth = 'http://' + host + ':' + AUTH_API_PORT;
  try {
    const r = await axios.get('/api-config', { headers: { Accept: 'application/json' }, timeout: 2000 });
    console.log('[LeafMap bootstrap] /api-config отговор:', r?.data);
    if (r && r.data && r.data.apiBaseUrl) {
      localStorage.setItem(API_URL_KEY, r.data.apiBaseUrl);
      localStorage.setItem(AUTH_API_URL_KEY, r.data.authApiBaseUrl || r.data.apiBaseUrl.replace(/:5202$/, ':5203'));
      console.log('[LeafMap bootstrap] API URL зададени:', r.data.apiBaseUrl, r.data.authApiBaseUrl);
    } else {
      localStorage.setItem(API_URL_KEY, fallbackData);
      localStorage.setItem(AUTH_API_URL_KEY, fallbackAuth);
      console.log('[LeafMap bootstrap] fallback URL:', fallbackData, fallbackAuth);
    }
  } catch (e) {
    console.log('[LeafMap bootstrap] /api-config грешка:', e.message);
    localStorage.setItem(API_URL_KEY, fallbackData);
    localStorage.setItem(AUTH_API_URL_KEY, fallbackAuth);
  }
  API_BASE_URL = getApiBaseUrl();
  console.log('[LeafMap bootstrap] getApiBaseUrl()=', API_BASE_URL);
}

document.addEventListener('DOMContentLoaded', async () => {
  await bootstrapApiConfig();
  API_BASE_URL = getApiBaseUrl();
  runInit();
});
