"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";

const STATES = [
  "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI","ID","IL","IN","IA","KS",
  "KY","LA","ME","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ","NM","NY",
  "NC","ND","OH","OK","OR","PA","RI","SC","SD","TN","TX","UT","VT","VA","WA","WV",
  "WI","WY","DC"
];

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5080";

const emptyForm = {
  firstName: "",
  lastName: "",
  address: "",
  state: "",
  companyName: "",
  requestedAmount: "",
  ssn: ""
};

export default function HomePage() {
  const router = useRouter();
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);

  const canSubmit = useMemo(() => {
    return (
      form.firstName &&
      form.lastName &&
      form.address &&
      form.state &&
      form.companyName &&
      form.requestedAmount &&
      form.ssn
    );
  }, [form]);

  function update(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function onSubmit(event) {
    event.preventDefault();
    setError("");
    setPending(true);

    try {
      const response = await fetch(`${API_URL}/api/applications`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          firstName: form.firstName,
          lastName: form.lastName,
          address: form.address,
          state: form.state,
          companyName: form.companyName,
          requestedAmount: Number(form.requestedAmount),
          ssn: form.ssn
        })
      });

      const payload = await response.json();

      if (!response.ok) {
        setError((payload.errors || ["Unable to submit the application."]).join(" "));
        return;
      }

      if (!payload.approved) {
        const params = new URLSearchParams({
          code: payload.denialCode || "",
          reason: payload.denialReason || "The application was denied."
        });
        router.push(`/denied?${params.toString()}`);
        return;
      }

      const params = new URLSearchParams({
        applicationId: payload.applicationId,
        returning: String(payload.returningCustomer)
      });
      router.push(`/approved?${params.toString()}`);
    } catch {
      setError("The API is not reachable. Start the backend on port 5080.");
    } finally {
      setPending(false);
    }
  }

  return (
    <main className="page">
      <p className="eyebrow">Northshore Lending</p>
      <h1>Apply for a small business loan</h1>
      <p className="lede">
        One form. A rules engine decides. Approved applications are saved once per SSN
        and forwarded to our partner service in the background.
      </p>

      <form className="card" onSubmit={onSubmit}>
        {error ? <div className="error">{error}</div> : null}

        <div className="grid">
          <div className="field">
            <label htmlFor="firstName">First name</label>
            <input id="firstName" value={form.firstName} onChange={(e) => update("firstName", e.target.value)} required />
          </div>
          <div className="field">
            <label htmlFor="lastName">Last name</label>
            <input id="lastName" value={form.lastName} onChange={(e) => update("lastName", e.target.value)} required />
          </div>
          <div className="field full">
            <label htmlFor="address">Street address</label>
            <input id="address" value={form.address} onChange={(e) => update("address", e.target.value)} required />
          </div>
          <div className="field">
            <label htmlFor="state">State</label>
            <select id="state" value={form.state} onChange={(e) => update("state", e.target.value)} required>
              <option value="">Select a state</option>
              {STATES.map((state) => (
                <option key={state} value={state}>{state}</option>
              ))}
            </select>
          </div>
          <div className="field">
            <label htmlFor="companyName">Company name</label>
            <input id="companyName" value={form.companyName} onChange={(e) => update("companyName", e.target.value)} required />
          </div>
          <div className="field">
            <label htmlFor="requestedAmount">Requested amount</label>
            <input
              id="requestedAmount"
              type="number"
              min="1"
              step="1"
              value={form.requestedAmount}
              onChange={(e) => update("requestedAmount", e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="ssn">SSN</label>
            <input
              id="ssn"
              inputMode="numeric"
              placeholder="123-45-6789"
              value={form.ssn}
              onChange={(e) => update("ssn", e.target.value)}
              required
            />
          </div>
        </div>

        <div className="actions">
          <p className="hint">NY applications and blacklisted SSNs are denied immediately.</p>
          <button type="submit" disabled={!canSubmit || pending}>
            {pending ? "Reviewing…" : "Submit application"}
          </button>
        </div>
      </form>
    </main>
  );
}
