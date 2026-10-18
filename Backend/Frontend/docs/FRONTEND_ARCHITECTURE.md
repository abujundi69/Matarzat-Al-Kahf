# Frontend architecture

## Overview

The client is a Vite React SPA organized by application infrastructure, shared components, layouts, features, and route pages. It consumes the ASP.NET API through typed contracts generated from the live OpenAPI 3.0 document.

## Data and API

- Axios centralizes the API base URL, timeout, bearer tokens, language, and guest cart header.
- TanStack Query owns server state, caching, request deduplication, pagination, invalidation, and retry policy.
- Zustand persists only client session metadata and the guest cart token.
- RFC Problem Details parsing is centralized in `src/lib/api/problemDetails.ts`.
- Binary exports use Blob responses and revoke object URLs after download.

## Authentication and permissions

Admin and customer sessions are separate. Route guards redirect unauthenticated users to the correct login surface. Granular permissions come from the admin session and gate the admin navigation and actions; backend authorization remains authoritative.

## Localization

i18next provides Arabic and English resources. Arabic is the default. Language changes update the root `lang` and `dir`, persist the choice, and cause Axios to send `Accept-Language`.

## UI and responsive system

Tailwind CSS is backed by heritage design tokens: burgundy, olive, sand, cream, gold, and charcoal. A restrained CSS geometric pattern replaces unavailable licensed imagery. Layouts use mobile-first grids and drawers. Admin tables retain a safe scroll fallback on narrow screens.

## Errors and accessibility

Loading, skeleton, empty, error, and unexpected-rendering states are shared. Forms use labels, React Hook Form, and Zod. Focus indicators, semantic elements, reduced-motion handling, accessible names, and minimum touch sizes are included.

## Security

Rendered content pages are sanitized with DOMPurify. External URL protocols are validated. Tokens, passwords, and OTP codes are never logged. Checkout does not collect cards or calculate authoritative totals locally.