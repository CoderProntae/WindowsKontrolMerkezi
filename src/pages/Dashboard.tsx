import { useEffect, useState } from 'react'
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts'

const MAX_POINTS = 30

function useSystemStats() {
  const [cpu, setCpu] = useState<number[]>([])
  const [mem, setMem] = useState<number[]>([])
  const [disk, setDisk] = useState<number[]>([])
  const [current, setCurrent] = useState({ cpu: 0, mem: 0, disk: 0 })

  useEffect(() => {
    const api = window.electronAPI
    if (!api) return

    const tick = async () => {
      try {
        const [cpuRes, memRes, diskRes] = await Promise.all([
          api.getCpuUsage(),
          api.getMem(),
          api.getDisk(),
        ])
        const c = cpuRes.usage
        const m = memRes.usagePercent
        const d = diskRes.usagePercent
        setCurrent({ cpu: c, mem: m, disk: d })
        setCpu((prev) => [...prev.slice(-(MAX_POINTS - 1)), c])
        setMem((prev) => [...prev.slice(-(MAX_POINTS - 1)), m])
        setDisk((prev) => [...prev.slice(-(MAX_POINTS - 1)), d])
      } catch {}
    }
    tick()
    const id = setInterval(tick, 1500)
    return () => clearInterval(id)
  }, [])

  const chartData = cpu.map((_, i) => ({
    t: i,
    cpu: cpu[i] ?? 0,
    mem: mem[i] ?? 0,
    disk: disk[i] ?? 0,
  }))

  return { current, chartData }
}

function Ring({ value, label, color }: { value: number; label: string; color: string }) {
  const r = 42
  const circ = 2 * Math.PI * r
  const stroke = (value / 100) * circ
  return (
    <div className="flex flex-col items-center">
      <svg width="120" height="120" className="rotate-[-90deg]">
        <circle cx="60" cy="60" r={r} fill="none" stroke="#2a2a2e" strokeWidth="8" />
        <circle
          cx="60"
          cy="60"
          r={r}
          fill="none"
          stroke={color}
          strokeWidth="8"
          strokeDasharray={`${stroke} ${circ}`}
          strokeLinecap="round"
          className="transition-all duration-500"
        />
      </svg>
      <span className="text-2xl font-semibold mt-2 text-zinc-200">{Math.round(value)}%</span>
      <span className="text-xs text-zinc-500">{label}</span>
    </div>
  )
}

export default function Dashboard() {
  const { current, chartData } = useSystemStats()

  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-zinc-200">Sistem Özeti</h2>

      <div className="grid grid-cols-3 gap-4">
        <div className="bg-card border border-border rounded-xl p-6 flex justify-center">
          <Ring value={current.cpu} label="CPU" color="#7c3aed" />
        </div>
        <div className="bg-card border border-border rounded-xl p-6 flex justify-center">
          <Ring value={current.mem} label="RAM" color="#06b6d4" />
        </div>
        <div className="bg-card border border-border rounded-xl p-6 flex justify-center">
          <Ring value={current.disk} label="Disk C:" color="#10b981" />
        </div>
      </div>

      <div className="bg-card border border-border rounded-xl p-4">
        <h3 className="text-sm font-medium text-zinc-400 mb-3">Canlı Kullanım (son 45 sn)</h3>
        <div className="h-48">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData} margin={{ top: 4, right: 4, left: 4, bottom: 4 }}>
              <defs>
                <linearGradient id="cpuGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#7c3aed" stopOpacity={0.4} />
                  <stop offset="100%" stopColor="#7c3aed" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="memGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#06b6d4" stopOpacity={0.4} />
                  <stop offset="100%" stopColor="#06b6d4" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="diskGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#10b981" stopOpacity={0.4} />
                  <stop offset="100%" stopColor="#10b981" stopOpacity={0} />
                </linearGradient>
              </defs>
              <XAxis dataKey="t" hide />
              <YAxis domain={[0, 100]} hide />
              <Tooltip
                contentStyle={{ background: '#16161a', border: '1px solid #2a2a2e', borderRadius: '8px' }}
                formatter={(value: number) => [`${Math.round(value)}%`, '']}
                labelFormatter={() => ''}
              />
              <Area type="monotone" dataKey="cpu" stroke="#7c3aed" fill="url(#cpuGrad)" strokeWidth={1.5} />
              <Area type="monotone" dataKey="mem" stroke="#06b6d4" fill="url(#memGrad)" strokeWidth={1.5} />
              <Area type="monotone" dataKey="disk" stroke="#10b981" fill="url(#diskGrad)" strokeWidth={1.5} />
            </AreaChart>
          </ResponsiveContainer>
        </div>
        <div className="flex gap-6 mt-2 text-xs text-zinc-500">
          <span className="flex items-center gap-1.5"><span className="w-2 h-2 rounded-full bg-accent" /> CPU</span>
          <span className="flex items-center gap-1.5"><span className="w-2 h-2 rounded-full bg-cyan-500" /> RAM</span>
          <span className="flex items-center gap-1.5"><span className="w-2 h-2 rounded-full bg-emerald-500" /> Disk</span>
        </div>
      </div>
    </div>
  )
}
