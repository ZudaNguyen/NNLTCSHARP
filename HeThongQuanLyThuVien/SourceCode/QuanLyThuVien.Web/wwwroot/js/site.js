document.addEventListener('DOMContentLoaded', () => {
    setupResponsiveShell();

    const search = document.querySelector('#bookSearch');
    search?.addEventListener('input', event => {
        const query = event.target.value.trim().toLocaleLowerCase('vi');
        document.querySelectorAll('[data-title]').forEach(card => card.classList.toggle('hidden', query.length > 0 && !card.dataset.title.includes(query)));
    });

    document.querySelectorAll('[data-search-suggest]').forEach(setupBookSuggestions);
});

function setupResponsiveShell() {
    const body = document.body;
    const sidebar = document.querySelector('#sidebar');
    const menu = document.querySelector('#menuToggle');
    const scrim = document.querySelector('#sidebarScrim');
    const searchForm = document.querySelector('.top-search');
    const searchToggle = document.querySelector('#searchToggle');
    const searchInput = searchForm?.querySelector('input[name="search"]');
    const mobileNav = window.matchMedia('(max-width: 900px)');
    const compactSearch = window.matchMedia('(max-width: 760px)');

    const closeSidebar = () => {
        sidebar?.classList.remove('open');
        body.classList.remove('sidebar-open');
        menu?.setAttribute('aria-expanded', 'false');
    };

    const openSidebar = () => {
        sidebar?.classList.add('open');
        body.classList.add('sidebar-open');
        menu?.setAttribute('aria-expanded', 'true');
    };

    const closeSearch = () => {
        body.classList.remove('search-open');
        searchToggle?.setAttribute('aria-expanded', 'false');
    };

    const openSearch = () => {
        if (!compactSearch.matches) return;
        body.classList.add('search-open');
        searchToggle?.setAttribute('aria-expanded', 'true');
        requestAnimationFrame(() => searchInput?.focus());
    };

    menu?.addEventListener('click', () => {
        if (sidebar?.classList.contains('open')) closeSidebar();
        else openSidebar();
    });

    scrim?.addEventListener('click', closeSidebar);

    sidebar?.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            if (mobileNav.matches) closeSidebar();
        });
    });

    searchToggle?.addEventListener('click', () => {
        if (body.classList.contains('search-open')) closeSearch();
        else openSearch();
    });

    searchForm?.addEventListener('submit', closeSearch);

    document.addEventListener('click', event => {
        if (body.classList.contains('sidebar-open')
            && !sidebar?.contains(event.target)
            && !menu?.contains(event.target)) {
            closeSidebar();
        }
        if (body.classList.contains('search-open')
            && !searchForm?.contains(event.target)
            && !searchToggle?.contains(event.target)) {
            closeSearch();
        }
    });

    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        if (body.classList.contains('search-open')) {
            closeSearch();
            searchToggle?.focus();
        } else if (body.classList.contains('sidebar-open')) {
            closeSidebar();
            menu?.focus();
        }
    });

    const syncViewport = () => {
        if (!mobileNav.matches) closeSidebar();
        if (!compactSearch.matches) closeSearch();
    };

    mobileNav.addEventListener('change', syncViewport);
    compactSearch.addEventListener('change', syncViewport);
}

function setupBookSuggestions(form) {
    const input = form.querySelector('input[name="search"]');
    const box = form.querySelector('.search-suggest');
    const endpoint = form.dataset.searchSuggest;
    if (!input || !box || !endpoint) return;

    let timer = null;
    let controller = null;
    let options = [];
    let activeIndex = -1;

    const close = () => {
        box.hidden = true;
        box.replaceChildren();
        options = [];
        activeIndex = -1;
        input.setAttribute('aria-expanded', 'false');
    };

    const setActive = index => {
        options.forEach((option, optionIndex) => {
            option.classList.toggle('active', optionIndex === index);
            option.setAttribute('aria-selected', optionIndex === index ? 'true' : 'false');
        });
        activeIndex = index;
        if (index >= 0) options[index].scrollIntoView({ block: 'nearest' });
    };

    const render = items => {
        box.replaceChildren();
        if (!items.length) {
            close();
            return;
        }

        items.forEach((item, index) => {
            const link = document.createElement('a');
            link.className = 'search-suggest-item';
            link.href = item.url;
            link.setAttribute('role', 'option');
            link.setAttribute('aria-selected', 'false');
            link.addEventListener('mouseenter', () => setActive(index));

            const title = document.createElement('strong');
            title.textContent = item.tenSach;

            const meta = document.createElement('small');
            meta.textContent = `${item.tenTacGia} · ${item.tenTheLoai}`;

            const status = document.createElement('em');
            status.textContent = item.conHang ? `Còn ${item.soLuongCon}` : 'Hết bản';
            if (!item.conHang) status.className = 'out';

            link.append(title, meta, status);
            box.append(link);
        });

        options = Array.from(box.querySelectorAll('.search-suggest-item'));
        activeIndex = -1;
        box.hidden = false;
        input.setAttribute('aria-expanded', 'true');
    };

    const load = async query => {
        controller?.abort();
        controller = new AbortController();
        try {
            const response = await fetch(`${endpoint}?q=${encodeURIComponent(query)}`, {
                signal: controller.signal,
                headers: { Accept: 'application/json' }
            });
            if (!response.ok) {
                close();
                return;
            }
            const items = await response.json();
            if (input.value.trim() === query) render(Array.isArray(items) ? items : []);
        } catch (error) {
            if (error.name !== 'AbortError') close();
        }
    };

    input.addEventListener('input', () => {
        const query = input.value.trim();
        clearTimeout(timer);
        if (query.length < 2) {
            controller?.abort();
            close();
            return;
        }
        timer = setTimeout(() => load(query), 220);
    });

    input.addEventListener('keydown', event => {
        if (box.hidden) return;
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            setActive((activeIndex + 1) % options.length);
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            setActive((activeIndex - 1 + options.length) % options.length);
        } else if (event.key === 'Enter' && activeIndex >= 0) {
            event.preventDefault();
            window.location.href = options[activeIndex].href;
        } else if (event.key === 'Escape') {
            close();
        }
    });

    form.addEventListener('submit', close);
    document.addEventListener('click', event => {
        if (!form.contains(event.target)) close();
    });
}
