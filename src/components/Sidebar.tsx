type Page = 'dashboard' | 'modes' | 'settings'

const nav = [
  { id: 'dashboard' as const, label: 'Panel', icon: '◉' },
  { id: 'modes' as const, label: 'Modlar', icon: '◆' },
  { id: 'settings' as const, label: 'Ayarlar', icon: '⚙' },
]

export default function Sidebar({ page, onNavigate }: { page: Page; onNavigate: (p: Page) => void }) {
  return (
    <aside className="w-52 border-r border-border bg-card flex flex-col py-4">
      <div className="px-4 mb-6">
        <h1 className="text-sm font-semibold text-zinc-300 tracking-tight">Windows Kontrol Merkezi</h1>
      </div>
      <nav className="flex-1">
        {nav.map(({ id, label, icon }) => (
          <button
            key={id}
            onClick={() => onNavigate(id)}
            className={`w-full text-left px-4 py-2.5 text-sm flex items-center gap-3 transition-colors ${
              page === id
                ? 'bg-accent/20 text-accent border-l-2 border-accent'
                : 'text-zinc-400 hover:text-zinc-200 hover:bg-white/5 border-l-2 border-transparent'
            }`}
          >
            <span className="opacity-80">{icon}</span>
            {label}
          </button>
        ))}
      </nav>
    </aside>
  )
}
