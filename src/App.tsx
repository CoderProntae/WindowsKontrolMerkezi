import { useState } from 'react'
import Sidebar from './components/Sidebar'
import Dashboard from './pages/Dashboard'
import Modes from './pages/Modes'
import Settings from './pages/Settings'

type Page = 'dashboard' | 'modes' | 'settings'

export default function App() {
  const [page, setPage] = useState<Page>('dashboard')

  return (
    <div className="flex h-full bg-surface">
      <Sidebar page={page} onNavigate={setPage} />
      <main className="flex-1 overflow-auto p-6">
        {page === 'dashboard' && <Dashboard />}
        {page === 'modes' && <Modes />}
        {page === 'settings' && <Settings />}
      </main>
    </div>
  )
}
