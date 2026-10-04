// ============================================================
// Dựng phần banner trường + sidebar điều hướng dùng chung
// activePage: id trang hiện tại để bôi đậm menu tương ứng
// ============================================================
function renderLayout(activePage) {
  const role = Api.getRole();

  document.getElementById("banner-placeholder").innerHTML = `
    <div class="school-banner">
      <div class="school-banner__inner">
        <img src="assets/images/logo-header.png" alt="Logo trường" class="school-banner__logo" />
        <div class="school-banner__system-tag" style="margin-left:16px;">
          Hệ thống đánh giá chất lượng hoạt động
        </div>
        <div class="user-bar">
          <span class="user-name">${Api.getFullName() || ""}</span>
          <span class="role-badge">${roleLabel(role)}</span>
          <span class="btn-logout" onclick="doLogout()">Đăng xuất</span>
        </div>
      </div>
    </div>
  `;

  const menuItems = [
    { id: "dashboard", href: "dashboard.html", label: "🏠 Tổng quan", roles: ["ADMIN","MANAGER","LECTURER","STUDENT"] },
    { id: "criteria", href: "criteria.html", label: "🎯 Tiêu chí / KPI", roles: ["ADMIN","MANAGER"] },
    { id: "survey", href: "survey.html", label: "📋 Quản lý khảo sát", roles: ["ADMIN","MANAGER"] },
    { id: "survey-take", href: "survey-take.html", label: "✍️ Tham gia khảo sát", roles: ["ADMIN","MANAGER","LECTURER","STUDENT"] },
    { id: "evaluation", href: "evaluation.html", label: "📊 Phân tích & đánh giá", roles: ["ADMIN","MANAGER"] },
    { id: "reports", href: "reports.html", label: "📈 Thống kê & báo cáo", roles: ["ADMIN","MANAGER","LECTURER"] },
    { id: "users", href: "users.html", label: "👥 Quản lý người dùng", roles: ["ADMIN"] },
    { id: "change-password", href: "change-password.html", label: "🔒 Đổi mật khẩu", roles: ["ADMIN","MANAGER","LECTURER","STUDENT"] },
  ];

  const links = menuItems
    .filter(m => m.roles.includes(role))
    .map(m => `<a href="${m.href}" class="${m.id === activePage ? "active" : ""}">${m.label}</a>`)
    .join("");

  document.getElementById("sidebar-placeholder").innerHTML = `
    <aside class="sidebar">
      <div class="nav-section-label">Chức năng chính</div>
      <nav>${links}</nav>
    </aside>
  `;
}

function roleLabel(code) {
  const map = { ADMIN: "Quản trị", MANAGER: "Cán bộ QL", LECTURER: "Giảng viên", STUDENT: "Sinh viên" };
  return map[code] || code || "";
}

async function doLogout() {
  try { await Api.post("/auth/logout", {}); } catch (e) {}
  Api.clearSession();
  window.location.href = "login.html";
}
