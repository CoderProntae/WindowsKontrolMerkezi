import { useEffect, useState } from 'react'

export default function Settings() {
  const [version, setVersion] = useState('')
  const [changelog, setChangelog] = useState('')
  const [updateInfo, setUpdateInfo] = useState<{
    hasUpdate: boolean
    current: string
    latest: string
    notes?: string
    url?: string
  } | null>(null)
  const [checking, setChecking] = useState(false)

  useEffect(() => {
    window.electronAPI?.getAppVersion().then(setVersion)
    window.electronAPI?.getChangelog().then(setChangelog)
  }, [])

  const checkUpdates = async () => {
    setChecking(true)
    try {
      const r = await window.electronAPI?.checkForUpdates()
      setUpdateInfo(r ?? null)
    } finally {
      setChecking(false)
    }
  }

  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-zinc-200">Ayarlar</h2>

      <div className="bg-card border border-border rounded-xl p-5 space-y-4">
        <h3 className="text-sm font-medium text-zinc-300">Sürüm</h3>
        <p className="text-sm text-zinc-400">Mevcut sürüm: <span className="text-accent font-mono">{version || '—'}</span></p>
        <button
          onClick={checkUpdates}
          disabled={checking}
          className="px-4 py-2 bg-accent hover:bg-accentDim disabled:opacity-50 text-white text-sm rounded-lg transition-colors"
        >
          {checking ? 'Kontrol ediliyor...' : 'Güncellemeleri kontrol et'}
        </button>

        {updateInfo && (
          <div className="mt-3 p-3 rounded-lg bg-accent/10 border border-accent/30">
            {updateInfo.hasUpdate ? (
              <>
                <p className="text-sm text-zinc-200">
                  Yeni sürüm: <strong>{updateInfo.latest}</strong> (mevcut: {updateInfo.current})
                </p>
                {updateInfo.notes && <pre className="text-xs text-zinc-400 mt-2 whitespace-pre-wrap">{updateInfo.notes}</pre>}
                {updateInfo.url && (
                  <button
                    onClick={() => window.electronAPI?.openUpdateUrl(updateInfo!.url!)}
                    className="mt-2 text-sm text-accent hover:underline"
                  >
                    İndir
                  </button>
                )}
              </>
            ) : (
              <p className="text-sm text-zinc-400">Uygulama güncel. (v{updateInfo.current})</p>
            )}
          </div>
        )}
      </div>

      <div className="bg-card border border-border rounded-xl p-5">
        <h3 className="text-sm font-medium text-zinc-300 mb-3">Değişiklik günlüğü</h3>
        <pre className="text-xs text-zinc-400 whitespace-pre-wrap font-sans max-h-64 overflow-auto rounded bg-surface p-3 border border-border">
          {changelog || 'Yükleniyor...'}
        </pre>
      </div>
    </div>
  )
}
