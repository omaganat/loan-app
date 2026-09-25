const express = require("express");

const app = express();
const port = process.env.PORT || 4000;
const store = new Map();

app.use(express.json());

app.get("/health", (_req, res) => res.json({ status: "ok" }));

app.get("/customers", (_req, res) => {
  res.json([...store.values()]);
});

app.get("/customers/:ssn", (req, res) => {
  const item = store.get(req.params.ssn);
  if (!item) return res.status(404).json({ error: "not found" });
  res.json(item);
});

app.post("/customers", (req, res) => {
  const payload = req.body;
  if (!payload?.ssn) return res.status(400).json({ error: "ssn is required" });
  store.set(payload.ssn, { ...payload, receivedAt: new Date().toISOString(), method: "POST" });
  console.log("CREATE", payload.ssn, payload.firstName, payload.requestedAmount);
  res.status(200).json({ ok: true, action: "created" });
});

app.put("/customers/:ssn", (req, res) => {
  const payload = req.body;
  const ssn = req.params.ssn;
  store.set(ssn, { ...payload, ssn, receivedAt: new Date().toISOString(), method: "PUT" });
  console.log("UPDATE", ssn, payload.firstName, payload.requestedAmount);
  res.status(200).json({ ok: true, action: "updated" });
});

app.listen(port, () => {
  console.log(`External mock listening on http://localhost:${port}`);
});
