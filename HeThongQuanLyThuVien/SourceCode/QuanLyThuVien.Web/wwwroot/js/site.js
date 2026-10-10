document.addEventListener('DOMContentLoaded', () => {
    const menu = document.querySelector('#menuToggle');
    const sidebar = document.querySelector('#sidebar');
    menu?.addEventListener('click', () => sidebar?.classList.toggle('open'));
    document.addEventListener('click', event => {
        if (window.innerWidth <= 760 && sidebar?.classList.contains('open') && !sidebar.contains(event.target) && !menu?.contains(event.target)) sidebar.classList.remove('open');
    });
    const search = document.querySelector('#bookSearch');
    search?.addEventListener('input', event => {
        const query = event.target.value.trim().toLocaleLowerCase('vi');
        document.querySelectorAll('[data-title]').forEach(card => card.classList.toggle('hidden', query.length > 0 && !card.dataset.title.includes(query)));
    });
});
