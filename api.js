// ============================================================
// Cấu hình chung để gọi API backend (ASP.NET Core Web API)
// ============================================================
const API_BASE_URL = "https://localhost:7080/api"; // Đổi lại theo cổng thực tế khi chạy backend

const Api = {
  getToken() {
    return localStorage.getItem("qeval_token");
  },
  setSession(token, fullName, role) {
    localStorage.setItem("qeval_token", token);
    localStorage.setItem("qeval_fullname", fullName);
    localStorage.setItem("qeval_role", role);
  },
  clearSession() {
    localStorage.removeItem("qeval_token");
    localStorage.removeItem("qeval_fullname");
    localStorage.removeItem("qeval_role");
  },
  getRole() {
    return localStorage.getItem("qeval_role");
  },
  getFullName() {
    return localStorage.getItem("qeval_fullname");
  },

  async request(path, { method = "GET", body = null, auth = true } = {}) {
    const headers = { "Content-Type": "application/json" };
    if (auth) {
      const token = this.getToken();
      if (token) headers["Authorization"] = `Bearer ${token}`;
    }

    const res = await fetch(`${API_BASE_URL}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : null,
    });

    if (res.status === 401) {
      this.clearSession();
      window.location.href = "login.html";
      return null;
    }

    const contentType = res.headers.get("content-type") || "";
    const data = contentType.includes("application/json") ? await res.json() : await res.text();

    if (!res.ok) {
      const message = (data && data.message) || "Đã có lỗi xảy ra. Vui lòng thử lại.";
      throw new Error(message);
    }
    return data;
  },

  get(path) { return this.request(path, { method: "GET" }); },
  post(path, body) { return this.request(path, { method: "POST", body }); },
  put(path, body) { return this.request(path, { method: "PUT", body }); },
  del(path) { return this.request(path, { method: "DELETE" }); },
};

// Bắt buộc đăng nhập cho các trang nội bộ (gọi ở đầu mỗi trang, trừ login.html)
function requireAuth(allowedRoles = null) {
  const token = Api.getToken();
  if (!token) {
    window.location.href = "login.html";
    return;
  }
  if (allowedRoles && !allowedRoles.includes(Api.getRole())) {
    alert("Bạn không có quyền truy cập trang này.");
    window.location.href = "dashboard.html";
  }
}
