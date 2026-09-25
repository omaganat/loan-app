import "./globals.css";

export const metadata = {
  title: "Northshore Lending",
  description: "Small business loan application"
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
