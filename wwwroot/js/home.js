
let booksRaw = [];

const state = {
    q: "",
    genre: "همه",
    sort: "popular",
};

const els = {
    q: document.getElementById("q"),
    sort: document.getElementById("sort"),
    grid: document.getElementById("bookGrid"),
    chips: document.getElementById("genreChips"),
    resultCount: document.getElementById("resultCount"),
    empty: document.getElementById("emptyState"),
    reset: document.getElementById("resetBtn"),
    burger: document.getElementById("burgerBtn"),
    mobileNav: document.getElementById("mobileNav"),
    statBooks: document.getElementById("statBooks"),
    statReviews: document.getElementById("statReviews"),
    statAvg: document.getElementById("statAvg"),
};

async function loadBooks() {
    const res = await fetch("/api/books", { headers: { "Accept": "application/json" } });
    if (!res.ok) {
        const t = await res.text().catch(() => "");
        throw new Error("GET /api/books failed: " + res.status + " " + t);
    }
    const data = await res.json();

    return (data || []).map(b => ({
        id: b.id,
        title: b.title,
        author: b.author,
        year: b.year,
        genre: b.genre,
        lang: b.lang,
        tags: b.tags || [],
        cover: b.cover || b.coverPath || "",
        reviewCount: Number(b.reviewCount || 0),
        avgRating: Number(b.avgRating || 0),
    }));
}

function enrichBooks(data) {
    return data.map(b => ({
        ...b,
        _realCount: b.reviewCount,
        _realAvg: b.avgRating
    }));
}

function toStars(rating) {
    const full = Math.floor(rating);
    const half = rating - full >= 0.5 ? 1 : 0;
    const empty = 5 - full - half;
    return "★".repeat(full) + (half ? "½" : "") + "☆".repeat(empty);
}

function formatNumber(n) {
    try { return new Intl.NumberFormat("fa-IR").format(n); }
    catch { return String(n); }
}

function getGenres(data) {
    const set = new Set(data.map(b => (b.genre || "").trim()).filter(Boolean));
    return ["همه", ...Array.from(set)];
}

function applyFilters(data) {
    let out = data;

    if (state.q.trim()) {
        const q = state.q.trim().toLowerCase();
        out = out.filter(b =>
            (b.title || "").toLowerCase().includes(q) ||
            (b.author || "").toLowerCase().includes(q)
        );
    }

    if (state.genre !== "همه") {
        out = out.filter(b => (b.genre || "").trim() === state.genre);
    }

    return out;
}

function applySort(data) {
    const out = [...data];

    switch (state.sort) {
        case "rating_desc":
            out.sort((a, b) => (b._realAvg || 0) - (a._realAvg || 0));
            break;

        case "title_asc":
            out.sort((a, b) => (a.title || "").localeCompare((b.title || ""), "fa"));
            break;

        case "year_desc":
            out.sort((a, b) => (b.year || 0) - (a.year || 0));
            break;

        case "popular":
        default:
            out.sort((a, b) => (b._realCount || 0) - (a._realCount || 0));
            break;
    }

    return out;
}

function renderChips(books) {
    if (!els.chips) return;

    const genres = getGenres(books);
    els.chips.innerHTML = genres.map(g => `
    <button class="chip ${g === state.genre ? "is-active" : ""}" data-genre="${g}" type="button">
      ${g}
    </button>
  `).join("");

    els.chips.querySelectorAll(".chip").forEach(btn => {
        btn.addEventListener("click", () => {
            state.genre = btn.dataset.genre;
            renderAll();
        });
    });
}

