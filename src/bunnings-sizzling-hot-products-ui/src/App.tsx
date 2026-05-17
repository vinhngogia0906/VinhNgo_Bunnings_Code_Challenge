import { NavLink, Route, Routes } from "react-router-dom"
import { DailyTopProductPage } from "./pages/DailyTopProductPage"
import { RollingTopProductPage } from "./pages/RollingTopProductPage"

export default function App() {
  const navClass = ({ isActive }: { isActive: boolean }) => isActive ? "is-active" : undefined

  return (
    <div className="shell">
      <header className="topbar">
        <div className="topbar__inner">
          <div className="brand">
            <span className="brand__mark">BSHP</span>
            <span className="brand__sub">Sizzling Hot Products</span>
          </div>
          <nav className="nav" aria-label="Primary">
            <NavLink to="/" end className={navClass}>Daily</NavLink>
            <NavLink to="/rolling" className={navClass}>Rolling</NavLink>
          </nav>
        </div>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<DailyTopProductPage />} />
          <Route path="/rolling" element={<RollingTopProductPage />} />
        </Routes>
      </main>

      <footer className="footer">
        <span>Bunnings — Coding Challenge</span>
        <span>Submitted by Vinh Ngo</span>
      </footer>
    </div>
  )
}
