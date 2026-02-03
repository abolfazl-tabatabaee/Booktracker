
const els = {
    statBooks: document.getElementById("statBooks"),
    statReviews: document.getElementById("statReviews"),
    statAvg: document.getElementById("statAvg"),
};

function formatNumber(n) {
    try { return new Intl.NumberFormat("fa-IR").format(n); }
    catch { return String(n); }
}

async function loadStats() {
    const res = await fetch("/api/stats", { headers: { "Accept": "application/json" } });
    if (!res.ok) {
        const t = await res.text().catch(() => "");
        throw new Error("GET /api/stats failed: " + res.status + " " + t);
    }
    return await res.json();
}

function renderStats(s) {
    if (els.statBooks) els.statBooks.textContent = formatNumber(s.totalBooks ?? 0);
    if (els.statReviews) els.statReviews.textContent = formatNumber(s.totalReviews ?? 0);
    if (els.statAvg) els.statAvg.textContent = formatNumber(s.genres ?? 0); // تعداد ژانرها
}

(async function init() {
    try {
        const stats = await loadStats();
        renderStats(stats);
    } catch (e) {
        console.error(e);
    }
})();
