/// <reference types="vite/client" />

interface Window {
  electronAPI: {
    getCpuUsage: () => Promise<{ usage: number; cores: number }>
    getMem: () => Promise<{ total: number; used: number; free: number; usagePercent: number }>
    getDisk: () => Promise<{ total: number; used: number; free: number; usagePercent: number }>
    getPowerPlans: () => Promise<{ plans: { guid: string; name: string; active: boolean }[]; activeGuid: string }>
    setPowerPlan: (guid: string) => Promise<{ ok: boolean; error?: string }>
    openGameModeSettings: () => Promise<{ ok: boolean }>
    openFocusAssist: () => Promise<{ ok: boolean }>
    getAppVersion: () => Promise<string>
    getChangelog: () => Promise<string>
    checkForUpdates: () => Promise<{ hasUpdate: boolean; current: string; latest: string; notes?: string; url?: string }>
    openUpdateUrl: (url: string) => Promise<void>
  }
}
