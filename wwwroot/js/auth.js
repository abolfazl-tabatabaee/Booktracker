

document.addEventListener("click", (e) => {
    const btn = e.target.closest("[data-eye]");
    if (!btn) return;
  
    const id = btn.getAttribute("data-eye");
    const input = document.getElementById(id);
    if (!input) return;
  
    const isHidden = input.type === "password";
    input.type = isHidden ? "text" : "password";
    btn.classList.toggle("is-on", isHidden);
  });
  
  (() => {
    const p1 = document.getElementById("password");
    const p2 = document.getElementById("password2");
    const hint = document.getElementById("pwdHint");
    const btn = document.getElementById("registerBtn");
  
    if (!p1 || !p2 || !hint || !btn) return;
  
    const validate = () => {
      const a = p1.value.trim();
      const b = p2.value.trim();
      const show = a.length > 0 && b.length > 0 && a !== b;
      hint.style.display = show ? "block" : "none";
    };
  
    p1.addEventListener("input", validate);
    p2.addEventListener("input", validate);
  })();
  