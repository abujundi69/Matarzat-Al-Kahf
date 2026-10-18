# Matarzat Al Kahf Frontend

Bilingual React storefront and administration client for the ASP.NET Core 9 API in the parent directory.

## Requirements

- Node.js 22 or newer (verified with Node 24.16)
- npm 10 or newer
- .NET SDK capable of building the parent solution
- SQL Server LocalDB with the checked-in migrations applied

## Setup

```powershell
cd Backend
dotnet run --project MatarzatAlKahf.Api --launch-profile http

cd Frontend
npm install
npm run dev
```

The Vite development server listens on `http://127.0.0.1:5173`.
`.env.development` sets `VITE_API_BASE_URL=/api/v1`, so requests stay same-origin and the Vite
proxy forwards `/api`, `/uploads`, `/robots.txt`, and `/sitemap.xml` to the API on
`http://127.0.0.1:5212`. Point `VITE_API_BASE_URL` at an absolute URL ending in `/api/v1` to
call a different API directly; that origin must then appear in the backend's
`Cors:AllowedOrigins`.

## Commands

```powershell
npm run dev
npm run lint
npm run check:encoding
npm run typecheck
npm run build
npm run preview
npm run generate:api
```

`generate:api` regenerates TypeScript contracts from `docs/openapi.json`. Refresh that document from the running API whenever backend contracts change:

```powershell
curl.exe -s http://127.0.0.1:5212/swagger/v1/swagger.json -o docs/openapi.json
npm run generate:api
```

## Brand and design system

The Matarzat Al Kahf emblem is the source of truth for the visual identity.

- **Logo files** live in `src/assets/brand/`: `logo-sm.png` (192 px: header, sidebar, menus, footer),
  `logo-md.png` (384 px: splash and story band) and `logo.png` (768 px: the sign-in brand panel).
  They are content-hashed by Vite, and each is at least 2x the largest size it is drawn at, so the
  emblem is sharp on high-density screens without downloading more than a page needs. The favicon (`favicon.ico`), the iOS touch icon, the
  web manifest and its icons are static files in `public/`. Use the `BrandLogo` component rather than
  importing the files, and `ImagePlaceholder` for a missing product or category photo.
- **Owner override:** a logo or site icon uploaded under Settings replaces the bundled emblem. The
  generic development placeholder image is ignored everywhere and never shown as content.
- **Palette** (`src/styles/globals.css`, measured on the artwork): thread red `--heritage-burgundy`
  `#850b12` for primary actions and headings, `--heritage-red` `#a8141d` for hover and active states,
  tan gold `--heritage-gold` `#b48852` for rules and focus rings (never body text), warm black
  `--heritage-charcoal` `#231c18` for text and dark surfaces, cream `--heritage-beige` `#f4e9d8` and
  `--heritage-cream` `#fffaf2` for surfaces. Feedback colours (`--state-*`) are separate from the
  brand red so an error never looks like a call to action.
- **Motifs:** `.tatreez-band` (the emblem's diamond border as a ribbon), `.stitch-field` and
  `.heritage-pattern` (a faint cross-stitch texture on dark and red surfaces), `.placeholder-art`.
- **Status badges:** `statusBadgeClass(status)` in `src/lib/statusTone.ts` is the one place that maps a
  backend status to a tone, so the same word is the same colour in every table and card.
- **Type:** Noto Sans Arabic and Noto Sans are self-hosted (`@fontsource-variable`), so the page looks
  the same on every device and the CSP needs no third-party font host.
- **Re-exporting the logo:** keep the source art square with a clean alpha channel. Trim the
  transparent margin, flatten semi-transparent pixels inside the artwork, and export the two sizes
  above as lossless PNG; do not lossy-compress it.

## Runtime behavior

- Arabic is the default language and sets `lang="ar"` and `dir="rtl"`.
- English switches the complete shell to LTR. The choice persists locally.
- Customer and admin JWT sessions are logically separated; signing in as one clears the other.
- Logout calls the server (`/customer/auth/logout`, `/admin/auth/logout`), which revokes the
  session behind the presented token, and then clears the local session and cached queries.
- The current backend returns bearer tokens rather than secure cookies. Tokens are persisted in local storage so sessions survive refresh; this is vulnerable to successful XSS, so CSP and strict dependency hygiene are required in deployment. A future HttpOnly-cookie backend flow would be preferable.
- The backend creates the anonymous cart's opaque `X-Cart-Token`; the client
  persists and reuses it only after receiving it from a cart response.
- Checkout totals are always calculated by the backend preview endpoint.
- Order placement uses a fresh `Idempotency-Key`.

## Production

Run `npm run build` and serve `dist/` from a static host with SPA fallback to `index.html`. Configure `VITE_API_BASE_URL` at build time and add the frontend origin to backend `Cors:AllowedOrigins`. Do not put secrets in Vite environment variables.

Because bearer tokens live in local storage, the host must send a Content Security Policy with the
built assets. The bundle loads no third-party scripts, so a strict policy works as-is — replace
`https://api.example.com` with the configured API origin, or drop it from `connect-src` when the
API is same-origin:

```text
Content-Security-Policy: default-src 'none'; script-src 'self'; style-src 'self' 'unsafe-inline';
  img-src 'self' data: https://api.example.com; font-src 'self'; connect-src 'self' https://api.example.com;
  form-action 'none'; frame-ancestors 'none'; base-uri 'none'
X-Content-Type-Options: nosniff
Referrer-Policy: no-referrer
```

`style-src 'unsafe-inline'` is required because the bundle sets inline styles; everything else is
locked down. Keep dependencies current, since any script executing on this origin can read the
stored token.

## External limitations

- Production OTP delivery requires a real backend `IOtpDeliveryService`; HTTP 503 is shown as a localized unavailable state.
- No online card gateway is configured. The UI only exposes payment methods
  currently enabled by the backend, such as COD, bank transfer, and electronic
  wallet.
- SMS, WhatsApp, and email notification delivery require providers. In-app notifications work.
- Files use backend local storage; no cloud storage is claimed.
- A final logo and product photography were not present. The current text mark and CSS embroidery motif are original placeholders.
