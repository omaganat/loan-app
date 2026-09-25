"use client";

import { useSearchParams } from "next/navigation";
import { Suspense } from "react";
import Link from "next/link";

function DeniedContent() {
  const params = useSearchParams();
  const reason = params.get("reason") || "This application cannot be approved.";
  const code = params.get("code");

  return (
    <main className="page">
      <p className="eyebrow">Decision</p>
      <h1>Application denied</h1>
      <div className="banner">
        <div className="result no">{reason}</div>
        {code ? <p className="hint">Rule: {code}</p> : null}
        <p>
          No customer or application record was created. You can correct the
          details and try again.
        </p>
        <Link href="/">Return to the form</Link>
      </div>
    </main>
  );
}

export default function DeniedPage() {
  return (
    <Suspense>
      <DeniedContent />
    </Suspense>
  );
}
