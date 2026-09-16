const root = document.getElementById('catalog');
const url = root.dataset.endpoint;

fetch(url)
    .then(r => r.ok ? r.json() : Promise.reject(r.status))
    .then(items => {
        root.innerHTML = items.map(i =>
            `<div class="card">${i.name} — ${i.price}₽</div>`  
        ).join('');
    })
    .catch(err => root.textContent = 'Ошибка: ' + err);