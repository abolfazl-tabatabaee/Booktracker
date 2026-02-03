
const burger = document.getElementById("burgerBtn");
const mobileNav = document.getElementById("mobileNav");

if (burger && mobileNav) {
  burger.addEventListener("click", () => {
    const isHidden = mobileNav.hasAttribute("hidden");
    if (isHidden) mobileNav.removeAttribute("hidden");
    else mobileNav.setAttribute("hidden", "");
  });
}

const form = document.getElementById("contactFormEl");
const toast = document.getElementById("toast");
const email = document.getElementById("email");
const emailHint = document.getElementById("emailHint");

function isValidEmail(value) {
  if (!value) return true; 
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
}

if (form) {
  form.addEventListener("submit", (e) => {
    e.preventDefault();

    const ok = isValidEmail(email?.value || "");
    if (emailHint) emailHint.hidden = ok;

    if (!ok) {
      email?.focus();
      return;
    }

    if (toast) {
      toast.hidden = false;
      setTimeout(() => (toast.hidden = true), 2200);
    }

  });
}
