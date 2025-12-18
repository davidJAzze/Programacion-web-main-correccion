// ===============================
// UPIICAFE - MENU + CARRITO
// ===============================

// La BD se inyecta desde la vista (Index.cshtml)
const db = window.__MENU_DB__ || [];
let carrito = [];

// Inicial
document.addEventListener("DOMContentLoaded", () => {
  const firstBtn = document.querySelector(".category-btn");
  if (firstBtn) filtrarMenu("comida", firstBtn);
});

// ===============================
// FILTRAR Y PINTAR PRODUCTOS
// ===============================
function filtrarMenu(categoria, btn) {
  document.querySelectorAll(".category-btn").forEach((b) => b.classList.remove("active"));
  if (btn) btn.classList.add("active");

  const grid = document.getElementById("gridProductos");
  if (!grid) return;

  grid.innerHTML = "";

  const items = db.filter((i) => i.cat === categoria);

  if (items.length === 0) {
    grid.innerHTML = `
      <div style="grid-column:1/-1; text-align:center; padding:50px; color:#888;">
        <h3>🍽️</h3>
        <p>No hay productos disponibles en esta categoría.</p>
      </div>
    `;
    return;
  }

  items.forEach((item) => {
    const card = document.createElement("div");
    card.className = "card";

    card.innerHTML = `
      <div class="food-img" style="background-image: url('/imagenes/${item.img}');"></div>

      <div class="food-title">${item.name}</div>
      <div class="food-desc">${item.desc}</div>
      <div class="food-price">$${Number(item.price).toFixed(2)}</div>

      <div class="controls">
        <button class="btn-pill" onclick="agregar(${item.id})">AGREGAR +</button>
      </div>
    `;

    grid.appendChild(card);
  });
}

// ===============================
// CARRITO
// ===============================
function agregar(id) {
  const item = db.find((i) => i.id === id);
  if (!item) return;

  carrito.push({
    ...item,
    uniqueId: Date.now() + Math.random(),
    price: Number(item.price),
  });

  actualizarCarritoUI();
}

function eliminarItem(uniqueId) {
  const index = carrito.findIndex((i) => i.uniqueId === uniqueId);
  if (index > -1) carrito.splice(index, 1);
  actualizarCarritoUI();
}

function actualizarCarritoUI() {
  const title = document.getElementById("cartTitle");
  const container = document.getElementById("cartItems");
  const txtTotal = document.getElementById("txtTotal"); // 👈 OJO: este ID debe existir en tu HTML

  if (!container || !txtTotal) return;

  const total = carrito.reduce((acc, item) => acc + item.price, 0);
  txtTotal.innerText = `$${total.toFixed(2)}`; // ✅ ESTA ERA LA LÍNEA QUE TENÍAS MAL

  if (carrito.length > 0) {
    if (title) title.style.display = "none";
    container.innerHTML = "";

    carrito.forEach((p) => {
      const box = document.createElement("div");
      box.className = "cart-item-box";

      box.innerHTML = `
        <div style="flex:1; overflow:hidden; text-overflow:ellipsis; font-weight:500;">
          ${p.name}
        </div>
        <div style="font-weight:bold; color:#004d40;">
          $${p.price.toFixed(2)}
        </div>
        <button class="btn-delete"
                style="margin-left:10px; color:#e53935; font-weight:bold; border:none; background:none; cursor:pointer; font-size:1.1rem;"
                onclick="eliminarItem(${p.uniqueId})">×</button>
      `;

      container.appendChild(box);
    });

    container.scrollTop = container.scrollHeight;
  } else {
    if (title) title.style.display = "block";
    container.innerHTML = "";
  }
}

// ===============================
// MODAL / TICKET
// ===============================
function abrirModalPago() {
  if (carrito.length === 0) {
    alert("El carrito está vacío. Agrega productos primero.");
    return;
  }

  // Numero de orden (visual)
  let ultimoNum = parseInt(localStorage.getItem("ultimoNumeroOrden")) || 0;
  let siguienteNum = ultimoNum + 1;

  const orderNum = document.getElementById("ticketOrderNum");
  if (orderNum) orderNum.innerText = "ORDEN #" + siguienteNum;

  const modal = document.getElementById("modalTicket");
  const content = document.getElementById("ticketContent");
  const totalDisplay = document.getElementById("ticketTotalDisplay");

  if (!modal || !content || !totalDisplay) return;

  content.innerHTML = "";

  carrito.forEach((item) => {
    const row = document.createElement("div");
    row.className = "ticket-row";
    row.style.display = "flex";
    row.style.justifyContent = "space-between";
    row.innerHTML = `
      <span>${item.name}</span>
      <span>$${item.price.toFixed(2)}</span>
    `;
    content.appendChild(row);
  });

  const total = carrito.reduce((acc, item) => acc + item.price, 0);
  totalDisplay.innerText = `$${total.toFixed(2)}`;

  modal.style.display = "flex";
}

function cerrarModal() {
  const modal = document.getElementById("modalTicket");
  if (modal) modal.style.display = "none";
}

function confirmarCompra() {
  let ultimoNum = parseInt(localStorage.getItem("ultimoNumeroOrden")) || 0;
  let nuevoNumeroOrden = ultimoNum + 1;

  localStorage.setItem("ultimoNumeroOrden", nuevoNumeroOrden);

  // imprimir
  window.print();

  // guardar a cocina
  const pedidosExistentes = JSON.parse(localStorage.getItem("pedidosCocina")) || [];

  const nuevoPedido = {
    id: Date.now(),
    numeroOrden: nuevoNumeroOrden,
    hora: new Date().toLocaleTimeString(),
    items: carrito,
    total: carrito.reduce((acc, item) => acc + item.price, 0),
  };

  pedidosExistentes.push(nuevoPedido);
  localStorage.setItem("pedidosCocina", JSON.stringify(pedidosExistentes));

  // limpiar
  carrito = [];
  actualizarCarritoUI();
  cerrarModal();
}
