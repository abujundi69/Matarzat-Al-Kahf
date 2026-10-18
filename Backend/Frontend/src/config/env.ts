import { z } from 'zod'

const schema = z.object({
  VITE_API_BASE_URL: z
    .string()
    .trim()
    .min(1, 'VITE_API_BASE_URL is not configured.')
    .refine((value) => value === '/api/v1' || URL.canParse(value), {
      message: 'VITE_API_BASE_URL must be /api/v1 or an absolute API URL.',
    })
    .transform((value) => value.replace(/\/+$/, ''))
    .refine((value) => value === '/api/v1' || new URL(value).pathname === '/api/v1', {
      message: 'VITE_API_BASE_URL must end with /api/v1.',
    }),
  VITE_REQUEST_TIMEOUT_MS: z.coerce.number().int().positive().default(15000),
})

export const env = schema.parse(import.meta.env)
