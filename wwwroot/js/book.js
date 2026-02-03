function getCsrfToken() {
    const meta = document.querySelector('meta[name="csrf-token"]');
    return meta ? (meta.getAttribute("content") || "") : "";
}

async function fetchBook(id) {
    if (!id) return null;

    const res = await fetch(`/api/books/${id}`, {
        headers: { "Accept": "application/json" }
    });

    if (!res.ok) return null;
    return await res.json();
}

async function postReview(bookId, payload) {
    const csrf = getCsrfToken();

    const headers = {
        "Content-Type": "application/json",
        "Accept": "application/json"
    };

    if (csrf) headers["X-CSRF-TOKEN"] = csrf;

    const res = await fetch(`/api/books/${bookId}/reviews`, {
        method: "POST",
        headers,
        body: JSON.stringify(payload)
    });

    if (res.status === 401 || res.status === 403) {
        throw new Error("برای ثبت نظر باید وارد حساب کاربری شوید.");
    }

    if (!res.ok) {
        const msg = await res.text().catch(() => "خطا");
        throw new Error(msg);
    }
}

const els = {
    bookTitle: document.getElementById("bookTitle"),
    bcTitle: document.getElementById("bcTitle"),
    bookSubtitle: document.getElementById("bookSubtitle"),

    statAvg: document.getElementById("statAvg"),
    statReviews: document.getElementById("statReviews"),
    statYear: document.getElementById("statYear"),

    coverWrap: document.getElementById("coverWrap"),
    genreBadge: document.getElementById("genreBadge"),

    starRow: document.getElementById("starRow"),
    ratingText: document.getElementById("ratingText"),
    genreChips: document.getElementById("genreChips"),

    bookDesc: document.getElementById("bookDesc"),
    bookAuthor: document.getElementById("bookAuthor"),
    bookYear: document.getElementById("bookYear"),
    bookGenre: document.getElementById("bookGenre"),
    bookId: document.getElementById("bookId"),
    bookLang: document.getElementById("bookLang"),

    reviewCountInline: document.getElementById("reviewCountInline"),
    reviewList: document.getElementById("reviewList"),

    reviewForm: document.getElementById("reviewForm"),
    rName: document.getElementById("rName"),
    rRate: document.getElementById("rRate"),
    rText: document.getElementById("rText"),
    charHint: document.getElementById("charHint"),
    clearBtn: document.getElementById("clearBtn"),
    formMsg: document.getElementById("formMsg"),

    wishBtn: document.getElementById("wishBtn"),
};

function formatFa(n) {
    try { return new Intl.NumberFormat("fa-IR").format(n); }
    catch { return String(n); }
}

