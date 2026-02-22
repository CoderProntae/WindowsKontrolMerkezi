import { contextBridge, ipcRenderer } from 'electron'

contextBridge.exposeInMainWorld('electronAPI', {
  getCpuUsage: () => ipcRenderer.invoke('get-cpu-usage'),
  getMem: () => ipcRenderer.invoke('get-mem'),
  getDisk: () => ipcRenderer.invoke('get-disk'),
  getPowerPlans: () => ipcRenderer.invoke('get-power-plans'),
  setPowerPlan: (guid: string) => ipcRenderer.invoke('set-power-plan', guid),
  openGameModeSettings: () => ipcRenderer.invoke('open-game-mode-settings'),
  openFocusAssist: () => ipcRenderer.invoke('open-focus-assist'),
  getAppVersion: () => ipcRenderer.invoke('get-app-version'),
  getChangelog: () => ipcRenderer.invoke('get-changelog'),
  checkForUpdates: () => ipcRenderer.invoke('check-for-updates'),
  openUpdateUrl: (url: string) => ipcRenderer.invoke('open-update-url', url),
})