function renderGrid(books) {
    if (!els.grid) return;

    const filtered = applyFilters(books);
    const sorted = applySort(filtered);

    if (els.resultCount) els.resultCount.textContent = `${formatNumber(sorted.length)} نتیجه`;

    if (sorted.length === 0) {
        els.grid.innerHTML = "";
        if (els.empty) els.empty.hidden = false;
        return;
    }
    if (els.empty) els.empty.hidden = true;

    els.grid.innerHTML = sorted.map(b => {
        const detailsUrl = `/Books/Details?id=${b.id}`;
        const reviewUrl = `/Books/Details?id=${b.id}#reviews`;

        const avg = b._realAvg || 0;
        const cnt = b._realCount || 0;

        return `
      <article class="card">
        <a href="${detailsUrl}" class="cover" title="جزئیات کتاب">
          ${b.cover
                ? `<img src="${b.cover}" alt="${b.title || "کتاب"}">`
                : `<span class="muted" style="font-weight:700;">کاور</span>`
            }
          <span class="badge">${b.genre || ""}</span>
        </a>

        <div class="card-body">
          <h3 class="title">${b.title || ""}</h3>
          <div class="author">
            ${b.author || ""} · <span class="muted">${b.year ? formatNumber(b.year) : "—"}</span>
          </div>

          <div class="meta">
            <div class="rating" title="${avg}">
              <span aria-hidden="true">⭐</span>
              <span>${cnt ? avg.toFixed(1) : "—"}</span>
              <small>(${formatNumber(cnt)})</small>
            </div>
            <div class="muted" style="font-size:.92rem;">${cnt ? toStars(avg) : "—"}</div>
          </div>

          <div class="card-actions">
            <a class="btn btn-ghost btn-sm" href="${detailsUrl}">مشاهده</a>
            <a class="btn btn-primary btn-sm" href="${reviewUrl}">ثبت نظر</a>
          </div>
        </div>
      </article>
    `;
    }).join("");
}

function renderStats(books) {
    const totalBooks = books.length;
    const totalReviews = books.reduce((sum, b) => sum + (b._realCount || 0), 0);

    const genres = new Set(books.map(b => (b.genre || "").trim()).filter(Boolean));

    if (els.statBooks) els.statBooks.textContent = formatNumber(totalBooks);
    if (els.statReviews) els.statReviews.textContent = formatNumber(totalReviews);
    if (els.statAvg) els.statAvg.textContent = formatNumber(genres.size); // تعداد ژانرها
}

function renderAll() {
    const books = enrichBooks(booksRaw);
    renderStats(books);
    renderChips(books);
    renderGrid(books);
}

if (els.q) {
    els.q.addEventListener("input", (e) => {
        state.q = e.target.value;
        renderAll();
    });
}

if (els.sort) {
    els.sort.addEventListener("change", (e) => {
        state.sort = e.target.value;
        renderAll();
    });
}

if (els.reset) {
    els.reset.addEventListener("click", () => {
        state.q = "";
        state.genre = "همه";
        state.sort = "popular";
        if (els.q) els.q.value = "";
        if (els.sort) els.sort.value = "popular";
        renderAll();
    });
}

if (els.burger && els.mobileNav) {
    els.burger.addEventListener("click", () => {
        const isHidden = els.mobileNav.hasAttribute("hidden");
        if (isHidden) els.mobileNav.removeAttribute("hidden");
        else els.mobileNav.setAttribute("hidden", "");
    });
}

(async () => {
    try {
        booksRaw = await loadBooks();
        renderAll();
    } catch (err) {
        console.error(err);
        if (els.resultCount) els.resultCount.textContent = "خطا در دریافت کتاب‌ها";
    }
})();

(() => {
    const btn = document.getElementById("helpBtn");
    const modal = document.getElementById("helpModal");
    const closeBtn = document.getElementById("helpCloseBtn");
    const okBtn = document.getElementById("helpOkBtn");

    if (!btn || !modal) return;

    const open = () => {
        modal.hidden = false;
        document.body.style.overflow = "hidden";
        setTimeout(() => (closeBtn || okBtn)?.focus?.(), 0);
    };

    const close = () => {
        modal.hidden = true;
        document.body.style.overflow = "";
        btn.focus();
    };

    btn.addEventListener("click", open);
    closeBtn?.addEventListener("click", close);
    okBtn?.addEventListener("click", close);

    modal.addEventListener("click", (e) => {
        if (e.target?.dataset?.close) close();
    });

    document.addEventListener("keydown", (e) => {
        if (!modal.hidden && e.key === "Escape") close();
    });
})();
