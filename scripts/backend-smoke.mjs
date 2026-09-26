// Runs only against the disposable localhost CI API, never a deployed service.
import assert from "node:assert/strict";
const base = "http://127.0.0.1:10000";
async function call(path, options = {}, status = 200) {
  const response = await fetch(base + path, options);
  assert.equal(response.status, status, `${path}: ${response.status} ${await response.clone().text()}`);
  return response;
}
async function login(email, password) {
  const result = await call("/api/auth/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, password }) });
  return (await result.json()).accessToken;
}
await call("/health/live");
await call("/health/ready");
await call("/api/database-backup/export", {}, 401);
const admin = await login("admin@orisia.bg", "Admin123!");
const editor = await login("editor@orisia.bg", "Editor123!");
await call("/api/database-backup/export", { headers: { Authorization: `Bearer ${editor}` } }, 403);
const headers = { Authorization: `Bearer ${admin}`, "Content-Type": "application/json" };
for (const path of ["/api/posts", "/api/events", "/api/feed", "/api/gallery", "/api/horoteka", "/api/reviews"])
  await call(path);
await call("/api/posts", { method: "POST", headers, body: "{}" }, 400);
const created = await (await call("/api/posts", { method: "POST", headers, body: JSON.stringify({
  slug: "ci-smoke-post", type: 0, titleBg: "CI test", titleEn: "CI test", bodyBg: "Test body", bodyEn: "Test body",
}) }, 201)).json();
await call(`/api/posts/${created.id}/publish`, { method: "POST", headers });
await call("/api/posts/ci-smoke-post");
// Validate native image decoding, multipart binding, storage and public file serving.
const image = new FormData();
image.append("file", new Blob([Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aZ1sAAAAASUVORK5CYII=", "base64")], { type: "image/png" }), "ci.png");
const media = await (await call("/api/admin/media", { method: "POST", headers: { Authorization: `Bearer ${admin}` }, body: image }, 201)).json();
await call(new URL(media.url, base).pathname);
const dump = await (await call("/api/database-backup/export", { headers: { Authorization: `Bearer ${admin}` } })).blob();
assert.equal((await dump.slice(0, 5).text()), "PGDMP");
await call(`/api/posts/${created.id}`, { method: "DELETE", headers }, 204);
await call("/api/posts/ci-smoke-post", {}, 404);
const archive = new FormData();
archive.append("archive", dump, "ci.dump");
await call("/api/database-backup/restore", { method: "POST", headers: { Authorization: `Bearer ${admin}` }, body: archive }, 400);
archive.append("confirmation", "RESTORE ORISIA");
await call("/api/database-backup/restore", { method: "POST", headers: { Authorization: `Bearer ${admin}` }, body: archive });
await call("/api/posts/ci-smoke-post");
console.log("Backend HTTP smoke passed: auth, role restrictions, validation, public CMS, publication, media, export and restore.");
