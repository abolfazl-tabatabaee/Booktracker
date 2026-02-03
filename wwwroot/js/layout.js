
function setActiveNav() {
    const page = document.body?.dataset?.page;
    if (!page) return;

    document.querySelectorAll("[data-nav]").forEach(a => {
        a.classList.toggle("is-active", a.dataset.nav === page);
    });
}

function wireMobileMenu() {
    const burger = document.getElementById("burgerBtn");
    const mobileNav = document.getElementById("mobileNav");
    if (!burger || !mobileNav) return;

    const open = () => mobileNav.removeAttribute("hidden");
    const close = () => mobileNav.setAttribute("hidden", "");

    burger.addEventListener("click", (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();

        const isHidden = mobileNav.hasAttribute("hidden");
        if (isHidden) open();
        else close();
    }, true); 

    mobileNav.querySelectorAll("a").forEach(a => {
        a.addEventListener("click", () => close());
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") close();
    });

    document.addEventListener("click", (e) => {
        if (mobileNav.hasAttribute("hidden")) return;
        const t = e.target;
        if (t === burger || burger.contains(t)) return;
        if (mobileNav.contains(t)) return;
        close();
    });
}

document.addEventListener("DOMContentLoaded", () => {
    setActiveNav();
    wireMobileMenu();
});
