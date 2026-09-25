"use client";

import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";

function ApprovedContent() {
  const params = useSearchParams();
  const applicationId = params.get("applicationId");
  const returning = params.get("returning") === "true";

  return (
    <main className="page">
      <p className="eyebrow">Decision</p>
      <h1>Application approved</h1>
      <div className="banner">
        <div className="result ok">
          {returning
            ? "Existing customer and application were updated."
            : "A new customer and application were saved."}
        </div>
        {applicationId ? <p className="hint">Application ID: {applicationId}</p> : null}
        <p>
          A background worker will send the same payload to the external mock
          service. Open <code>http://localhost:4000/customers</code> to confirm it arrived.
        </p>
        <Link href="/">Submit another application</Link>
      </div>
    </main>
  );
}

export default function ApprovedPage() {
  return (
    <Suspense>
      <ApprovedContent />
    </Suspense>
  );
}
