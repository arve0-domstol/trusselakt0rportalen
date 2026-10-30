// Small standalone internal platform/status service used by other internal
// systems.
//
// All data below is synthetic training data.

import crypto from 'node:crypto'
import express from 'express'

const app = express()
const port = process.env.PORT ?? 9090

// Generated fresh each time this service starts, so the admin password
// only ever exists in this process's memory - never written to disk,
// never committed, and different on every restart.
function generateAdminPassword(length = 14) {
  const alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789'
  let password = ''
  for (let i = 0; i < length; i++) {
    password += alphabet[crypto.randomInt(alphabet.length)]
  }
  return password
}

const adminPassword = generateAdminPassword()

app.get('/internal/status', (_req, res) => {
  res.json({
    service: 'internal-mock-service',
    status: 'running',
    uptimeSeconds: Math.floor(process.uptime()),
  })
})

app.get('/internal/config', (_req, res) => {
  res.json({
    environment: 'training',
    featureFlags: { newCaseWorkflow: true, exportV2: false },
    // Clearly-fake, training-only value — never a real secret.
    internalServiceToken: 'TRAINING-ONLY-SECRET-INTERNAL-TOKEN-0001',
  })
})

app.get('/internal/appsettings', (_req, res) => {
  res.json({
    AdminCredentials: {
      Username: 'admin',
      Password: adminPassword
    },
    })
  })

app.get('/internal/service-info', (_req, res) => {
  res.json({
    name: 'saksbehandling-internal-core',
    version: '3.4.1-training',
    dependencies: ['directory-service', 'notification-relay'],
    maintainer: 'internal-platform-team@example-training.local',
  })
})

app.get('/health', (_req, res) => res.json({ status: 'ok' }))

app.listen(port, () => {
  console.log(`internal-mock-service listening on port ${port}`)
})
