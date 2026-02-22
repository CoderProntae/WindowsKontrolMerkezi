import { app, BrowserWindow, ipcMain, shell } from 'electron'
import path from 'path'
import { exec } from 'child_process'
import { promisify } from 'util'
import fs from 'fs'

const execAsync = promisify(exec)
const si = require('systeminformation')

let mainWindow: BrowserWindow | null = null

const isDev = !app.isPackaged

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1100,
    height: 700,
    minWidth: 900,
    minHeight: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
    frame: true,
    backgroundColor: '#0f0f12',
    show: false,
  })
  mainWindow.once('ready-to-show', () => mainWindow?.show())
  if (isDev) {
    mainWindow.loadURL('http://localhost:5173')
    mainWindow.webContents.openDevTools()
  } else {
    mainWindow.loadFile(path.join(__dirname, '../dist/index.html'))
  }
  mainWindow.on('closed', () => { mainWindow = null })
}

app.whenReady().then(createWindow)
app.on('window-all-closed', () => app.quit())
app.on('activate', () => { if (!mainWindow) createWindow() })

// Sistem bilgisi (CPU, RAM)
ipcMain.handle('get-cpu-usage', async () => {
  const c = await si.currentLoad()
  return { usage: c.currentLoad, cores: c.cpus?.length ?? 0 }
})

ipcMain.handle('get-mem', async () => {
  const m = await si.mem()
  return {
    total: m.total,
    used: m.used,
    free: m.free,
    usagePercent: (m.used / m.total) * 100,
  }
})

ipcMain.handle('get-disk', async () => {
  const d = await si.fsSize()
  const c = d.find((x: { mount: string }) => x.mount === 'C:') || d[0]
  if (!c) return { total: 0, used: 0, free: 0, usagePercent: 0 }
  return {
    total: c.size,
    used: c.used,
    free: c.available,
    usagePercent: (c.used / c.size) * 100,
  }
})

// Güç planları (Performans modu)
ipcMain.handle('get-power-plans', async () => {
  try {
    const { stdout } = await execAsync('powercfg /list')
    const lines = stdout.split('\n').filter((l: string) => l.includes('GUID'))
    const plans: { guid: string; name: string; active: boolean }[] = []
    let activeGuid = ''
    for (const line of lines) {
      const guidMatch = line.match(/([a-f0-9-]{36})/gi)
      const guid = guidMatch?.[0] ?? ''
      const active = line.includes('*')
      if (active) activeGuid = guid
      const nameMatch = line.match(/\(([^)]+)\)\s*$/)?.[1]
      if (guid && nameMatch) plans.push({ guid, name: nameMatch.trim(), active })
    }
    return { plans, activeGuid }
  } catch {
    return { plans: [], activeGuid: '' }
  }
})

ipcMain.handle('set-power-plan', async (_e, guid: string) => {
  try {
    await execAsync(`powercfg /setactive ${guid}`)
    return { ok: true }
  } catch (err) {
    return { ok: false, error: String(err) }
  }
})

// Oyun modu: Windows ayarlarını aç (Game Bar / Oyun Modu)
ipcMain.handle('open-game-mode-settings', async () => {
  try {
    await execAsync('start ms-settings:gaming-gamemode')
    return { ok: true }
  } catch {
    try {
      await shell.openExternal('ms-settings:gaming-gamemode')
    } catch {}
    return { ok: true }
  }
})

// Sessiz mod: Bildirimleri kapat (Focus Assist)
ipcMain.handle('open-focus-assist', async () => {
  try {
    await execAsync('start ms-settings:quiethours')
    return { ok: true }
  } catch {
    await shell.openExternal('ms-settings:quiethours')
    return { ok: true }
  }
})

// Uygulama sürümü
ipcMain.handle('get-app-version', () => {
  return app.getVersion()
})

// Changelog (dev: proje kökü, prod: app.getAppPath())
ipcMain.handle('get-changelog', async () => {
  try {
    const base = app.isPackaged ? app.getAppPath() : path.join(__dirname, '..')
    const p = path.join(base, 'CHANGELOG.md')
    if (fs.existsSync(p)) return fs.readFileSync(p, 'utf-8')
    return ''
  } catch {
    return ''
  }
})

// Güncelleme kontrolü: geliştirmede public/version.json, aksi halde URL (kendi sunucunu ekleyebilirsin)
const UPDATE_MANIFEST_URL = 'https://raw.githubusercontent.com/placeholder/windows-kontrol-merkezi/main/version.json'

function parseVersion(v: string): number[] {
  return v.replace(/^v/, '').split('.').map(Number)
}
function isNewer(latest: string, current: string): boolean {
  const a = parseVersion(latest), b = parseVersion(current)
  for (let i = 0; i < Math.max(a.length, b.length); i++) {
    const x = a[i] ?? 0, y = b[i] ?? 0
    if (x > y) return true
    if (x < y) return false
  }
  return false
}

ipcMain.handle('check-for-updates', async () => {
  const current = app.getVersion()
  try {
    let data: { version?: string; notes?: string; changelog?: string; downloadUrl?: string }
    if (!app.isPackaged) {
      const p = path.join(__dirname, '../public/version.json')
      if (fs.existsSync(p)) data = JSON.parse(fs.readFileSync(p, 'utf-8'))
      else data = {}
    } else {
      const res = await fetch(UPDATE_MANIFEST_URL)
      if (!res.ok) return { hasUpdate: false, current, latest: current }
      data = await res.json()
    }
    const latest = (data.version as string) || current
    const hasUpdate = isNewer(latest, current)
    return {
      hasUpdate,
      current,
      latest,
      notes: data.notes || data.changelog || '',
      url: data.downloadUrl || '',
    }
  } catch {
    return { hasUpdate: false, current, latest: current }
  }
})

ipcMain.handle('open-update-url', async (_e, url: string) => {
  if (url) shell.openExternal(url)
})
