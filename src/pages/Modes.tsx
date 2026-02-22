import { useEffect, useState } from 'react'

type PowerPlan = { guid: string; name: string; active: boolean }

export default function Modes() {
  const [plans, setPlans] = useState<PowerPlan[]>([])
  const [activeGuid, setActiveGuid] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    window.electronAPI?.getPowerPlans().then((r) => {
      setPlans(r.plans)
      setActiveGuid(r.activeGuid)
      setLoading(false)
    })
  }, [])

  const setPlan = async (guid: string) => {
    const res = await window.electronAPI?.setPowerPlan(guid)
    if (res?.ok) setActiveGuid(guid)
  }

  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-zinc-200">Modlar</h2>

      <div className="grid gap-4">
        <div className="bg-card border border-border rounded-xl p-5">
          <h3 className="text-sm font-medium text-zinc-300 mb-1">Oyun Modu</h3>
          <p className="text-xs text-zinc-500 mb-3">Windows Oyun Modu ayarlarını aç (Game Bar, kaynak önceliği).</p>
          <button
            onClick={() => window.electronAPI?.openGameModeSettings()}
            className="px-4 py-2 bg-accent hover:bg-accentDim text-white text-sm rounded-lg transition-colors"
          >
            Oyun Modu ayarlarını aç
          </button>
        </div>

        <div className="bg-card border border-border rounded-xl p-5">
          <h3 className="text-sm font-medium text-zinc-300 mb-1">Güç planı (Performans)</h3>
          <p className="text-xs text-zinc-500 mb-3">Yüksek performans veya dengeli plan seçin.</p>
          {loading ? (
            <p className="text-xs text-zinc-500">Yükleniyor...</p>
          ) : (
            <div className="flex flex-wrap gap-2">
              {plans.map((p) => (
                <button
                  key={p.guid}
                  onClick={() => setPlan(p.guid)}
                  className={`px-3 py-1.5 text-sm rounded-lg border transition-colors ${
                    p.guid === activeGuid
                      ? 'bg-accent/20 border-accent text-accent'
                      : 'border-border text-zinc-400 hover:text-zinc-200 hover:border-zinc-500'
                  }`}
                >
                  {p.name}
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="bg-card border border-border rounded-xl p-5">
          <h3 className="text-sm font-medium text-zinc-300 mb-1">Sessiz mod (Odak yardımı)</h3>
          <p className="text-xs text-zinc-500 mb-3">Bildirimleri azaltmak için Odak yardımı ayarlarını aç.</p>
          <button
            onClick={() => window.electronAPI?.openFocusAssist()}
            className="px-4 py-2 bg-card border border-border hover:bg-white/5 text-zinc-300 text-sm rounded-lg transition-colors"
          >
            Odak yardımı ayarlarını aç
          </button>
        </div>
      </div>
    </div>
  )
}