function esc(s) {
    return String(s ?? "").replace(/[&<>"']/g, m => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[m]));
}

function starsText(r) {
    const x = Math.max(0, Math.min(5, Math.round(Number(r) || 0)));
    return "★".repeat(x) + "☆".repeat(5 - x);
}

function avgRating(arr) {
    if (!arr || !arr.length) return 0;
    const sum = arr.reduce((a, b) => a + (Number(b.rating) || 0), 0);
    return sum / arr.length;
}

function initials(name) {
    const t = (name || "کاربر").trim();
    return t.split(/\s+/).slice(0, 2).map(x => x[0]).join("").toUpperCase();
}

function getBookId() {
    const url = new URL(window.location.href);
    return url.searchParams.get("id");
}

function buildCover(book) {
    els.coverWrap.innerHTML = `
    <span class="badge" id="genreBadge">${esc(book.genre || "—")}</span>
  `;

    if (book.cover) {
        const img = document.createElement("img");
        img.src = book.cover;
        img.alt = book.title || "Book Cover";
        els.coverWrap.appendChild(img);
    } else {
        els.coverWrap.insertAdjacentHTML("beforeend", `<div class="muted">بدون تصویر</div>`);
    }
}

function renderReviews(shownReviews) {
    els.reviewList.innerHTML = "";

    if (!shownReviews.length) {
        els.reviewList.innerHTML = `
      <div class="review">
        <div class="muted">هنوز نظری ثبت نشده. اولین نفر باش! 🙂</div>
      </div>
    `;
        return;
    }

    const sorted = shownReviews
        .slice()
        .sort((a, b) => (b.createdAt || 0) - (a.createdAt || 0));

    sorted.forEach(r => {
        const name = r.name || "کاربر";
        const date = new Date(r.createdAt || Date.now()).toLocaleDateString("fa-IR");
        const rating = Number(r.rating || 0);

        const div = document.createElement("div");
        div.className = "review";
        div.innerHTML = `
      <div class="review-head">
        <div class="r-user">
          <div class="avatar">${esc(initials(name))}</div>
          <div>
            <div class="r-name">${esc(name)}</div>
            <div class="muted" style="font-size:.9rem;">نظر کاربر</div>
          </div>
        </div>

        <div class="r-meta">
          <div class="r-stars" aria-label="امتیاز">${esc(starsText(rating))}</div>
          <div class="r-date">${esc(date)}</div>
        </div>
      </div>

      <p>${esc(r.text || "")}</p>
    `;
        els.reviewList.appendChild(div);
    });
}

function renderPage(book) {
    const shownReviews = Array.isArray(book.reviewList) ? book.reviewList : [];

    els.bookTitle.textContent = book.title || "جزئیات کتاب";
    els.bcTitle.textContent = book.title || "جزئیات کتاب";
    els.bookSubtitle.textContent = `${book.author || "—"} • ${book.genre || "—"} • ${book.year || "—"}`;

    els.statYear.textContent = book.year ?? "—";
    const count = shownReviews.length;
    const avg = count ? avgRating(shownReviews) : 0;

    els.statReviews.textContent = formatFa(count);
    els.statAvg.textContent = count ? avg.toFixed(1) : "—";

    buildCover(book);

    els.genreChips.innerHTML = "";
    if (book.genre) {
        const s = document.createElement("span");
        s.className = "chip is-active";
        s.textContent = book.genre;
        els.genreChips.appendChild(s);
    }

    els.starRow.textContent = count ? starsText(avg) : "☆☆☆☆☆";
    els.ratingText.textContent = count ? `${avg.toFixed(1)} از 5` : "هنوز امتیازی ثبت نشده";

    els.bookDesc.textContent = book.description || "توضیحی برای این کتاب ثبت نشده است.";
    els.bookAuthor.textContent = book.author || "—";
    els.bookYear.textContent = book.year || "—";
    els.bookGenre.textContent = book.genre || "—";
    els.bookId.textContent = book.id ?? "—";
    els.bookLang.textContent = (book.lang || "FA").toUpperCase();

    els.reviewCountInline.textContent = `${formatFa(count)} نظر`;

    renderReviews(shownReviews);
}

async function init() {
    const id = getBookId();
    const book = await fetchBook(id);

    if (!book) {
        document.body.innerHTML = `
      <div style="padding:2rem; color:white; font-family:Vazirmatn">
        کتاب پیدا نشد.
      </div>`;
        return;
    }

    let currentBook = book;
    renderPage(currentBook);

    const gate = document.getElementById("authGate");
    const isAuth = (gate?.dataset?.auth === "true");
    const userNameFromClaim = (gate?.dataset?.userName || "").trim();

    if (!isAuth) {
        if (els.reviewForm) els.reviewForm.hidden = true;
        if (gate) gate.hidden = false;
        return;
    }

    if (els.rName) {
        if (userNameFromClaim) els.rName.value = userNameFromClaim;
        els.rName.readOnly = true;
    }

    const updateCount = () => {
        const len = (els.rText.value || "").length;
        els.charHint.textContent = `${len}/400`;
    };
    updateCount();
    els.rText.addEventListener("input", updateCount);

    els.clearBtn.addEventListener("click", () => {
        if (!els.rName.readOnly) els.rName.value = "";
        els.rRate.value = "5";
        els.rText.value = "";
        updateCount();
        els.formMsg.hidden = true;
    });

    els.reviewForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const name = (els.rName.value || "").trim() || "کاربر";
        const text = (els.rText.value || "").trim();
        const rating = Number(els.rRate.value || 5);

        if (text.length < 10) {
            els.formMsg.hidden = false;
            els.formMsg.textContent = "متن نظر باید حداقل 10 کاراکتر باشد.";
            return;
        }

        try {
            await postReview(currentBook.id, { displayName: name, rating, text });

            const updated = await fetchBook(currentBook.id);
            if (updated) currentBook = updated;

            if (!els.rName.readOnly) els.rName.value = "";
            els.rRate.value = "5";
            els.rText.value = "";
            updateCount();

            renderPage(currentBook);

            els.formMsg.hidden = false;
            els.formMsg.textContent = "نظر شما ثبت شد ✅";
        } catch (err) {
            els.formMsg.hidden = false;
            els.formMsg.textContent = err?.message || "خطا در ثبت نظر";
        }
    });

    els.wishBtn?.addEventListener("click", () => {
        alert(" به علاقه‌مندی‌ها اضافه شد ✅");
    });
}

init();
