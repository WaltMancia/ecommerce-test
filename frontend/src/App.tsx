import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';

import { Login } from './pages/Login';
import { Catalog } from './pages/Catalog';
import { AdminDashboard } from './pages/AdminDashboard';

const Navbar = () => {
  const { user, logout, isAuthenticated } = useAuth();

  return (
    <nav className="bg-gray-800 text-white p-4 flex justify-between items-center shadow-md">
      <div className="flex gap-6 items-center">
        <Link to="/" className="text-xl font-bold tracking-wider">SECURE<span className="text-blue-400">SHOP</span></Link>
        <Link to="/" className="hover:text-gray-300 transition">Catálogo</Link>
        { user?.role === 'Admin' && (
          <Link to="/admin" className="text-red-400 hover:text-red-300 font-medium">Panel Admin</Link>
        ) }
      </div>
      <div className="flex gap-4 items-center">
        { isAuthenticated ? (
          <>
            <span className="text-sm text-gray-400">{ user?.email }</span>
            <button onClick={ logout } className="bg-gray-700 px-3 py-1 rounded hover:bg-gray-600 text-sm">Salir</button>
          </>
        ) : (
          <Link to="/login" className="bg-blue-600 px-4 py-1 rounded hover:bg-blue-700 transition">Iniciar Sesión</Link>
        ) }
      </div>
    </nav>
  );
};

function App () {
  return (
    <AuthProvider>
      <BrowserRouter>
        <div className="min-h-screen flex flex-col bg-gray-50 text-gray-800 font-sans">
          <Navbar />
          <main className="flex-grow">
            <Routes>
              {/* Públicas */ }
              <Route path="/" element={ <Catalog /> } />
              <Route path="/login" element={ <Login /> } />

              {/* Protegidas */ }
              <Route element={ <ProtectedRoute allowedRoles={ ['Admin'] } /> }>
                <Route path="/admin" element={ <AdminDashboard /> } />
              </Route>
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;